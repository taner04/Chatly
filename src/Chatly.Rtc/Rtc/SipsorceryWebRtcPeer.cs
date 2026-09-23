using System.Threading.Channels;
using Chatly.Rtc.Abstractions;
using Chatly.Rtc.Enums;
using Chatly.Rtc.Events;
using Chatly.Rtc.Models;
using Chatly.Rtc.Options;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace Chatly.Rtc.Rtc;

internal sealed class SipsorceryWebRtcPeer : IWebRtcPeer
{
    private const int QueueCapacity = 16;

    private readonly RTCPeerConnection _peerConnection;
    private readonly AudioEncoder _audioEncoder;
    private readonly SipsorceryRtcAudioTransport _audioTransport;
    private readonly Channel<float[]> _outgoingAudio;
    private readonly Channel<EncodedAudio> _incomingAudio;
    private readonly Channel<Action> _eventQueue;
    private readonly CancellationTokenSource _workerCancellation = new();
    private readonly Task _outgoingWorker;
    private readonly Task _incomingWorker;
    private readonly Task _eventWorker;
    private readonly Lock _formatLock = new();
    private readonly SemaphoreSlim _remoteDescriptionLock = new(1, 1);
    private readonly Queue<IceCandidate> _pendingIceCandidates = new();
    private readonly object _closeLock = new();

    private AudioFormat? _sendingFormat;
    private bool _remoteDescriptionAccepted;
    private Task? _closeTask;
    private int _disposed;
    private int _workerFailed;
    private Exception? _workerFailure;

    internal SipsorceryWebRtcPeer(RtcOption rtcOption)
    {
        _audioEncoder = new AudioEncoder(includeLinearFormats: false, includeOpus: true);
        _outgoingAudio = CreateChannel<float[]>();
        _incomingAudio = CreateChannel<EncodedAudio>();
        _eventQueue = Channel.CreateUnbounded<Action>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        _audioTransport = new SipsorceryRtcAudioTransport(samples => _outgoingAudio.Writer.TryWrite(samples));

        _peerConnection = new RTCPeerConnection(SipsorceryWebRtcConfiguration.Create(rtcOption));
        var audioFormats = _audioEncoder.SupportedFormats
            .Where(static format =>
                format.FormatName.Equals("OPUS", StringComparison.OrdinalIgnoreCase)
                || format.FormatName.Equals("PCMU", StringComparison.OrdinalIgnoreCase)
                || format.FormatName.Equals("PCMA", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static format =>
                format.FormatName.Equals("OPUS", StringComparison.OrdinalIgnoreCase))
            .ToList();
        _peerConnection.addTrack(new MediaStreamTrack(
            audioFormats,
            MediaStreamStatusEnum.SendRecv));

        _peerConnection.onicecandidate += HandleIceCandidate;
        _peerConnection.onconnectionstatechange += HandleConnectionStateChanged;
        _peerConnection.OnAudioFormatsNegotiated += HandleAudioFormatsNegotiated;
        _peerConnection.OnAudioFrameReceived += HandleAudioFrameReceived;

        _outgoingWorker = Task.Run(ProcessOutgoingAudioAsync);
        _incomingWorker = Task.Run(ProcessIncomingAudioAsync);
        _eventWorker = Task.Run(ProcessEventsAsync);
    }

    public IRtcAudioTransport Audio => _audioTransport;

    public event EventHandler<IceCandidateCreatedEventArgs>? IceCandidateCreated;
    public event EventHandler<PeerConnectionStateChangedEventArgs>? StateChanged;

    public async Task<string> CreateOfferAsync()
    {
        ThrowIfClosed();
        var offer = _peerConnection.createOffer();
        await _peerConnection.setLocalDescription(offer);
        return offer.sdp;
    }

    public Task SetRemoteOfferAsync(string sdp) =>
        SetRemoteDescriptionAsync(sdp, RTCSdpType.offer);

    public async Task<string> CreateAnswerAsync()
    {
        ThrowIfClosed();
        var answer = _peerConnection.createAnswer();
        await _peerConnection.setLocalDescription(answer);
        return answer.sdp;
    }

    public Task SetRemoteAnswerAsync(string sdp) =>
        SetRemoteDescriptionAsync(sdp, RTCSdpType.answer);

    public async Task AddIceCandidateAsync(IceCandidate candidate)
    {
        ThrowIfClosed();
        await _remoteDescriptionLock.WaitAsync();
        try
        {
            ThrowIfClosed();
            if (!_remoteDescriptionAccepted)
            {
                _pendingIceCandidates.Enqueue(candidate);
                return;
            }

            AddIceCandidate(candidate);
        }
        finally
        {
            _remoteDescriptionLock.Release();
        }
    }

    public Task CloseAsync()
    {
        lock (_closeLock)
        {
            return _closeTask ??= CloseCoreAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            await CloseAsync();
        }
        finally
        {
            _remoteDescriptionLock.Dispose();
            _workerCancellation.Dispose();
            _audioEncoder.Dispose();
            _peerConnection.Dispose();
        }
    }

    private static Channel<T> CreateChannel<T>() => Channel.CreateBounded<T>(new BoundedChannelOptions(QueueCapacity)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = true,
        SingleWriter = false
    });

    private async Task SetRemoteDescriptionAsync(string sdp, RTCSdpType type)
    {
        ThrowIfClosed();
        await _remoteDescriptionLock.WaitAsync();
        try
        {
            ThrowIfClosed();
            var result = _peerConnection.setRemoteDescription(new RTCSessionDescriptionInit
            {
                type = type,
                sdp = sdp
            });

            if (result != SetDescriptionResultEnum.OK)
            {
                throw new InvalidOperationException($"Could not set remote {type}: {result}");
            }

            _remoteDescriptionAccepted = true;
            while (_pendingIceCandidates.TryDequeue(out var candidate))
            {
                AddIceCandidate(candidate);
            }
        }
        finally
        {
            _remoteDescriptionLock.Release();
        }
    }

    private void AddIceCandidate(IceCandidate candidate)
    {
        _peerConnection.addIceCandidate(new RTCIceCandidateInit
        {
            candidate = candidate.Candidate,
            sdpMid = candidate.SdpMid,
            sdpMLineIndex = candidate.SdpMLineIndex ?? 0
        });
    }

    private void HandleIceCandidate(RTCIceCandidate? candidate)
    {
        if (candidate is null || IsClosed)
        {
            return;
        }

        var eventArgs = new IceCandidateCreatedEventArgs(new IceCandidate(
            candidate.candidate,
            candidate.sdpMid,
            candidate.sdpMLineIndex));
        QueueEvent(() => RaiseIceCandidateCreated(eventArgs));
    }

    private void HandleConnectionStateChanged(RTCPeerConnectionState state)
    {
        if (!IsClosed)
        {
            var eventArgs = new PeerConnectionStateChangedEventArgs(MapState(state));
            QueueEvent(() => RaiseStateChanged(eventArgs));
        }
    }

    private void HandleAudioFormatsNegotiated(List<AudioFormat> formats)
    {
        lock (_formatLock)
        {
            _sendingFormat = formats.Count > 0 ? formats[0] : null;
        }
    }

    private void HandleAudioFrameReceived(EncodedAudioFrame frame)
    {
        if (IsClosed)
        {
            return;
        }

        try
        {
            _incomingAudio.Writer.TryWrite(new EncodedAudio(
                frame.EncodedAudio.ToArray(),
                new AudioFormat(frame.AudioFormat)));
        }
        catch (Exception exception) when (IsRecoverableMediaError(exception))
        {
            // Drop malformed frame metadata without escaping into SIPSorcery.
        }
        catch (Exception exception)
        {
            FailWorkers(exception);
        }
    }

    private async Task ProcessOutgoingAudioAsync()
    {
        var frame = new float[RtcAudioProcessing.SamplesPerFrame];
        var frameOffset = 0;

        try
        {
            await foreach (var samples in _outgoingAudio.Reader.ReadAllAsync(_workerCancellation.Token))
            {
                var sourceOffset = 0;
                while (sourceOffset < samples.Length)
                {
                    var count = Math.Min(RtcAudioProcessing.SamplesPerFrame - frameOffset, samples.Length - sourceOffset);
                    samples.AsSpan(sourceOffset, count).CopyTo(frame.AsSpan(frameOffset));
                    sourceOffset += count;
                    frameOffset += count;

                    if (frameOffset == RtcAudioProcessing.SamplesPerFrame)
                    {
                        try
                        {
                            SendAudioFrame(frame);
                        }
                        catch (Exception exception)
                        {
                            FailWorkers(exception);
                            return;
                        }

                        frameOffset = 0;
                    }
                }
            }
        }
        catch (OperationCanceledException) when (_workerCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            FailWorkers(exception);
        }
    }

    private void SendAudioFrame(float[] samples)
    {
        AudioFormat? format;
        lock (_formatLock)
        {
            format = _sendingFormat;
        }

        if (!format.HasValue)
        {
            return;
        }

        var sendingFormat = format.Value;
        if (sendingFormat.ClockRate <= 0 || sendingFormat.RtpClockRate <= 0)
        {
            return;
        }

        var pcm = new short[samples.Length];
        for (var i = 0; i < samples.Length; i++)
        {
            pcm[i] = RtcAudioProcessing.FloatToPcm16(samples[i]);
        }

        if (sendingFormat.ClockRate != RtcAudioProcessing.SampleRate)
        {
            pcm = RtcAudioProcessing.Resample(pcm, RtcAudioProcessing.SampleRate, sendingFormat.ClockRate);
        }

        var encoded = _audioEncoder.EncodeAudio(pcm, sendingFormat);
        var duration = RtcAudioProcessing.CalculateRtpDuration(
            pcm.Length,
            sendingFormat.ClockRate,
            sendingFormat.RtpClockRate);
        _peerConnection.SendAudio(duration, encoded);
    }

    private async Task ProcessIncomingAudioAsync()
    {
        try
        {
            await foreach (var frame in _incomingAudio.Reader.ReadAllAsync(_workerCancellation.Token))
            {
                if (frame.Format.ClockRate <= 0)
                {
                    continue;
                }

                try
                {
                    var pcm = _audioEncoder.DecodeAudio(frame.Payload, frame.Format);
                    if (frame.Format.ClockRate != RtcAudioProcessing.SampleRate)
                    {
                        pcm = RtcAudioProcessing.Resample(
                            pcm,
                            frame.Format.ClockRate,
                            RtcAudioProcessing.SampleRate);
                    }

                    var samples = new float[pcm.Length];
                    for (var i = 0; i < pcm.Length; i++)
                    {
                        samples[i] = RtcAudioProcessing.Pcm16ToFloat(pcm[i]);
                    }

                    _audioTransport.PublishReceived(samples);
                }
                catch (Exception exception) when (IsRecoverableMediaError(exception))
                {
                    // A malformed or unsupported frame does not invalidate the connection.
                }
            }
        }
        catch (OperationCanceledException) when (_workerCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            FailWorkers(exception);
        }
    }

    private async Task CloseCoreAsync()
    {
        _peerConnection.onicecandidate -= HandleIceCandidate;
        _peerConnection.onconnectionstatechange -= HandleConnectionStateChanged;
        _peerConnection.OnAudioFormatsNegotiated -= HandleAudioFormatsNegotiated;
        _peerConnection.OnAudioFrameReceived -= HandleAudioFrameReceived;

        await _audioTransport.CloseAsync();
        _outgoingAudio.Writer.TryComplete();
        _incomingAudio.Writer.TryComplete();
        _workerCancellation.Cancel();

        try
        {
            await Task.WhenAll(_outgoingWorker, _incomingWorker);
        }
        finally
        {
            await _remoteDescriptionLock.WaitAsync();
            try
            {
                _pendingIceCandidates.Clear();
                _peerConnection.Close("Call ended.");
            }
            finally
            {
                _remoteDescriptionLock.Release();
            }

            _eventQueue.Writer.TryComplete();
        }
    }

    private async Task ProcessEventsAsync()
    {
        await foreach (var callback in _eventQueue.Reader.ReadAllAsync())
        {
            try
            {
                callback();
            }
            catch
            {
                // Public observers cannot escape into SIPSorcery or stop ordered delivery.
            }
        }

        IceCandidateCreated = null;
        StateChanged = null;
    }

    private void QueueEvent(Action callback) => _eventQueue.Writer.TryWrite(callback);

    private void RaiseIceCandidateCreated(IceCandidateCreatedEventArgs eventArgs)
    {
        var subscribers = IceCandidateCreated?.GetInvocationList();
        if (subscribers is null)
        {
            return;
        }

        foreach (var subscriber in subscribers)
        {
            try
            {
                ((EventHandler<IceCandidateCreatedEventArgs>)subscriber)(this, eventArgs);
            }
            catch
            {
                // One observer must not interrupt the remaining observers.
            }
        }
    }

    private void RaiseStateChanged(PeerConnectionStateChangedEventArgs eventArgs)
    {
        var subscribers = StateChanged?.GetInvocationList();
        if (subscribers is null)
        {
            return;
        }

        foreach (var subscriber in subscribers)
        {
            try
            {
                ((EventHandler<PeerConnectionStateChangedEventArgs>)subscriber)(this, eventArgs);
            }
            catch
            {
                // One observer must not interrupt the remaining observers.
            }
        }
    }

    private void FailWorkers(Exception exception)
    {
        if (Interlocked.CompareExchange(ref _workerFailed, 1, 0) != 0)
        {
            return;
        }

        _workerFailure = exception;
        _outgoingAudio.Writer.TryComplete(exception);
        _incomingAudio.Writer.TryComplete(exception);
        _workerCancellation.Cancel();
        QueueEvent(() => RaiseStateChanged(
            new PeerConnectionStateChangedEventArgs(PeerConnectionState.Failed)));
    }

    private static bool IsRecoverableMediaError(Exception exception) =>
        exception is ArgumentException or InvalidOperationException or NotSupportedException;

    private void ThrowIfClosed()
    {
        if (IsClosed || Volatile.Read(ref _workerFailed) != 0)
        {
            throw _workerFailure is { } failure
                ? new InvalidOperationException("The RTC audio worker failed.", failure)
                : new ObjectDisposedException(nameof(SipsorceryWebRtcPeer));
        }
    }

    private bool IsClosed
    {
        get
        {
            lock (_closeLock)
            {
                return _closeTask is not null;
            }
        }
    }

    private static PeerConnectionState MapState(RTCPeerConnectionState state) => state switch
    {
        RTCPeerConnectionState.@new => PeerConnectionState.New,
        RTCPeerConnectionState.connecting => PeerConnectionState.Connecting,
        RTCPeerConnectionState.connected => PeerConnectionState.Connected,
        RTCPeerConnectionState.disconnected => PeerConnectionState.Disconnected,
        RTCPeerConnectionState.failed => PeerConnectionState.Failed,
        RTCPeerConnectionState.closed => PeerConnectionState.Closed,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown peer connection state.")
    };

    private sealed record EncodedAudio(byte[] Payload, AudioFormat Format);
}

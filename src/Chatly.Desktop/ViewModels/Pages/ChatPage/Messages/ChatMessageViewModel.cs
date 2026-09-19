using System.Collections.ObjectModel;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Desktop.Abstraction.Storage;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Storage;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage.Messages;

public sealed partial class ChatMessageViewModel : ViewModelBase, IDisposable
{
    private const string DeletedMessageContent = "Message was deleted";
    private readonly Guid _currentUserId;
    private readonly CancellationTokenSource _lifetimeCancellation;
    private readonly MessageApiClient _messageApiClient;
    private readonly string _otherUserName;
    private readonly ReactionApiClient _reactionApiClient;
    private readonly IToastService _toastService;
    private bool _isDisposed;

    public ChatMessageViewModel(
        MessageContract message,
        Guid currentUserId,
        string otherUserName,
        MessageApiClient messageApiClient,
        ReactionApiClient reactionApiClient,
        IFilePicker filePicker,
        FileDownloadClient fileDownloadClient,
        IToastService toastService,
        CancellationToken conversationCancellationToken)
    {
        MessageId = message.MessageId;
        Content = message.IsDeleted ? DeletedMessageContent : message.Content;
        SentAt = message.SentAt;
        IsOwnMessage = message.SenderUserId == currentUserId;
        IsDeleted = message.IsDeleted;
        _currentUserId = currentUserId;
        _otherUserName = otherUserName;
        _messageApiClient = messageApiClient;
        _reactionApiClient = reactionApiClient;
        _toastService = toastService;
        _lifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(conversationCancellationToken);

        if (!message.IsDeleted)
        {
            foreach (var attachment in message.Attachments)
            {
                var viewModel = new MessageAttachmentViewModel(
                    attachment,
                    filePicker,
                    fileDownloadClient,
                    toastService);
                if (viewModel.IsImage)
                {
                    ImageAttachments.Add(viewModel);
                }
                else
                {
                    FileAttachments.Add(viewModel);
                }
            }
        }

        foreach (var reaction in message.Reactions)
        {
            Reactions.Add(CreateReaction(reaction));
        }
    }

    public Guid MessageId { get; }

    [ObservableProperty] public partial string Content { get; private set; }

    public DateTimeOffset SentAt { get; }

    public bool IsOwnMessage { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRemoveMessage))]
    [NotifyPropertyChangedFor(nameof(CanReact))]
    [NotifyCanExecuteChangedFor(nameof(RemoveMessageCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetReactionCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveReactionCommand))]
    public partial bool IsDeleted { get; private set; }

    public bool CanRemoveMessage => IsOwnMessage && !IsDeleted;

    public bool CanReact => !IsDeleted;

    public string Time => SentAt.ToLocalTime().ToString("t");

    public ObservableCollection<ChatReactionViewModel> Reactions { get; } = [];

    public ObservableCollection<MessageAttachmentViewModel> ImageAttachments { get; } = [];

    public ObservableCollection<MessageAttachmentViewModel> FileAttachments { get; } = [];

    public MessageAttachmentViewModel? PrimaryImage => ImageAttachments.FirstOrDefault();

    public IReadOnlyList<MessageAttachmentViewModel> SecondaryImages => ImageAttachments.Skip(1).ToList();

    public bool HasSingleImage => ImageAttachments.Count == 1;

    public bool HasTwoImages => ImageAttachments.Count == 2;

    public bool HasThreeImages => ImageAttachments.Count == 3;

    public bool HasImageGrid => ImageAttachments.Count >= 4;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetReactionCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveReactionCommand))]
    public partial bool IsReactionPending { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveMessageCommand))]
    public partial bool IsRemovePending { get; private set; }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _lifetimeCancellation.Cancel();
        _lifetimeCancellation.Dispose();
        SetReactionCommand.NotifyCanExecuteChanged();
        RemoveReactionCommand.NotifyCanExecuteChanged();
        RemoveMessageCommand.NotifyCanExecuteChanged();
    }

    internal void UpsertReaction(MessageReactionContract reaction)
    {
        var existing = Reactions.FirstOrDefault(item => item.UserId == reaction.UserId);
        if (existing is not null &&
            existing.ReactionId == reaction.ReactionId &&
            existing.ReactionType == reaction.ReactionType)
        {
            return;
        }

        var index = existing is null ? Reactions.Count : Reactions.IndexOf(existing);
        if (existing is not null)
        {
            Reactions.RemoveAt(index);
        }

        Reactions.Insert(index, CreateReaction(reaction));
    }

    internal void RemoveReaction(Guid reactionId)
    {
        var reaction = Reactions.FirstOrDefault(item => item.ReactionId == reactionId);

        if (reaction is not null)
        {
            Reactions.Remove(reaction);
        }
    }

    internal void MarkDeleted()
    {
        if (IsDeleted)
        {
            return;
        }

        Content = DeletedMessageContent;
        ImageAttachments.Clear();
        FileAttachments.Clear();
        NotifyImageGalleryChanged();
        Reactions.Clear();
        IsDeleted = true;
    }

    private void NotifyImageGalleryChanged()
    {
        OnPropertyChanged(nameof(PrimaryImage));
        OnPropertyChanged(nameof(SecondaryImages));
        OnPropertyChanged(nameof(HasSingleImage));
        OnPropertyChanged(nameof(HasTwoImages));
        OnPropertyChanged(nameof(HasThreeImages));
        OnPropertyChanged(nameof(HasImageGrid));
    }

    private bool CanChangeReaction() => !_isDisposed && !IsDeleted && !IsReactionPending;

    private bool CanRemove() => !_isDisposed && CanRemoveMessage && !IsRemovePending;

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task RemoveMessageAsync(CancellationToken cancellationToken)
    {
        if (!CanRemove())
        {
            return;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _lifetimeCancellation.Token);
        IsRemovePending = true;

        try
        {
            var result = await _messageApiClient.RemoveMessageAsync(MessageId, linkedCancellation.Token);
            linkedCancellation.Token.ThrowIfCancellationRequested();
            if (result.IsFailure)
            {
                _toastService.ShowError(result.Error);
                return;
            }

            MarkDeleted();
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
        }
        finally
        {
            IsRemovePending = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanChangeReaction))]
    private async Task SetReactionAsync(
        ReactionType reactionType,
        CancellationToken cancellationToken)
    {
        if (!CanChangeReaction())
        {
            return;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _lifetimeCancellation.Token);
        IsReactionPending = true;

        try
        {
            var result = await _reactionApiClient.SetReactionAsync(
                MessageId,
                reactionType,
                linkedCancellation.Token);
            linkedCancellation.Token.ThrowIfCancellationRequested();
            if (result.IsFailure)
            {
                _toastService.ShowError(result.Error);
                return;
            }

            UpsertReaction(result.Value);
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
        }
        finally
        {
            IsReactionPending = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanChangeReaction))]
    private async Task RemoveReactionAsync(
        ChatReactionViewModel? reaction,
        CancellationToken cancellationToken)
    {
        if (!CanChangeReaction() || reaction is null || !reaction.IsOwnReaction)
        {
            return;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _lifetimeCancellation.Token);
        IsReactionPending = true;

        try
        {
            var result = await _reactionApiClient.RemoveReactionAsync(
                reaction.ReactionId,
                linkedCancellation.Token);
            linkedCancellation.Token.ThrowIfCancellationRequested();
            if (result.IsFailure)
            {
                _toastService.ShowError(result.Error);
                return;
            }

            RemoveReaction(reaction.ReactionId);
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
        }
        finally
        {
            IsReactionPending = false;
        }
    }

    private ChatReactionViewModel CreateReaction(MessageReactionContract reaction) =>
        new(
            reaction,
            reaction.UserId == _currentUserId,
            reaction.UserId == _currentUserId ? "You" : _otherUserName);
}
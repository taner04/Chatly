using Chatly.WebApi.Features.Calls.Enums;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Users.Models;

namespace Chatly.Calls.Tests;

public sealed class CallAggregateTests
{
    private static readonly UserId Caller = UserId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    private static readonly UserId Receiver = UserId.From(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    private static readonly UserId Outsider = UserId.From(Guid.Parse("33333333-3333-3333-3333-333333333333"));
    private static readonly DateTimeOffset AcceptedAt = new(2026, 9, 23, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndedAt = new(2026, 9, 23, 10, 5, 0, TimeSpan.Zero);

    [Fact]
    public void New_call_starts_ringing()
    {
        var call = CreateCall();

        Assert.Equal(CallStatus.Ringing, call.Status);
        Assert.Null(call.AcceptedAt);
        Assert.Null(call.EndedAt);
        Assert.Null(call.EndReason);
    }

    [Fact]
    public void Self_call_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => new Call(Caller, Caller));
    }

    [Fact]
    public void Only_receiver_can_accept()
    {
        var call = CreateCall();

        Assert.Throws<UnauthorizedAccessException>(() => call.Accept(Caller, AcceptedAt));
        AssertRinging(call);
    }

    [Fact]
    public void Only_receiver_can_reject()
    {
        var call = CreateCall();

        Assert.Throws<UnauthorizedAccessException>(() => call.Reject(Caller, EndedAt));
        AssertRinging(call);
    }

    [Fact]
    public void Only_caller_can_offer()
    {
        var call = CreateAcceptedCall();

        Assert.Throws<UnauthorizedAccessException>(() => call.Offer(Receiver));
        Assert.Equal(CallStatus.Accepted, call.Status);
        Assert.Equal(AcceptedAt, call.AcceptedAt);
    }

    [Fact]
    public void Only_receiver_can_answer()
    {
        var call = CreateOfferedCall();

        Assert.Throws<UnauthorizedAccessException>(() => call.Answer(Caller));
        Assert.Equal(CallStatus.Offered, call.Status);
        Assert.Equal(AcceptedAt, call.AcceptedAt);
    }

    [Fact]
    public void Happy_lifecycle_records_timestamps_and_completed_reason()
    {
        var call = CreateCall();

        call.Accept(Receiver, AcceptedAt);
        Assert.Equal(CallStatus.Accepted, call.Status);
        Assert.Equal(AcceptedAt, call.AcceptedAt);

        call.Offer(Caller);
        Assert.Equal(CallStatus.Offered, call.Status);

        call.Answer(Receiver);
        Assert.Equal(CallStatus.Active, call.Status);

        call.End(Caller, EndedAt);
        Assert.Equal(CallStatus.Ended, call.Status);
        Assert.Equal(CallEndReason.Completed, call.EndReason);
        Assert.Equal(AcceptedAt, call.AcceptedAt);
        Assert.Equal(EndedAt, call.EndedAt);
    }

    [Fact]
    public void Out_of_order_transition_does_not_corrupt_state()
    {
        var call = CreateCall();

        Assert.ThrowsAny<Exception>(() => call.Answer(Receiver));

        AssertRinging(call);
    }

    [Fact]
    public void Duplicate_terminal_transition_does_not_corrupt_state()
    {
        var call = CreateActiveCall();
        call.End(Caller, EndedAt);

        var duplicateTimestamp = EndedAt.AddMinutes(1);
        Assert.ThrowsAny<Exception>(() => call.End(Receiver, duplicateTimestamp));

        Assert.Equal(CallStatus.Ended, call.Status);
        Assert.Equal(CallEndReason.Completed, call.EndReason);
        Assert.Equal(EndedAt, call.EndedAt);
    }

    [Theory]
    [InlineData(CallStatus.Ringing, true, CallEndReason.Cancelled)]
    [InlineData(CallStatus.Ringing, false, CallEndReason.Declined)]
    [InlineData(CallStatus.Accepted, true, CallEndReason.Failed)]
    [InlineData(CallStatus.Offered, false, CallEndReason.Failed)]
    [InlineData(CallStatus.Active, true, CallEndReason.Completed)]
    public void End_reason_depends_on_state_and_actor(
        CallStatus status,
        bool endedByCaller,
        CallEndReason expectedReason)
    {
        var call = CreateAt(status);

        call.End(endedByCaller ? Caller : Receiver, EndedAt);

        Assert.Equal(CallStatus.Ended, call.Status);
        Assert.Equal(expectedReason, call.EndReason);
        Assert.Equal(EndedAt, call.EndedAt);
    }

    [Fact]
    public void Ringing_expiry_is_missed()
    {
        var call = CreateCall();

        call.Expire(EndedAt);

        Assert.Equal(CallStatus.Ended, call.Status);
        Assert.Equal(CallEndReason.Missed, call.EndReason);
        Assert.Equal(EndedAt, call.EndedAt);
    }

    [Theory]
    [InlineData(CallStatus.Accepted)]
    [InlineData(CallStatus.Offered)]
    [InlineData(CallStatus.Active)]
    public void Abandoned_expiry_ends_non_ringing_call_as_failed(CallStatus status)
    {
        var call = CreateAt(status);

        call.ExpireAbandoned(EndedAt);

        Assert.Equal(CallStatus.Ended, call.Status);
        Assert.Equal(CallEndReason.Failed, call.EndReason);
        Assert.Equal(EndedAt, call.EndedAt);
    }

    [Fact]
    public void Abandoned_expiry_rejects_ringing_call_without_corrupting_state()
    {
        var call = CreateCall();

        Assert.ThrowsAny<Exception>(() => call.ExpireAbandoned(EndedAt));

        AssertRinging(call);
    }

    [Fact]
    public void Abandoned_expiry_rejects_ended_call_without_corrupting_state()
    {
        var call = CreateActiveCall();
        call.End(Caller, EndedAt);

        Assert.ThrowsAny<Exception>(() => call.ExpireAbandoned(EndedAt.AddMinutes(1)));

        Assert.Equal(CallStatus.Ended, call.Status);
        Assert.Equal(CallEndReason.Completed, call.EndReason);
        Assert.Equal(EndedAt, call.EndedAt);
    }

    [Fact]
    public void Counterpart_is_returned_for_each_participant()
    {
        var call = CreateCall();

        Assert.Equal(Receiver, call.GetCounterpart(Caller));
        Assert.Equal(Caller, call.GetCounterpart(Receiver));
    }

    [Fact]
    public void Counterpart_rejects_non_participant()
    {
        var call = CreateCall();

        Assert.Throws<UnauthorizedAccessException>(() => call.GetCounterpart(Outsider));
    }

    [Fact]
    public void Ice_authorization_accepts_participants_and_rejects_outsider()
    {
        var call = CreateOfferedCall();

        call.EnsureCanSendIce(Caller);
        call.EnsureCanSendIce(Receiver);
        Assert.Throws<UnauthorizedAccessException>(() => call.EnsureCanSendIce(Outsider));

        Assert.Equal(CallStatus.Offered, call.Status);
    }

    [Fact]
    public void End_rejects_non_participant_without_corrupting_state()
    {
        var call = CreateActiveCall();

        Assert.Throws<UnauthorizedAccessException>(() => call.End(Outsider, EndedAt));

        Assert.Equal(CallStatus.Active, call.Status);
        Assert.Null(call.EndReason);
        Assert.Null(call.EndedAt);
    }

    private static Call CreateCall() => new(Caller, Receiver);

    private static Call CreateAcceptedCall()
    {
        var call = CreateCall();
        call.Accept(Receiver, AcceptedAt);
        return call;
    }

    private static Call CreateOfferedCall()
    {
        var call = CreateAcceptedCall();
        call.Offer(Caller);
        return call;
    }

    private static Call CreateActiveCall()
    {
        var call = CreateOfferedCall();
        call.Answer(Receiver);
        return call;
    }

    private static Call CreateAt(CallStatus status) => status switch
    {
        CallStatus.Ringing => CreateCall(),
        CallStatus.Accepted => CreateAcceptedCall(),
        CallStatus.Offered => CreateOfferedCall(),
        CallStatus.Active => CreateActiveCall(),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    private static void AssertRinging(Call call)
    {
        Assert.Equal(CallStatus.Ringing, call.Status);
        Assert.Null(call.AcceptedAt);
        Assert.Null(call.EndedAt);
        Assert.Null(call.EndReason);
    }
}

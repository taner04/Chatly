using Chatly.Contracts.Features.DeviceSessions.Models;
using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Chatly.WebApi.Features.DeviceSessions.Exceptions;
using Chatly.WebApi.Features.DeviceSessions.Models;

namespace Chatly.WebApi.Features.DeviceSessions.Services;

[ScopedService]
public sealed class DeviceSessionService(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    NotificationPublisher notificationPublisher)
{
    private static readonly TimeSpan LastSeenUpdateInterval = TimeSpan.FromMinutes(1);

    internal Task<DeviceSession> GetOrCreateCurrentActiveAsync(CancellationToken cancellationToken) =>
        GetOrCreateActiveAsync(
            currentUserService.UserId,
            currentUserService.Device,
            currentUserService.IdentitySessionId,
            cancellationToken);

    internal async Task<DeviceSession> GetOrCreateActiveAsync(
        UserId userId,
        DeviceInfo device,
        string? identitySessionId,
        CancellationToken cancellationToken)
    {
        var session = await FindAsync(userId, device.DeviceId, cancellationToken);
        if (session is null)
        {
            return await CreateAsync(userId, device, identitySessionId, cancellationToken);
        }

        if (session is { RevokedAt: not null })
        {
            throw new DeviceSessionRevokedException();
        }

        var now = DateTimeOffset.UtcNow;
        var identitySessionChanged = identitySessionId is not null && session.IdentitySessionId != identitySessionId;
        if (!identitySessionChanged && now - session.LastSeenAt < LastSeenUpdateInterval)
        {
            return session;
        }

        session.DeviceName = device.DeviceName;
        session.Platform = device.Platform;
        session.AppVersion = device.AppVersion;
        session.IdentitySessionId = identitySessionId ?? session.IdentitySessionId;
        session.LastSeenAt = now;
        await context.SaveChangesAsync(cancellationToken);

        return session;
    }

    internal Task<List<DeviceSession>> GetActiveAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        return context.DeviceSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .OrderByDescending(session => session.LastSeenAt)
            .ToListAsync(cancellationToken);
    }

    internal async Task<DeviceSession> RevokeAsync(
        DeviceSessionId sessionId,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var currentDeviceId = currentUserService.Device.DeviceId;

        var session = await context.DeviceSessions
                          .SingleOrDefaultAsync(
                              session => session.Id == sessionId
                                         && session.UserId == userId
                                         && session.RevokedAt == null,
                              cancellationToken)
                      ?? throw new EntityNotFoundException<DeviceSession>(sessionId.Value);

        if (session.DeviceId == currentDeviceId)
        {
            throw new CurrentDeviceSessionRevocationException();
        }

        session.RevokedAt = DateTimeOffset.UtcNow;
        return session;
    }

    internal async Task<DeviceSession?> RevokeCurrentAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var currentDeviceId = currentUserService.Device.DeviceId;

        var session = await FindAsync(userId, currentDeviceId, cancellationToken);
        if (session is not { RevokedAt: null })
        {
            return null;
        }

        session.RevokedAt = DateTimeOffset.UtcNow;
        return session;
    }

    internal async Task<List<DeviceSession>> RevokeOthersAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var currentDeviceId = currentUserService.Device.DeviceId;

        var sessions = await context.DeviceSessions
            .Where(session => session.UserId == userId
                              && session.DeviceId != currentDeviceId
                              && session.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        return sessions;
    }

    internal async Task<List<DeviceSession>> RevokeByIdentitySessionAsync(
        string identitySessionId,
        CancellationToken cancellationToken)
    {
        var sessions = await context.DeviceSessions
            .Where(session => session.IdentitySessionId == identitySessionId && session.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        return sessions;
    }

    internal async Task PublishRevokedAsync(IReadOnlyCollection<DeviceSession> sessions)
    {
        await notificationPublisher.PublishAsync(
            sessions.Select(session => session.Id),
            new DeviceSessionRevokedNotification());
        await notificationPublisher.PublishAsync(
            sessions.Select(session => session.UserId),
            new DeviceSessionsChangedNotification());
    }

    internal DeviceSessionContract CreateContract(DeviceSession session) =>
        new(
            session.Id.Value,
            session.DeviceName,
            session.Platform,
            session.AppVersion,
            session.CreatedAt,
            session.LastSeenAt,
            session.DeviceId == currentUserService.Device.DeviceId);

    private Task<DeviceSession?> FindAsync(
        UserId userId,
        Guid deviceId,
        CancellationToken cancellationToken) =>
        context.DeviceSessions.SingleOrDefaultAsync(
            session => session.UserId == userId && session.DeviceId == deviceId,
            cancellationToken);

    private async Task<DeviceSession> CreateAsync(
        UserId userId,
        DeviceInfo device,
        string? identitySessionId,
        CancellationToken cancellationToken)
    {
        if (identitySessionId is not null
            && await context.DeviceSessions.AnyAsync(
                session => session.IdentitySessionId == identitySessionId && session.RevokedAt != null,
                cancellationToken))
        {
            throw new DeviceSessionRevokedException();
        }

        var session = new DeviceSession(
            userId,
            device.DeviceId,
            device.DeviceName,
            device.Platform,
            device.AppVersion)
        {
            IdentitySessionId = identitySessionId
        };

        context.DeviceSessions.Add(session);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await notificationPublisher.PublishAsync(userId, new DeviceSessionsChangedNotification());
            return session;
        }
        catch (DbUpdateException exception) when (exception.IsUniqueViolation())
        {
            context.Entry(session).State = EntityState.Detached;
            return await FindAsync(userId, device.DeviceId, cancellationToken) switch
            {
                null => throw new InvalidOperationException("The concurrently created device session was not found."),
                { RevokedAt: not null } => throw new DeviceSessionRevokedException(),
                var existing => existing
            };
        }
    }
}
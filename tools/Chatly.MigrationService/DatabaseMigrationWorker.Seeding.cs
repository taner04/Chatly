using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Friendships.Models;
using Chatly.WebApi.Features.Messages.Models;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Chatly.MigrationService;

public sealed partial class DatabaseMigrationWorker
{
    private static async Task RunSeedAsync(
        ChatlyDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            const string seedCreatedBy = "DevelopmentSeed";
            const string secondUserAuth0Id = "auth0|6a88a4ef8a2e6820529a6ab5";
            const string primaryUserAuth0Id = "auth0|69d50162f32c110c72fdfeb9";

            var seedUsers = Enumerable.Range(1, 50)
                .Select(index => new
                {
                    Email = $"testuser{index:D3}@chatly.test",
                    Auth0Id = $"seed|user-{index:D3}",
                    Username = $"testuser{index:D3}"
                })
                .Append(new
                {
                    Email = "test@byom.de",
                    Auth0Id = secondUserAuth0Id,
                    Username = "Tester"
                })
                .Append(new
                {
                    Email = "taner@byom.de",
                    Auth0Id = primaryUserAuth0Id,
                    Username = "Taner"
                })
                .ToList();

            var existingAuth0Ids = await dbContext.Users
                .Select(user => user.Auth0Id)
                .ToListAsync(cancellationToken);
            var usersToAdd = seedUsers
                .Where(user => !existingAuth0Ids.Contains(user.Auth0Id))
                .Select(user => new User(user.Email, user.Auth0Id)
                {
                    Username = user.Username,
                    OnboardingCompleted = true
                })
                .ToList();

            usersToAdd.ForEach(user => user.SetCreated(seedCreatedBy));
            if (usersToAdd.Count > 0)
            {
                dbContext.Users.AddRange(usersToAdd);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var users = await dbContext.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var primaryUser = users.Single(user => user.Auth0Id == primaryUserAuth0Id);
            var secondUser = users.Single(user => user.Auth0Id == secondUserAuth0Id);
            var paginationUsers = users
                .Where(user => user.Auth0Id.StartsWith("seed|user-", StringComparison.Ordinal))
                .OrderBy(user => user.Username)
                .ToList();

            var existingRequestPairs = await dbContext.FriendRequests
                .AsNoTracking()
                .Where(request => request.ReceiverUserId == primaryUser.Id ||
                                  request.ReceiverUserId == secondUser.Id)
                .Select(request => new
                {
                    request.SenderUserId,
                    request.ReceiverUserId
                })
                .ToListAsync(cancellationToken);

            foreach (var (sender, index) in paginationUsers.Select((user, index) => (user, index)))
            {
                var receiverId = index < 25 ? primaryUser.Id : secondUser.Id;
                if (existingRequestPairs.Any(pair =>
                        pair.SenderUserId == sender.Id && pair.ReceiverUserId == receiverId))
                {
                    continue;
                }

                var friendRequest = new FriendRequest(sender.Id, receiverId);
                friendRequest.SetCreated(seedCreatedBy);
                dbContext.FriendRequests.Add(friendRequest);
            }

            var pair = UserPair.Create(primaryUser.Id, secondUser.Id);
            var friendship = await dbContext.Friendships.SingleOrDefaultAsync(
                candidate => candidate.FirstUserId == pair.FirstUserId &&
                             candidate.SecondUserId == pair.SecondUserId,
                cancellationToken);

            if (friendship is null)
            {
                friendship = new Friendship(primaryUser.Id, secondUser.Id);
                friendship.SetCreated(seedCreatedBy);
                dbContext.Friendships.Add(friendship);
            }

            var chat = await dbContext.Chats.SingleOrDefaultAsync(
                candidate => candidate.FirstUserId == pair.FirstUserId &&
                             candidate.SecondUserId == pair.SecondUserId,
                cancellationToken);

            if (chat is null)
            {
                chat = new Chat(primaryUser.Id, secondUser.Id);
                chat.SetCreated(seedCreatedBy);
                dbContext.Chats.Add(chat);
            }

            var script = new (bool FromOwner, string Content, int MinutesAgo)[]
            {
                (false, "Hey, this is a seeded conversation so you can preview the chat layout.", 42),
                (true, "Perfect. I wanted to check the incoming and outgoing message bubbles.", 39),
                (false, "Short messages work too.", 36),
                (true, "Nice!", 35),
                (false,
                    "Here is a longer message to demonstrate wrapping. The bubble should stop growing at its maximum width and continue naturally onto another line without making the conversation difficult to scan.",
                    30),
                (true, "The spacing and timestamp treatment look much clearer with realistic content.", 24),
                (false, "Try sending a new message below. It uses the real API and appears immediately.", 17),
                (true, "Testing the composer now.", 11),
                (false, "Everything is connected to persisted message history.", 5)
            };

            var paginationScript = Enumerable.Range(1, 65)
                .Select(index => (
                    FromOwner: index % 2 == 0,
                    Content: $"Pagination test message {index:D2} of 65.",
                    MinutesAgo: 120 + 65 - index));
            var existingSeedMessageContents = await dbContext.Messages
                .AsNoTracking()
                .Where(message => message.ChatId == chat.Id && message.CreatedBy == seedCreatedBy)
                .Select(message => message.Content)
                .ToHashSetAsync(cancellationToken);

            foreach (var entry in script.Concat(paginationScript))
            {
                if (existingSeedMessageContents.Contains(entry.Content))
                {
                    continue;
                }

                var message = new Message(
                    chat.Id,
                    entry.FromOwner ? primaryUser.Id : secondUser.Id,
                    entry.Content)
                {
                    SentAt = DateTimeOffset.UtcNow.AddMinutes(-entry.MinutesAgo)
                };
                message.SetCreated(seedCreatedBy);
                dbContext.Messages.Add(message);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        });
    }
}
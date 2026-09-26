using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Models.Settings.Theme;

namespace Chatly.Desktop.UnitTests.Tests.Extensions;

public sealed class EnumExtensionsTests
{
    [Fact]
    public void ToEmoji_Should_MapEveryReactionToAUniqueEmoji_When_AllReactionsAreUsed()
    {
        var emojis = Enum.GetValues<ReactionType>().Select(reaction => reaction.ToEmoji()).ToList();

        emojis.Should().OnlyHaveUniqueItems().And.OnlyContain(emoji => !string.IsNullOrWhiteSpace(emoji));
    }

    [Fact]
    public void GetHexCode_Should_ReturnHexColor_When_AccentColorIsDefined()
    {
        foreach (var accentColor in Enum.GetValues<AccentColor>())
        {
            accentColor.GetHexCode().Should().MatchRegex("^#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?$", accentColor.ToString());
        }
    }
}

using System.Globalization;
using Chatly.Desktop.Converters;

namespace Chatly.Desktop.UnitTests.Tests.Converters;

public sealed class ConverterTests
{
    [Theory]
    [InlineData("  taner", "T")]
    [InlineData("", "?")]
    [InlineData(null, "?")]
    public void FirstCharacterConverter_Should_ReturnUppercaseInitial_When_TextIsGiven(string? value, string expected)
    {
        new FirstCharacterConverter().Convert(value, typeof(string), null, CultureInfo.InvariantCulture)
            .Should().Be(expected);
    }

    [Theory]
    [InlineData("text", null, true)]
    [InlineData("   ", null, false)]
    [InlineData(null, null, false)]
    [InlineData("text", "Invert", false)]
    [InlineData(null, "Invert", true)]
    public void StringHasValueConverter_Should_ReportPresence_When_ValueIsGiven(string? value, string? parameter, bool expected)
    {
        new StringHasValueConverter().Convert(value, typeof(bool), parameter, CultureInfo.InvariantCulture)
            .Should().Be(expected);
    }

    [Fact]
    public void ImageUrlConverter_Should_NormalizeUrls_When_ValueIsUriOrText()
    {
        var converter = new ImageUrlConverter();

        converter.Convert(new Uri("https://cdn.test/a.png"), typeof(string), null, CultureInfo.InvariantCulture)
            .Should().Be("https://cdn.test/a.png");
        converter.Convert("   ", typeof(string), null, CultureInfo.InvariantCulture).Should().BeNull();
    }
}

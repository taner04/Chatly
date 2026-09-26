using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.UnitTests.Tests.Utilities;

public sealed class FileSizeFormatterTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1024 * 1024, "1 MB")]
    [InlineData(10 * 1024 * 1024 + 512 * 1024, "10.5 MB")]
    public void Format_Should_UseLargestFittingUnit_When_SizeIsGiven(long size, string expected)
    {
        FileSizeFormatter.Format(size).Should().Be(expected.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
    }
}

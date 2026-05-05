using PortableMarkdownFormat.Core;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class PmfPathNormalizerTests
{
    [Fact]
    public void NormalizeRelativePath_ReplacesBackslashes()
    {
        var normalized = PmfPathNormalizer.NormalizeRelativePath(@"docs\guide\intro.md");

        Assert.Equal("docs/guide/intro.md", normalized);
    }

    [Theory]
    [InlineData("/docs/intro.md", "RootedPath")]
    [InlineData("docs//intro.md", "EmptySegment")]
    [InlineData("docs/../intro.md", "RelativeSegment")]
    [InlineData("_pmf/info.md", "ReservedSegment")]
    public void NormalizeRelativePath_RejectsInvalidPaths(string path, string expectedCode)
    {
        var exception = Assert.Throws<PmfException>(() => PmfPathNormalizer.NormalizeRelativePath(path));

        Assert.Equal(expectedCode, exception.Code);
    }
}

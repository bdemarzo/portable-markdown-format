using PortableMarkdownFormat.Core;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class CoreModelTests
{
    [Fact]
    public void ArchiveEntry_ExposesFinalPathSegmentAsName()
    {
        var entry = new PmfArchiveEntry("docs/intro.md");

        Assert.Equal("intro.md", entry.Name);
    }

    [Fact]
    public void ArchiveDocument_ExposesFinalPathSegmentAsName()
    {
        var document = new PmfArchiveDocument("docs/intro.md", "# Intro");

        Assert.Equal("intro.md", document.Name);
    }

    [Fact]
    public void Archive_GetDocumentThrowsForUnknownPath()
    {
        var archive = new PmfArchive(
            "sample.pmf",
            [new PmfArchiveDocument("README.md", "# Home")]);

        var exception = Assert.Throws<PmfException>(() => archive.GetDocument("missing.md"));

        Assert.Equal("DocumentNotFound", exception.Code);
    }
}

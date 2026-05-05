using System.IO.Compression;
using PortableMarkdownFormat.Core;
using PortableMarkdownFormat.Core.Tests.TestSupport;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class PmfArchiveBuilderTests
{
    [Fact]
    public void Build_PackagesMarkdownFilesAndPreservesHierarchy()
    {
        using var temp = new TemporaryDirectory();
        temp.CreateFile("README.md", "# Root");
        temp.CreateFile(System.IO.Path.Combine("docs", "guide.markdown"), "## Guide");
        temp.CreateFile(System.IO.Path.Combine("docs", "image.png"), "not markdown");

        var archivePath = System.IO.Path.Combine(temp.Path, "output.pmf");
        var builder = new PmfArchiveBuilder();

        var result = builder.Build(temp.Path, archivePath);

        Assert.Equal(2, result.EntryCount);
        Assert.Collection(
            result.Entries,
            entry => Assert.Equal("docs/guide.markdown", entry.RelativePath),
            entry => Assert.Equal("README.md", entry.RelativePath));

        using var stream = File.OpenRead(archivePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        Assert.Collection(
            archive.Entries.OrderBy(entry => entry.FullName, StringComparer.OrdinalIgnoreCase),
            entry => Assert.Equal("docs/guide.markdown", entry.FullName),
            entry => Assert.Equal("README.md", entry.FullName));
    }

    [Fact]
    public void Build_ThrowsWhenNoMarkdownFilesArePresent()
    {
        using var temp = new TemporaryDirectory();
        temp.CreateFile("notes.txt", "hello");

        var archivePath = System.IO.Path.Combine(temp.Path, "output.pmf");
        var builder = new PmfArchiveBuilder();

        var exception = Assert.Throws<PmfException>(() => builder.Build(temp.Path, archivePath));

        Assert.Equal("NoMarkdownFiles", exception.Code);
    }

    [Fact]
    public void Build_ThrowsWhenArchiveExistsWithoutOverwrite()
    {
        using var temp = new TemporaryDirectory();
        temp.CreateFile("README.md", "# Root");

        var archivePath = System.IO.Path.Combine(temp.Path, "output.pmf");
        File.WriteAllText(archivePath, "existing");
        var builder = new PmfArchiveBuilder();

        var exception = Assert.Throws<PmfException>(() => builder.Build(temp.Path, archivePath));

        Assert.Equal("ArchiveAlreadyExists", exception.Code);
    }

    [Fact]
    public void Build_OverwriteReplacesExistingArchive()
    {
        using var temp = new TemporaryDirectory();
        temp.CreateFile("README.md", "# Root");

        var archivePath = System.IO.Path.Combine(temp.Path, "output.pmf");
        File.WriteAllText(archivePath, "existing");
        var builder = new PmfArchiveBuilder();

        var result = builder.Build(temp.Path, archivePath, overwrite: true);

        Assert.True(File.Exists(result.ArchivePath));
        Assert.Equal(1, result.EntryCount);
    }
}

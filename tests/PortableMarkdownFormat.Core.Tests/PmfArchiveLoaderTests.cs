using System.IO.Compression;
using PortableMarkdownFormat.Core;
using PortableMarkdownFormat.Core.Tests.TestSupport;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class PmfArchiveLoaderTests
{
    [Fact]
    public void Load_ReadsDocumentsAndBuildsHierarchy()
    {
        using var temp = new TemporaryDirectory();
        var archivePath = System.IO.Path.Combine(temp.Path, "sample.pmf");

        using (var stream = File.Create(archivePath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            using (var firstWriter = new StreamWriter(archive.CreateEntry("README.md").Open()))
            {
                firstWriter.Write("# Home");
            }

            using (var secondWriter = new StreamWriter(archive.CreateEntry("docs/getting-started.md").Open()))
            {
                secondWriter.Write("## Start");
            }
        }

        var loader = new PmfArchiveLoader();
        var archiveModel = loader.Load(archivePath);

        Assert.Equal(2, archiveModel.Documents.Count);
        Assert.Equal("# Home", archiveModel.GetDocument("README.md").Markdown);
        Assert.Collection(
            archiveModel.Hierarchy,
            node => Assert.Equal("docs", node.Name),
            node => Assert.Equal("README.md", node.Name));
    }

    [Fact]
    public void Load_RejectsNonMarkdownEntries()
    {
        using var temp = new TemporaryDirectory();
        var archivePath = System.IO.Path.Combine(temp.Path, "sample.pmf");

        using (var stream = File.Create(archivePath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            using var writer = new StreamWriter(archive.CreateEntry("image.png").Open());
            writer.Write("png");
        }

        var loader = new PmfArchiveLoader();

        var exception = Assert.Throws<PmfException>(() => loader.Load(archivePath));

        Assert.Equal("NonMarkdownEntry", exception.Code);
    }

    [Fact]
    public void Load_RejectsCaseInsensitiveDuplicateEntries()
    {
        using var temp = new TemporaryDirectory();
        var archivePath = System.IO.Path.Combine(temp.Path, "sample.pmf");

        using (var stream = File.Create(archivePath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            using (var firstWriter = new StreamWriter(archive.CreateEntry("docs/Readme.md").Open()))
            {
                firstWriter.Write("# One");
            }

            using (var secondWriter = new StreamWriter(archive.CreateEntry("docs/readme.md").Open()))
            {
                secondWriter.Write("# Two");
            }
        }

        var loader = new PmfArchiveLoader();

        var exception = Assert.Throws<PmfException>(() => loader.Load(archivePath));

        Assert.Equal("DuplicateEntry", exception.Code);
    }

    [Fact]
    public void Load_RejectsEmptyArchives()
    {
        using var temp = new TemporaryDirectory();
        var archivePath = System.IO.Path.Combine(temp.Path, "sample.pmf");

        using (var stream = File.Create(archivePath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
        }

        var loader = new PmfArchiveLoader();

        var exception = Assert.Throws<PmfException>(() => loader.Load(archivePath));

        Assert.Equal("EmptyArchive", exception.Code);
    }
}

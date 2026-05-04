using System.IO.Compression;
using PortableMarkdownFormat.Core;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class PmfArchiveWriterTests
{
    [Fact]
    public async Task CreateAsync_PackagesMarkdownFilesAndWritesManifest()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        using TemporaryDirectory source = new();
        source.WriteFile("README.md", "# Root");
        source.WriteFile(Path.Combine("guide", "intro.markdown"), "## Intro");
        source.WriteFile(Path.Combine("guide", "notes.txt"), "ignore me");

        string packagePath = Path.Combine(source.RootPath, "artifacts", "docs.pmf");
        PmfArchiveWriter writer = new();

        PmfBuildResult result = await writer.CreateAsync(source.RootPath, packagePath, cancellationToken);

        Assert.Equal(2, result.FileCount);
        Assert.True(File.Exists(packagePath));
        Assert.Equal("README.md", result.Manifest.Files[0].RelativePath);
        Assert.Equal("guide/intro.markdown", result.Manifest.Files[1].RelativePath);

        using ZipArchive archive = ZipFile.OpenRead(packagePath);
        Assert.NotNull(archive.GetEntry(PmfConstants.ManifestEntryName));
        Assert.NotNull(archive.GetEntry("content/README.md"));
        Assert.NotNull(archive.GetEntry("content/guide/intro.markdown"));
        Assert.Null(archive.GetEntry("content/guide/notes.txt"));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenSourceFolderContainsNoMarkdown()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        using TemporaryDirectory source = new();
        source.WriteFile("notes.txt", "plain text");
        string packagePath = Path.Combine(source.RootPath, "empty.pmf");

        PmfArchiveWriter writer = new();

        await Assert.ThrowsAsync<PmfPackageValidationException>(
            () => writer.CreateAsync(source.RootPath, packagePath, cancellationToken));
    }
}

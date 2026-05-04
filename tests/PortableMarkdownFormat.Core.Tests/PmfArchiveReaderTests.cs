using System.IO.Compression;
using System.Text;
using PortableMarkdownFormat.Core;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class PmfArchiveReaderTests
{
    [Fact]
    public async Task OpenAsync_ReadsMarkdownFilesFromPackage()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        using TemporaryDirectory source = new();
        source.WriteFile("README.md", "# Portable Markdown Format");
        source.WriteFile(Path.Combine("guides", "quickstart.mkd"), "1. Build\n2. View");

        string packagePath = Path.Combine(source.RootPath, "bundle.pmf");
        PmfArchiveWriter writer = new();
        await writer.CreateAsync(source.RootPath, packagePath, cancellationToken);

        PmfArchiveReader reader = new();
        PmfPackage package = await reader.OpenAsync(packagePath, cancellationToken);

        Assert.Equal(2, package.Files.Count);
        Assert.Equal("README.md", package.Files[0].RelativePath);
        Assert.Equal("# Portable Markdown Format", package.Files[0].Markdown);
        Assert.Equal("guides/quickstart.mkd", package.Files[1].RelativePath);
        Assert.Equal("1. Build\n2. View", package.Files[1].Markdown);
    }

    [Fact]
    public async Task OpenAsync_ThrowsWhenManifestIsMissing()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        using TemporaryDirectory source = new();
        string packagePath = Path.Combine(source.RootPath, "broken.pmf");

        await using (FileStream stream = new(packagePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: false))
        {
            ZipArchiveEntry entry = archive.CreateEntry("content/readme.md", CompressionLevel.Optimal);
            await using Stream entryStream = entry.Open();
            byte[] content = Encoding.UTF8.GetBytes("# Missing manifest");
            await entryStream.WriteAsync(content, cancellationToken);
        }

        PmfArchiveReader reader = new();

        await Assert.ThrowsAsync<PmfPackageValidationException>(
            () => reader.OpenAsync(packagePath, cancellationToken));
    }
}

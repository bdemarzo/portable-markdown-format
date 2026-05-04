using System.IO.Compression;
using System.Text.Json;

namespace PortableMarkdownFormat.Core;

public sealed class PmfArchiveWriter
{
    public async Task<PmfBuildResult> CreateAsync(
        string sourceDirectory,
        string packagePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

        string fullSourceDirectory = Path.GetFullPath(sourceDirectory);
        if (!Directory.Exists(fullSourceDirectory))
        {
            throw new DirectoryNotFoundException($"The source directory '{fullSourceDirectory}' does not exist.");
        }

        string fullPackagePath = Path.GetFullPath(packagePath);
        string? outputDirectory = Path.GetDirectoryName(fullPackagePath);
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("The package path must include a directory.", nameof(packagePath));
        }

        Directory.CreateDirectory(outputDirectory);

        IReadOnlyList<MarkdownSourceFile> markdownFiles = MarkdownFileDiscovery.DiscoverFiles(fullSourceDirectory);
        if (markdownFiles.Count == 0)
        {
            throw new PmfPackageValidationException("The selected source folder does not contain any Markdown files.");
        }

        List<PmfManifestEntry> manifestFiles = new(markdownFiles.Count);
        await using FileStream packageStream = new(fullPackagePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        using ZipArchive archive = new(packageStream, ZipArchiveMode.Create, leaveOpen: false);

        foreach (MarkdownSourceFile markdownFile in markdownFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] content = await File.ReadAllBytesAsync(markdownFile.SourcePath, cancellationToken);
            ZipArchiveEntry entry = archive.CreateEntry(markdownFile.ArchiveEntryName, CompressionLevel.Optimal);

            await using (Stream entryStream = entry.Open())
            {
                await entryStream.WriteAsync(content, cancellationToken);
            }

            manifestFiles.Add(new PmfManifestEntry
            {
                RelativePath = markdownFile.RelativePath,
                EntryName = markdownFile.ArchiveEntryName,
                ByteLength = content.LongLength,
                Sha256 = PmfHashUtility.ToSha256Hex(content),
            });
        }

        PmfManifest manifest = new()
        {
            Format = PmfConstants.FormatName,
            SpecificationVersion = PmfConstants.SpecificationVersion,
            CreatedUtc = DateTimeOffset.UtcNow,
            RootDirectoryName = new DirectoryInfo(fullSourceDirectory).Name,
            Files = manifestFiles,
        };

        ZipArchiveEntry manifestEntry = archive.CreateEntry(PmfConstants.ManifestEntryName, CompressionLevel.Optimal);
        await using (Stream manifestStream = manifestEntry.Open())
        {
            await JsonSerializer.SerializeAsync(manifestStream, manifest, PmfJson.Options, cancellationToken);
        }

        long totalBytes = manifestFiles.Sum(static file => file.ByteLength);
        return new PmfBuildResult(fullSourceDirectory, fullPackagePath, manifestFiles.Count, totalBytes, manifest);
    }
}

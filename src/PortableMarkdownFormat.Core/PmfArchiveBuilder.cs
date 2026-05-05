using System.IO.Compression;

namespace PortableMarkdownFormat.Core;

public sealed class PmfArchiveBuilder : IPmfArchiveBuilder
{
    public PmfBuildResult Build(string sourceDirectory, string archivePath, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        var fullSourceDirectory = Path.GetFullPath(sourceDirectory);
        var fullArchivePath = Path.GetFullPath(archivePath);

        if (!Directory.Exists(fullSourceDirectory))
        {
            throw new PmfException("SourceDirectoryNotFound", $"The source directory '{fullSourceDirectory}' does not exist.");
        }

        if (Directory.Exists(fullArchivePath))
        {
            throw new PmfException("ArchivePathIsDirectory", $"The archive path '{fullArchivePath}' points to a directory.");
        }

        if (File.Exists(fullArchivePath) && !overwrite)
        {
            throw new PmfException("ArchiveAlreadyExists", $"The archive '{fullArchivePath}' already exists. Use --overwrite to replace it.");
        }

        var files = Directory
            .EnumerateFiles(fullSourceDirectory, "*", SearchOption.AllDirectories)
            .Where(MarkdownFileRules.IsSupportedMarkdownPath)
            .Select(filePath => new
            {
                SourcePath = filePath,
                RelativePath = PmfPathNormalizer.NormalizeRelativePath(Path.GetRelativePath(fullSourceDirectory, filePath)),
            })
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
        {
            throw new PmfException(
                "NoMarkdownFiles",
                $"The source directory '{fullSourceDirectory}' does not contain supported Markdown files.");
        }

        var duplicates = files
            .GroupBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicates is not null)
        {
            throw new PmfException("DuplicateEntry", $"The source directory would create duplicate archive entry '{duplicates.Key}'.");
        }

        var archiveDirectory = Path.GetDirectoryName(fullArchivePath);
        if (!string.IsNullOrWhiteSpace(archiveDirectory))
        {
            Directory.CreateDirectory(archiveDirectory);
        }

        if (File.Exists(fullArchivePath))
        {
            File.Delete(fullArchivePath);
        }

        using (var fileStream = new FileStream(fullArchivePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.RelativePath, CompressionLevel.Optimal);

                using var sourceStream = File.OpenRead(file.SourcePath);
                using var destinationStream = entry.Open();
                sourceStream.CopyTo(destinationStream);
            }
        }

        return new PmfBuildResult(
            fullSourceDirectory,
            fullArchivePath,
            files.Select(file => new PmfArchiveEntry(file.RelativePath)).ToList());
    }
}

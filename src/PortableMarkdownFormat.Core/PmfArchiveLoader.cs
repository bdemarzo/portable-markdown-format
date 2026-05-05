using System.IO.Compression;
using System.Text;

namespace PortableMarkdownFormat.Core;

public sealed class PmfArchiveLoader : IPmfArchiveLoader
{
    public PmfArchive Load(string archivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        var fullArchivePath = Path.GetFullPath(archivePath);
        if (!File.Exists(fullArchivePath))
        {
            throw new PmfException("ArchiveNotFound", $"The archive '{fullArchivePath}' does not exist.");
        }

        var documents = new List<PmfArchiveDocument>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var stream = File.OpenRead(fullArchivePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

        foreach (var entry in archive.Entries)
        {
            if (IsDirectoryEntry(entry))
            {
                continue;
            }

            var relativePath = PmfPathNormalizer.NormalizeRelativePath(entry.FullName);

            if (!MarkdownFileRules.IsSupportedMarkdownPath(relativePath))
            {
                throw new PmfException(
                    "NonMarkdownEntry",
                    $"The archive contains unsupported entry '{relativePath}'. PMF v1 only allows Markdown files.");
            }

            if (!seenPaths.Add(relativePath))
            {
                throw new PmfException("DuplicateEntry", $"The archive contains duplicate path '{relativePath}'.");
            }

            using var reader = new StreamReader(entry.Open(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            documents.Add(new PmfArchiveDocument(relativePath, reader.ReadToEnd()));
        }

        if (documents.Count == 0)
        {
            throw new PmfException("EmptyArchive", $"The archive '{fullArchivePath}' does not contain any Markdown files.");
        }

        documents.Sort(static (left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.RelativePath, right.RelativePath));

        return new PmfArchive(fullArchivePath, documents);
    }

    private static bool IsDirectoryEntry(ZipArchiveEntry entry) =>
        string.IsNullOrEmpty(entry.Name) && entry.FullName.EndsWith("/", StringComparison.Ordinal);
}

namespace PortableMarkdownFormat.Core;

public sealed class PmfBuildResult(
    string sourceDirectory,
    string archivePath,
    IReadOnlyList<PmfArchiveEntry> entries)
{
    public string SourceDirectory { get; } = sourceDirectory;

    public string ArchivePath { get; } = archivePath;

    public IReadOnlyList<PmfArchiveEntry> Entries { get; } = entries;

    public int EntryCount => Entries.Count;
}

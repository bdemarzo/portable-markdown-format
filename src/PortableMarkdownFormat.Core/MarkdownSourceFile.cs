namespace PortableMarkdownFormat.Core;

public sealed record MarkdownSourceFile(
    string SourcePath,
    string RelativePath,
    string ArchiveEntryName);

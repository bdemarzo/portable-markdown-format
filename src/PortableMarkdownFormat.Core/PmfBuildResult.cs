namespace PortableMarkdownFormat.Core;

public sealed record PmfBuildResult(
    string SourceDirectory,
    string PackagePath,
    int FileCount,
    long TotalBytes,
    PmfManifest Manifest);

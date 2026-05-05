namespace PortableMarkdownFormat.Core;

public sealed record PmfArchiveNode(
    string Name,
    string RelativePath,
    bool IsDirectory,
    IReadOnlyList<PmfArchiveNode> Children);

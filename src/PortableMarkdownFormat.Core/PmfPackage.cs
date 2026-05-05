namespace PortableMarkdownFormat.Core;

public sealed record PmfPackage(
    string PackagePath,
    PmfManifest Manifest,
    IReadOnlyList<PmfDocumentFile> Files);

namespace PortableMarkdownFormat.Core;

public sealed class PmfManifest
{
    public required string Format { get; init; }

    public required string SpecificationVersion { get; init; }

    public required DateTimeOffset CreatedUtc { get; init; }

    public required string RootDirectoryName { get; init; }

    public required IReadOnlyList<PmfManifestEntry> Files { get; init; }
}

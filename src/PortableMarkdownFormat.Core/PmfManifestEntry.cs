namespace PortableMarkdownFormat.Core;

public sealed class PmfManifestEntry
{
    public required string RelativePath { get; init; }

    public required string EntryName { get; init; }

    public required long ByteLength { get; init; }

    public required string Sha256 { get; init; }
}

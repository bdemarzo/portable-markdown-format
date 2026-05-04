namespace PortableMarkdownFormat.Core;

public sealed record PmfDocumentFile(
    string RelativePath,
    string EntryName,
    long ByteLength,
    string Sha256,
    string Markdown);

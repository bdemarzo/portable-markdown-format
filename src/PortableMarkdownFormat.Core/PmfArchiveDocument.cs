namespace PortableMarkdownFormat.Core;

public sealed record PmfArchiveDocument(string RelativePath, string Markdown)
{
    public string Name => RelativePath.Split('/')[^1];
}

namespace PortableMarkdownFormat.Core;

public sealed record PmfArchiveEntry(string RelativePath)
{
    public string Name => RelativePath.Split('/')[^1];
}

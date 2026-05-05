namespace PortableMarkdownFormat.Core;

public sealed class PmfException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

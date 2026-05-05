namespace PortableMarkdownFormat.Core;

public static class MarkdownFileRules
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md",
        ".markdown",
        ".mdown",
        ".mkd",
        ".mkdn",
    };

    public static IReadOnlyCollection<string> Extensions => SupportedExtensions;

    public static bool IsSupportedMarkdownPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return SupportedExtensions.Contains(Path.GetExtension(path));
    }
}

namespace PortableMarkdownFormat.Core;

public static class PmfConstants
{
    public const string FormatName = "PMF";
    public const string SpecificationVersion = "1.0";
    public const string ManifestEntryName = "pmf-manifest.json";
    public const string ContentRoot = "content";
    public const string PackageExtension = ".pmf";

    public static ISet<string> MarkdownExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".md",
        ".markdown",
        ".mdown",
        ".mkd",
    };
}

namespace PortableMarkdownFormat.Core;

public static class MarkdownFileDiscovery
{
    public static IReadOnlyList<MarkdownSourceFile> DiscoverFiles(string sourceDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);

        string fullSourceDirectory = Path.GetFullPath(sourceDirectory);
        if (!Directory.Exists(fullSourceDirectory))
        {
            throw new DirectoryNotFoundException($"The source directory '{fullSourceDirectory}' does not exist.");
        }

        return Directory
            .EnumerateFiles(fullSourceDirectory, "*", SearchOption.AllDirectories)
            .Where(static path => PmfConstants.MarkdownExtensions.Contains(Path.GetExtension(path)))
            .Select(path =>
            {
                string relativePath = PmfPathUtility.NormalizeRelativePath(Path.GetRelativePath(fullSourceDirectory, path));
                return new MarkdownSourceFile(path, relativePath, PmfPathUtility.GetArchiveEntryName(relativePath));
            })
            .OrderBy(static file => file.RelativePath, StringComparer.Ordinal)
            .ToArray();
    }
}

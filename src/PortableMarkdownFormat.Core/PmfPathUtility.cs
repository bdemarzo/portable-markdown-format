namespace PortableMarkdownFormat.Core;

internal static class PmfPathUtility
{
    public static string NormalizeRelativePath(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        string normalized = relativePath
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/')
            .Trim('/');

        if (normalized.Length == 0)
        {
            throw new PmfPackageValidationException("PMF paths cannot be empty.");
        }

        foreach (string segment in normalized.Split('/', StringSplitOptions.None))
        {
            if (string.IsNullOrWhiteSpace(segment) || segment is "." or "..")
            {
                throw new PmfPackageValidationException($"PMF path '{relativePath}' contains an invalid segment.");
            }
        }

        return normalized;
    }

    public static string GetArchiveEntryName(string relativePath)
        => $"{PmfConstants.ContentRoot}/{NormalizeRelativePath(relativePath)}";
}

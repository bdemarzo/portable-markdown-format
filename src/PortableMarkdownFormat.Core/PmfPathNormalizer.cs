namespace PortableMarkdownFormat.Core;

public static class PmfPathNormalizer
{
    public const string ReservedTopLevelSegment = "_pmf";

    public static string NormalizeRelativePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var normalized = path.Replace('\\', '/').Trim();

        if (normalized.StartsWith("/", StringComparison.Ordinal))
        {
            throw new PmfException("RootedPath", $"The path '{path}' cannot start with '/'.");
        }

        if (normalized.EndsWith("/", StringComparison.Ordinal))
        {
            throw new PmfException("DirectoryEntry", $"The path '{path}' must reference a file, not a directory.");
        }

        if (normalized.IndexOf(':') >= 0)
        {
            throw new PmfException("InvalidPath", $"The path '{path}' cannot contain ':'.");
        }

        var segments = normalized.Split('/');
        if (segments.Length == 0)
        {
            throw new PmfException("EmptyPath", "Archive paths cannot be empty.");
        }

        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];

            if (string.IsNullOrWhiteSpace(segment))
            {
                throw new PmfException("EmptySegment", $"The path '{path}' contains an empty segment.");
            }

            if (segment.Equals(".", StringComparison.Ordinal) || segment.Equals("..", StringComparison.Ordinal))
            {
                throw new PmfException("RelativeSegment", $"The path '{path}' cannot contain '.' or '..' segments.");
            }

            if (index == 0 && segment.Equals(ReservedTopLevelSegment, StringComparison.OrdinalIgnoreCase))
            {
                throw new PmfException(
                    "ReservedSegment",
                    $"The top-level path segment '{ReservedTopLevelSegment}' is reserved for future PMF metadata.");
            }
        }

        return string.Join('/', segments);
    }
}

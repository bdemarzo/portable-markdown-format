namespace PortableMarkdownFormat.Core;

public static class PmfHierarchyBuilder
{
    public static IReadOnlyList<PmfArchiveNode> Build(IEnumerable<PmfArchiveDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var root = new NodeBuilder(string.Empty, string.Empty, isDirectory: true);

        foreach (var document in documents.OrderBy(document => document.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var segments = document.RelativePath.Split('/');
            var current = root;

            for (var index = 0; index < segments.Length - 1; index++)
            {
                var partialPath = string.Join('/', segments.Take(index + 1));
                current = current.GetOrAddDirectory(segments[index], partialPath);
            }

            current.AddFile(segments[^1], document.RelativePath);
        }

        return root.ToChildren();
    }

    private sealed class NodeBuilder(string name, string relativePath, bool isDirectory)
    {
        private readonly Dictionary<string, NodeBuilder> _children = new(StringComparer.OrdinalIgnoreCase);

        public string Name { get; } = name;

        public string RelativePath { get; } = relativePath;

        public bool IsDirectory { get; } = isDirectory;

        public NodeBuilder GetOrAddDirectory(string name, string relativePath)
        {
            if (_children.TryGetValue(name, out var existing))
            {
                if (!existing.IsDirectory)
                {
                    throw new PmfException("PathConflict", $"The path '{relativePath}' conflicts with an existing file.");
                }

                return existing;
            }

            var created = new NodeBuilder(name, relativePath, isDirectory: true);
            _children.Add(name, created);
            return created;
        }

        public void AddFile(string name, string relativePath)
        {
            if (_children.ContainsKey(name))
            {
                throw new PmfException("DuplicateEntry", $"The archive contains duplicate paths for '{relativePath}'.");
            }

            _children.Add(name, new NodeBuilder(name, relativePath, isDirectory: false));
        }

        public IReadOnlyList<PmfArchiveNode> ToChildren() =>
            _children.Values
                .OrderByDescending(child => child.IsDirectory)
                .ThenBy(child => child.Name, StringComparer.OrdinalIgnoreCase)
                .Select(child => child.ToNode())
                .ToList();

        private PmfArchiveNode ToNode() =>
            new(Name, RelativePath, IsDirectory, ToChildren());
    }
}

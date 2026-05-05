namespace PortableMarkdownFormat.Core;

public sealed class PmfArchive
{
    private readonly IReadOnlyDictionary<string, PmfArchiveDocument> _documentsByPath;

    public PmfArchive(string archivePath, IReadOnlyList<PmfArchiveDocument> documents)
    {
        ArchivePath = archivePath;
        Documents = documents;
        Hierarchy = PmfHierarchyBuilder.Build(documents);
        _documentsByPath = documents.ToDictionary(
            document => document.RelativePath,
            StringComparer.OrdinalIgnoreCase);
    }

    public string ArchivePath { get; }

    public IReadOnlyList<PmfArchiveDocument> Documents { get; }

    public IReadOnlyList<PmfArchiveNode> Hierarchy { get; }

    public PmfArchiveDocument GetDocument(string relativePath)
    {
        var normalizedPath = PmfPathNormalizer.NormalizeRelativePath(relativePath);

        if (_documentsByPath.TryGetValue(normalizedPath, out var document))
        {
            return document;
        }

        throw new PmfException("DocumentNotFound", $"The document '{relativePath}' does not exist in the archive.");
    }
}

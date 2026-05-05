namespace PortableMarkdownFormat.Core;

public interface IPmfArchiveLoader
{
    PmfArchive Load(string archivePath);
}

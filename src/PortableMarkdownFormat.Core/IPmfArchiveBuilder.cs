namespace PortableMarkdownFormat.Core;

public interface IPmfArchiveBuilder
{
    PmfBuildResult Build(string sourceDirectory, string archivePath, bool overwrite = false);
}

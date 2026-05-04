namespace PortableMarkdownFormat.Viewer.ViewModels;

public sealed class PmfDocumentItemViewModel
{
    public PmfDocumentItemViewModel(string relativePath, string markdown, long byteLength)
    {
        RelativePath = relativePath;
        Markdown = markdown;
        Subtitle = byteLength == 1 ? "1 byte" : $"{byteLength:N0} bytes";
    }

    public string RelativePath { get; }

    public string Markdown { get; }

    public string Subtitle { get; }
}

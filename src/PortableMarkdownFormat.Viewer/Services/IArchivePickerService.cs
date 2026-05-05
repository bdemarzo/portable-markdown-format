namespace PortableMarkdownFormat.Viewer.Services;

public interface IArchivePickerService
{
    Task<string?> OpenArchiveAsync();
}

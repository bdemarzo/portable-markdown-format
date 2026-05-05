namespace PortableMarkdownFormat.Viewer.Services;

public sealed class NullArchivePickerService : IArchivePickerService
{
    public Task<string?> OpenArchiveAsync() => Task.FromResult<string?>(null);
}

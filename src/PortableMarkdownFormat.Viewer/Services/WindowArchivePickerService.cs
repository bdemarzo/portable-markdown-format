using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace PortableMarkdownFormat.Viewer.Services;

[ExcludeFromCodeCoverage]
public sealed class WindowArchivePickerService(Window window) : IArchivePickerService
{
    private readonly Window _window = window;

    public async Task<string?> OpenArchiveAsync()
    {
        var files = await _window.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open PMF archive",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Portable Markdown Format")
                    {
                        Patterns = ["*.pmf"],
                    },
                ],
            });

        return files.Count == 0 ? null : files[0].TryGetLocalPath();
    }
}

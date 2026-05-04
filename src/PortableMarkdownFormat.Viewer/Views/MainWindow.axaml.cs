using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PortableMarkdownFormat.Viewer.ViewModels;

namespace PortableMarkdownFormat.Viewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OpenPackageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Choose a PMF package",
            FileTypeFilter =
            [
                new FilePickerFileType("Portable Markdown Format")
                {
                    Patterns = ["*.pmf"],
                },
            ],
        });

        IStorageFile? file = files.FirstOrDefault();
        if (file?.Path is not null && DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.OpenPackageAsync(file.Path.LocalPath);
        }
    }
}

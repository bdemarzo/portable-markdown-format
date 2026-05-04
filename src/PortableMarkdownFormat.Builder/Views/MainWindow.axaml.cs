using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PortableMarkdownFormat.Core;
using PortableMarkdownFormat.Builder.ViewModels;

namespace PortableMarkdownFormat.Builder.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowseSourceFolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Choose the Markdown source folder",
        });

        IStorageFolder? folder = folders.FirstOrDefault();
        if (folder?.Path is null || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        string localPath = folder.Path.LocalPath;
        viewModel.SourceFolderPath = localPath;

        if (string.IsNullOrWhiteSpace(viewModel.OutputFilePath))
        {
            string suggestedFileName = $"{Path.GetFileName(localPath)}{PmfConstants.PackageExtension}";
            viewModel.OutputFilePath = Path.Combine(localPath, suggestedFileName);
        }

        await viewModel.RefreshFilesAsync();
    }

    private async void BrowseOutputFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Choose where to save the PMF package",
            DefaultExtension = "pmf",
            SuggestedFileName = GetSuggestedFileName(viewModel),
            FileTypeChoices =
            [
                new FilePickerFileType("Portable Markdown Format")
                {
                    Patterns = ["*.pmf"],
                },
            ],
        });

        if (file?.Path is not null)
        {
            viewModel.OutputFilePath = file.Path.LocalPath;
        }
    }

    private async void BuildPackageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.BuildPackageAsync();
        }
    }

    private static string GetSuggestedFileName(MainWindowViewModel viewModel)
    {
        if (!string.IsNullOrWhiteSpace(viewModel.OutputFilePath))
        {
            return Path.GetFileName(viewModel.OutputFilePath);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.SourceFolderPath))
        {
            return $"{Path.GetFileName(viewModel.SourceFolderPath)}{PmfConstants.PackageExtension}";
        }

        return $"portable-markdown-format{PmfConstants.PackageExtension}";
    }
}

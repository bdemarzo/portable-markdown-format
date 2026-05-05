using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PortableMarkdownFormat.Core;

namespace PortableMarkdownFormat.Viewer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const string EmptyPreviewMarkdown =
        "# Portable Markdown Format\n\nOpen a `.pmf` archive to preview packaged Markdown files.";

    private readonly PmfArchiveReader _archiveReader = new();

    public MainWindowViewModel()
    {
        PackageSummary = "No PMF package loaded.";
        StatusMessage = "Choose a .pmf file to inspect.";
        SelectedMarkdown = EmptyPreviewMarkdown;
    }

    public ObservableCollection<PmfDocumentItemViewModel> Files { get; } = new();

    [ObservableProperty]
    private string packagePath = string.Empty;

    [ObservableProperty]
    private string packageSummary = string.Empty;

    [ObservableProperty]
    private string selectedMarkdown = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private PmfDocumentItemViewModel? selectedFile;

    [ObservableProperty]
    private bool isBusy;

    public string SelectedPathHeader => SelectedFile?.RelativePath ?? "Markdown preview";

    public async Task OpenPackageAsync(string path)
    {
        try
        {
            IsBusy = true;

            PmfPackage package = await _archiveReader.OpenAsync(path);

            Files.Clear();
            foreach (PmfDocumentFile document in package.Files)
            {
                Files.Add(new PmfDocumentItemViewModel(document.RelativePath, document.Markdown, document.ByteLength));
            }

            PackagePath = package.PackagePath;
            PackageSummary = package.Files.Count switch
            {
                1 => $"1 Markdown file from {package.Manifest.RootDirectoryName}",
                _ => $"{package.Files.Count} Markdown files from {package.Manifest.RootDirectoryName}",
            };

            SelectedFile = Files.FirstOrDefault();
            StatusMessage = $"Loaded {Path.GetFileName(package.PackagePath)}.";
        }
        catch (ArgumentException exception)
        {
            ResetPackageState();
            StatusMessage = exception.Message;
        }
        catch (FileNotFoundException exception)
        {
            ResetPackageState();
            StatusMessage = exception.Message;
        }
        catch (InvalidDataException exception)
        {
            ResetPackageState();
            StatusMessage = exception.Message;
        }
        catch (UnauthorizedAccessException exception)
        {
            ResetPackageState();
            StatusMessage = exception.Message;
        }
        catch (IOException exception)
        {
            ResetPackageState();
            StatusMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedFileChanged(PmfDocumentItemViewModel? value)
    {
        SelectedMarkdown = value?.Markdown ?? EmptyPreviewMarkdown;
        OnPropertyChanged(nameof(SelectedPathHeader));
    }

    private void ResetPackageState()
    {
        Files.Clear();
        PackagePath = string.Empty;
        PackageSummary = "No PMF package loaded.";
        SelectedFile = null;
        SelectedMarkdown = EmptyPreviewMarkdown;
    }
}

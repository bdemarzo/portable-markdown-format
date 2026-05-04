using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PortableMarkdownFormat.Core;

namespace PortableMarkdownFormat.Builder.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly PmfArchiveWriter _archiveWriter = new();

    public MainWindowViewModel()
    {
        Summary = "Choose a source folder to discover Markdown files.";
        StatusMessage = "Ready to build a PMF package.";
    }

    public ObservableCollection<string> Files { get; } = new();

    [ObservableProperty]
    private string sourceFolderPath = string.Empty;

    [ObservableProperty]
    private string outputFilePath = string.Empty;

    [ObservableProperty]
    private string summary = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public bool CanBuild =>
        !IsBusy &&
        Files.Count > 0 &&
        !string.IsNullOrWhiteSpace(SourceFolderPath) &&
        !string.IsNullOrWhiteSpace(OutputFilePath);

    public async Task RefreshFilesAsync()
    {
        if (string.IsNullOrWhiteSpace(SourceFolderPath))
        {
            Files.Clear();
            Summary = "Choose a source folder to discover Markdown files.";
            StatusMessage = "Select a source folder.";
            OnPropertyChanged(nameof(CanBuild));
            return;
        }

        try
        {
            IsBusy = true;

            IReadOnlyList<MarkdownSourceFile> discoveredFiles =
                await Task.Run(() => MarkdownFileDiscovery.DiscoverFiles(SourceFolderPath));

            Files.Clear();
            foreach (MarkdownSourceFile discoveredFile in discoveredFiles)
            {
                Files.Add(discoveredFile.RelativePath);
            }

            Summary = Files.Count switch
            {
                0 => "No Markdown files were found in the selected folder.",
                1 => "1 Markdown file is ready to package.",
                _ => $"{Files.Count} Markdown files are ready to package.",
            };

            StatusMessage = Files.Count == 0
                ? "The selected folder does not contain supported Markdown files."
                : "Source folder scanned successfully.";
        }
        catch (DirectoryNotFoundException exception)
        {
            Files.Clear();
            Summary = "Choose a source folder to discover Markdown files.";
            StatusMessage = exception.Message;
        }
        catch (UnauthorizedAccessException exception)
        {
            Files.Clear();
            Summary = "Choose a source folder to discover Markdown files.";
            StatusMessage = exception.Message;
        }
        catch (IOException exception)
        {
            Files.Clear();
            Summary = "Choose a source folder to discover Markdown files.";
            StatusMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(CanBuild));
        }
    }

    public async Task BuildPackageAsync()
    {
        try
        {
            IsBusy = true;

            PmfBuildResult result = await _archiveWriter.CreateAsync(SourceFolderPath, OutputFilePath);
            Summary = result.FileCount switch
            {
                1 => "1 Markdown file was packaged successfully.",
                _ => $"{result.FileCount} Markdown files were packaged successfully.",
            };
            StatusMessage = $"Created {Path.GetFileName(result.PackagePath)}.";
        }
        catch (ArgumentException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (DirectoryNotFoundException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (PmfPackageValidationException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (UnauthorizedAccessException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (IOException exception)
        {
            StatusMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(CanBuild));
        }
    }

    partial void OnSourceFolderPathChanged(string value)
        => OnPropertyChanged(nameof(CanBuild));

    partial void OnOutputFilePathChanged(string value)
        => OnPropertyChanged(nameof(CanBuild));

    partial void OnIsBusyChanged(bool value)
        => OnPropertyChanged(nameof(CanBuild));
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PortableMarkdownFormat.Core;
using PortableMarkdownFormat.Viewer.Services;

namespace PortableMarkdownFormat.Viewer.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private const string WelcomeMarkdown =
        """
        # Portable Markdown Format Viewer

        Open a `.pmf` archive to browse the packaged Markdown files.
        """;

    private readonly IPmfArchiveLoader _archiveLoader;
    private readonly IArchivePickerService _archivePickerService;
    private PmfArchive? _archive;
    private ArchiveTreeNodeViewModel? _selectedNode;
    private string _selectedMarkdown = WelcomeMarkdown;
    private string _statusMessage = "Open a PMF archive to begin.";
    private string _currentArchiveLabel = "No archive loaded";

    public MainWindowViewModel()
        : this(new PmfArchiveLoader(), new NullArchivePickerService())
    {
    }

    public MainWindowViewModel(IPmfArchiveLoader archiveLoader, IArchivePickerService archivePickerService)
    {
        _archiveLoader = archiveLoader;
        _archivePickerService = archivePickerService;
        OpenArchiveCommand = new AsyncRelayCommand(OpenArchiveAsync);
    }

    public ObservableCollection<ArchiveTreeNodeViewModel> RootNodes { get; } = [];

    public IAsyncRelayCommand OpenArchiveCommand { get; }

    public ArchiveTreeNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (!SetProperty(ref _selectedNode, value) || value?.RelativePath is null || _archive is null)
            {
                return;
            }

            SelectedMarkdown = _archive.GetDocument(value.RelativePath).Markdown;
            StatusMessage = $"Viewing {value.RelativePath}";
        }
    }

    public string SelectedMarkdown
    {
        get => _selectedMarkdown;
        private set => SetProperty(ref _selectedMarkdown, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string CurrentArchiveLabel
    {
        get => _currentArchiveLabel;
        private set => SetProperty(ref _currentArchiveLabel, value);
    }

    public async Task OpenArchiveAsync()
    {
        var archivePath = await _archivePickerService.OpenArchiveAsync();
        if (!string.IsNullOrWhiteSpace(archivePath))
        {
            TryLoadArchive(archivePath);
        }
    }

    public bool TryLoadArchive(string archivePath)
    {
        try
        {
            LoadArchive(archivePath);
            return true;
        }
        catch (PmfException exception)
        {
            ShowError("Unable to open archive", exception.Message);
            return false;
        }
        catch (IOException exception)
        {
            ShowError("Unable to read archive", exception.Message);
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            ShowError("Access denied", exception.Message);
            return false;
        }
        catch (InvalidDataException exception)
        {
            ShowError("Invalid archive", exception.Message);
            return false;
        }
    }

    public void LoadArchive(string archivePath)
    {
        var archive = _archiveLoader.Load(archivePath);
        _archive = archive;

        RootNodes.Clear();
        foreach (var node in archive.Hierarchy.Select(MapNode))
        {
            RootNodes.Add(node);
        }

        CurrentArchiveLabel = archive.ArchivePath;

        var firstDocument = FindFirstDocument(RootNodes);
        if (firstDocument is null)
        {
            SelectedNode = null;
            SelectedMarkdown = WelcomeMarkdown;
            StatusMessage = $"Loaded {archive.Documents.Count} Markdown file(s).";
            return;
        }

        SelectedNode = firstDocument;
    }

    private void ShowError(string title, string message)
    {
        _archive = null;
        RootNodes.Clear();
        SelectedNode = null;
        CurrentArchiveLabel = "No archive loaded";
        SelectedMarkdown = $"# {title}\n\n{message}";
        StatusMessage = message;
    }

    private static ArchiveTreeNodeViewModel MapNode(PmfArchiveNode node) =>
        new(
            node.Name,
            node.IsDirectory,
            node.IsDirectory ? null : node.RelativePath,
            node.Children.Select(MapNode).ToList());

    private static ArchiveTreeNodeViewModel? FindFirstDocument(IEnumerable<ArchiveTreeNodeViewModel> nodes)
    {
        foreach (var node in nodes)
        {
            if (!node.IsDirectory)
            {
                return node;
            }

            var descendant = FindFirstDocument(node.Children);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}

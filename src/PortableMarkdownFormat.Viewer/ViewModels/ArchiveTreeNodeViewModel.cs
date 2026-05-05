using System.Collections.ObjectModel;

namespace PortableMarkdownFormat.Viewer.ViewModels;

public sealed class ArchiveTreeNodeViewModel : ViewModelBase
{
    public ArchiveTreeNodeViewModel(
        string displayName,
        bool isDirectory,
        string? relativePath,
        IReadOnlyList<ArchiveTreeNodeViewModel> children)
    {
        DisplayName = displayName;
        IsDirectory = isDirectory;
        RelativePath = relativePath;
        Children = new ObservableCollection<ArchiveTreeNodeViewModel>(children);
    }

    public string DisplayName { get; }

    public bool IsDirectory { get; }

    public string? RelativePath { get; }

    public ObservableCollection<ArchiveTreeNodeViewModel> Children { get; }
}

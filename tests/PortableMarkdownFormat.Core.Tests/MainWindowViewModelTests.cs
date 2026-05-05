using PortableMarkdownFormat.Core;
using PortableMarkdownFormat.Core.Tests.TestSupport;
using PortableMarkdownFormat.Viewer.Services;
using PortableMarkdownFormat.Viewer.ViewModels;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void LoadArchive_PopulatesTreeAndSelectsFirstDocument()
    {
        using var temp = new TemporaryDirectory();
        temp.CreateFile("README.md", "# Root");
        temp.CreateFile(System.IO.Path.Combine("docs", "guide.md"), "## Guide");

        var archivePath = System.IO.Path.Combine(temp.Path, "sample.pmf");
        new PmfArchiveBuilder().Build(temp.Path, archivePath);

        var viewModel = new MainWindowViewModel(new PmfArchiveLoader(), new NullArchivePickerService());

        viewModel.LoadArchive(archivePath);

        Assert.Equal(2, viewModel.RootNodes.Count);
        Assert.NotNull(viewModel.SelectedNode);
        Assert.Contains("Guide", viewModel.SelectedMarkdown);
        Assert.Equal(archivePath, viewModel.CurrentArchiveLabel);
    }

    [Fact]
    public async Task OpenArchiveAsync_UsesPickerService()
    {
        using var temp = new TemporaryDirectory();
        temp.CreateFile("README.md", "# Root");

        var archivePath = System.IO.Path.Combine(temp.Path, "sample.pmf");
        new PmfArchiveBuilder().Build(temp.Path, archivePath);

        var viewModel = new MainWindowViewModel(new PmfArchiveLoader(), new FakeArchivePickerService(archivePath));

        await viewModel.OpenArchiveAsync();

        Assert.Equal(archivePath, viewModel.CurrentArchiveLabel);
        Assert.Contains("Root", viewModel.SelectedMarkdown);
    }

    [Fact]
    public void TryLoadArchive_ShowsErrorForInvalidArchive()
    {
        using var temp = new TemporaryDirectory();
        var archivePath = System.IO.Path.Combine(temp.Path, "invalid.pmf");
        File.WriteAllText(archivePath, "not a zip");

        var viewModel = new MainWindowViewModel(new PmfArchiveLoader(), new NullArchivePickerService());

        var wasLoaded = viewModel.TryLoadArchive(archivePath);

        Assert.False(wasLoaded);
        Assert.Equal("No archive loaded", viewModel.CurrentArchiveLabel);
        Assert.Contains("Invalid archive", viewModel.SelectedMarkdown);
    }

    [Fact]
    public async Task OpenArchiveAsync_WithDefaultConstructorLeavesWelcomeStateWhenNoFileIsPicked()
    {
        var viewModel = new MainWindowViewModel();

        await viewModel.OpenArchiveAsync();

        Assert.Equal("No archive loaded", viewModel.CurrentArchiveLabel);
        Assert.Contains("Portable Markdown Format Viewer", viewModel.SelectedMarkdown);
    }

    [Fact]
    public void TryLoadArchive_ShowsOpenErrorForPmfExceptions()
    {
        var viewModel = new MainWindowViewModel(
            new FakeArchiveLoader(_ => throw new PmfException("ArchiveNotFound", "missing")),
            new NullArchivePickerService());

        var wasLoaded = viewModel.TryLoadArchive("missing.pmf");

        Assert.False(wasLoaded);
        Assert.Contains("Unable to open archive", viewModel.SelectedMarkdown);
    }

    [Fact]
    public void TryLoadArchive_ShowsReadErrorForIoExceptions()
    {
        var viewModel = new MainWindowViewModel(
            new FakeArchiveLoader(_ => throw new IOException("disk failure")),
            new NullArchivePickerService());

        var wasLoaded = viewModel.TryLoadArchive("broken.pmf");

        Assert.False(wasLoaded);
        Assert.Contains("Unable to read archive", viewModel.SelectedMarkdown);
    }

    [Fact]
    public void TryLoadArchive_ShowsAccessDeniedForUnauthorizedExceptions()
    {
        var viewModel = new MainWindowViewModel(
            new FakeArchiveLoader(_ => throw new UnauthorizedAccessException("denied")),
            new NullArchivePickerService());

        var wasLoaded = viewModel.TryLoadArchive("secret.pmf");

        Assert.False(wasLoaded);
        Assert.Contains("Access denied", viewModel.SelectedMarkdown);
    }

    [Fact]
    public void LoadArchive_KeepsWelcomeMarkdownWhenArchiveContainsNoDocuments()
    {
        var emptyArchive = new PmfArchive("empty.pmf", []);
        var viewModel = new MainWindowViewModel(
            new FakeArchiveLoader(_ => emptyArchive),
            new NullArchivePickerService());

        viewModel.LoadArchive("empty.pmf");

        Assert.Null(viewModel.SelectedNode);
        Assert.Contains("Portable Markdown Format Viewer", viewModel.SelectedMarkdown);
        Assert.Contains("Loaded 0 Markdown file(s).", viewModel.StatusMessage);
    }

    [Fact]
    public void SelectingDirectoryNodeDoesNotReplaceCurrentDocument()
    {
        var archive = new PmfArchive(
            "sample.pmf",
            [
                new PmfArchiveDocument("docs/guide.md", "## Guide"),
                new PmfArchiveDocument("README.md", "# Home"),
            ]);

        var viewModel = new MainWindowViewModel(
            new FakeArchiveLoader(_ => archive),
            new NullArchivePickerService());

        viewModel.LoadArchive("sample.pmf");
        var currentMarkdown = viewModel.SelectedMarkdown;

        viewModel.SelectedNode = viewModel.RootNodes[0];

        Assert.Equal(currentMarkdown, viewModel.SelectedMarkdown);
    }

    private sealed class FakeArchivePickerService(string archivePath) : IArchivePickerService
    {
        public Task<string?> OpenArchiveAsync() => Task.FromResult<string?>(archivePath);
    }

    private sealed class FakeArchiveLoader(Func<string, PmfArchive> load) : IPmfArchiveLoader
    {
        private readonly Func<string, PmfArchive> _load = load;

        public PmfArchive Load(string archivePath) => _load(archivePath);
    }
}

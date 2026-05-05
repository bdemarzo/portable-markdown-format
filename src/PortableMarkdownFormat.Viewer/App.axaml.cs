using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PortableMarkdownFormat.Core;
using PortableMarkdownFormat.Viewer.Services;
using PortableMarkdownFormat.Viewer.ViewModels;
using PortableMarkdownFormat.Viewer.Views;

namespace PortableMarkdownFormat.Viewer;

[ExcludeFromCodeCoverage]
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            var viewModel = new MainWindowViewModel(
                new PmfArchiveLoader(),
                new WindowArchivePickerService(mainWindow));

            mainWindow.DataContext = viewModel;
            desktop.MainWindow = mainWindow;

            if (desktop.Args is { Length: > 0 })
            {
                viewModel.TryLoadArchive(desktop.Args[0]);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}

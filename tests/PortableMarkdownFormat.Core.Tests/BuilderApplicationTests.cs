using PortableMarkdownFormat.Builder;
using PortableMarkdownFormat.Core;
using PortableMarkdownFormat.Core.Tests.TestSupport;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class BuilderApplicationTests
{
    [Fact]
    public void Run_CreatesArchiveAndPrintsSummary()
    {
        using var temp = new TemporaryDirectory();
        temp.CreateFile("README.md", "# Root");
        var archivePath = System.IO.Path.Combine(temp.Path, "output.pmf");

        var output = new StringWriter();
        var error = new StringWriter();
        var application = new BuilderApplication(new PmfArchiveBuilder());

        var exitCode = application.Run(["build", "--source", temp.Path, "--output", archivePath], output, error);

        Assert.Equal(BuilderApplication.SuccessExitCode, exitCode);
        Assert.Contains("Created PMF archive:", output.ToString());
        Assert.True(string.IsNullOrWhiteSpace(error.ToString()));
    }

    [Fact]
    public void Run_ReturnsUsageErrorForUnknownCommand()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var application = new BuilderApplication(new PmfArchiveBuilder());

        var exitCode = application.Run(["unknown"], output, error);

        Assert.Equal(BuilderApplication.UsageErrorExitCode, exitCode);
        Assert.Contains("Unknown command", error.ToString());
    }

    [Fact]
    public void Run_ReturnsBuildErrorWhenPackagingFails()
    {
        using var temp = new TemporaryDirectory();
        var archivePath = System.IO.Path.Combine(temp.Path, "output.pmf");

        var output = new StringWriter();
        var error = new StringWriter();
        var application = new BuilderApplication(new PmfArchiveBuilder());

        var exitCode = application.Run(["build", "--source", temp.Path, "--output", archivePath], output, error);

        Assert.Equal(BuilderApplication.BuildErrorExitCode, exitCode);
        Assert.Contains("NoMarkdownFiles", error.ToString());
    }

    [Fact]
    public void Run_ReturnsSuccessAndUsageForHelp()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var application = new BuilderApplication(new FakeArchiveBuilder(_ => throw new InvalidOperationException()));

        var exitCode = application.Run([], output, error);

        Assert.Equal(BuilderApplication.SuccessExitCode, exitCode);
        Assert.Contains("Usage:", output.ToString());
        Assert.True(string.IsNullOrWhiteSpace(error.ToString()));
    }

    [Theory]
    [InlineData("io")]
    [InlineData("access")]
    [InlineData("invalid")]
    public void Run_ReturnsBuildErrorForInfrastructureExceptions(string mode)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var application = new BuilderApplication(new FakeArchiveBuilder(mode switch
        {
            "io" => _ => throw new IOException("disk failure"),
            "access" => _ => throw new UnauthorizedAccessException("denied"),
            _ => _ => throw new InvalidDataException("bad zip"),
        }));

        var exitCode = application.Run(["build", "--source", "docs", "--output", "docs.pmf"], output, error);

        Assert.Equal(BuilderApplication.BuildErrorExitCode, exitCode);
        Assert.NotEmpty(error.ToString());
    }

    private sealed class FakeArchiveBuilder(Func<(string SourceDirectory, string OutputPath, bool Overwrite), PmfBuildResult> build)
        : IPmfArchiveBuilder
    {
        private readonly Func<(string SourceDirectory, string OutputPath, bool Overwrite), PmfBuildResult> _build = build;

        public PmfBuildResult Build(string sourceDirectory, string archivePath, bool overwrite = false) =>
            _build((sourceDirectory, archivePath, overwrite));
    }
}

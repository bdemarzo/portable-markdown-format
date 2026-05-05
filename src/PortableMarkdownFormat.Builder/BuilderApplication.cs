using PortableMarkdownFormat.Core;

namespace PortableMarkdownFormat.Builder;

public sealed class BuilderApplication(IPmfArchiveBuilder archiveBuilder)
{
    public const int SuccessExitCode = 0;
    public const int UsageErrorExitCode = 1;
    public const int BuildErrorExitCode = 2;

    private readonly IPmfArchiveBuilder _archiveBuilder = archiveBuilder;

    public int Run(IReadOnlyList<string> args, TextWriter standardOutput, TextWriter standardError)
    {
        var parseResult = BuilderCommandParser.Parse(args);

        if (parseResult.ShowHelp)
        {
            standardOutput.WriteLine(UsageText);
            return SuccessExitCode;
        }

        if (!parseResult.IsSuccess || parseResult.Options is null)
        {
            standardError.WriteLine(parseResult.ErrorMessage);
            standardError.WriteLine();
            standardError.WriteLine(UsageText);
            return UsageErrorExitCode;
        }

        try
        {
            var buildResult = _archiveBuilder.Build(
                parseResult.Options.SourceDirectory,
                parseResult.Options.OutputPath,
                parseResult.Options.Overwrite);

            standardOutput.WriteLine($"Created PMF archive: {buildResult.ArchivePath}");
            standardOutput.WriteLine($"Source directory: {buildResult.SourceDirectory}");
            standardOutput.WriteLine($"Markdown files: {buildResult.EntryCount}");

            foreach (var entry in buildResult.Entries)
            {
                standardOutput.WriteLine($" - {entry.RelativePath}");
            }

            return SuccessExitCode;
        }
        catch (PmfException exception)
        {
            standardError.WriteLine($"Error [{exception.Code}]: {exception.Message}");
            return BuildErrorExitCode;
        }
        catch (IOException exception)
        {
            standardError.WriteLine($"Error [IoFailure]: {exception.Message}");
            return BuildErrorExitCode;
        }
        catch (UnauthorizedAccessException exception)
        {
            standardError.WriteLine($"Error [AccessDenied]: {exception.Message}");
            return BuildErrorExitCode;
        }
        catch (InvalidDataException exception)
        {
            standardError.WriteLine($"Error [InvalidArchive]: {exception.Message}");
            return BuildErrorExitCode;
        }
    }

    public static string UsageText =>
        """
        Portable Markdown Format Builder

        Usage:
          pmf-builder build --source <folder> --output <file.pmf> [--overwrite]

        Options:
          -s, --source     Source folder to package.
          -o, --output     Destination .pmf archive path.
              --overwrite  Replace the destination archive if it already exists.
          -h, --help       Show help.
        """;
}

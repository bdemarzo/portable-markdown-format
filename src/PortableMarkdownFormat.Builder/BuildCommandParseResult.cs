namespace PortableMarkdownFormat.Builder;

public sealed record BuildCommandParseResult(
    bool IsSuccess,
    bool ShowHelp,
    string? ErrorMessage,
    BuildCommandOptions? Options)
{
    public static BuildCommandParseResult Success(BuildCommandOptions options) =>
        new(true, false, null, options);

    public static BuildCommandParseResult Failure(string errorMessage) =>
        new(false, false, errorMessage, null);

    public static BuildCommandParseResult Help() =>
        new(true, true, null, null);
}

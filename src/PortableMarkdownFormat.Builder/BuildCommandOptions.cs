namespace PortableMarkdownFormat.Builder;

public sealed record BuildCommandOptions(string SourceDirectory, string OutputPath, bool Overwrite);

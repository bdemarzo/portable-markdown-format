namespace PortableMarkdownFormat.Builder;

public static class BuilderCommandParser
{
    public static BuildCommandParseResult Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            return BuildCommandParseResult.Help();
        }

        var command = args[0];
        if (IsHelpToken(command))
        {
            return BuildCommandParseResult.Help();
        }

        if (!command.Equals("build", StringComparison.OrdinalIgnoreCase))
        {
            return BuildCommandParseResult.Failure($"Unknown command '{command}'.");
        }

        string? source = null;
        string? output = null;
        var overwrite = false;

        for (var index = 1; index < args.Count; index++)
        {
            var token = args[index];

            if (IsHelpToken(token))
            {
                return BuildCommandParseResult.Help();
            }

            switch (token)
            {
                case "--overwrite":
                    overwrite = true;
                    break;

                case "--source":
                case "-s":
                    if (!TryReadValue(args, ref index, out var sourceValue))
                    {
                        return BuildCommandParseResult.Failure($"Missing value for '{token}'.");
                    }

                    source = sourceValue;
                    break;

                case "--output":
                case "-o":
                    if (!TryReadValue(args, ref index, out var outputValue))
                    {
                        return BuildCommandParseResult.Failure($"Missing value for '{token}'.");
                    }

                    output = outputValue;
                    break;

                default:
                    return BuildCommandParseResult.Failure($"Unknown option '{token}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            return BuildCommandParseResult.Failure("The --source option is required.");
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            return BuildCommandParseResult.Failure("The --output option is required.");
        }

        return BuildCommandParseResult.Success(new BuildCommandOptions(source, output, overwrite));
    }

    private static bool TryReadValue(IReadOnlyList<string> args, ref int index, out string? value)
    {
        if (index + 1 >= args.Count)
        {
            value = null;
            return false;
        }

        value = args[index + 1];
        index++;
        return true;
    }

    private static bool IsHelpToken(string token) =>
        token.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
        token.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
        token.Equals("help", StringComparison.OrdinalIgnoreCase);
}

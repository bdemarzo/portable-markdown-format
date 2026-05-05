using PortableMarkdownFormat.Builder;

namespace PortableMarkdownFormat.Core.Tests;

public sealed class BuilderCommandParserTests
{
    [Fact]
    public void Parse_ReturnsOptionsForBuildCommand()
    {
        var result = BuilderCommandParser.Parse(["build", "--source", "docs", "--output", "site.pmf", "--overwrite"]);

        Assert.True(result.IsSuccess);
        Assert.False(result.ShowHelp);
        Assert.NotNull(result.Options);
        Assert.Equal("docs", result.Options!.SourceDirectory);
        Assert.Equal("site.pmf", result.Options.OutputPath);
        Assert.True(result.Options.Overwrite);
    }

    [Fact]
    public void Parse_ReturnsHelpWhenNoArgumentsAreProvided()
    {
        var result = BuilderCommandParser.Parse([]);

        Assert.True(result.IsSuccess);
        Assert.True(result.ShowHelp);
    }

    [Fact]
    public void Parse_ReturnsErrorWhenRequiredOptionIsMissing()
    {
        var result = BuilderCommandParser.Parse(["build", "--source", "docs"]);

        Assert.False(result.IsSuccess);
        Assert.Equal("The --output option is required.", result.ErrorMessage);
    }

    [Fact]
    public void Parse_AcceptsShortOptions()
    {
        var result = BuilderCommandParser.Parse(["build", "-s", "docs", "-o", "site.pmf"]);

        Assert.True(result.IsSuccess);
        Assert.Equal("docs", result.Options!.SourceDirectory);
        Assert.Equal("site.pmf", result.Options.OutputPath);
    }

    [Fact]
    public void Parse_ReturnsErrorForUnknownCommand()
    {
        var result = BuilderCommandParser.Parse(["pack"]);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unknown command 'pack'.", result.ErrorMessage);
    }

    [Fact]
    public void Parse_ReturnsErrorForUnknownOption()
    {
        var result = BuilderCommandParser.Parse(["build", "--source", "docs", "--output", "site.pmf", "--verbose"]);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unknown option '--verbose'.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("--source")]
    [InlineData("--output")]
    public void Parse_ReturnsErrorWhenOptionValueIsMissing(string option)
    {
        var result = BuilderCommandParser.Parse(["build", option]);

        Assert.False(result.IsSuccess);
        Assert.Equal($"Missing value for '{option}'.", result.ErrorMessage);
    }

    [Fact]
    public void Parse_ReturnsHelpWhenHelpTokenAppearsAfterCommand()
    {
        var result = BuilderCommandParser.Parse(["build", "--help"]);

        Assert.True(result.IsSuccess);
        Assert.True(result.ShowHelp);
    }
}

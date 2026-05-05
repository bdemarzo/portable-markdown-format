using System.Diagnostics.CodeAnalysis;
using PortableMarkdownFormat.Core;

namespace PortableMarkdownFormat.Builder;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static int Main(string[] args)
    {
        var application = new BuilderApplication(new PmfArchiveBuilder());
        return application.Run(args, Console.Out, Console.Error);
    }
}

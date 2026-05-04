using System.Text;

namespace PortableMarkdownFormat.Core.Tests;

internal sealed class TemporaryDirectory : IDisposable
{
    public TemporaryDirectory()
    {
        RootPath = Path.Combine(
            Path.GetTempPath(),
            "PortableMarkdownFormat.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public string WriteFile(string relativePath, string content)
    {
        string fullPath = Path.Combine(RootPath, relativePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return fullPath;
    }

    public void Dispose()
    {
        if (Directory.Exists(RootPath))
        {
            Directory.Delete(RootPath, recursive: true);
        }
    }
}

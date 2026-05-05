using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace PortableMarkdownFormat.Core;

public sealed class PmfArchiveReader
{
    public async Task<PmfPackage> OpenAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

        string fullPackagePath = Path.GetFullPath(packagePath);
        if (!File.Exists(fullPackagePath))
        {
            throw new FileNotFoundException($"The PMF package '{fullPackagePath}' does not exist.", fullPackagePath);
        }

        await using FileStream packageStream = new(fullPackagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using ZipArchive archive = new(packageStream, ZipArchiveMode.Read, leaveOpen: false);

        ZipArchiveEntry manifestEntry = archive.GetEntry(PmfConstants.ManifestEntryName)
            ?? throw new PmfPackageValidationException("The PMF package is missing the required pmf-manifest.json entry.");

        PmfManifest manifest;
        await using (Stream manifestStream = manifestEntry.Open())
        {
            manifest = await JsonSerializer.DeserializeAsync<PmfManifest>(manifestStream, PmfJson.Options, cancellationToken)
                ?? throw new PmfPackageValidationException("The PMF manifest could not be read.");
        }

        ValidateManifest(manifest);

        List<PmfDocumentFile> files = new(manifest.Files.Count);
        foreach (PmfManifestEntry manifestFile in manifest.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ZipArchiveEntry contentEntry = archive.GetEntry(manifestFile.EntryName)
                ?? throw new PmfPackageValidationException($"The PMF package is missing '{manifestFile.EntryName}'.");

            byte[] content;
            await using (Stream contentStream = contentEntry.Open())
            await using (MemoryStream buffer = new())
            {
                await contentStream.CopyToAsync(buffer, cancellationToken);
                content = buffer.ToArray();
            }

            if (content.LongLength != manifestFile.ByteLength)
            {
                throw new PmfPackageValidationException(
                    $"The payload '{manifestFile.RelativePath}' does not match the byte length declared in the manifest.");
            }

            string actualHash = PmfHashUtility.ToSha256Hex(content);
            if (!string.Equals(actualHash, manifestFile.Sha256, StringComparison.Ordinal))
            {
                throw new PmfPackageValidationException(
                    $"The payload '{manifestFile.RelativePath}' does not match the hash declared in the manifest.");
            }

            string markdown;
            await using (MemoryStream markdownStream = new(content, writable: false))
            using (StreamReader reader = new(markdownStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                markdown = await reader.ReadToEndAsync();
            }

            files.Add(new PmfDocumentFile(
                manifestFile.RelativePath,
                manifestFile.EntryName,
                manifestFile.ByteLength,
                manifestFile.Sha256,
                markdown));
        }

        return new PmfPackage(fullPackagePath, manifest, files);
    }

    private static void ValidateManifest(PmfManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        if (!string.Equals(manifest.Format, PmfConstants.FormatName, StringComparison.Ordinal))
        {
            throw new PmfPackageValidationException($"Unsupported PMF format '{manifest.Format}'.");
        }

        if (!string.Equals(manifest.SpecificationVersion, PmfConstants.SpecificationVersion, StringComparison.Ordinal))
        {
            throw new PmfPackageValidationException(
                $"Unsupported PMF specification version '{manifest.SpecificationVersion}'.");
        }

        if (string.IsNullOrWhiteSpace(manifest.RootDirectoryName))
        {
            throw new PmfPackageValidationException("The PMF manifest must declare a rootDirectoryName.");
        }

        if (manifest.Files.Count == 0)
        {
            throw new PmfPackageValidationException("The PMF manifest must contain at least one file entry.");
        }

        HashSet<string> relativePaths = new(StringComparer.Ordinal);
        foreach (PmfManifestEntry file in manifest.Files)
        {
            string normalizedRelativePath = PmfPathUtility.NormalizeRelativePath(file.RelativePath);
            if (!string.Equals(normalizedRelativePath, file.RelativePath, StringComparison.Ordinal))
            {
                throw new PmfPackageValidationException(
                    $"The manifest path '{file.RelativePath}' is not normalized.");
            }

            string expectedEntryName = PmfPathUtility.GetArchiveEntryName(file.RelativePath);
            if (!string.Equals(expectedEntryName, file.EntryName, StringComparison.Ordinal))
            {
                throw new PmfPackageValidationException(
                    $"The entry name '{file.EntryName}' does not match '{expectedEntryName}'.");
            }

            if (file.ByteLength < 0)
            {
                throw new PmfPackageValidationException(
                    $"The manifest entry '{file.RelativePath}' declares an invalid byte length.");
            }

            if (!relativePaths.Add(file.RelativePath))
            {
                throw new PmfPackageValidationException(
                    $"The PMF manifest contains a duplicate path '{file.RelativePath}'.");
            }

            if (file.Sha256.Length != 64 || file.Sha256.Any(static character => !Uri.IsHexDigit(character)))
            {
                throw new PmfPackageValidationException(
                    $"The manifest entry '{file.RelativePath}' does not declare a valid SHA-256 hash.");
            }
        }
    }
}

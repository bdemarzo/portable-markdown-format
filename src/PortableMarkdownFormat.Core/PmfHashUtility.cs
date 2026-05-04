using System.Security.Cryptography;

namespace PortableMarkdownFormat.Core;

internal static class PmfHashUtility
{
    public static string ToSha256Hex(byte[] content)
        => Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
}

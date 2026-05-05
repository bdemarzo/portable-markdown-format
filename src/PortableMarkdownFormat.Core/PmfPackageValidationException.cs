namespace PortableMarkdownFormat.Core;

public sealed class PmfPackageValidationException : IOException
{
    public PmfPackageValidationException(string message)
        : base(message)
    {
    }
}

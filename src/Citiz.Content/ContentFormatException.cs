namespace Citiz.Content;

/// <summary>A content file could not be read, parsed or mapped. The message names the file and what is wrong.</summary>
public sealed class ContentFormatException : Exception
{
    /// <summary>Creates the exception for <paramref name="file"/>.</summary>
    public ContentFormatException(string file, string message, Exception? innerException = null)
        : base($"{file}: {message}", innerException)
    {
        File = file;
    }

    /// <summary>The content-relative path of the offending file.</summary>
    public string File { get; }
}

namespace JWLMerge.Models;

internal sealed class FileFormatErrorListItem
{
    public FileFormatErrorListItem(string filename, string errorMsg)
    {
        Filename = filename;
        ErrorMsg = errorMsg;
    }

    public string Filename { get; }

    public string ErrorMsg { get; }
}
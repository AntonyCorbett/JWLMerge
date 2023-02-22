using System;
using System.Text;

namespace JWLMerge.BackupFileServices.Helpers;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class RedactService
{
    private readonly Lazy<string[]> _loremIpsumLines;
    private readonly Random _random = new();

    public RedactService()
    {
        _loremIpsumLines = new Lazy<string[]>(LoremIpsumFactory);
    }

    public string GetNoteTitle(int length)
    {
        return GenerateText(length);
    }
        
    public string GenerateNoteContent(int length)
    {
        return GenerateText(length);
    }

    private string GenerateText(int length)
    {
        var sb = new StringBuilder();

        while (sb.Length < length)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.Append(GetRandomSentence());
        }
            
        return sb.ToString()[..length];
    }

    private string GetRandomSentence()
    {
        return _loremIpsumLines.Value[_random.Next(0, _loremIpsumLines.Value.Length)].Trim();
    }

    private string[] LoremIpsumFactory()
    {
        return Properties.Resources.LoremIpsum.Split('\n');
    }
}
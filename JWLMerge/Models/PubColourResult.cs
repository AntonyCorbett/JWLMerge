namespace JWLMerge.Models;

internal sealed class PubColourResult
{
    public string? PublicationSymbol { get; set; }

    public int ColorIndex { get; set; }

    public bool AnyColor { get; set; }

    public bool AnyPublication { get; set; }

    public bool RemoveAssociatedNotes { get; set; }

    public bool IsInvalid =>
        (ColorIndex == 0 && !AnyColor) ||
        (string.IsNullOrEmpty(PublicationSymbol) && !AnyPublication);
}
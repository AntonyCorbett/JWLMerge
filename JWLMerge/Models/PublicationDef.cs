namespace JWLMerge.Models;

internal sealed class PublicationDef
{
    public PublicationDef(string keySymbol, bool isAllPubSymbol)
    {
        KeySymbol = keySymbol;
        IsAllPublicationsSymbol = isAllPubSymbol;
    }

    public string KeySymbol { get; }

    public bool IsAllPublicationsSymbol { get; }
}
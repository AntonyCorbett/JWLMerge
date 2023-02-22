namespace JWLMerge.Services;

internal sealed class NotesByTagResult
{
    public int[]? TagIds { get; set; }

    public bool RemoveUntaggedNotes { get; set; }

    public bool RemoveAssociatedUnderlining { get; set; }

    public bool RemoveAssociatedTags { get; set; }
}
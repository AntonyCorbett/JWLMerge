namespace JWLMerge.Services
{
    internal interface IRedactService
    {
        string GetNoteTitle(int length);

        string GenerateNoteContent(int length);
    }
}

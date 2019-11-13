namespace JWLMerge.Services
{
    using System.Threading.Tasks;

    internal interface IDialogService
    {
        Task<bool> ShouldRedactNotes();

        bool IsDialogVisible();
    }
}

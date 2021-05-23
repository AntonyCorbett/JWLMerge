namespace JWLMerge.ViewModel
{
    using Microsoft.Toolkit.Mvvm.DependencyInjection;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    internal class ViewModelLocator
    {
        public static MainViewModel Main => Ioc.Default.GetService<MainViewModel>()!;

        public RedactNotesPromptViewModel RedactNotesPromptDialog => Ioc.Default.GetService<RedactNotesPromptViewModel>()!;

        public RemoveNotesByTagViewModel RemoveNotesByTagDialog => Ioc.Default.GetService<RemoveNotesByTagViewModel>()!;

        public RemoveUnderliningByColourViewModel RemoveUnderliningByColourDialog => Ioc.Default.GetService<RemoveUnderliningByColourViewModel>()!;

        public RemoveUnderliningByPubAndColourViewModel RemoveUnderliningByPubAndColourDialog => Ioc.Default.GetService<RemoveUnderliningByPubAndColourViewModel>()!;

        public RemoveFavouritesPromptViewModel RemoveFavouritesPromptDialog => Ioc.Default.GetService<RemoveFavouritesPromptViewModel>()!;

        public ImportBibleNotesViewModel ImportBibleNotesDialog => Ioc.Default.GetService<ImportBibleNotesViewModel>()!;

        public BackupFileFormatErrorViewModel BackupFileFormatErrorDialog => Ioc.Default.GetService<BackupFileFormatErrorViewModel>()!;

        public DetailViewModel Detail => Ioc.Default.GetService<DetailViewModel>()!;
    }
}
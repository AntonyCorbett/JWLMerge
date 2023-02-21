using CommunityToolkit.Mvvm.DependencyInjection;

namespace JWLMerge.ViewModel;

/// <summary>
/// This class contains static references to all the view models in the
/// application and provides an entry point for the bindings.
/// </summary>
internal sealed class ViewModelLocator
{
    public static MainViewModel Main => Ioc.Default.GetService<MainViewModel>()!;

#pragma warning disable CA1822
    public RedactNotesPromptViewModel RedactNotesPromptDialog => Ioc.Default.GetService<RedactNotesPromptViewModel>()!;
#pragma warning restore CA1822

#pragma warning disable CA1822
    public RemoveNotesByTagViewModel RemoveNotesByTagDialog => Ioc.Default.GetService<RemoveNotesByTagViewModel>()!;
#pragma warning restore CA1822

#pragma warning disable CA1822
    public RemoveUnderliningByColourViewModel RemoveUnderliningByColourDialog => Ioc.Default.GetService<RemoveUnderliningByColourViewModel>()!;
#pragma warning restore CA1822

#pragma warning disable CA1822
    public RemoveUnderliningByPubAndColourViewModel RemoveUnderliningByPubAndColourDialog => Ioc.Default.GetService<RemoveUnderliningByPubAndColourViewModel>()!;
#pragma warning restore CA1822

#pragma warning disable CA1822
    public RemoveFavouritesPromptViewModel RemoveFavouritesPromptDialog => Ioc.Default.GetService<RemoveFavouritesPromptViewModel>()!;
#pragma warning restore CA1822

#pragma warning disable CA1822
    public ImportBibleNotesViewModel ImportBibleNotesDialog => Ioc.Default.GetService<ImportBibleNotesViewModel>()!;
#pragma warning restore CA1822

#pragma warning disable CA1822
    public BackupFileFormatErrorViewModel BackupFileFormatErrorDialog => Ioc.Default.GetService<BackupFileFormatErrorViewModel>()!;
#pragma warning restore CA1822

#pragma warning disable CA1822
    public DetailViewModel Detail => Ioc.Default.GetService<DetailViewModel>()!;
#pragma warning restore CA1822
}
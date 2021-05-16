namespace JWLMerge.ViewModel
{
    using System;
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using JWLMerge.BackupFileServices;
    using JWLMerge.ExcelServices;
    using JWLMerge.Services;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    internal class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IDragDropService, DragDropService>();
            SimpleIoc.Default.Register<IBackupFileService, BackupFileService>();
            SimpleIoc.Default.Register<IFileOpenSaveService, FileOpenSaveService>();
            SimpleIoc.Default.Register<IWindowService, WindowService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<ISnackbarService, SnackbarService>();
            SimpleIoc.Default.Register<IExcelService, ExcelService>();

            SimpleIoc.Default.Register<BackupFileFormatErrorViewModel>();
            SimpleIoc.Default.Register<RedactNotesPromptViewModel>();
            SimpleIoc.Default.Register<RemoveFavouritesPromptViewModel>();
            SimpleIoc.Default.Register<RemoveNotesByTagViewModel>();
            SimpleIoc.Default.Register<RemoveUnderliningByColourViewModel>();
            SimpleIoc.Default.Register<RemoveUnderliningByPubAndColourViewModel>();
            SimpleIoc.Default.Register<ImportBibleNotesViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<DetailViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public RedactNotesPromptViewModel RedactNotesPromptDialog => ServiceLocator.Current.GetInstance<RedactNotesPromptViewModel>();

        public RemoveNotesByTagViewModel RemoveNotesByTagDialog => ServiceLocator.Current.GetInstance<RemoveNotesByTagViewModel>();

        public RemoveUnderliningByColourViewModel RemoveUnderliningByColourDialog => ServiceLocator.Current.GetInstance<RemoveUnderliningByColourViewModel>();

        public RemoveUnderliningByPubAndColourViewModel RemoveUnderliningByPubAndColourDialog => ServiceLocator.Current.GetInstance<RemoveUnderliningByPubAndColourViewModel>();

        public RemoveFavouritesPromptViewModel RemoveFavouritesPromptDialog => ServiceLocator.Current.GetInstance<RemoveFavouritesPromptViewModel>();

        public ImportBibleNotesViewModel ImportBibleNotesDialog => ServiceLocator.Current.GetInstance<ImportBibleNotesViewModel>();

        public BackupFileFormatErrorViewModel BackupFileFormatErrorDialog => ServiceLocator.Current.GetInstance<BackupFileFormatErrorViewModel>();

        // NB - the guid key produces a new instance per view.
        public DetailViewModel Detail => ServiceLocator.Current.GetInstance<DetailViewModel>(Guid.NewGuid().ToString());
    }
}
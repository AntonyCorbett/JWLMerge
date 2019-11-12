namespace JWLMerge.ViewModel
{
    using System;
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using JWLMerge.BackupFileServices;
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
            SimpleIoc.Default.Register<IRedactService, RedactService>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<DetailViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        
        // NB - the guid key produces a new instance per view.
        public DetailViewModel Detail => ServiceLocator.Current.GetInstance<DetailViewModel>(Guid.NewGuid().ToString());

        public static void Cleanup()
        {
            // todo: Clear the ViewModels
        }
    }
}
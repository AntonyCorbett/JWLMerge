namespace JWLMerge.Services
{
    using System;
    using System.IO;
    using Microsoft.Win32;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class FileOpenSaveService : IFileOpenSaveService
    {
        private static string SaveDirectory;
        
        public string GetSaveFilePath(string title)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                Title = title,
                Filter = "JW Library backup file (*.jwlibrary)|*.jwlibrary",
                InitialDirectory = SaveDirectory ?? GetDefaultSaveFolder()
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
                return saveFileDialog.FileName;
            }
            
            return null;
        }

        private string GetDefaultSaveFolder()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "JWLMerge");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}

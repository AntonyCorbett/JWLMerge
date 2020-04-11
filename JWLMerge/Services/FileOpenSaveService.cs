namespace JWLMerge.Services
{
    using System;
    using System.IO;
    using Microsoft.Win32;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class FileOpenSaveService : IFileOpenSaveService
    {
        private static string SaveDirectory { get; set; }

        private static string ImportDirectory { get; set; }

        public string GetBibleNotesImportFilePath(string title)
        {
            var openFileDialog = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                DefaultExt = ".txt",
                Title = title,
                Filter = "Text file (*.txt)|*.txt",
                InitialDirectory = ImportDirectory ?? GetDefaultImportFolder(),
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImportDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                return openFileDialog.FileName;
            }

            return null;
        }

        public string GetSaveFilePath(string title)
        {
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                Title = title,
                Filter = "JW Library backup file (*.jwlibrary)|*.jwlibrary",
                InitialDirectory = SaveDirectory ?? GetDefaultSaveFolder(),
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

        private string GetDefaultImportFolder()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}

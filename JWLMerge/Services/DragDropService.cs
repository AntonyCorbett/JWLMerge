namespace JWLMerge.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;

    internal class DragDropService : IDragDropService
    {
        /// <summary>
        /// Determines whether we can accept the drag and drop operation
        /// </summary>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        /// <returns>
        ///   <c>true</c> if we accept; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Note that we accept multiple files so long as at least one is a ".jwlibrary" file.
        /// </remarks>
        public bool CanAcceptDrop(DragEventArgs e)
        {
            if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
            {
                foreach (string filePath in dataObject.GetFileDropList())
                {
                    if (IsJwLibraryFile(filePath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IEnumerable<string> GetDroppedFiles(DragEventArgs e)
        {
            var result = new List<string>();
            
            if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
            {
                foreach (string filePath in dataObject.GetFileDropList())
                {
                    if (IsJwLibraryFile(filePath))
                    {
                        result.Add(filePath);
                    }
                }
            }

            return result;
        }

        private bool IsJwLibraryFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            return ext != null && ext.Equals(".jwlibrary", StringComparison.OrdinalIgnoreCase);
        }
    }
}

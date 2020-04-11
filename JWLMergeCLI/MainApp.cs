namespace JWLMergeCLI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Events;
    using JWLMergeCLI.Exceptions;
    using Serilog;

    /// <summary>
    /// The main app.
    /// </summary>
    internal sealed class MainApp
    {
        public event EventHandler<ProgressEventArgs> ProgressEvent;
        
        /// <summary>
        /// Runs the app.
        /// </summary>
        /// <param name="args">Program arguments</param>
        public void Run(string[] args)
        {
            var files = GetInputFiles(args);
            
            IBackupFileService backupFileService = new BackupFileService();
            backupFileService.ProgressEvent += BackupFileServiceProgress;
            
            var backup = backupFileService.Merge(files);
            string outputFileName = $"{backup.Manifest.Name}.jwlibrary";
            backupFileService.WriteNewDatabase(backup, outputFileName, files.First());

            var logMessage = $"{files.Count} backup files merged to {outputFileName}";
            Log.Logger.Information(logMessage);
            OnProgressEvent(logMessage);
        }

        private void BackupFileServiceProgress(object sender, ProgressEventArgs e)
        {
            OnProgressEvent(e);
        }

        private IReadOnlyCollection<string> GetInputFiles(string[] args)
        {
            OnProgressEvent("Checking files exist");
            
            var result = new List<string>();
            
            foreach (var arg in args)
            {
                if (!File.Exists(arg))
                {
                    throw new JwlMergeCmdLineException($"File does not exist: {arg}");
                }
                
                Log.Logger.Debug("Found file: {file}", arg);
                result.Add(arg);
            }

            if (result.Count < 2)
            {
                throw new JwlMergeCmdLineException("Specify at least 2 files to merge");
            }

            return result;
        }

        private void OnProgressEvent(ProgressEventArgs e)
        {
            ProgressEvent?.Invoke(this, e);
        }

        private void OnProgressEvent(string message)
        {
            OnProgressEvent(new ProgressEventArgs { Message = message });
        }
    }
}

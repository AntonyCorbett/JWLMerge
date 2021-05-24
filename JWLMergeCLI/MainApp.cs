namespace JWLMergeCLI
{
    using System;
    using System.Linq;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Events;
    using Args;
    using Serilog;

    /// <summary>
    /// The main app.
    /// </summary>
    internal sealed class MainApp
    {
        public event EventHandler<ProgressEventArgs>? ProgressEvent;
        
        /// <summary>
        /// Runs the app.
        /// </summary>
        /// <param name="args">Program arguments</param>
        public void Run(CommandLineArgs args)
        {
            var backupFileService = new BackupFileService();
            backupFileService.ProgressEvent += BackupFileServiceProgress;
            
            var backup = backupFileService.Merge(args.BackupFiles);
            string outputFileName = args.OutputFilePath ?? $"{backup.Manifest.Name}.jwlibrary";
            backupFileService.WriteNewDatabase(backup, outputFileName, args.BackupFiles.First());

            var logMessage = $"{args.BackupFiles.Length} backup files merged to {outputFileName}";
            Log.Logger.Information(logMessage);
            OnProgressEvent(logMessage);
        }

        private void BackupFileServiceProgress(object? sender, ProgressEventArgs e)
        {
            OnProgressEvent(e);
        }

        private void OnProgressEvent(ProgressEventArgs e)
        {
            ProgressEvent?.Invoke(this, e);
        }

        private void OnProgressEvent(string message)
        {
            OnProgressEvent(new ProgressEventArgs(message));
        }
    }
}

namespace JWLMerge.BackupFileServices.Events
{
    using System;

    public class ProgressEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}

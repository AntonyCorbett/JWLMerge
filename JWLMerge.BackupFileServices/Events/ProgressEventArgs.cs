namespace JWLMerge.BackupFileServices.Events
{
    using System;

    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(string msg)
        {
            Message = msg;
        }

        public string Message { get; }
    }
}

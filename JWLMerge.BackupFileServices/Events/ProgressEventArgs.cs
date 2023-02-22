using System;

namespace JWLMerge.BackupFileServices.Events;

public class ProgressEventArgs : EventArgs
{
    public ProgressEventArgs(string msg)
    {
        Message = msg;
    }

    public string Message { get; }
}
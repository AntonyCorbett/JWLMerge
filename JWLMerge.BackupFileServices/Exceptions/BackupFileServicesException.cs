namespace JWLMerge.BackupFileServices.Exceptions
{
    using System;

    [Serializable]
    public class BackupFileServicesException : Exception
    {
        public BackupFileServicesException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}

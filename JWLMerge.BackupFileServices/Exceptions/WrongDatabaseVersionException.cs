namespace JWLMerge.BackupFileServices.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WrongDatabaseVersionException : BackupFileServicesException
    {
        public WrongDatabaseVersionException(string filename, int expectedVersion, int foundVersion)
            : base($"Wrong database version found ({foundVersion}) in {filename}. Expecting {expectedVersion}")
        {
            Filename = filename;
            ExpectedVersion = expectedVersion;
            FoundVersion = foundVersion;
        }

        // Without this constructor, deserialization will fail
        protected WrongDatabaseVersionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Filename { get; }

        public int ExpectedVersion { get; }

        public int FoundVersion { get; }
    }
}

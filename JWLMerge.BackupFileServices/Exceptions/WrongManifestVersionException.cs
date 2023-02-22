using System;
using System.Runtime.Serialization;

namespace JWLMerge.BackupFileServices.Exceptions;

[Serializable]
public class WrongManifestVersionException : BackupFileServicesException
{
    public WrongManifestVersionException(string filename, int expectedVersion, int foundVersion)
        : base($"Wrong manifest version found ({foundVersion}) in {filename}. Expecting {expectedVersion}")
    {
        Filename = filename;
        ExpectedVersion = expectedVersion;
        FoundVersion = foundVersion;
    }

    public WrongManifestVersionException()
    {
    }

    public WrongManifestVersionException(string errorMessage) 
        : base(errorMessage)
    {
    }

    public WrongManifestVersionException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    // Without this constructor, deserialization will fail
    protected WrongManifestVersionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string? Filename { get; }

    public int ExpectedVersion { get; }

    public int FoundVersion { get; }
}
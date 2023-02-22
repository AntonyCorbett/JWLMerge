using System;
using System.Runtime.Serialization;

namespace JWLMerge.ExcelServices.Exceptions;

[Serializable]
public class ExcelServicesException : Exception
{
    public ExcelServicesException()
    {
    }

    public ExcelServicesException(string errorMessage)
        : base(errorMessage)
    {
    }

    public ExcelServicesException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    // Without this constructor, deserialization will fail
    protected ExcelServicesException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
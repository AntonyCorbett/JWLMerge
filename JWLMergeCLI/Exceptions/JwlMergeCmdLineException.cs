namespace JWLMergeCLI.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class JwlMergeCmdLineException : Exception
    {
        public JwlMergeCmdLineException()
        {
        }

        public JwlMergeCmdLineException(string message)
            : base(message)
        {
        }

        public JwlMergeCmdLineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // Without this constructor, deserialization will fail
        protected JwlMergeCmdLineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

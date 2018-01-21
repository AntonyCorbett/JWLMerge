namespace JWLMergeCLI.Exceptions
{
    using System;

    [Serializable]
    // ReSharper disable once InconsistentNaming
    public class JWLMergeCLIException : Exception
    {
        public JWLMergeCLIException(string message)
            : base(message)
        {
        }
    }
}

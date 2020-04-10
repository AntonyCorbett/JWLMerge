namespace JWLMergeCLI.Exceptions
{
    using System;

    [Serializable]
    public class JWLMergeCLIException : Exception
    {
        public JWLMergeCLIException(string message)
            : base(message)
        {
        }
    }
}

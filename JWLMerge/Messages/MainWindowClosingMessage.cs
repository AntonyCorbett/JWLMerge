namespace JWLMerge.Messages
{
    using System.ComponentModel;

    internal sealed class MainWindowClosingMessage
    {
        public MainWindowClosingMessage(CancelEventArgs args)
        {
            CancelEventArgs = args;
        }

        public CancelEventArgs CancelEventArgs { get; }
    }
}

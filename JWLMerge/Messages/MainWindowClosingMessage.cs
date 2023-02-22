using System.ComponentModel;

namespace JWLMerge.Messages;

internal sealed class MainWindowClosingMessage
{
    public MainWindowClosingMessage(CancelEventArgs args)
    {
        CancelEventArgs = args;
    }

    public CancelEventArgs CancelEventArgs { get; }
}
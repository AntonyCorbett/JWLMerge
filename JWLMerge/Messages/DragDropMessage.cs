using System.Windows;

namespace JWLMerge.Messages;

internal sealed class DragDropMessage
{
    public DragDropMessage(DragEventArgs args)
    {
        DragEventArgs = args;
    }

    public DragEventArgs DragEventArgs { get; }
}
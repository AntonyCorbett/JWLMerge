using System.Windows;

namespace JWLMerge.Messages;

internal sealed class DragOverMessage
{
    public DragOverMessage(DragEventArgs args)
    {
        DragEventArgs = args;
    }

    public DragEventArgs DragEventArgs { get; }
}
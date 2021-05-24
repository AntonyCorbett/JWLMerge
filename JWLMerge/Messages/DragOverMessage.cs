namespace JWLMerge.Messages
{
    using System.Windows;

    internal sealed class DragOverMessage
    {
        public DragOverMessage(DragEventArgs args)
        {
            DragEventArgs = args;
        }

        public DragEventArgs DragEventArgs { get; }
    }
}

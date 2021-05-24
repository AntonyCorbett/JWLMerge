namespace JWLMerge.Messages
{
    using System.Windows;

    internal sealed class DragDropMessage
    {
        public DragDropMessage(DragEventArgs args)
        {
            DragEventArgs = args;
        }

        public DragEventArgs DragEventArgs { get; }
    }
}

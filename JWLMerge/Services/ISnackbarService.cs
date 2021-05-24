namespace JWLMerge.Services
{
    using System;
    using MaterialDesignThemes.Wpf;

    public interface ISnackbarService
    {
        ISnackbarMessageQueue TheSnackbarMessageQueue { get; }

        void Enqueue(object content, object actionContent, Action actionHandler, bool promote = false);

        void Enqueue(
            object content,
            object actionContent,
            Action<object?> actionHandler,
            object actionArgument,
            bool promote,
            bool neverConsiderToBeDuplicate);

        void Enqueue(object content);

        void EnqueueWithOk(object content);
    }
}

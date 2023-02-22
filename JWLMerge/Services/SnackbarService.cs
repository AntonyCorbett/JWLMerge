using System;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace JWLMerge.Services;

#pragma warning disable CA1416 // Validate platform compatibility

public sealed class SnackbarService : ISnackbarService, IDisposable
{
    public ISnackbarMessageQueue TheSnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(4));

    public void Enqueue(object content, object actionContent, Action actionHandler, bool promote = false)
    {
        TheSnackbarMessageQueue.Enqueue(content, actionContent, actionHandler, promote);
    }

    public void Enqueue(
        object content,
        object actionContent,
        Action<object?> actionHandler,
        object actionArgument,
        bool promote,
        bool neverConsiderToBeDuplicate)
    {
        TheSnackbarMessageQueue.Enqueue(
            content,
            actionContent,
            actionHandler,
            actionArgument,
            promote,
            neverConsiderToBeDuplicate);
    }

    public void Enqueue(object content)
    {
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (Application.Current.MainWindow?.WindowState != WindowState.Minimized)
            {
                TheSnackbarMessageQueue.Enqueue(content);
            }
        }));
    }

    public void EnqueueWithOk(object content)
    {
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (Application.Current.MainWindow?.WindowState != WindowState.Minimized)
            {
                TheSnackbarMessageQueue.Enqueue(content, "OK", (_) => { }, () => { }, true, true, TimeSpan.FromSeconds(20));
            }
        }));
    }

    public void Dispose()
    {
        ((SnackbarMessageQueue)TheSnackbarMessageQueue).Dispose();
    }
}

#pragma warning restore CA1416 // Validate platform compatibility
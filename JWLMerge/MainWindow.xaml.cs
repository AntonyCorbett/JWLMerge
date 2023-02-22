using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows;
using JWLMerge.Messages;

namespace JWLMerge;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void PanelOnDragOver(object sender, DragEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new DragOverMessage(e));
    }

    private void PanelOnDrop(object sender, DragEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new DragDropMessage(e));
    }

    private void MainWindowOnClosing(object sender, CancelEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new MainWindowClosingMessage(e));
    }
}
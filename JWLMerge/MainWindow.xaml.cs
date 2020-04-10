namespace JWLMerge
{
    using System.ComponentModel;
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using JWLMerge.Messages;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PanelOnDragOver(object sender, DragEventArgs e)
        {
            Messenger.Default.Send(new DragOverMessage { DragEventArgs = e });
        }

        private void PanelOnDrop(object sender, DragEventArgs e)
        {
            Messenger.Default.Send(new DragDropMessage { DragEventArgs = e });
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs e)
        {
            Messenger.Default.Send(new MainWindowClosingMessage { CancelEventArgs = e });
        }
    }
}

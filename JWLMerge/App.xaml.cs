namespace JWLMerge
{
    using System;
    using System.IO;
    using System.Windows;
    using GalaSoft.MvvmLight.Threading;
    using Serilog;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherHelper.Initialize();
            
            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile(GetRollingFileFormat())
                .MinimumLevel.Debug()
                .CreateLogger();
            
            Log.Logger.Information("JWLMerge Launched");
        }

        private string GetRollingFileFormat()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "JWLMerge\\Logs");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return string.Concat(folder, "\\log-{Date}.txt");
        }
    }
}

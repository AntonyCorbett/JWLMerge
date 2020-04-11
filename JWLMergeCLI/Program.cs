namespace JWLMergeCLI
{
    using System;
    using System.Diagnostics;
    using Serilog;

    public static class Program
    {
        /// <summary>
        /// The main entry point.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile("logs\\log-{Date}.txt")
                .MinimumLevel.Debug()
                .CreateLogger();
            
            try
            {
                Log.Logger.Information("Started");

                if (args == null || args.Length < 2)
                {
                    ShowUsage();
                }
                else
                {
                    var app = new MainApp();
                    app.ProgressEvent += AppProgress;
                    app.Run(args);
                }

                Environment.ExitCode = 0;
                Log.Logger.Information("Finished");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error");
                
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Environment.ExitCode = 1;
            }

            Log.CloseAndFlush();
        }

        private static string GetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        private static void ShowUsage()
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine($" JWLMerge version {GetVersion()} ");
            Console.WriteLine();
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("   Description:");
            Console.ResetColor();
            Console.WriteLine("    JWLMergeCLI is used to merge the contents of 2 or more jwlibrary backup");
            Console.WriteLine("    files. These files are produced by the JW Library backup command and");
            Console.WriteLine("    contain your personal study notes and highlighting.");
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("   Usage:");
            Console.ResetColor();
            Console.WriteLine("    JWLMergeCLI <jwlibrary file 1> <jwlibrary file 2>...");
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("   An example:");
            Console.ResetColor();
            Console.WriteLine("    JWLMergeCLI \"C:\\Backup_PC16.jwlibrary\" \"C:\\Backup_iPad.jwlibrary\"");
            Console.WriteLine();
        }

        private static void AppProgress(object sender, JWLMerge.BackupFileServices.Events.ProgressEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}

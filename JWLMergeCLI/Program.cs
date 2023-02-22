using System;
using System.Globalization;
using System.IO;
using JWLMergeCLI.Args;
using Serilog;

namespace JWLMergeCLI;

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
        ConfigureLogging();
            
        try
        {
            Log.Logger.Information("Started");

            var commandLineArgs = ArgsHelper.Parse(args);
            if (commandLineArgs == null)
            {
                ShowUsage();
            }
            else
            {
                var app = new MainApp();
                app.ProgressEvent += AppProgress;
                app.Run(commandLineArgs);
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

    private static void ConfigureLogging()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "JWLMergeCLI\\Logs");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(folder, "log-{Date}.txt"), retainedFileCountLimit: 28, formatProvider: CultureInfo.InvariantCulture)
            .CreateLogger();

        Log.Logger.Information("==== Launched ====");
        Log.Logger.Information($"Version {GetVersion()}");
    }

    private static string GetVersion()
    {
        var ver = typeof(Program).Assembly.GetName().Version;
        return ver?.ToString() ?? "Unknown";
    }

    private static void ShowUsage()
    {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.BackgroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine($@" JWLMergeCLI version {GetVersion()} ");
        Console.WriteLine();
        Console.ResetColor();
            
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(@"   Description:");
        Console.ResetColor();
        Console.WriteLine(@"    JWLMergeCLI is used to merge the contents of 2 or more jwlibrary backup");
        Console.WriteLine(@"    files. These files are produced by the JW Library backup command and");
        Console.WriteLine(@"    contain your personal study notes and highlighting.");
        Console.WriteLine();
            
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(@"   Usage:");
        Console.ResetColor();
        Console.WriteLine(@"    JWLMergeCLI <jwlibrary file 1> <jwlibrary file 2>... [-o output file]");
        Console.WriteLine();
        Console.WriteLine(@"   Note that you can optionally specify the full path and name of the merged");
        Console.WriteLine(@"   file using the -o (or --output directive). If you omit it, the merged");
        Console.WriteLine(@"   file is stored in the current folder.");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(@"   An example:");
        Console.ResetColor();
        Console.WriteLine(@"    JWLMergeCLI ""C:\Backup_PC16.jwlibrary"" ""C:\Backup_iPad.jwlibrary""");
        Console.WriteLine();
    }

    private static void AppProgress(object? sender, JWLMerge.BackupFileServices.Events.ProgressEventArgs e)
    {
        Console.WriteLine(e.Message);
    }
}
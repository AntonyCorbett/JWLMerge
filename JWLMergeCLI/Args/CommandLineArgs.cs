namespace JWLMergeCLI.Args;

internal sealed class CommandLineArgs
{
    public string[] BackupFiles { get; set; } = null!;

    public string? OutputFilePath { get; set; }
}
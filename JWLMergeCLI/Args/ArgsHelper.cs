namespace JWLMergeCLI.Args
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal static class ArgsHelper
    {
        public static CommandLineArgs Parse(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                return null;
            }

            var state = State.Unknown;

            var result = new CommandLineArgs();
            var inputFiles = new List<string>();
            var outputFilePath = string.Empty;

            foreach (var a in args)
            {
                if (a.Equals("-o") || a.Equals("--output"))
                {
                    state = State.AwaitingOutput;
                    continue;
                }

                switch (state)
                {
                    case State.AwaitingOutput:
                        outputFilePath = a;
                        break;

                    case State.Unknown:
                        if (IsInputFile(a))
                        {
                            inputFiles.Add(a);
                        }

                        break;
                }
            }

            if (inputFiles.Count < 2)
            {
                return null;
            }

            result.OutputFilePath = outputFilePath;
            result.BackupFiles = inputFiles.ToArray();

            return result;
        }

        private static bool IsInputFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            return ext.Equals(".jwlibrary", StringComparison.OrdinalIgnoreCase);
        }
        
        private enum State
        {
            Unknown,
            AwaitingOutput,
        }
    }
}

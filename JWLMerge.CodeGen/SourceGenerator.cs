using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace JWLMerge.CodeGen
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        // we store AppCenter secrets outside the repo. You only need to modify this if you want to 
        // include your build in AppCenter analytics
        const string PathToSecrets = @"D:\My Documents\ApplicationSecrets\ProjectsPersonal\secrets.json";

        private string _secret = string.Empty;

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("GeneratedCodeForSecrets.cs", SourceText.From($@"
namespace SecretsNamespace
{{
    public class GeneratedCodeForSecrets
    {{
        public static string GetAppCenterSecret()
        {{            
            return ""{_secret}"";
        }}
    }}
}}", Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            _secret = "Hello There";
            try
            {
                if (File.Exists(PathToSecrets))
                {
                    var s = File.ReadAllText(PathToSecrets);
                    var secrets = JsonConvert.DeserializeObject<Secrets>(s); 
                    
                    _secret = secrets?.Apps.SingleOrDefault(
                                      x => x.Name.Equals("jwlMerge"))?.AppCenter ?? string.Empty;
                }
            }
            catch (Exception)
            {
                _secret = string.Empty;
            }            
        }

        class Secrets
        {
            public App[] Apps { get; set; }
        }

        class App
        {
            public string Name { get; set; }

            public string AppCenter { get; set; }
        }
    }
}

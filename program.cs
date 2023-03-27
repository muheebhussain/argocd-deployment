using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReplaceK8sPlaceholders
{
    class Program
    {
        static void ReplacePlaceholders(string templateDir, string valuesFile, string outputDir)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(valuesFile));

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            foreach (var templateFile in Directory.EnumerateFiles(templateDir, "*.yaml"))
            {
                var template = File.ReadAllText(templateFile);
                var output = ReplacePlaceholders(template, values);
                var outputFile = Path.Combine(outputDir, Path.GetFileName(templateFile));
                File.WriteAllText(outputFile, output);
            }
        }

        static string ReplacePlaceholders(string template, Dictionary<string, object> values)
        {
            foreach (var pair in FlattenDictionary(values))
            {
                var placeholder = $"{{{{ {pair.Key} }}}}";
                template = template.Replace(placeholder, pair.Value.ToString());
            }
            return template;
        }

        static Dictionary<string, object> FlattenDictionary(Dictionary<string, object> dict, string prefix = "")
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in dict)
            {
                if (pair.Value is JObject jObject)
                {
                    var nestedDict = jObject.ToObject<Dictionary<string, object>>();
                    var nestedResult = FlattenDictionary(nestedDict, $"{pair.Key}.");
                    foreach (var nestedPair in nestedResult)
                    {
                        result.Add($"{prefix}{nestedPair.Key}", nestedPair.Value);
                    }
                }
                else
                {
                    result.Add($"{prefix}{pair.Key}", pair.Value);
                }
            }
            return result;
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: dotnet run TEMPLATE_DIR VALUES_FILE OUTPUT_DIR");
                return;
            }

            ReplacePlaceholders(args[0], args[1], args[2]);
        }
    }
}


{
  "replicaCount": 1,
  "image": {
    "repository": "myrepo/myapp",
    "tag": "latest",
    "pullPolicy": "IfNotPresent"
  }
}

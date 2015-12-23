using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnx.Extensions.VisualStudio
{
    public class RnxProjectTaskFileFinder
    {
        private const string DEFAULT_CODE_FILENAME = "Rnx.cs";
        private const string DEFAULT_JSON_FILENAME = "rnx.json";
        private const string JSON_SOURCES_ELEMENT = "sources";

        public IEnumerable<string> FindCodeFiles(string rnxProjectDirectory)
        {
            var rnxCsFilename = Path.Combine(rnxProjectDirectory, DEFAULT_CODE_FILENAME);

            if (File.Exists(rnxCsFilename))
            {
                return Enumerable.Repeat(rnxCsFilename, 1);
            }

            var rnxJsonFilename = Path.Combine(rnxProjectDirectory, DEFAULT_JSON_FILENAME);

            if (File.Exists(rnxJsonFilename))
            {
                var rnxJson = JObject.Parse(File.ReadAllText(rnxJsonFilename));
                var sourceNames = rnxJson[JSON_SOURCES_ELEMENT].Select(f => f.ToString());
                var sourcesFullPath = sourceNames.Select(f => Path.GetFullPath(Path.Combine(rnxProjectDirectory, f)));
                var sourceFiles = new List<string>();

                foreach (var sourcePath in sourcesFullPath)
                {
                    if (File.Exists(sourcePath))
                    {
                        sourceFiles.Add(sourcePath);
                    }
                    else if (Directory.Exists(sourcePath))
                    {
                        sourceFiles.AddRange(Directory.EnumerateFiles(sourcePath, "*.cs", SearchOption.AllDirectories));
                    }
                }

                return sourceFiles;
            }

            return Enumerable.Empty<string>();
        }
    }
}
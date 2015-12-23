using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnx.Extensions.VisualStudio
{
    public class RnxVsRunnerSettings
    {
        private const string JSON_PROJECT_DIR_ELEMENT = "projectDir";
        private const string JSON_ARGS_ELEMENT = "args";

        public string ProjectDir { get; set; }
        public string Args { get; set; }

        public static RnxVsRunnerSettings FromJsonConfigFile(string jsonConfigFilename)
        {
            var json = JObject.Parse(File.ReadAllText(jsonConfigFilename));

            return new RnxVsRunnerSettings
            {
                ProjectDir = json[JSON_PROJECT_DIR_ELEMENT]?.ToString(),
                Args = json[JSON_ARGS_ELEMENT]?.ToString()
            };
        }
    }
}
using Newtonsoft.Json.Linq;
using System.IO;

namespace Rnx.Extensions.VisualStudio
{
    public class RnxVsRunnerSettings
    {
        private const string JSON_ARGS_ELEMENT = "args";

        public string Args { get; set; }

        public static RnxVsRunnerSettings FromJsonConfigFile(string jsonConfigFilename)
        {
            var json = JObject.Parse(File.ReadAllText(jsonConfigFilename));

            return new RnxVsRunnerSettings
            {
                Args = json[JSON_ARGS_ELEMENT]?.ToString()
            };
        }
    }
}
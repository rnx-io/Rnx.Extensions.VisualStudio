using Microsoft.VisualStudio.TaskRunnerExplorer;
using System.IO;
using System.Windows.Media;

namespace Rnx.Extensions.VisualStudio
{
    public class RnxTaskRunnerConfig : ITaskRunnerConfig
    {
        public ImageSource Icon { get; private set; }

        public RnxTaskRunnerConfig(ITaskRunnerNode taskHierarchy, ImageSource icon)
        {
            TaskHierarchy = taskHierarchy;
            Icon = icon;
        }

        public ITaskRunnerNode TaskHierarchy { get; private set; }

        public void Dispose()
        {}

        public string LoadBindings(string configPath)
        {
            var bindingsFile = BindingsFilename(configPath);

            if (!File.Exists(bindingsFile))
            {
                return "<binding />";
            }

            return File.ReadAllText(bindingsFile);
        }

        public bool SaveBindings(string configPath, string bindingsXml)
        {
            var bindingsFile = BindingsFilename(configPath);

            File.WriteAllText(bindingsFile, bindingsXml);
            return true;
        }

        private string BindingsFilename(string configPath)
        {
            configPath = configPath.Replace(".vs.json", ".cs");
            return configPath + ".xml";
        }
    }
}

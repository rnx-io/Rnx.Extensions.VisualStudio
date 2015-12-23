using Microsoft.VisualStudio.TaskRunnerExplorer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rnx.Extensions.VisualStudio
{
    [TaskRunnerExport(DEFAULT_CONFIG_FILENAME)]
    public class RnxTaskRunner : ITaskRunner
    {
        private const string DEFAULT_CONFIG_FILENAME = "rnx.vs.json";

        public List<ITaskRunnerOption> Options => null;

        private RnxTaskConfigParser _configParser;
        private ImageSource _icon;
        private RnxProjectTaskFileFinder _rnxProjectTaskFileFinder;
        private bool _dummy;

        public RnxTaskRunner()
        {
            _configParser = new RnxTaskConfigParser();
            _rnxProjectTaskFileFinder = new RnxProjectTaskFileFinder();
            _icon = new BitmapImage(new Uri(@"pack://application:,,,/Rnx.Extensions.VisualStudio;component/Resources/logo.png"));
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            return await Task.Run(() =>
            {
                var rootNode = new TaskRunnerNode("Rnx tasks") { Description = "Rnx Task Runner" };
                var workingDirectory = Path.GetDirectoryName(configPath);
                var rnxProjectDirectory = workingDirectory;
                var dnxArgs = "";
                var vsRunnerSettings = RnxVsRunnerSettings.FromJsonConfigFile(configPath);
                var rnxArgs = vsRunnerSettings.Args ?? "";

                if (!string.IsNullOrWhiteSpace(vsRunnerSettings.ProjectDir))
                {
                    rnxProjectDirectory = Path.GetFullPath(Path.Combine(workingDirectory, vsRunnerSettings.ProjectDir));
                    dnxArgs = $"-p \"{rnxProjectDirectory}\"";
                }

                var sourceCodeFiles = _rnxProjectTaskFileFinder.FindCodeFiles(rnxProjectDirectory).ToArray();
                var tasks = _configParser.ParseCodeFiles(sourceCodeFiles).ToArray();

                // hack start
                // 
                // visual studio is caching task runner commands if their name doesn't change
                // this means, that changes in the "rnx.vs.json" file will not trigger an update, because the underlying command names
                // remain the same. By alternating between adding a space (or not) at the end of the task name, we force VS to recreate the tasks,
                // because the task names are now not equal anymore. This will not affect the display (VS seems to Trim() the task names anyway).
                var appendToTaskName = _dummy ? " " : string.Empty;
                _dummy = !_dummy;
                // hack end

                if (tasks.Length > 0)
                {
                    var classGroups = tasks.GroupBy(f => f.ParentClassName).ToArray();

                    foreach (var g in classGroups)
                    {
                        TaskRunnerNode taskGroupNode;

                        if (classGroups.Count() == 1)
                        {
                            var taskNode = new TaskRunnerNode("Rnx tasks");
                            rootNode.Children.Add(taskNode);
                            taskGroupNode = taskNode;
                        }
                        else
                        {
                            taskGroupNode = new TaskRunnerNode(g.Key);
                            rootNode.Children.Add(taskGroupNode);
                        }

                        foreach (var task in g)
                        {
                            var cmd = new TaskRunnerCommand(workingDirectory, "dnx", $"{dnxArgs} Rnx {task.Name} {rnxArgs}");
                            taskGroupNode.Children.Add(new TaskRunnerNode(task.Name + appendToTaskName, true) { Command = cmd, Description = task.Description });
                        }
                    }
                }
                else
                {
                    rootNode.Children.Add(new TaskRunnerNode("(No tasks found)"));
                }

                return new RnxTaskRunnerConfig(rootNode, _icon);
            });
        }
    }
}
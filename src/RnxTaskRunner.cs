using Microsoft.VisualStudio.TaskRunnerExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rnx.Extensions.VisualStudio
{
    [TaskRunnerExport(DEFAULT_CONFIG_FILENAME, DEFAULT_RNX_FILENAME)]
    public class RnxTaskRunner : ITaskRunner
    {
        private const string DEFAULT_CONFIG_FILENAME = "rnx.vs.json";
        private const string DEFAULT_RNX_FILENAME = "rnx.cs";

        public List<ITaskRunnerOption> Options => null;

        private ImageSource _icon;
        private RnxTaskNameFinder _rnxTaskNameFinder;
        private bool _dummy;

        public RnxTaskRunner()
        {
            _rnxTaskNameFinder = new RnxTaskNameFinder();
            _icon = new BitmapImage(new Uri(@"pack://application:,,,/Rnx.Extensions.VisualStudio;component/Resources/logo.png"));
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            return await Task.Run(() =>
            {
                var workingDirectory = Path.GetDirectoryName(configPath);
                var rnxFilePathGlobs = new []{ "rnx.cs" };
                var rnxArgs = "";

                if (string.Equals(Path.GetFileName(configPath), DEFAULT_CONFIG_FILENAME, StringComparison.OrdinalIgnoreCase))
                {
                    var vsRunnerSettings = RnxVsRunnerSettings.FromJsonConfigFile(configPath);
                    rnxArgs = vsRunnerSettings.Args ?? "";

                    if (rnxArgs.Length > 0)
                    {
                        var args = SplitCommandLine(rnxArgs).ToList();
                        var idx = args.FindIndex(f => string.Equals(f, "--rnx-file", StringComparison.OrdinalIgnoreCase)
                                                   || string.Equals(f, "-f", StringComparison.OrdinalIgnoreCase));
                        if (idx > -1)
                        {
                            rnxFilePathGlobs = args.Skip(idx + 1).TakeWhile(f => !f.StartsWith("-")).Select(f => f.Trim('"')).ToArray();
                        }
                    }
                }

                var tasks = _rnxTaskNameFinder.FindAvailableTaskNames(workingDirectory, rnxFilePathGlobs).ToArray();
                var rootNode = new TaskRunnerNode("Rnx tasks") { Description = "Rnx Task Runner" };

                if (tasks.Length > 0)
                {
                    // hack start
                    // 
                    // visual studio is caching task runner commands if their name doesn't change
                    // this means, that changes in the "rnx.vs.json" file will not trigger an update, because the underlying command names
                    // remain the same. By alternating between adding a space (or not) at the end of the task name, we force VS to recreate the tasks,
                    // because the task names are now not equal anymore. This will not affect the display (VS seems to Trim() the task names anyway).
                    var appendToTaskName = _dummy ? " " : string.Empty;
                    _dummy = !_dummy;
                    // hack end

                    foreach (var task in tasks)
                    {
                        var cmd = new TaskRunnerCommand(workingDirectory, "dnx", $"Rnx {task} {rnxArgs}");
                        rootNode.Children.Add(new TaskRunnerNode(task + appendToTaskName, true) { Command = cmd });
                    }
                }
                else
                {
                    rootNode.Children.Add(new TaskRunnerNode("(No tasks found)"));
                }

                return new RnxTaskRunnerConfig(rootNode, _icon);
            });
        }

        // based on http://jake.ginnivan.net/c-sharp-argument-parser/
        private static string[] SplitCommandLine(string commandLine)
        {
            var translatedArguments = new StringBuilder(commandLine);
            var escaped = false;

            for (var i = 0; i < translatedArguments.Length; i++)
            {
                if (translatedArguments[i] == '"')
                {
                    escaped = !escaped;
                }
                else if (translatedArguments[i] == ' ' && !escaped)
                {
                    translatedArguments[i] = '\n';
                }
            }

            return translatedArguments.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(f => f.Trim('"')).ToArray();
        }
    }
}
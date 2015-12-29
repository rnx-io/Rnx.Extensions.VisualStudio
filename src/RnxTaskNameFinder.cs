using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rnx.Extensions.VisualStudio
{
    public class RnxTaskNameFinder
    {
        public IEnumerable<string> FindAvailableTaskNames(string baseDirectory, params string[] searchPatterns)
        {
            var tasks = new List<string>();
            var matcher = new Matcher();
            matcher.AddIncludePatterns(searchPatterns);
            var codeFileFilenames = matcher.GetResultsInFullPath(baseDirectory);

            foreach (var code in codeFileFilenames.Select(f => File.ReadAllText(f)))
            {
                var treeRoot = CSharpSyntaxTree.ParseText(code).GetRoot();

                // iterate through all public classes that are not nested
                foreach (var configClass in treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>()
                                                    .Where(f => !(f.Parent is ClassDeclarationSyntax) && f.Modifiers.Select(x => x.ToString()).Contains("public")))
                {
                    var publicMethods = configClass.ChildNodes().OfType<MethodDeclarationSyntax>()
                                                                .Where(f => f.Modifiers.Select(x => x.ToString()).Contains("public") && f.ReturnType.ToString().Contains("ITaskDescriptor"));
                    var publicProperties = configClass.ChildNodes().OfType<PropertyDeclarationSyntax>()
                                                                .Where(f => f.Modifiers.Select(x => x.ToString()).Contains("public") && f.Type.ToString().Contains("ITaskDescriptor"));

                    tasks.AddRange(publicMethods.Select(f => f.Identifier.ToString())
                                                .Concat(publicProperties.Select(f => f.Identifier.ToString()))
                                                .Select(f => f));
                }
            }

            return tasks;
        }
    }
}
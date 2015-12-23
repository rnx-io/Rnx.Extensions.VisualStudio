using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rnx.Extensions.VisualStudio
{
    public class RnxTaskConfigParser
    {
        public IEnumerable<RnxTask> ParseCodeFiles(params string[] csFilenames)
        {
            var tasks = new List<RnxTask>();

            foreach(var code in csFilenames.Select(f => File.ReadAllText(f)))
            {
                var treeRoot = CSharpSyntaxTree.ParseText(code).GetRoot();

                foreach(var configClass in treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(f => f.Modifiers.Select(x => x.ToString()).Contains("public")))
                {
                    var publicMethods = configClass.ChildNodes().OfType<MethodDeclarationSyntax>().Where(f => f.Modifiers.Select(x => x.ToString()).Contains("public"));
                    var publicProperties = configClass.ChildNodes().OfType<PropertyDeclarationSyntax>().Where(f => f.Modifiers.Select(x => x.ToString()).Contains("public"));

                    tasks.AddRange(publicMethods.Select(f => f.Identifier.ToString())
                                           .Concat(publicProperties.Select(f => f.Identifier.ToString()))
                                           .Select(f => new RnxTask(f, configClass.Identifier.ToString()))
                                  );
                }
            }

            return tasks;
        }
    }
}
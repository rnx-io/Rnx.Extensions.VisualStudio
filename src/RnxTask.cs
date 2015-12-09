using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnx.VisualStudioTaskRunner
{
    public class RnxTask
    {
        public string ParentClassName { get; private set; }
        public string Name { get; private set; }
        public string Description { get; set; }

        public RnxTask(string name, string parentClassName)
        {
            Name = name;
            ParentClassName = parentClassName;
        }
    }
}

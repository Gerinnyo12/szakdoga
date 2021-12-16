using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class LoadContext
    {
        public AssemblyLoadContext Context { get; set; }
        public Assembly Assembly { get; set; }

        public LoadContext(AssemblyLoadContext context, Assembly assembly)
        {
            Context = context;
            Assembly = assembly;
        }
    }
}

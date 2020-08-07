using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Watertight.Modules
{
    class ModuleLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver resolver;       

        public ModuleLoadContext(string modulePath)
        {
            resolver = new AssemblyDependencyResolver(modulePath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {     
            Assembly PotentiallyLoaded = All.SelectMany(x => x.Assemblies).DefaultIfEmpty(null).FirstOrDefault(x => x.GetName() == assemblyName);
            if(PotentiallyLoaded !=null)
            {
                return PotentiallyLoaded;
            }

            string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                Assembly asm = LoadFromAssemblyPath(assemblyPath);
                return asm;
            }

            return null;
        }

        
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if(libPath != null)
            {
                return LoadUnmanagedDllFromPath(libPath);
            }

            return IntPtr.Zero;
        }

    }
    

}

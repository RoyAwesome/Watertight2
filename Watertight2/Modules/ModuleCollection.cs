using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Watertight.Filesystem;

namespace Watertight.Modules
{
    public class ModuleCollection
    {
        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
       

        public class LoadedModule
        {
            internal Assembly Assembly
            {
                get;
                set;
            }

            internal string Path
            {
                get;
                set;
            }

            public Module Module
            {
                get;
                internal set;
            }

            public StartupPhase CurrentPhase
            {
                get;
                internal set;
            }          
        }

        List<LoadedModule> Modules
        {
            get;
        } = new List<LoadedModule>();

        StartupPhase MostRecentStartup = StartupPhase.None;

        IEnumerable<string> ModuleFolderPaths;

        internal ModuleCollection(params string[] ModuleFolderPath)
        {
            this.ModuleFolderPaths = ModuleFolderPath;

            foreach (string ImportantDir in ModuleFolderPath)
            {
                if (!Directory.Exists(ImportantDir)) Directory.CreateDirectory(ImportantDir);
            }
        }

        public IEnumerable<Tuple<LoadedModule, Attribute>> GetModulesWithAttribute(SubclassOf<Attribute> AttributeType)
        {
            foreach(LoadedModule module in Modules)
            {
                Attribute CustomAttribute = module.Module.GetType().GetCustomAttribute(AttributeType);
                if (CustomAttribute != null)
                {
                    yield return new Tuple<LoadedModule, Attribute>(module, CustomAttribute);
                }
            }
        }

        

        private void LoadModuleAtPath(string ModulePath)
        {
            ModuleLoadContext loadContext = new ModuleLoadContext(ModulePath);
            Assembly asm = null;
            try
            {
                asm = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(ModulePath)));
               
            }
            catch(BadImageFormatException )
            {
                return;
            }


            if (asm == null)
            {
                return;
            }

            LoadModulesInAssembly(asm);
        }

        private void LoadModulesInAssembly(Assembly asm)
        {
            string ModulePath = new Uri(asm.CodeBase).AbsolutePath;

            foreach (Type t in asm.GetTypes())
            {
                if (typeof(Module).IsAssignableFrom(t) && !t.IsAbstract)
                {
                    Logger.Info("Loading Module {0} from assembly {1}", t.Name, asm.FullName);

                    LoadedModule loadedModule = new LoadedModule
                    {
                        Assembly = asm,
                        Path = ModulePath,
                    };

                    Modules.Add(loadedModule);
                }
            }
        }

        private void InstantiateModules()
        {
            foreach (LoadedModule loadedModule in Modules)
            {
                bool bHasModules = false;

                Assembly asm = loadedModule.Assembly;

                foreach (Type t in asm.GetTypes())
                {
                    if (typeof(Module).IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        loadedModule.Module = Activator.CreateInstance(t) as Module;
                        loadedModule.Module.ModulePath = loadedModule.Path;

                        if (loadedModule.Module == null)
                        {
                            Logger.Warn("Could not load Module {0} from assembly {1}", loadedModule.Module.GetType().Name, asm.FullName);
                            continue;
                        }


                        Logger.Info("Instantiated Module {0} from assembly {1}", loadedModule.Module.GetType().Name, asm.FullName);

                        CatchupModuleStartup(loadedModule);

                        bHasModules = true;
                    }
                }

                if (bHasModules)
                {
                    //Add this path to the filesystem
                    string Dir = Path.GetDirectoryName(loadedModule.Path);
                    FileSystem.AddFilesystemPath(Path.Combine(Dir, "Resources"));
                }
            }
        }

        private void LoadModulesInDir(string Folder)
        {
            foreach (string ModuleDll in Directory.GetFiles(Folder, "*.dll"))
            {
                LoadModuleAtPath(ModuleDll);
            }
        }

        public void LoadModules()
        {
            Logger.Info("Loading Modules");

            LoadModulesInAssembly(Assembly.GetExecutingAssembly());

            foreach(string ModuleSubFolder in ModuleFolderPaths)
            {
                string ModuleSource = Path.Combine(Directory.GetCurrentDirectory(), ModuleSubFolder);

                foreach(string Dir in Directory.GetDirectories(ModuleSource))
                {
                    LoadModulesInDir(Dir);
                }                
            }

            Logger.Info("Instantiating Modules");
            InstantiateModules();
        }     


        public void EnterPhase(StartupPhase Phase)
        {
            if(MostRecentStartup >= Phase)
            {
                return;
            }
            Logger.Info("Entering Module Startup Phase {0}", Phase.ToString());
            MostRecentStartup = Phase;

            foreach (LoadedModule loadedModule in Modules)
            {
                StartupModuleToPhase(loadedModule, Phase);
            }
        }

        private void StartupModuleToPhase(LoadedModule module, StartupPhase phase)
        {
            if (MostRecentStartup <= module.CurrentPhase)
            {
                return;
            }

            module.Module.StartupModule(phase);
            module.CurrentPhase = phase;
        }

        private void CatchupModuleStartup(LoadedModule Module)
        {
            for (byte i = (byte)Module.CurrentPhase; i < (byte)MostRecentStartup; i++)
            {
                Logger.Info("Catching Module {0} up to Phase {1}", Module.Module.GetType().FullName, ((StartupPhase)i).ToString());
                StartupModuleToPhase(Module, (StartupPhase)i);
            }
        }
       

    }
}

using System;
using Watertight.Modules;

namespace Watertight.Editor
{
    public class WatertightEditorModule : IModule
    {
        public string ModuleName => "Watertight Editor";

        public string ModuleVersion => "1.0.0.0";

        public void ShutdownModule()
        {
            
        }

        public void StartupModule(StartupPhase Phase)
        {
            
        }
    }
}

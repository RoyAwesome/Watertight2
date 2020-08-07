using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Modules;

namespace TiledSharp
{
    class TiledSharpModule : IModule
    {
        public string ModuleName => "Tiled Sharp";

        public string ModuleVersion => "1.0.0.0";

        public void ShutdownModule()
        {
           
        }

        public void StartupModule(StartupPhase Phase)
        {
            
        }
    }
}

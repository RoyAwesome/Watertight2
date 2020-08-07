using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Modules;

namespace Watertight.Middleware.DearImGUI
{
    class ImGUIModule : IModule
    {
        public string ModuleName => "ImGUI";

        public string ModuleVersion => "1.0.0.0";

        public void ShutdownModule()
        {
            
        }

        public void StartupModule(StartupPhase Phase)
        {
           if(Phase == StartupPhase.Start)
            {
                IEngine.Instance.UseMiddleware(typeof(ImGUIMiddleware));
            }
        }
    }
}

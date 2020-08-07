using System;
using Watertight.Middleware.DearImGUI;
using Watertight.Modules;
using Watertight.Tickable;

namespace Watertight.Console
{
    public class WatertightConsoleModule : IModule
    {
        public string ModuleName => "Console";

        public string ModuleVersion => "1.0.0.0";

        WatertightConsole Console = new WatertightConsole();

        public void ShutdownModule()
        {
           
        }

        public void StartupModule(StartupPhase Phase)
        {
           if(Phase == StartupPhase.PreEngineInit)
           {
                IEngine.Instance.BindLogOutput(Console.RecieveLogMessage);
                IEngine.Instance.GameThreadTickManager.AddTick(new Tickable.TickFunction
                {
                    CanTick = true,
                    TickPriority = ImGUIMiddleware.ImGUI_FrameStartTick - 1,
                    TickFunc = Console.Tick
                });
           }
        }
    }
}

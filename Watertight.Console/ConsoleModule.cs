using System;
using Watertight.DearImGUI;
using Watertight.Modules;
using Watertight.Tickable;

namespace Watertight.Console
{
    public class WatertightConsoleModule : Module
    {
        public override string ModuleName => "Console";

        public override string ModuleVersion => "1.0.0.0";

        WatertightConsole Console = new WatertightConsole();

        public override void ShutdownModule()
        {
           
        }

        public override void StartupModule(StartupPhase Phase)
        {
           if(Phase == StartupPhase.PreEngineInit)
           {
                IEngine.Instance.BindLogOutput(Console.RecieveLogMessage);
                IEngine.Instance.GameThreadTickManager.AddTick(new Tickable.TickFunction
                {
                    CanTick = true,
                    TickPriority = ImGUIModule.ImGUI_FrameStartTick - 1,
                    TickFunc = Console.Tick
                });
           }
        }
    }
}

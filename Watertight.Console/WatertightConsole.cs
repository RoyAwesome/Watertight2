using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Interfaces;

namespace Watertight.Console
{
    class WatertightConsole : ITickable
    {
        List<string> ConsoleMessages = new List<string>();

        public void RecieveLogMessage(string logMessage)
        {
            ConsoleMessages.Add(logMessage);
        }

        public void Tick(float DeltaTime)
        {
            
            //ImGui.Begin("Console");

            //ImGui.BeginChild("Scrolling");
            //for(int i = 0; i < ConsoleMessages.Count; i++)
            //{
            //    ImGui.Text(ConsoleMessages[i]);
            //}
            //ImGui.EndChild();

            //ImGui.End();
        }
    }
}

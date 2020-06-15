using System;
using Watertight;
using Watertight.VulkanRenderer;

namespace SimpleTestGame
{
    class Program
    {
        static void Main(string[] args)
        {
            IEngine Engine = new SampleGameEngine("Sample Game", "0.0.0.0");

            Engine.Init();
            Engine.Run();
        }
    }
}

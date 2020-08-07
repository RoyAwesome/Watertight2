using System;

namespace Watertight.Startup
{
    class Program
    {
        static void Main(string[] args)
        {
            Watertight.Engine Engine = new Watertight.Engine();

            Engine.Init(args);
            Engine.Run();
        }
    }
}

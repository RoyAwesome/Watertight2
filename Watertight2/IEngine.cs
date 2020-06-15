using System.Collections.Generic;
using Watertight.Filesystem;
using Watertight.Middleware;
using Watertight.Rendering;
using Watertight.Scripts;
using Watertight.Tickable;

namespace Watertight
{
    public interface IEngine
    {
        public static IEngine Instance
        {
            get;
            internal protected set;
        }

        World ActiveWorld { get; }
        float FPS { get; set; }
        TickManager GameThreadTickManager { get; set; }
        float MaxFPS { get; set; }
        IEnumerable<IMiddleware> Middlewares { get; }
        string Name { get; }
        IEnumerable<ResourcePtr> PreloadResources { get; }
        Renderer Renderer { get; }
        bool Running { get; set; }
        string Version { get; }

        void Init();
        World LoadWorld(World WorldToLoad);
        World LoadWorld(WorldScript WorldScript);
        void OnInit();
        void Run();
        void Shutdown();
        void Tick(float DeltaTime);
        void UseMiddleware(SubclassOf<IMiddleware> MiddlewareType);
    }
}
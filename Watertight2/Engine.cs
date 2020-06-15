using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Watertight.Filesystem;
using Watertight.Tickable;
using Watertight.Scripts;
using Watertight.Rendering;
using System.Reflection;
using Watertight.Middleware;

namespace Watertight
{
    public class Engine<RendererType> : IEngine where RendererType : Renderer, new()
    {
        /// <summary>
        /// Gets the name of the Watertight Game
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the version of the Watertight Engine
        /// </summary>
        /// <returns></returns>
        public string Version
        {
            get;
            protected set;
        }

        public bool Running
        {
            get;
            set;
        }

        public float FPS
        {
            get;
            set;
        }

        public float MaxFPS
        {
            get
            {
                return (1 / (GameThreadTickManager.MinFrameTime * 1000));
            }
            set
            {
                GameThreadTickManager.MinFrameTime = 1 / (value / 1000);
            }

        }

        public World ActiveWorld
        {
            get;
            internal set;
        }


        public virtual IEnumerable<ResourcePtr> PreloadResources
        {
            get;
        } = new List<ResourcePtr>();

        public TickManager GameThreadTickManager
        {
            get;
            set;
        } = new TickManager();

        private Thread RenderThread;
        public Renderer Renderer
        {
            get;
            private set;
        }

        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        public IEnumerable<IMiddleware> Middlewares
        {
            get => _Middlewares;
        }
        private List<IMiddleware> _Middlewares = new List<IMiddleware>();

        public Engine(string Name, string version)
        {
            IEngine.Instance = this;
            this.Name = Name;
            this.Version = version;
        }

        private void LoadReferencedAssembly(Assembly asm)
        {
            foreach (AssemblyName name in asm.GetReferencedAssemblies())
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName))
                {
                    LoadReferencedAssembly(Assembly.Load(name));
                }
            }
        }

        public void Init()
        {
            //Make sure all of our dependent assemblies are loaded. 
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                this.LoadReferencedAssembly(assembly);
            }

            ConfigureLogger();

            Logger.Info("Starting Up Watertight Engine!  Game {0}, Version {1}", Name, Version);

            //Init the Filesystem
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(FileSystem).TypeHandle);

            //TODO: Configure Engine from a config file

            GameThreadTickManager.ShouldGCIfAble = true;

            TickFunction EngineTick = new TickFunction
            {
                TickFunc = Tick,
                TickPriority = TickFunction.HighPriority,
                CanTick = true,
            };

            GameThreadTickManager.AddTick(EngineTick);

            Renderer = new RendererType();
            //TODO: Thread This.  For now, we just tick it
            GameThreadTickManager.AddTick(Renderer.RenderTickFunction);

            MaxFPS = 120;

        

            OnInit();

            Logger.Info("Finished Loading Engine.  Ready to Begin");
        }

        public virtual void OnInit()
        {

        }

        public virtual void OnStart()
        {

        }

        public virtual void OnRendererCreated()
        {

        }


        public void Run()
        {
            //TODO: Render Thread this
            Renderer.CreateRenderer();
            OnRendererCreated();

            Renderer.CreateWindow();

            Renderer.ConstructRendererDefaults();

            foreach(IMiddleware middleware in Middlewares)
            {
                middleware.PostRenderInitialize(Renderer);
            }

            Running = true;
            GameThreadTickManager.Init();
            PreloadAssets();


            OnStart();
            while (ExecuteTick())
            {

            }
        }

        private bool ExecuteTick()
        {
            float TotalFrameDeltaTime = GameThreadTickManager.ExecuteSingleTick();
            FPS = (1 / TotalFrameDeltaTime) * 1000;

            return Running;
        }

        public virtual void Tick(float DeltaTime)
        {

        }

        private void PreloadAssets()
        {
            foreach (ResourcePtr resourcePtr in PreloadResources)
            {
                Logger.Info("Preloading Asset: {0}", resourcePtr.ToString());
                resourcePtr.Load();
            }
        }


        public World LoadWorld(World WorldToLoad)
        {
            if (ActiveWorld != null)
            {
                //Unload this world
                ActiveWorld.UnloadWorld();
            }

            ActiveWorld = WorldToLoad;
            ActiveWorld.BeginLoadingWorld();

            return ActiveWorld;
        }

        public World LoadWorld(WorldScript WorldScript)
        {
            return LoadWorld(WorldScript.CreateInstance<World>());
        }

        public virtual void Shutdown()
        {
            Running = false;
        }

        protected internal void AddTickfunc(TickFunction TickFunc)
        {
            GameThreadTickManager.AddTick(TickFunc);
        }

        protected internal void RemoveTickfunc(TickFunction TickFunc)
        {
            GameThreadTickManager.RemoveTick(TickFunc);
        }

        public void UseMiddleware(SubclassOf<IMiddleware> MiddlewareType)
        {
            IMiddleware instance = Activator.CreateInstance(MiddlewareType) as IMiddleware;
            _Middlewares.Add(instance);
            instance.Initialize(this);

        }

        private static void ConfigureLogger()
        {
            var Config = new NLog.Config.LoggingConfiguration();
            var LogConsole = new NLog.Targets.ConsoleTarget("LogConsole");
            var FileTarget = new NLog.Targets.FileTarget("File")
            {
                FileName = "WatertightLog.txt",
                //Layout = "${message}"
            };

            Config.AddRuleForAllLevels(LogConsole);
            //  Config.AddRuleForAllLevels(FileTarget);

            NLog.LogManager.Configuration = Config;
        }

    }
}

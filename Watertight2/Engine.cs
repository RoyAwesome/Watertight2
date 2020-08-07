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
using Watertight.Modules;
using Watertight.Game;
using NLog.Targets;
using NLog;

namespace Watertight
{
    public class Engine : IEngine 
    {
        /// <summary>
        /// Gets the name of the Watertight Game
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get;           
        } = "Watertight Engine";

        /// <summary>
        /// Gets the version of the Watertight Engine
        /// </summary>
        /// <returns></returns>
        public string Version
        {
            get;            
        } = "InDev";

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


        private List<ResourcePtr> PreloadResources
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

        public string[] StartupArgs
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

        ModuleCollection EngineModuleCollection
        {
            get;
        } = new ModuleCollection("Modules/", "Renderer/", "Game/");

        IGameInstance GameInstance;

        public Engine()
        {
            IEngine.Instance = this;
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

        private Watertight.Rendering.Renderer ConstructRenderer(string PreferredRenderer)
        {
            var Modules = EngineModuleCollection.GetModulesWithAttribute(typeof(RenderModuleAttribute));

            foreach(var ModulePair in Modules)
            {
                //TODO: Test if this module supports the game that we've loaded.  For now, just construct the first one we find
                RenderModuleAttribute attrib = ModulePair.Item2 as RenderModuleAttribute;

                return Activator.CreateInstance(attrib.RendererType) as Renderer;
            }

            return null;
        }

        private IGameInstance ConstructGameInstance(string PreferredGameInstance)
        {
            var Modules = EngineModuleCollection.GetModulesWithAttribute(typeof(GameModuleAttribute));

            foreach(var ModulePair in Modules)
            {
                GameModuleAttribute GameModule = ModulePair.Item2 as GameModuleAttribute;

                return Activator.CreateInstance(GameModule.GameInstance) as IGameInstance;
            }

            return null;
        }

        public void Init(string[] Args)
        {
            //TODO: Configure Engine from command line
            StartupArgs = Args;

            //Make sure all of our dependent assemblies are loaded. 
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                this.LoadReferencedAssembly(assembly);
            }

            ConfigureLogger();

            Logger.Info("Starting Up Watertight Engine! Version {0}", Version);

            //Init the Filesystem
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(FileSystem).TypeHandle);

            EngineModuleCollection.LoadModules();

            //TODO: Get the preferred game instance from command line
            GameInstance = ConstructGameInstance("");

            if(GameInstance == null)
            {
                throw new Exception("Error starting up engine, Cannot find a valid game module!");
            }
            
            

           
            GameThreadTickManager.ShouldGCIfAble = true;

            TickFunction EngineTick = new TickFunction
            {
                TickFunc = Tick,
                TickPriority = TickFunction.HighPriority,
                CanTick = true,
            };
            AddTickfunc(EngineTick);

            TickFunction GameInstanceTick = new TickFunction
            {
                TickFunc = GameInstance.Tick,
                TickPriority = TickFunction.HighPriority,
                CanTick = true,
            };
            AddTickfunc(GameInstanceTick);

            //TODO: Select the preferred renderer
            Renderer = ConstructRenderer("");
            if(Renderer == null)
            {
                throw new Exception("Error starting up engine, cannot find a valid renderer from any module!");
            }
            //TODO: Thread This.  For now, we just tick it
            AddTickfunc(Renderer.RenderTickFunction);

            MaxFPS = 120;

            //Begin starting up modules
            EngineModuleCollection.EnterPhase(StartupPhase.PreEngineInit);

            OnInit();

            EngineModuleCollection.EnterPhase(StartupPhase.PostEngineInit);

            Logger.Info("Finished Loading Engine.  Ready to Begin");
        }

        public virtual void OnInit()
        {
            EngineModuleCollection.EnterPhase(StartupPhase.EngineInit);
            GameInstance.OnInit();
        }

        public virtual void OnStart()
        {
            EngineModuleCollection.EnterPhase(StartupPhase.Start);
            GameInstance.OnStart();
        }

        public virtual void OnRendererCreated()
        {
            EngineModuleCollection.EnterPhase(StartupPhase.RendererInit);
            GameInstance.OnRendererCreated();
        }


        public void Run()
        {
            EngineModuleCollection.EnterPhase(StartupPhase.PreRendererInit);
            //TODO: Render Thread this
            Renderer.CreateRenderer();
            OnRendererCreated();
          

            Renderer.CreateWindow();

            Renderer.ConstructRendererDefaults();
           

            foreach (IMiddleware middleware in Middlewares)
            {
                middleware.PostRenderInitialize(Renderer);
            }
            EngineModuleCollection.EnterPhase(StartupPhase.PostRendererInit);

            EngineModuleCollection.EnterPhase(StartupPhase.PreStart);
            Running = true;
            GameThreadTickManager.Init();
            PreloadAssets();


            OnStart();
            EngineModuleCollection.EnterPhase(StartupPhase.PostStart);

            EngineModuleCollection.EnterPhase(StartupPhase.LastChance);

            EngineModuleCollection.EnterPhase(StartupPhase.FullyLoaded);
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
            GameInstance.CollectResources(PreloadResources);

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
               
        public event RecieveLogMessage OnLogMessage;

        internal class WatertightLogOutput : TargetWithLayout
        {
            protected override void Write(LogEventInfo logEvent)
            {
                string msg = this.Layout.Render(logEvent);
                if ( (IEngine.Instance as Engine).OnLogMessage != null)
                {
                    (IEngine.Instance as Engine).OnLogMessage.Invoke(msg);
                }
            }
        }

        public void BindLogOutput(RecieveLogMessage LogMessageHandler)
        {
            OnLogMessage += LogMessageHandler;
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

            var LogWTConsole = new WatertightLogOutput();

            Config.AddRuleForAllLevels(LogConsole);
            Config.AddRuleForAllLevels(LogWTConsole);
            //  Config.AddRuleForAllLevels(FileTarget);

            NLog.LogManager.Configuration = Config;
        }

    }
}

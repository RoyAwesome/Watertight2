using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Watertight.Tickable;
using Watertight.Interfaces;

namespace Watertight.Filesystem
{
    public delegate void ResourceLoadingProgress(ResourcePtr Resource, int ResouceCount, int TotalResources);

    internal class ResourceLoadingTask
    {
        const int LoadsPerTick = 10;
        internal TickFunction TickFunc = new TickFunction
        { 
            CanTick = true,
            TickPriority = TickFunction.Last
        };

        IEnumerable<ResourcePtr> Resources
        {
            get;
            set;
        }

        int ResourcesLoaded = 0;
        int TotalResources = 0;

        public Action PostLoadAction;
        public ResourceLoadingProgress LoadingProgressDelegate;

        public ResourceLoadingTask(IEnumerable<ResourcePtr> Resources)
        {
            this.Resources = Resources;
            TotalResources = Resources.Count();
            TickFunc.TickFunc = Tick;
        }

        public void Tick(float DeltaTime)
        {
            IEnumerable<ResourcePtr> ResourcesThisTick = Resources;
            if (Resources.Count() > LoadsPerTick)
            {
                ResourcesThisTick = Resources.Take(LoadsPerTick);
                Resources = Resources.Skip(LoadsPerTick);
            }
            
            foreach(ResourcePtr Ptr in ResourcesThisTick)
            {
                ResourcesLoaded++;
                LoadingProgressDelegate?.Invoke(Ptr, ResourcesLoaded, TotalResources);
                Ptr.Load();
            }  
            
            //Kill ourself
            if(ResourcesLoaded >= TotalResources || Resources.Count() == 0)
            {
                FileSystem.BulkLoadComplete(this);
            }
        }

    }

    public static partial class FileSystem
    {
        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string GameResources = "Resources/";
        public static readonly string ModDirectory = "mods/";
        public static readonly string CacheDirectory = "cache/";


        static List<ResourceFactory> Factories;
        static FileSystemPathFinder[] pathOrder;

        static Dictionary<ResourcePtr, object> ResourceCache = new Dictionary<ResourcePtr, object>();

        static List<ResourceLoadingTask> LoadingTasks = new List<ResourceLoadingTask>();
        
        static FileSystem()
        {
            if (!Directory.Exists(ModDirectory)) Directory.CreateDirectory(ModDirectory);
            if (!Directory.Exists(CacheDirectory)) Directory.CreateDirectory(CacheDirectory);

            Factories = new List<ResourceFactory>();
            

            pathOrder = new FileSystemPathFinder[] {                
                new FileSystemSearchPath(ModDirectory),
                new FileSystemSearchPath(GameResources),
            };

            ScanAssembliesForResourceFactories();

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            ScanAssemblyForResourceFactories(args.LoadedAssembly);
        }



        #region Public API
        public static bool EmplaceInMemoryResource(ResourcePtr Ptr, object Resource)
        {
            if(ResourceCache.ContainsKey(Ptr))
            {
                return false;
            }

            Logger.Info("Emplaced object: {0} into {1}", Resource.ToString(), Ptr.ToString());
            ResourceCache.Add(Ptr, Resource);

            Ptr.NotifyLoaded();

            return true;
        }

        public static object LoadResource(ResourcePtr Ptr)
        {
            if (!IsResourceLoaded(Ptr))
            {
                if (!ResourceExists(Ptr))
                {
                    throw new ArgumentException("Cannot find file: " + Ptr.ToString() + " In any search path!");
                }

                ResourceFactory factory = GetFactoryForResource(Ptr);

                if(factory == null)
                {
                    Logger.Error("Failed to find Factory for resource: {0}", Ptr.ToString());
                    return null;
                }

                using (Stream stream = GetFileStream(Ptr))
                {
                    object resource = factory.GetResource(Ptr, stream);
                    if(resource is Interfaces.IIsResource)
                    {
                        (resource as Interfaces.IIsResource).ResourcePtr = Ptr;
                    }
                    ResourceCache.Add(Ptr, resource);
                }
                Logger.Info("Loaded Resource {0}", Ptr.ToString());               
            }

            Ptr.NotifyLoaded();

            return ResourceCache[Ptr];
        }

        public static T LoadResource<T>(ResourcePtr Ptr) where T : class
        {
            return LoadResource(Ptr) as T;
        }

        public static bool IsResourceLoaded(ResourcePtr Ptr)
        {
            return ResourceCache.ContainsKey(Ptr);
        }

        public static void UnloadResource(ResourcePtr Ptr)
        {
            ResourceCache.Remove(Ptr);
        }

        #endregion

        public static void BulkLoadAssets(IEnumerable<ResourcePtr> Resources, Action LoadCompleteDelegate, ResourceLoadingProgress resourceLoadingProgressDelegate)
        {
            if(Resources == null)
            {
                LoadCompleteDelegate?.Invoke();
                return;
            }

            //If the engine doesn't exist, just process them now
            if(IEngine.Instance == null)
            {
                for (int i = 0; i < Resources.Count(); i++)
                {
                    ResourcePtr Ptr = Resources.ElementAt(i);
                    resourceLoadingProgressDelegate?.Invoke(Ptr, i, Resources.Count());
                    Ptr.Load();
                }
                LoadCompleteDelegate?.Invoke();
            }
            else
            {
                //Create a bulk task for the engine to process
                ResourceLoadingTask Task = new ResourceLoadingTask(Resources);
                Task.LoadingProgressDelegate = resourceLoadingProgressDelegate;
                Task.PostLoadAction = LoadCompleteDelegate;
                LoadingTasks.Add(Task);

                IEngine.Instance.GameThreadTickManager.AddTick(Task.TickFunc);

            }
        }

        internal static void BulkLoadComplete(ResourceLoadingTask Task)
        {
            Task.PostLoadAction?.Invoke();

            IEngine.Instance.GameThreadTickManager.RemoveTick(Task.TickFunc);
            LoadingTasks.Remove(Task);
        }

        public static void ScanAssembliesForResourceFactories()
        {
            foreach (var Asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                ScanAssemblyForResourceFactories(Asm);
            }
            //ScanAssemblyForResourceFactories(Assembly.GetExecutingAssembly());
           // ScanAssemblyForResourceFactories(Assembly.GetEntryAssembly());          
        }

        public static void ScanAssemblyForResourceFactories(Assembly asm)
        {
            foreach (Type t in asm.DefinedTypes)
            {
                if (typeof(ResourceFactory).IsAssignableFrom(t) && !t.IsAbstract)
                {                 
                    AddResourceFactory(t);
                }
            }
        }

        public static void AddResourceFactory(SubclassOf<ResourceFactory> ResourceFactoryClass)
        {
            if(Factories.FirstOrDefault(x => x.GetType() == ResourceFactoryClass) == null)
            {
                ResourceFactory rf = Activator.CreateInstance(ResourceFactoryClass) as ResourceFactory;
                Factories.Add(rf);
                Logger.Info("Found Resource Factory {0} for types {1}", ResourceFactoryClass.ToString(), string.Join(',', rf.ResourceSchemes));
            }            
        } 

        internal static object GetLoadedResource(ResourcePtr Ptr)
        {
            if(IsResourceLoaded(Ptr))
            {
                return ResourceCache[Ptr];
            }
            return null;
        }

        #region Resource Loading
        internal static bool ResourceExists(ResourcePtr Ptr)
        {
            for (int i = 0; i < pathOrder.Length; i++)
            {
                if (pathOrder[i].ExistsInPath(Ptr))
                {
                    return true;
                }
            }

            return false;
        }
      
        internal static Stream GetFileStream(ResourcePtr Ptr)
        {
            for (int i = 0; i < pathOrder.Length; i++)
            {
                if (pathOrder[i].ExistsInPath(Ptr))
                {
                    return pathOrder[i].GetFileStream(Ptr);
                }
            }

            return null;            
        }

        internal static ResourceFactory GetFactoryForResource(ResourcePtr Ptr)
        {
            return Factories.FirstOrDefault((f) => f.ResourceSchemes.Contains(Ptr.ResourceScheme)
                && f.FileExtensions.Select(x => x.ToLower()).Contains(Path.GetExtension(Ptr.FilePath).ToLower()));
        }
        #endregion
    }

}

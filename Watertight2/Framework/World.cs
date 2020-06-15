using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Watertight.Attributes;
using Watertight.Filesystem;
using Watertight.Framework;
using Watertight.Interfaces;
using Watertight.Scripts;

namespace Watertight
{
    public class World : IHasScript
    {
        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        
       
        public virtual IEnumerable<ResourcePtr> PreloadResources
        {
            get;
            set;
        }

        public bool HasBegunPlay
        {
            get;
            set;
        }

        public IEnumerable<Actor> AllActors
        {
            get
            {
                return _AllActors;
            }

        }

        public ObjectScript Script 
        { 
            get;
            set;
        }

        private List<Actor> _AllActors = new List<Actor>();


        public World()
        {
        }

        public T CreateActor<T>(ActorScript actorScript) where T : Actor
        {
            return CreateActor<T>(null, actorScript);
        }

        public T CreateActor<T>(SubclassOf<Actor> ActorClass) where T : Actor
        {
            return CreateActor<T>(ActorClass, null);
        }

        private T CreateActor<T>(SubclassOf<Actor> ActorClass, ActorScript actorScript) where T : Actor
        {
            Actor actor = null;
            if(ActorClass != null)
            {
                actor = Activator.CreateInstance(ActorClass) as Actor;
                actor.PostConstruct();
                if(actorScript != null)
                {
                    actorScript.ApplyToObject(actor);
                }
                else
                {
                    actor.PostScriptApplied();                    
                }
            }
            else if(actorScript != null)
            {
                actor = actorScript.CreateInstance<Actor>();
            }
            else
            {
                throw new ArgumentNullException(nameof(ActorClass), "Both Actor Class and ActorScript cannot be null.  Provide one.");
            }

            if (actor.Name == null)
            {
                int CompCount = AllActors.Count(x => x.GetType().Name == actor.GetType().Name);
                actor.Name = string.Format("{0}_{1}", actor.GetType().Name, CompCount);
            }

            _AllActors.Add(actor);
            
            //Init all constructor created actors
            foreach(var Property in actor.GetType().GetProperties())
            {
               if(typeof(ActorComponent).IsAssignableFrom(Property.PropertyType))
                {
                    ActorComponent comp = Property.GetValue(actor) as ActorComponent;

                    if(comp == null)
                    {
                        if(Property.SetMethod != null && Property.GetCustomAttribute<DontConstructComponentAttribute>(true) != null)
                        {
                            comp = Activator.CreateInstance(Property.PropertyType) as ActorComponent;
                            Property.SetValue(actor, comp);
                        }
                        else
                        {
                            continue;
                        }
                       
                    }

                    if(!comp.Registered)
                    {
                        comp.Register();
                    }
                }
            }

            List<ResourcePtr> ResourceCollector = new List<ResourcePtr>();
            actor.CollectResources(ResourceCollector);

            if (ResourceCollector.Count > 0)
            {
                FileSystem.BulkLoadAssets(ResourceCollector,
                    () => FinishSpawningActor(actor), null);
            }
            else
            {
                FinishSpawningActor(actor);
            }

            return actor as T;
        }

        public void FinishSpawningActor(Actor actor)
        {            
            actor.PostInitializeComponents();

            if (HasBegunPlay)
            {
                IEngine.Instance.GameThreadTickManager.AddTick(actor.PrimaryActorTick);
                actor.BeginPlay();
            }            
        }

        internal void DestroyActor(Actor actor)
        {
            IEngine.Instance.GameThreadTickManager.RemoveTick(actor.PrimaryActorTick);
            _AllActors.Remove(actor);
            actor.OnDestroy();                        
        }

        internal virtual void BeginLoadingWorld()
        {
            FileSystem.BulkLoadAssets(PreloadResources, () => { BeginPlay(); }, (ptr, s, t) => {
                Logger.Info("World Loading: {0} of {1}", s, t);
            });
          
        }

        internal virtual void BeginPlay()
        {
            HasBegunPlay = true;
            foreach(Actor actor in AllActors)
            {
                IEngine.Instance.GameThreadTickManager.AddTick(actor.PrimaryActorTick);
                actor.BeginPlay();
            }
        }

        internal virtual void UnloadWorld()
        {
            //Destroy all actors
            foreach(Actor actor in AllActors)
            {
                actor.Destroy();
            }
        }

        public virtual void PostScriptApplied()
        {
            
        }
    }
}

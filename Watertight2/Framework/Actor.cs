using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Filesystem;
using Watertight.Interfaces;
using Watertight.Scripts;
using Watertight.Tickable;
using System.Linq;

namespace Watertight.Framework
{
    public partial class Actor : IHasResources, IHasScript, ITransformable, INamed, IPostConstruct
    {
        internal protected TickFunction PrimaryActorTick = new TickFunction()
        {
            TickPriority = TickFunction.World,
            CanTick = true,
        };

        public World World
        {
            get;
            internal set;
        }
        public string Name
        {
            get;
            set;
        }

        public ObjectScript Script
        {
            get;
            set;
        }
        public Vector3 Location
        {
            get;
            set;
        } = Vector3.Zero;
        public Quaternion Rotation
        {
            get;
            set;
        } = Quaternion.Identity;
        public Vector3 Scale
        {
            get;
            set;
        } = Vector3.One;

       
        protected internal Actor()
        {
            PrimaryActorTick.TickFunc = Tick;
        }

        

        public virtual void BeginPlay()
        {

        }

        public virtual void Tick(float DeltaTime)
        {

        }

        public virtual void EndPlay()
        {

        }

        public virtual void OnDestroy()
        { 
            foreach(ActorComponent comp in AllComponents)
            {
                comp.OnDestroy();
            }
        }

        public void Destroy()
        {
            World.DestroyActor(this);
        }

        public virtual void CollectResources(IList<ResourcePtr> ResourceCollector)
        {
            foreach(IHasResources AC in AllComponents.Where(x => x is IHasResources))
            {
                AC.CollectResources(ResourceCollector);
            }
        }

        public void PostScriptApplied()
        {
                  
        }

        public Vector3 GetLocation_WorldSpace()
        {
            return Location;
        }

        public Vector3 GetLocation_Relative()
        {
            return Location;
        }

        public Quaternion GetRotation_WorldSpace()
        {
            return Rotation;
        }

        public Quaternion GetRotation_Relative()
        {
            return Rotation;
        }

        public Vector3 GetScale_WorldSpace()
        {
            return Scale;
        }

        public Vector3 GetScale_Relative()
        {
            return Scale;
        }

        public  virtual void PostConstruct()
        {
           
        }
    }
}

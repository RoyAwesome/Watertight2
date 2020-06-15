using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Interfaces;
using Watertight.Scripts;
using Watertight.Tickable;

namespace Watertight.Framework
{
    public class ActorComponent : IHasScript, INamed, IHasOwner<Actor>
    {
        public Actor Owner
        {
            get;
            protected internal set;
        }
        public ObjectScript Script 
        { 
            get; 
            set;
        }

        public bool Registered
        {
            get;
            set;
        }
        public string Name 
        { 
            get; 
            set; 
        }

        internal protected TickFunction PrimaryTick = new TickFunction()
        {
            TickPriority = TickFunction.World,
            CanTick = true,
        };

        internal protected ActorComponent()
        {
            PrimaryTick.TickFunc = OnTick;
        }

        public ActorComponent(Actor Owner)
            : this()
        {
            this.Owner = Owner;
        }

        public virtual void PostScriptApplied()
        {
            
        }

        public void Register()
        {
            Registered = true;
            Owner?.RegisterComponent_Internal(this);
           

            IEngine.Instance.GameThreadTickManager.AddTick(PrimaryTick);
            if (this is IRenderable)
            {
                IEngine.Instance.Renderer.AddRenderable(this as IRenderable);
            }

            OnRegister();
        }

        public void Destroy()
        {
            Owner.UnregisterComponent_Internal(this);

            Internal_Destroy();

            OnDestroy();
        }

        protected virtual void OnRegister()
        {

        }

        public virtual void OnTick(float DeltaTime)
        {

        }

        internal void Internal_Destroy()
        {
            IEngine.Instance.GameThreadTickManager.RemoveTick(PrimaryTick);
            if (this is IRenderable)
            {
                IEngine.Instance.Renderer.RemoveRenderable(this as IRenderable);
            }

            Registered = false;
        }

        public void OnDestroy()
        {
            
        }
    }
}

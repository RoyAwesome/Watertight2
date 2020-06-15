using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Watertight.Framework
{
    public class DuplicateComponentException : Exception
    {
        public DuplicateComponentException()
        {
        }

        public DuplicateComponentException(string message) : base(message)
        {
        }

        public DuplicateComponentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DuplicateComponentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }


    public partial class Actor
    {

        public IEnumerable<ActorComponent> AllComponents
        {
            get
            {
                return _AllComponents;
            }
        } 
        List<ActorComponent> _AllComponents = new List<ActorComponent>();

       

        public bool HasComponent(SubclassOf<ActorComponent> ComponentClass)
        {
            return GetComponentByClass(ComponentClass) != null;
        }

        public ActorComponent GetComponentByClass(SubclassOf<ActorComponent> Component)
        {
            return AllComponents.FirstOrDefault(x => x.GetType().IsAssignableFrom(Component));
        }

        public bool HasComponent(string Name)
        {
            return GetComponentByName(Name) != null;
        }

        public ActorComponent GetComponentByName(string Name)
        {
            return AllComponents.FirstOrDefault(x => x.Name == Name);
        }
               
        internal void RegisterComponent_Internal(ActorComponent Component)
        {
            if(Component == null)
            {
                throw new ArgumentNullException(nameof(Component));
            }
            if(HasComponent(Component.Name))
            {
                throw new DuplicateComponentException("Duplicate Name: " + Component.Name);
            }

            if(Component.Name == null)
            {
                string BaseName = Component.GetType().Name;
                if(Component.Script == null)
                {
                    BaseName = "NATIVE_" + BaseName;
                }                

                int CompCount = AllComponents.Count(x => x.Name.Remove(x.Name.LastIndexOf("_")) == BaseName );
                Component.Name = string.Format("{0}_{1}", BaseName, CompCount);
                
            }

            _AllComponents.Add(Component);
        }

        internal void UnregisterComponent_Internal(ActorComponent Component)
        {
            if (Component == null)
            {
                throw new ArgumentNullException(nameof(Component));
            }

            Component.Internal_Destroy();

            _AllComponents.Remove(Component);            
        }

        public virtual void PostInitializeComponents()
        {

        }
    }
}

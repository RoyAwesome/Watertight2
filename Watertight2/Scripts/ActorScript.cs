
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Filesystem;
using Watertight.Framework;
using Watertight.Util;

namespace Watertight.Scripts
{
    
    public class ActorScript : ObjectScript
    {
        const string ComponentEntryName = "$Components";        //$Components

        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
      
        public override void ApplyToObject(object obj)
        {
            if(!(obj is Actor))
            {
                throw new ArgumentException("obj must be an actor", nameof(obj));               
            }
            base.ApplyToObject(obj);
        }

        protected override void Internal_ApplyToObject(object obj)
        {           
            base.Internal_ApplyToObject(obj);

            Actor actor = obj as Actor;

            if (JObject.ContainsKey(ComponentEntryName) && JObject[ComponentEntryName] is JArray)
            {
                JArray ComponetArray = JObject[ComponentEntryName] as JArray;
                foreach (JObject jo in ComponetArray)
                {
                    ObjectScript compscript = new ObjectScript()
                    {
                        JObject = jo
                    };

                    Internal_AddComponent(actor, compscript); 
                }
            }
        }


        protected void Internal_AddComponent(Actor actor, ObjectScript compScript)
        {
            //Does the component already exist on the actor?  If it does, just apply this script. 
            string CompName = compScript.FindObjectName();
            if(CompName != null && actor.HasComponent(CompName))                
            {
                compScript.ApplyToObject(actor.GetComponentByName(CompName));
                return;                    
            }

            //Otherwise we create the Component
            ActorComponent AC = compScript.CreateInstance<ActorComponent>();
            AC.Owner = actor;
            AC.Register();
        }       
    }
}

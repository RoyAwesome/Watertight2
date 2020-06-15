using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Serialization;
using Watertight.Framework;

namespace Watertight.Scripts
{
    public class WorldScript : ObjectScript
    {
        //$LevelActors
        const string LevelActorName = "$" + nameof(LevelActors);
        IEnumerable<ActorScript> LevelActors
        {
            get
            {
                if(_LevelActors == null)
                {
                    if (JObject.ContainsKey(LevelActorName) && JObject[LevelActorName] is JArray)
                    {
                        _LevelActors = (JObject[LevelActorName] as JArray).Select(x =>
                        {
                            if(!(x is JObject))
                            {
                                throw new InvalidDataException(LevelActorName + " array must be all json objects in the format of actor script");
                            }
                            return new ActorScript
                            {
                                JObject = (JObject)x,
                            };
                        });
                    }
                }
                return _LevelActors;
            }
        }
        IEnumerable<ActorScript> _LevelActors;

        public override void ApplyToObject(object obj)
        {
            if (!(obj is World))
            {
                throw new ArgumentException("obj must be an World", nameof(obj));
            }


            base.ApplyToObject(obj);
        }

        protected override void Internal_ApplyToObject(object obj)
        {
            base.Internal_ApplyToObject(obj);

            //Create the actors in the world
            World world = obj as World;

            foreach(ActorScript AS in LevelActors)
            {
                Internal_CreateActorForWorld(world, AS);
            }
        }

        protected void Internal_CreateActorForWorld(World world, ActorScript AS)
        {
            //Does the Actor already exist the world?  If it does, just apply this script. 
            string ActorName = AS.FindObjectName();
            Actor Actor = world.AllActors.Where(x=> x.Name != null).FirstOrDefault(x => x.Name == ActorName);
            if (Actor != null)
            {
                AS.ApplyToObject(Actor);
                return;
            }

            //Otherwise we create the Actor
            world.CreateActor<Actor>(AS);
        }
        
    }
}

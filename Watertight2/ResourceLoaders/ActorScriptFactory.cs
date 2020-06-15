using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watertight.Filesystem;
using Watertight.Scripts;

namespace Watertight.ResourceLoaders
{
    public class ActorScriptFactory : ObjectScriptFactory
    {
        protected override SubclassOf<ObjectScript> ScriptType
        {
            get;
            set;
        } = typeof(ActorScript);

        public override string[] ResourceSchemes
        {
            get
            {
                return new string[]
                {
                    "ascript",
                    "actor"
                };
            }
        }

        public override IEnumerable<string> FileExtensions => new string[] { ".ascript", ".actor" };
                
    }
}

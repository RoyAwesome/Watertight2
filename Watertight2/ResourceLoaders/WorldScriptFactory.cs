using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Scripts;

namespace Watertight.ResourceLoaders
{
    public class WorldScriptFactory : ObjectScriptFactory
    {

        protected override SubclassOf<ObjectScript> ScriptType
        {
            get;
            set;
        } = typeof(WorldScript);

        public override string[] ResourceSchemes
        {
            get
            {
                return new string[]
                {
                    "wscript",
                    "world"
                };
            }
        }

        public override IEnumerable<string> FileExtensions => new string[] { ".wscript", ".world" };
    }
}

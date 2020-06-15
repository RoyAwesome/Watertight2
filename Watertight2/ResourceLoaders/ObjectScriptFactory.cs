using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watertight.Filesystem;
using Watertight.Scripts;

namespace Watertight.ResourceLoaders
{
    public class ObjectScriptFactory : ResourceFactory
    {
        protected virtual SubclassOf<ObjectScript> ScriptType
        {
            get;
            set;
        } = typeof(ObjectScript);

        public override string[] ResourceSchemes
        {
            get
            {
                return new string[]
                {
                    "oscript",
                    "object"
                };
            }
        }

        public override IEnumerable<string> FileExtensions => new string[] { ".oscript", ".object" };

        public override IEnumerable<Type> ResourceTypes => new Type[] { ScriptType };

        public override object GetResource(ResourcePtr Ptr, Stream stream)
        {
            using(TextReader Reader = new StreamReader(stream))
            using(JsonReader JReader = new JsonTextReader(Reader))
            {
                ObjectScript os = Activator.CreateInstance(ScriptType) as ObjectScript;
                os.JObject = JObject.Load(JReader);
                return os;
            }
            
        }
    }
}

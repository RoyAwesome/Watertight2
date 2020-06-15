using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Filesystem;
using Watertight.Interfaces;
using Watertight.Scripts.Parsers;

namespace Watertight.Rendering.Materials
{
    public class MaterialScript : IIsResource
    {
        private static JsonSerializer Serializer = new JsonSerializer();
        static MaterialScript()
        {
            Serializer.Converters.Add(new VectorParser());
        }

        public ResourcePtr ResourcePtr 
        { 
            get; 
            set; 
        }

        const string ParentScriptName = "$" + nameof(ParentScript);     //$ParentScript
        const string FragmentShaderName = "$" + nameof(FragmentShader);
        const string VertexShaderName = "$" + nameof(VertexShader);

        public JObject JObject
        {
            get;
            set;
        }

        public virtual MaterialScript ParentScript
        {
            get
            {
                if (_ParentScript == null)
                {
                    _ParentScript = JObject.SelectToken(ParentScriptName)?.ToObject<ResourcePtr>();
                    _ParentScript?.Load();
                }
                return _ParentScript?.Get<MaterialScript>();
            }
        }
        private ResourcePtr _ParentScript;

        public bool HasParentScript
        {
            get
            {
                return ParentScript != null;
            }
        }

        public virtual ResourcePtr FragmentShader
        {
            get
            {
                if (_FragmentShader == null)
                {
                    _FragmentShader = JObject.SelectToken(FragmentShaderName)?.ToObject<ResourcePtr>();
                    if(HasParentScript)
                    {
                        _FragmentShader = ParentScript.FragmentShader;
                    }
                }
                return _FragmentShader;
            }
        }
        private ResourcePtr _FragmentShader;

        public virtual ResourcePtr VertexShader
        {
            get
            {
                if (_VertexShader == null)
                {
                    _VertexShader = JObject.SelectToken(VertexShaderName)?.ToObject<ResourcePtr>();
                    if (HasParentScript)
                    {
                        _VertexShader = ParentScript.VertexShader;
                    }
                }
                return _VertexShader;
            }
        }
        private ResourcePtr _VertexShader;


        public virtual void ApplyToObject(object obj)
        {
            if (obj == null)
            {
                return;
            }

            Internal_ApplyToObject(obj);
        }

        protected virtual void Internal_ApplyToObject(object obj)
        {
            if (ParentScript != null)
            {
                ParentScript?.Internal_ApplyToObject(obj);
            }

            Type t = obj.GetType();

            foreach (var Property in t.GetProperties())
            {
                if (JObject.ContainsKey(Property.Name))
                {
                    object val = JObject.SelectToken(Property.Name).ToObject(Property.PropertyType, Serializer);
                    Property.SetValue(obj, val);
                }
            }           
        }

    }
}

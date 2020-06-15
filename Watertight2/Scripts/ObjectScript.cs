
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Watertight.Filesystem;
using Watertight.Interfaces;
using Watertight.Scripts.Parsers;
using Watertight.Util;

namespace Watertight.Scripts
{
    public class MissingRequiredScriptFieldException : Exception
    {
        public MissingRequiredScriptFieldException()
        {
        }

        public MissingRequiredScriptFieldException(string message) : base(message)
        {
        }

        public MissingRequiredScriptFieldException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ObjectScript : Interfaces.IIsResource
    {
        private static JsonSerializer Serializer = new JsonSerializer();
        static ObjectScript()
        {
            Serializer.Converters.Add(new VectorParser());
        }


        const string NativeClassName = "$" + nameof(NativeClass);       //$NativeClass
        const string ParentScriptName = "$" + nameof(ParentScript);     //$ParentScript
        const string ObjectNameName = "$" + nameof(ObjectName);         //$ObjectName


        public JObject JObject
        {
            get;
            set;
        }

        public virtual Type NativeClass
        {
            get
            {
                if (_NativeClass == null)
                {
                    string ClassName = JObject.Value<string>(NativeClassName);
                    _NativeClass = Utils.FindTypeFromString(ClassName);                    
                }
                return _NativeClass;
            }
        }
        private Type _NativeClass;

        public virtual ObjectScript ParentScript
        {
            get
            {
                if (_ParentScript == null)
                {
                    _ParentScript = JObject.SelectToken(ParentScriptName)?.ToObject<ResourcePtr>(); 
                    _ParentScript?.Load();
                }
                return _ParentScript?.Get<ObjectScript>();
            }
        }
        private ResourcePtr _ParentScript;

        public virtual string ObjectName
        {
            get
            {
                if(_ObjectName == null)
                {
                    _ObjectName = JObject.Value<string>(ObjectNameName);
                }
                return _ObjectName;
            }
        }
        private string _ObjectName;

        public ResourcePtr ResourcePtr
        {
            get;
            set;
        }

        

        public  bool HasParentScript
        {
            get
            {
                return ParentScript != null;
            }
        }

        public bool HasNativeClass
        {
            get
            {
                return NativeClass != null;
            }
        }        
        

        public virtual void ApplyToObject(object obj)
        {
            if(obj == null)
            {
                return;
            }

            Internal_ApplyToObject(obj);
                        
            if(obj is IHasScript)
            {
                (obj as IHasScript).Script = this;
                (obj as IHasScript).PostScriptApplied();
            }
        }

        

        protected virtual void Internal_ApplyToObject(object obj)
        {
            if(ParentScript != null)
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

            if(obj is INamed)
            {
                (obj as INamed).Name = ObjectName ?? (obj as INamed).Name;
            }
        }

        public Type FindNativeClass()
        {       
            ObjectScript SearchScript = this;
            Type FoundNativeClass = NativeClass;
            while (FoundNativeClass == null)
            {
                SearchScript = SearchScript.ParentScript;
                if (SearchScript == null)
                {
                    return null;
                }
                FoundNativeClass = SearchScript.NativeClass;
            }

            return FoundNativeClass;
        }
        
        public string FindObjectName()
        {          
            ObjectScript SearchScript = this;
            string FoundName = ObjectName;
            while (FoundName == null)
            {
                SearchScript = SearchScript.ParentScript;
                if(SearchScript == null)
                {
                    return null;
                }
                FoundName = SearchScript.ObjectName;
            }

            return FoundName;
        }

        public virtual object CreateInstance(Type NativeClass = null)
        {
            //If we have a parent script and a native class, instantiate the native class and apply all parent scripts.  
            //If we have a Parent Script and No Native Class, walk up the chain until we find a Native Class (if no class, throw exception)
            //If we have No Parent Script and a Native Class, create the native class and apply this script.
            //If we have Neither Parent Script or Native Class, throw an exception.

            //Find the parent native class

            Type FoundNativeClass = FindNativeClass() ?? NativeClass;

            if(FoundNativeClass == null)
            {
                throw new MissingRequiredScriptFieldException(string.Format("{0} Has no {1} in any search path", ResourcePtr.ToString(), NativeClassName));
            }
                

            object o = Activator.CreateInstance(FoundNativeClass);
            if(o is IPostConstruct)
            {
                (o as IPostConstruct).PostConstruct();
            }
            ApplyToObject(o);

            return o;
        }

        public virtual T CreateInstance<T>() where T : class
        {
            return CreateInstance(typeof(T)) as T;
        }
    }
}

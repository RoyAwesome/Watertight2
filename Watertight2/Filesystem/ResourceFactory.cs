using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace Watertight.Filesystem
{    
    public abstract class ResourceFactory
    {
        public abstract IEnumerable<Type> ResourceTypes
        {
            get;
        }

        public abstract string[] ResourceSchemes
        {
            get;
            
        }

        public virtual IEnumerable<string> FileExtensions
        {
            get
            {
                return null;
            }
        }

        public abstract object GetResource(ResourcePtr ptr, Stream stream);

        public virtual object GetResource(ResourcePtr Ptr)
        {
            object instance = null;

            using (Stream s = FileSystem.GetFileStream(Ptr))
            {
                instance = GetResource(Ptr, s);
            }                
            
            return instance;
        }
    }

    public class WatertightLoadingFailureException : System.Exception
    {
        public WatertightLoadingFailureException()
        {
        }

        public WatertightLoadingFailureException(string message) : base(message)
        {
        }

        public WatertightLoadingFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WatertightLoadingFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}

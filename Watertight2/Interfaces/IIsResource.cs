using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Filesystem;

namespace Watertight.Interfaces
{
    public interface IIsResource
    {
        public ResourcePtr ResourcePtr
        {
            get;
            set;
        }

    }
}

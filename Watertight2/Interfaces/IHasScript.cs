using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Scripts;

namespace Watertight.Interfaces
{
    public interface IHasScript
    {
        public ObjectScript Script
        {
            get;
            set;
        }

        public void PostScriptApplied();
    }
}

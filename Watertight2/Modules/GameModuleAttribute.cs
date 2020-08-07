using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.Modules
{
    public class GameModuleAttribute : Attribute
    {
        internal Type GameInstance;
        public GameModuleAttribute(Type GameInstance)
        {
            this.GameInstance = GameInstance;
        }
    }
}

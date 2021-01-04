using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.Modules
{

    public enum StartupPhase : byte
    {
        None = byte.MinValue,

        PreEngineInit,
        EngineInit,
        PostEngineInit,

        PreRendererInit,
        RendererInit,
        PostRendererInit,

        PreStart,
        Start,
        PostStart,

        LastChance,

        FullyLoaded = byte.MaxValue,
    };


    public abstract class Module
    {
        public virtual string ModuleName
        {
            get;
        }

        public virtual string ModuleVersion
        {
            get;
        }

        //TODO: Filesystem Information

        public string ModulePath
        {
            get;
            internal set;
        }

        public abstract void StartupModule(StartupPhase Phase);

        public abstract void ShutdownModule();
    }
}

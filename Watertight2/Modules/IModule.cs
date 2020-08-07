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


    public interface IModule
    {
        public string ModuleName
        {
            get;
        }

        public string ModuleVersion
        {
            get;
        }

        //TODO: Filesystem Information

        public void StartupModule(StartupPhase Phase);

        public void ShutdownModule();
    }
}

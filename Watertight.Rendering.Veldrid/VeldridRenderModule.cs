using NativeLibraryLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watertight.Modules;

namespace Watertight.Rendering.VeldridRendering
{
    [RenderModule(RenderModuleAttribute.RendererCapabilities.All, typeof(VeldridRenderer))]
    public class VeldridRenderModule : Module
    {
        public override string ModuleName => "Veldrid Renderer";

        public override string ModuleVersion => "1.0.0.0";

        public override void ShutdownModule()
        {
          
        }

        public override void StartupModule(StartupPhase Phase)
        {
            if(Phase == StartupPhase.PreEngineInit)
            {
                //Hack taken from the maintainer of this library
                //TODO: Determine the path for non win-64 builds
                new NativeLibrary(Path.Combine(Path.GetDirectoryName(ModulePath), "runtimes", "win-x64", "native", "SDL2.dll"));
                new NativeLibrary(Path.Combine(Path.GetDirectoryName(ModulePath), "runtimes", "win-x64", "native", "libveldrid-spirv.dll"));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Modules;

namespace Watertight.Rendering.VeldridRendering
{
    [RenderModule(RenderModuleAttribute.RendererCapabilities.All, typeof(VeldridRenderer))]
    public class VeldridRenderModule : IModule
    {
        public string ModuleName => "Veldrid Renderer";

        public string ModuleVersion => "1.0.0.0";

        public void ShutdownModule()
        {
          
        }

        public void StartupModule(StartupPhase Phase)
        {
            
        }
    }
}

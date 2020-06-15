using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Framework.Components;
using Watertight.Rendering.Interfaces;

namespace Watertight.Rendering.VeldridRendering.ResourceFactories
{
    class VeldridResourceFactory : IRendererResourceFactory
    {
        public ICamera CreateCamera(CameraComponent Owner)
        {
            return new VeldridCamera(Owner);
        }

        public RenderingCommand CreateRenderCommand()
        {
            return new VeldridRenderingCommand();
        }

        public IVertexBuffer CreateVertexBuffer()
        {
            return new VeldridVertexBuffer();
        }
    }
}

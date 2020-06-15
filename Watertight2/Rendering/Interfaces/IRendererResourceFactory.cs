using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Framework.Components;

namespace Watertight.Rendering.Interfaces
{
    public interface IRendererResourceFactory
    {
        public RenderingCommand CreateRenderCommand();
        public ICamera CreateCamera(CameraComponent Owner);

        public IVertexBuffer CreateVertexBuffer();
    }
}

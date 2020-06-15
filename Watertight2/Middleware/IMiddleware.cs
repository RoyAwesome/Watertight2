using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Rendering;

namespace Watertight.Middleware
{
    public interface IMiddleware
    {
        public void Initialize(IEngine EngineInstance);

        public void PostRenderInitialize(Renderer Renderer);
    }
}

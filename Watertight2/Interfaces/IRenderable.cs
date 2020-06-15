using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Rendering;

namespace Watertight.Interfaces
{
    public interface IRenderable
    {
        public void PreRender(Renderer renderer);

        public void Render(Renderer renderer);
    }
}

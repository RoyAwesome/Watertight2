using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Watertight.Rendering.Interfaces
{
    public interface ITexture
    {
        public Vector2 Size
        {
            get;
        }

        public void SetData(byte[] PixelData);
    }
}

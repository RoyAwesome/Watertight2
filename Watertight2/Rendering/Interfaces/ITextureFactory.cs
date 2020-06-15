using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;

namespace Watertight.Rendering.Interfaces
{
    public interface ITextureFactory
    {

        public ITexture Create(int width, int height);
        public ITexture Create(Vector2 Size);

        public ITexture Create(Stream InStream);

        public ITexture Create(Color[] Colors, Vector2 Size);
        public ITexture Create(byte[] Bytes, int BytePerPixel, Vector2 Size);

        public ITexture Create(byte[] Raw);
    }
}

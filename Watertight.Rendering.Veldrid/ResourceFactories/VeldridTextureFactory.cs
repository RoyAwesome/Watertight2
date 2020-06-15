using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;
using Watertight.Math;
using Watertight.Rendering.Interfaces;

namespace Watertight.Rendering.VeldridRendering.ResourceFactories
{
    class VeldridTextureFactory : ITextureFactory
    {
        public ITexture Create(int width, int height)
        {
            return Create(new Vector2(width, height));
        }

        public ITexture Create(Vector2 Size)
        {
            return new VeldridTexture(Size);
        }

        public ITexture Create(Stream InStream)
        {
            using (MemoryStream s = new MemoryStream())
            {
                InStream.CopyTo(s);
                return Create(s.ToArray());
            }
        }

        public ITexture Create(Color[] Colors, Vector2 Size)
        {
            return Create(MathConvert.ToRawBytes(Colors), 4, Size);
        }

        public ITexture Create(byte[] Bytes, int BytePerPixel, Vector2 Size)
        {
            return new VeldridTexture(Size, Bytes);
        }

        public ITexture Create(byte[] Raw)
        {
            ITexture Texture = new VeldridTexture(Raw);
            return Texture;
        }
    }
}

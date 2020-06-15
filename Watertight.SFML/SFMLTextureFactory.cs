using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;
using Watertight.Rendering.Interfaces;

namespace Watertight.SFML
{
    public class SFMLTextureFactory : ITextureFactory
    {
        public ITexture Create(int width, int height)
        {
            return new SFMLWTTexture((uint)width, (uint)height);
        }

        public ITexture Create(Vector2 Size)
        {
            return Create((int)Size.X, (int)Size.Y);
        }

        public ITexture Create(Stream InStream)
        {
            using (MemoryStream s = new MemoryStream())
            {
                InStream.CopyTo(s);
                return Create(s.ToArray());
            }
        }

        public ITexture Create(System.Drawing.Color[] Colors, Vector2 Size)
        {
            byte[] Buffer = new byte[(int)Size.X * (int)Size.Y * 4];
            
            for(int i  =0; i < Colors.Length; i++)
            {
                ArraySegment<byte> Arr = new ArraySegment<byte>(Buffer, i * 4, 4);
                Arr[0] = Colors[i].R;
                Arr[1] = Colors[i].G;
                Arr[2] = Colors[i].B;
                Arr[3] = Colors[i].A;
            }

            return Create(Buffer, 4, Size);
        }

        public ITexture Create(byte[] Bytes, int BytePerPixel, Vector2 Size)
        {
            ITexture texture = Create(Size);
            (texture as SFMLWTTexture).SetData(Bytes);
            return texture;
        }

        public ITexture Create(byte[] Raw)
        {
            return new SFMLWTTexture(Raw);
        }
    }
}

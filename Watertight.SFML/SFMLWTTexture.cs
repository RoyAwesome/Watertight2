using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Watertight.Rendering.Interfaces;

namespace Watertight.SFML
{
    internal class SFMLWTTexture : Texture, ITexture
    {
        public SFMLWTTexture(string filename) : base(filename)
        {
        }

        public SFMLWTTexture(Stream stream) : base(stream)
        {
        }

        public SFMLWTTexture(Image image) : base(image)
        {
        }

        public SFMLWTTexture(byte[] bytes) : base(bytes)
        {
        }

        public SFMLWTTexture(Texture copy) : base(copy)
        {
        }

        public SFMLWTTexture(uint width, uint height) : base(width, height)
        {
        }

        public SFMLWTTexture(string filename, IntRect area) : base(filename, area)
        {
        }

        public SFMLWTTexture(Stream stream, IntRect area) : base(stream, area)
        {
        }

        public SFMLWTTexture(Image image, IntRect area) : base(image, area)
        {
        }

        Vector2 ITexture.Size
        {
            get
            {
                return new Vector2(this.Size.X, this.Size.Y);
            }
        }

        public void SetData(byte[] PixelData)
        {
            Update(PixelData);
        }
    }
}

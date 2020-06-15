using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Watertight.Filesystem;
using SFML.Graphics;

namespace Watertight.SFML.ResourceLoaders
{
    class SpriteFactory : ResourceFactory
    {
        public override string[] ResourceSchemes
        {
            get
            {
                return new string[]
                {
                    "sprite",
                };
            }
        }

        public override IEnumerable<string> FileExtensions => new string[] { ".bmp", ".png", ".tga", ".jpg", ".gif", ".psd" , ".hdr", ".pic" };

        public override IEnumerable<Type> ResourceTypes => new Type[] { typeof(Sprite) };

        public override object GetResource(ResourcePtr Ptr, Stream stream)
        {
            using (MemoryStream s = new MemoryStream())
            {
                stream.CopyTo(s);
                return new Sprite(new Texture(s.ToArray()));
            }
        }
    }
}

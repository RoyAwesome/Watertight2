using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watertight.Filesystem;
using Watertight.Rendering.Interfaces;

namespace Watertight.ResourceLoaders.Rendering
{
    class TextureResourceFactory : ResourceFactory
    {
        public override string[] ResourceSchemes
        {
            get
            {
                return new string[]
                {
                    "texture",
                };
            }
        }

        public override IEnumerable<string> FileExtensions => new string[] { ".bmp", ".png", ".tga", ".jpg", ".gif", ".psd", ".hdr", ".pic" };

        public override IEnumerable<Type> ResourceTypes => new Type[] { typeof(ITexture) };

        public override object GetResource(ResourcePtr ptr, Stream stream)
        {
            return IEngine.Instance.Renderer.TextureFactory.Create(stream);
        }
    }
}

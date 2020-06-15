using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watertight.Filesystem;

namespace Watertight.SFML.ResourceLoaders
{
    class FontLoader : ResourceFactory
    {
        public override string[] ResourceSchemes
        {
            get
            {
                return new string[]
                    {
                        "font",
                    };

            }
        }


        public override IEnumerable<string> FileExtensions => new string[] { ".ttf", ".sfnt", ".fnt", ".bdf", ".PFR", ".otf", ".otc", ".ttc", ".cff" };

        public override IEnumerable<Type> ResourceTypes => new Type[] { typeof(Font) };

        public override object GetResource(ResourcePtr Ptr, Stream stream)
        {
            using (MemoryStream s = new MemoryStream())
            {
                stream.CopyTo(s);
                return new Font(s.ToArray());
            }
        }
    }
}

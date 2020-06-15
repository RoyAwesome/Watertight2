using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Watertight.Filesystem;

namespace TiledSharp.Loaders
{
    class TilesetResourceFactory : ResourceFactory
    {
        public override IEnumerable<Type> ResourceTypes => new Type[] { typeof(TmxTileset) };

        public override string[] ResourceSchemes => new string[] { "tileset" };

        public override IEnumerable<string> FileExtensions => new string[] { ".tsx" };

        public override object GetResource(ResourcePtr ptr, Stream stream)
        {
            using(XmlReader xml = XmlReader.Create(stream))
            {
                TmxTileset Tileset = new TmxTileset(XDocument.Load(xml).Element("tileset"), ptr.FolderPath);
                return Tileset;
            }
        }
    }
}

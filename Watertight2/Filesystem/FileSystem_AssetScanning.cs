using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Watertight.Filesystem
{
    public static partial class FileSystem
    {


        public static List<ResourcePtr> ScanForAssetType(string Scheme)
        {
            List<ResourcePtr> ResourcePtrs = new List<ResourcePtr>();

            //First, Get a list of file extensions we are looking for.
            IEnumerable<string> FileExts = Factories.Where(x => x.ResourceSchemes.Contains(Scheme))
                .SelectMany(x => x.FileExtensions);

            //Get the files from the search paths where we have a file extension
            IEnumerable<string> Paths =  pathOrder.SelectMany(x => x.Files(FileExts.ToArray()));
            foreach(string Path in Paths)
            {
                foreach(string Ext in FileExts)
                {
                    if(Path.EndsWith(Ext, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ResourcePtr ptr = new ResourcePtr(string.Format("{0}:{1}", Scheme, Path));
                        ResourcePtrs.Add(ptr);
                        break;                        
                    }

                }
            }

            return ResourcePtrs;
        }



    }
}

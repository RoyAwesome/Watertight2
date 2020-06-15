using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Watertight.Filesystem
{
    abstract class FileSystemPathFinder
    {

        public FileSystemPathFinder()
        {
            
        }

        protected abstract bool ExistsInPath(string Filename);
        public virtual bool ExistsInPath(ResourcePtr Ptr)
        {
            return ExistsInPath(Ptr.FilePath);
        }

        protected abstract Stream GetFileStream(string Filename);
        public virtual Stream GetFileStream(ResourcePtr Ptr)
        {
            return GetFileStream(Ptr.FilePath);
        }

        public abstract IEnumerable<string> Files(params string[] extensions);
    }

    
    internal class ModFileSearchPath : FileSystemPathFinder
    {
        public ModFileSearchPath()
        {

        }
               
        public override IEnumerable<string> Files(params string[] extensions)
        {
            throw new NotImplementedException();
        }

        protected override bool ExistsInPath(string Filename)
        {
            throw new NotImplementedException();
        }       

        protected override Stream GetFileStream(string Filename)
        {
            throw new NotImplementedException();
        }
    }
    

    internal class FileSystemSearchPath : FileSystemPathFinder
    {
        protected string Directory
        {
            get;
            set;
        }

        public FileSystemSearchPath(string folder)            
        {
            Directory = folder;
        }

        protected override bool ExistsInPath(string Filename)
        {
            return File.Exists(Path.Combine(Directory, Filename));
        }

        protected override Stream GetFileStream(string Filename)
        {
            return File.OpenRead(Path.Combine(Directory, Filename));
        }
            

        public override IEnumerable<string> Files(params string[] extensions)
        {
            IEnumerable<string> files = new string[] { };
            foreach(string ext in extensions)
            {
                files = files.Concat(System.IO.Directory.EnumerateFileSystemEntries(Directory, "*" + ext, SearchOption.AllDirectories)
                    .Select(x => x.Replace('\\', '/').Replace(Directory, "")));
            }

            return files;
        }
    }

    
}

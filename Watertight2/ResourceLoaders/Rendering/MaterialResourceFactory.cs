using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Watertight.Filesystem;
using Watertight.Rendering.Materials;
using Watertight.Util;

namespace Watertight.ResourceLoaders.Rendering
{
    class MaterialResourceFactory : ResourceFactory
    {
        public override string[] ResourceSchemes
        {
            get
            {
                return new string[]
                {
                    "mat",
                    "material",
                };
            }
        }

        public override IEnumerable<string> FileExtensions => new string[] { ".mat" };

        public override IEnumerable<Type> ResourceTypes => new Type[] { typeof(Material) };

        public override object GetResource(ResourcePtr ptr, Stream stream)
        {
            using (TextReader Reader = new StreamReader(stream))
            using (JsonReader JReader = new JsonTextReader(Reader))
            {
                MaterialScript ms = Activator.CreateInstance<MaterialScript>();
                ms.JObject = JObject.Load(JReader);

                //Validate that we have required shaders
                if(ms.FragmentShader == null)
                {
                    throw new WatertightLoadingFailureException(string.Format("Error loading material: {0}, missing $FragmentShader", ptr.ToString()));
                }
                if (ms.VertexShader == null)
                {
                    throw new WatertightLoadingFailureException(string.Format("Error loading material: {0}, missing $VertexShader", ptr.ToString()));
                }

                Material Mat = IEngine.Instance.Renderer.MaterialFactory.Create(ms);

                //Load the shaders
                Mat._Shaders[Shader.Stage.Fragment] = GetShader(ms.FragmentShader, Shader.Stage.Fragment);
                Mat._Shaders[Shader.Stage.Vertex] = GetShader(ms.VertexShader, Shader.Stage.Vertex);              

                Mat.PostLoad();

                return Mat;

            }          
        }

        private Shader GetShader(ResourcePtr ptr, Shader.Stage stage)
        {
            if(ptr.Loaded)
            {
                return ptr.Get<Shader>();
            }
            Shader s = IEngine.Instance.Renderer.MaterialFactory.CreateShader(stage, FileSystem.GetFileStream(ptr));
            s.ShaderFormat = Shader.SPIRV;
            FileSystem.EmplaceInMemoryResource(ptr, s);

            return s;
        }
    }
}

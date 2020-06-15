using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watertight.Rendering.Materials;
using Watertight.Util;

namespace Watertight.Rendering.VeldridRendering.ResourceFactories
{
    class VeldridMaterialFactory : IMaterialFactory
    {
        public Material Create(MaterialScript Script)
        {
            Material m = Create();
            Script?.ApplyToObject(m);

            return m;
        }

        public Material Create()
        {
            return new VeldridMaterial();
        }

        public Shader CreateShader(Shader.Stage ShaderStage)
        {
            return new VeldridShader(ShaderStage);
        }

        public Shader CreateShader(Shader.Stage ShaderStage, Stream InputStream)
        {
            Shader s = CreateShader(ShaderStage);
            s.Data = InputStream.ReadToEnd();
            return s;
        }
    }
}

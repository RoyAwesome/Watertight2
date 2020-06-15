using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watertight.Rendering.Materials;
using Watertight.Util;

namespace Watertight.VulkanRenderer.ResourceFactories
{
    class VulkanMaterialFactory : IMaterialFactory
    {
        public Material Create(MaterialScript Script)
        {
            Material m = Create();
            Script?.ApplyToObject(m);

            return m;
        }

        public Material Create()
        {
            return new VulkanMaterial();
        }

        public Shader CreateShader(Shader.Stage ShaderStage)
        {
            return new VulkanShader(ShaderStage);
        }

        public Shader CreateShader(Shader.Stage ShaderStage, Stream InputStream)
        {
            Shader s = CreateShader(ShaderStage);
            s.Data = InputStream.ReadToEnd();

            return s;
        }
    }
}

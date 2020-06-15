using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Watertight.Rendering.Materials
{
    public interface IMaterialFactory
    {
        public Material Create(MaterialScript Script);
        public Material Create();

        public Shader CreateShader(Shader.Stage ShaderStage);
        public Shader CreateShader(Shader.Stage ShaderStage, Stream InputStream);
    }
}

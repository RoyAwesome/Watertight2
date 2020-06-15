using System;
using System.Collections.Generic;
using System.Text;
using Veldrid.SPIRV;
using Watertight.Rendering.Materials;

namespace Watertight.Rendering.VeldridRendering
{
    class VeldridShader : Shader
    {
        static VeldridRenderer Renderer => IEngine.Instance.Renderer as VeldridRenderer;

        public Veldrid.Shader VelShader
        {
            get;
            private set;
        }


        Veldrid.ShaderStages VeldridStage
        {
            get => ShaderStage switch
            {
                Stage.Vertex => Veldrid.ShaderStages.Vertex,
                Stage.Fragment => Veldrid.ShaderStages.Fragment,
                Stage.Geometry => Veldrid.ShaderStages.Geometry,
                Stage.TessellationControl => Veldrid.ShaderStages.TessellationControl,
                Stage.TessellationEvaluation => Veldrid.ShaderStages.TessellationEvaluation,
                _ => Veldrid.ShaderStages.None,
            };
        }

        Veldrid.ShaderDescription ShaderDescription => new Veldrid.ShaderDescription(VeldridStage, Data, "main");

        public VeldridShader(Stage Stage) : base(Stage)
        {
        }

        public void Bind()
        {
            if(VelShader == null)
            {
                if (!HasSpirVHeader())
                {
                    SpirvCompilationResult glslcompile = SpirvCompilation.CompileGlslToSpirv(
                            Encoding.UTF8.GetString(Data),
                            "Internal Compile",
                            VeldridStage,
                            new GlslCompileOptions()
                        );
                    if (glslcompile.SpirvBytes == null)
                    {
                        throw new Exception("Shader Compile Failed!");
                    }
                    Data = glslcompile.SpirvBytes;

                    ShaderFormat = SPIRV;
                }

                VelShader = Renderer.VeldridFactory.CreateShader(ShaderDescription);
            }
        }
          
    }
}

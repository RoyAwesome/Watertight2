using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Rendering.Materials;

namespace Watertight.VulkanRenderer
{
    public class VulkanShader : Shader
    {
        public override string ShaderFormat
        {
            get => base.ShaderFormat;
            set
            {
                if (value != SPIRV)
                {
                    throw new NotSupportedException("Only SPIR-V Shaders can be loaded by the Vulkan Renderer.  TODO: Compile Shaders to SPIR-V");
                }
                base.ShaderFormat = value;
            }

        }

        public VulkanShader(Stage Stage) 
            : base(Stage)
        {
            ShaderFormat = Shader.SPIRV;
        }

        public Vulkan.ShaderStageFlags ShaderStageFlag
        {
            get
            {
                return ShaderStage switch
                { 
                    Stage.Fragment => Vulkan.ShaderStageFlags.Fragment,
                    Stage.Vertex => Vulkan.ShaderStageFlags.Vertex,
                    Stage.Geometry => Vulkan.ShaderStageFlags.Geometry,
                    Stage.TessellationEvaluation => Vulkan.ShaderStageFlags.TessellationEvaluation,
                    Stage.TessellationControl => Vulkan.ShaderStageFlags.TessellationControl,
                    _ => (Vulkan.ShaderStageFlags)0,                
                };

            }
        }

        public Vulkan.ShaderModule ShaderModule
        { 
            get
            {
                if (_ShaderModule == null)
                {
                    _ShaderModule = BindShader();
                }
                return _ShaderModule;
            }
            
        }
        Vulkan.ShaderModule _ShaderModule;

        public Vulkan.PipelineShaderStageCreateInfo PipelineShaderCreateInfo
        {
            get
            {
                if(_PipelineShaderCreateInfo == null)
                {
                    _PipelineShaderCreateInfo = new Vulkan.PipelineShaderStageCreateInfo
                    {
                        Module = ShaderModule,
                        Name = "main",
                        Stage = ShaderStageFlag
                    };
                }
                return _PipelineShaderCreateInfo;
            }
        }
        Vulkan.PipelineShaderStageCreateInfo _PipelineShaderCreateInfo;

        private Vulkan.ShaderModule BindShader()
        {
            Vulkan.ShaderModuleCreateInfo smci = new Vulkan.ShaderModuleCreateInfo()
            {
                CodeBytes = Data,
            };

            return VulkanRenderer.Instance.Device.CreateShaderModule(smci);
        }        
    }
}

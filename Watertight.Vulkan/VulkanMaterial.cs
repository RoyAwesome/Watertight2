using System.Linq;
using Watertight.Filesystem;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;

namespace Watertight.VulkanRenderer
{
    internal class VulkanMaterial : Material
    {
        public static ResourcePtr DefaultShader_Fragment = new ResourcePtr("shader:DefaultShader_Fragment");
        public static ResourcePtr DefaultShader_Vertex = new ResourcePtr("shader:DefaultShader_Vertex");


        public Vulkan.PolygonMode VKFillMode
        { 
            get
            {
                return this.Fill switch
                {
                    FillMode.Fill => Vulkan.PolygonMode.Fill,
                    FillMode.Line => Vulkan.PolygonMode.Line,
                    FillMode.Points => Vulkan.PolygonMode.Point,
                    _ => Vulkan.PolygonMode.Fill,
                };
            }
        }

        public Vulkan.GraphicsPipelineCreateInfo PipelineCreateInfo
        {
            get => new Vulkan.GraphicsPipelineCreateInfo
            {
                Stages = ShaderStageInfos,
                VertexInputState = VertexInputStateCreateInfo,
                InputAssemblyState = PipelineInputAssemblyStateCreateInfo,
                ViewportState = Renderer.PipelineViewportStateCreateInfo,
                RasterizationState = RasterizerCreateInfo,
                MultisampleState = MultisampleStateCreateInfo,
                DepthStencilState = null,
                ColorBlendState = PipelineColorBlendStateCreateInfo,
                Layout = PipelineLayout,
                RenderPass = RenderPass.vkRenderPass,
                Subpass = 0,
            };
        }

        Vulkan.DescriptorSetLayoutBinding MVPLayoutBinding
        {
            get
            {
                return new Vulkan.DescriptorSetLayoutBinding
                {
                    Binding = 0,
                    DescriptorType = Vulkan.DescriptorType.UniformBuffer,
                    DescriptorCount = 1,
                    StageFlags = Vulkan.ShaderStageFlags.Vertex,                   
                };
            }
        }

        Vulkan.DescriptorSetLayoutBinding TextureLayoutBinding
        {
            get => new Vulkan.DescriptorSetLayoutBinding
            {
                Binding = 1,
                DescriptorCount = 1,
                DescriptorType = Vulkan.DescriptorType.CombinedImageSampler,
                StageFlags = Vulkan.ShaderStageFlags.Fragment,
            };
        }

        private Vulkan.Device Device
        { 
            get
            {
                return VulkanRenderer.Instance.Device;
            }
        }

        public VulkanRenderer Renderer
        {
            get => VulkanRenderer.Instance;
        }


        public Vulkan.DescriptorSetLayout MVPDescriptorSetLayout
        {
            get
            {
                if(_MVPDescriptorSetLayout == null)
                {
                    _MVPDescriptorSetLayout = CreateDescriptorSetLayouts(MVPLayoutBinding, TextureLayoutBinding);
                }
                return _MVPDescriptorSetLayout;
            }

        }
        Vulkan.DescriptorSetLayout _MVPDescriptorSetLayout;
        
        public virtual Vulkan.PipelineShaderStageCreateInfo[] ShaderStageInfos
        {
            get => Shaders.Values.Select(x => (x as VulkanShader).PipelineShaderCreateInfo).ToArray();
        }

        public virtual Vulkan.PipelineVertexInputStateCreateInfo VertexInputStateCreateInfo
        {
            get => new Vulkan.PipelineVertexInputStateCreateInfo()
            {

            };
        }

        public virtual Vulkan.PipelineInputAssemblyStateCreateInfo PipelineInputAssemblyStateCreateInfo
        {
            get => new Vulkan.PipelineInputAssemblyStateCreateInfo
            {
                PrimitiveRestartEnable = false,
                Topology = Vulkan.PrimitiveTopology.TriangleList,
            };
        }

        public virtual Vulkan.PipelineRasterizationStateCreateInfo RasterizerCreateInfo
        {
            get => new Vulkan.PipelineRasterizationStateCreateInfo
            {
                DepthClampEnable = false,
                PolygonMode = VKFillMode,
                LineWidth = 1.0f,
                CullMode = Vulkan.CullModeFlags.None,
                FrontFace = Vulkan.FrontFace.Clockwise,
                DepthBiasEnable = false,
            };
        }

        public virtual Vulkan.PipelineMultisampleStateCreateInfo MultisampleStateCreateInfo
        {
            get => new Vulkan.PipelineMultisampleStateCreateInfo
            {
                //SampleShadingEnable = false,
                RasterizationSamples = Vulkan.SampleCountFlags.Count1,
                MinSampleShading = 1.0f,
            };
        }

        public virtual Vulkan.PipelineColorBlendAttachmentState PipelineColorBlend
        {
            get => AlphaBlending;
        }
        

        public virtual Vulkan.PipelineColorBlendStateCreateInfo PipelineColorBlendStateCreateInfo
        {
            get => new Vulkan.PipelineColorBlendStateCreateInfo
            {
                LogicOpEnable = false,
                AttachmentCount = 1,
                Attachments = new Vulkan.PipelineColorBlendAttachmentState[] { PipelineColorBlend },
            };
        }

        public virtual Vulkan.DescriptorSetLayout[] DescriptorSetLayouts
        {
            get => new Vulkan.DescriptorSetLayout[] { MVPDescriptorSetLayout };
        }

        public virtual VulkanRenderPass RenderPass
        {
            get => Renderer.MainRenderPass;
        }

        public virtual Vulkan.PipelineLayout PipelineLayout
        {
            get
            {
                if(_PipelineLayoutCreateInfo == null)
                {
                    Vulkan.PipelineLayoutCreateInfo plci = new Vulkan.PipelineLayoutCreateInfo
                    {
                        SetLayouts = DescriptorSetLayouts,
                    };

                    _PipelineLayoutCreateInfo = Device.CreatePipelineLayout(plci);
                }

                return _PipelineLayoutCreateInfo;
            }
        }
        Vulkan.PipelineLayout _PipelineLayoutCreateInfo;

        public virtual bool HasUniformBuffers
        {
            get;
        } = true;
        

        public VulkanMaterial()
        {
            SetShader(DefaultShader_Fragment.Get<Shader>());
            SetShader(DefaultShader_Vertex.Get<Shader>());
        }

        internal void SetShader(Shader shader)
        {            
            _Shaders[shader.ShaderStage] = shader;
        }

        public override void PostLoad()
        {   
           
            Vulkan.DynamicState[] dynamicStates =
            {
                Vulkan.DynamicState.Viewport,
                Vulkan.DynamicState.LineWidth,
            };

            Vulkan.PipelineDynamicStateCreateInfo pdsci = new Vulkan.PipelineDynamicStateCreateInfo
            {
                DynamicStateCount = 2,
                DynamicStates = dynamicStates,
            };

          
            
            if(HasUniformBuffers)
            {
                CreateUniformBuffers(VulkanRenderer.Instance.SwapChainImageCount);
                CreateDescriptorPool();
                CreateDescriptorSets((uint)VulkanRenderer.Instance.SwapChainImageCount);
            }
            
        }
               
        public static Vulkan.PipelineColorBlendAttachmentState AlphaBlending
        {           
            get => new Vulkan.PipelineColorBlendAttachmentState
            {
                BlendEnable = true,
                SrcColorBlendFactor = Vulkan.BlendFactor.SrcAlpha,
                DstColorBlendFactor = Vulkan.BlendFactor.OneMinusSrcAlpha,
                ColorBlendOp = Vulkan.BlendOp.Add,
                SrcAlphaBlendFactor = Vulkan.BlendFactor.One,
                DstAlphaBlendFactor = Vulkan.BlendFactor.Zero,
                AlphaBlendOp = Vulkan.BlendOp.Add,

                ColorWriteMask = Vulkan.ColorComponentFlags.R | Vulkan.ColorComponentFlags.G | Vulkan.ColorComponentFlags.B | Vulkan.ColorComponentFlags.A,
            };
        }

        public static Vulkan.PipelineColorBlendAttachmentState NoBlending
        {
            get => new Vulkan.PipelineColorBlendAttachmentState
            {
                SrcColorBlendFactor = Vulkan.BlendFactor.One,
                DstColorBlendFactor = Vulkan.BlendFactor.Zero,
                ColorBlendOp = Vulkan.BlendOp.Add,

                SrcAlphaBlendFactor = Vulkan.BlendFactor.One,
                DstAlphaBlendFactor = Vulkan.BlendFactor.Zero,
                AlphaBlendOp = Vulkan.BlendOp.Add,

                ColorWriteMask = Vulkan.ColorComponentFlags.R | Vulkan.ColorComponentFlags.G | Vulkan.ColorComponentFlags.B | Vulkan.ColorComponentFlags.A,
            };

        }

        private Vulkan.DescriptorSetLayout CreateDescriptorSetLayouts(params Vulkan.DescriptorSetLayoutBinding[] Bindings)
        {
            Vulkan.DescriptorSetLayoutCreateInfo createInfo = new Vulkan.DescriptorSetLayoutCreateInfo
            {
                Bindings = Bindings,
            };

            return Device.CreateDescriptorSetLayout(createInfo);
        }

        public VulkanGPUBuffer[] UniformBuffers;

        public void CreateUniformBuffers(int swapchainImages)
        {
            UniformBuffers = new VulkanGPUBuffer[swapchainImages];
            for(int i = 0; i < swapchainImages; i++)
            {
                UniformBuffers[i] = new VulkanGPUBuffer(Vulkan.BufferUsageFlags.UniformBuffer);
                UniformBuffers[i].CreateBuffer(200); //TODO: Figure out how much space we actually need
                UniformBuffers[i].AllocateMemory();
            }
        }

        Vulkan.DescriptorPool descriptorPool;
        public void CreateDescriptorPool()
        {
            Vulkan.DescriptorPoolSize poolSize = new Vulkan.DescriptorPoolSize
            {
                Type = Vulkan.DescriptorType.UniformBuffer,
                DescriptorCount = (uint)Renderer.SwapChainImageCount,
            };

            Vulkan.DescriptorPoolCreateInfo poolCreateInfo = new Vulkan.DescriptorPoolCreateInfo
            {
                PoolSizes = new Vulkan.DescriptorPoolSize[] 
                { 
                    poolSize,
                    new Vulkan.DescriptorPoolSize
                    {
                        Type = Vulkan.DescriptorType.CombinedImageSampler,
                        DescriptorCount = (uint)Renderer.SwapChainImageCount,
                    },                
                },
                MaxSets = (uint)Renderer.SwapChainImageCount,
            };

            descriptorPool = Device.CreateDescriptorPool(poolCreateInfo);
        }

        public Vulkan.DescriptorSet[] descriptorSet;
        private void CreateDescriptorSets(uint SwapChainImages)
        {
            
            Vulkan.DescriptorSetLayout[] layouts = new Vulkan.DescriptorSetLayout[SwapChainImages];
            for (int i = 0; i < SwapChainImages; i++)
            {
                layouts[i] = MVPDescriptorSetLayout;
            }

            Vulkan.DescriptorSetAllocateInfo allocInfo = new Vulkan.DescriptorSetAllocateInfo
            {
                DescriptorPool = descriptorPool,
                DescriptorSetCount = SwapChainImages,
                SetLayouts = layouts,
            };

            descriptorSet = Device.AllocateDescriptorSets(allocInfo);

            for(int i = 0; i < SwapChainImages; i++)
            {
                Vulkan.WriteDescriptorSet[] writeDescriptorSets = new Vulkan.WriteDescriptorSet[2];

                {
                    Vulkan.DescriptorBufferInfo bufferInfo = new Vulkan.DescriptorBufferInfo
                    {
                        Buffer = UniformBuffers[i].Buffer,
                        Offset = UniformBuffers[i].Offset,
                        Range = UniformBuffers[i].Size,
                    };

                    writeDescriptorSets[0] = new Vulkan.WriteDescriptorSet
                    {
                        DstSet = descriptorSet[i],
                        DstBinding = 0,
                        DstArrayElement = 0,
                        DescriptorType = Vulkan.DescriptorType.UniformBuffer,
                        DescriptorCount = 1,
                        BufferInfo = new Vulkan.DescriptorBufferInfo[] { bufferInfo },
                    };
                }
                
                {
                    ITexture Texture = Rendering.Renderer.DefaultTexturePtr.Get<ITexture>();
                    Vulkan.DescriptorImageInfo descriptorImageInfo = new Vulkan.DescriptorImageInfo()
                    {
                        ImageView = (Texture as VulkanTexture).vkImageView,
                        Sampler = VulkanTexture.vkSampler,
                        ImageLayout = Vulkan.ImageLayout.ShaderReadOnlyOptimal,
                    };

                    writeDescriptorSets[1] = new Vulkan.WriteDescriptorSet
                    {
                        DstSet = descriptorSet[i],
                        DstBinding = 1,
                        DstArrayElement = 0,
                        DescriptorType = Vulkan.DescriptorType.CombinedImageSampler,
                        DescriptorCount = 1,
                        ImageInfo = new Vulkan.DescriptorImageInfo[] { descriptorImageInfo },
                    };
                }


                //Device.UpdateDescriptorSet(writeDescriptorSets[0], null);
                Device.UpdateDescriptorSets(writeDescriptorSets, null);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watertight.Math;
using Watertight.Rendering.Materials;

namespace Watertight.Rendering.VeldridRendering
{
    class VeldridMaterial : Material
    {
        static VeldridRenderer Renderer => IEngine.Instance.Renderer as VeldridRenderer;

        public Veldrid.PrimitiveTopology VeldridTopology
        {
            get => Topology switch
            {
                RenderTopology.Line_List => Veldrid.PrimitiveTopology.LineList,
                RenderTopology.Line_Strip => Veldrid.PrimitiveTopology.LineStrip,
                RenderTopology.Triangle_List => Veldrid.PrimitiveTopology.TriangleList,
                RenderTopology.Triangle_Strip => Veldrid.PrimitiveTopology.TriangleStrip,
                RenderTopology.Point_List => Veldrid.PrimitiveTopology.PointList,
                _ => throw new NotSupportedException("Topology " + Topology.ToString() + " Not supported by Veldrid"),
            };
        }


        public Veldrid.Pipeline Pipeline;

        public override RenderTopology Topology 
        { 
            get => base.Topology; 
            set
            {
                base.Topology = value;
                if(_TexturedPipeline != null)
                {
                    _TexturedPipeline.Dispose();
                    _TexturedPipeline = null;
                }
                if(_UntexturedPipeline != null)
                {
                    _UntexturedPipeline.Dispose();
                    _UntexturedPipeline = null;
                }
            }
        }

        static Veldrid.DepthStencilStateDescription DepthModeEnable
        {
            get => new Veldrid.DepthStencilStateDescription
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = Veldrid.ComparisonKind.LessEqual
            };
        }

        static Veldrid.DepthStencilStateDescription DepthModeDisable
        {
            get => new Veldrid.DepthStencilStateDescription
            {
                DepthTestEnabled = false,
                DepthWriteEnabled = false,
                DepthComparison = Veldrid.ComparisonKind.Always
            };
        }

        Veldrid.DepthStencilStateDescription DepthMode
        {
            get => DepthTest ? DepthModeEnable : DepthModeDisable;
        }

        private Veldrid.Pipeline _TexturedPipeline;
        public Veldrid.Pipeline TexturedPipeline
        {
            get
            {
                if(_TexturedPipeline == null)
                {
                    Veldrid.GraphicsPipelineDescription pipelineDesc = new Veldrid.GraphicsPipelineDescription()
                    {
                        BlendState = Veldrid.BlendStateDescription.SingleAlphaBlend,
                        DepthStencilState = DepthMode,
                        RasterizerState = new Veldrid.RasterizerStateDescription()
                        {
                            CullMode = Veldrid.FaceCullMode.None,
                            FillMode = Veldrid.PolygonFillMode.Solid,
                            FrontFace = Veldrid.FrontFace.CounterClockwise,
                            DepthClipEnabled = false,
                            ScissorTestEnabled = false,
                        },
                        PrimitiveTopology = VeldridTopology,
                        ShaderSet = new Veldrid.ShaderSetDescription
                        {
                            VertexLayouts = new Veldrid.VertexLayoutDescription[] { VeldridVertexBuffer.VertexLayoutDescription },
                            Shaders = Shaders.Values.Select(x => (x as VeldridShader).VelShader).ToArray(),
                            Specializations = new Veldrid.SpecializationConstant[]
                            {
                                new Veldrid.SpecializationConstant(0, Renderer.GraphicsDevice.IsClipSpaceYInverted),
                            }
                        },
                        Outputs = Renderer.GraphicsDevice.SwapchainFramebuffer.OutputDescription,
                        ResourceLayouts = new Veldrid.ResourceLayout[] { VeldridCamera.ProjectionViewResourceLayout, VeldridRenderingCommand.ModelResourceLayout, VeldridTexture.ResourceLayout },
                    };

                    _TexturedPipeline = Renderer.VeldridFactory.CreateGraphicsPipeline(pipelineDesc);
                }
                return _TexturedPipeline;
            }
        }

        private Veldrid.Pipeline _UntexturedPipeline;
        public Veldrid.Pipeline UntexturedPipeline
        {
            get
            {
                if (_UntexturedPipeline == null)
                {
                    Veldrid.GraphicsPipelineDescription pipelineDesc = new Veldrid.GraphicsPipelineDescription()
                    {
                        BlendState = Veldrid.BlendStateDescription.SingleAlphaBlend,
                        DepthStencilState = DepthMode,
                        RasterizerState = new Veldrid.RasterizerStateDescription()
                        {
                            CullMode = Veldrid.FaceCullMode.None,
                            FillMode = Veldrid.PolygonFillMode.Solid,
                            FrontFace = Veldrid.FrontFace.CounterClockwise,
                            DepthClipEnabled = false,
                            ScissorTestEnabled = false,
                        },
                        PrimitiveTopology = VeldridTopology,
                        ShaderSet = new Veldrid.ShaderSetDescription
                        {
                            VertexLayouts = new Veldrid.VertexLayoutDescription[] { VeldridVertexBuffer.VertexLayoutDescription },
                            Shaders = new Veldrid.Shader[]
                            {
                                (Shaders[Shader.Stage.Vertex] as VeldridShader).VelShader,
                                (VeldridRenderer.DefaultShaderUntextured_Fragment.Get<Shader>() as VeldridShader).VelShader,
                            },                            
                            Specializations = new Veldrid.SpecializationConstant[]
                            {
                                new Veldrid.SpecializationConstant(0, Renderer.GraphicsDevice.IsClipSpaceYInverted),
                            }
                        },
                        Outputs = Renderer.GraphicsDevice.SwapchainFramebuffer.OutputDescription,
                        ResourceLayouts = new Veldrid.ResourceLayout[] { VeldridCamera.ProjectionViewResourceLayout, VeldridRenderingCommand.ModelResourceLayout },
                    };

                    _UntexturedPipeline = Renderer.VeldridFactory.CreateGraphicsPipeline(pipelineDesc);
                }
                return _UntexturedPipeline;
            }
        }

        public VeldridMaterial()
        {
            SetShader(VeldridRenderer.DefaultShader_Fragment.Get<Shader>());
            SetShader(VeldridRenderer.DefaultShader_Vertex.Get<Shader>());
        }

        internal void SetShader(Shader shader)
        {
            _Shaders[shader.ShaderStage] = shader;
        }

        public override void PostLoad()
        {
            foreach(Shader s in Shaders.Values)
            {
                (s as VeldridShader).Bind();
            }

        }     
    }
}

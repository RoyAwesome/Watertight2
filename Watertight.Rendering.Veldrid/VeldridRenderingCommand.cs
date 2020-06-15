using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using Watertight.Math;
using Watertight.Util;

namespace Watertight.Rendering.VeldridRendering
{
    class VeldridRenderingCommand : RenderingCommand
    {
        static VeldridRenderer Renderer => IEngine.Instance.Renderer as VeldridRenderer;

        private Veldrid.CommandList CommandList;

        ~VeldridRenderingCommand()
        {
            ModelBuffer?.Dispose();
            CommandList?.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();

            Renderer.GraphicsDevice.WaitForIdle();
            CommandList?.Dispose();
            ModelBuffer?.Dispose();
            ModelResourceSet?.Dispose();
        }

        public Veldrid.CommandList BuildStandardCommandList()
        {
            if(CommandList == null)
            {
                CommandList = Renderer.VeldridFactory.CreateCommandList();
            }

            CommandList.Begin();
            CommandList.SetFramebuffer(Renderer.GraphicsDevice.SwapchainFramebuffer);

            if(ClearColor.HasValue)
            {
                float[] Floats = ClearColor.Value.GetFloats();
                CommandList.ClearColorTarget(0, new RgbaFloat(Floats[0], Floats[1], Floats[2], Floats[3]));
            }

            if (VertexBuffer != null)
            { 
                if(!VertexBuffer.Bound)
                {
                    VertexBuffer.Bind();
                }
                VeldridCamera vCam = Camera as VeldridCamera;
                vCam.Update();

                UpdateModelBuffer();

                VeldridTexture texture = Texture as VeldridTexture;

                CommandList.SetVertexBuffer(0, (VertexBuffer as VeldridVertexBuffer).VertexBuffer);
                CommandList.SetIndexBuffer((VertexBuffer as VeldridVertexBuffer).IndexBuffer, IndexFormat.UInt16);
                if(texture != null)
                {
                    CommandList.SetPipeline((Material as VeldridMaterial).TexturedPipeline);
                }
                else
                {
                    CommandList.SetPipeline((Material as VeldridMaterial).UntexturedPipeline);
                }
                CommandList.SetGraphicsResourceSet(0, vCam.ProjectionViewResourceSet);
                CommandList.SetGraphicsResourceSet(1, ModelResourceSet);

                if (texture != null)
                {
                    CommandList.SetGraphicsResourceSet(2, texture.GetTextureResourceSet());
                }

                CommandList.DrawIndexed((uint)NumIndicies, 1, (uint)StartIndex, 0, 0);
                
            }

            CommandList.End();

            return CommandList;
        }

        Veldrid.DeviceBuffer ModelBuffer;
        Veldrid.ResourceSet ModelResourceSet;

        private void CreateModelBuffer()
        {
            ModelBuffer = Renderer.VeldridFactory.CreateBuffer(new BufferDescription(sizeof(float) * 16, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            ModelResourceSet = Renderer.VeldridFactory.CreateResourceSet(new ResourceSetDescription(ModelResourceLayout, ModelBuffer));
        }
        private void UpdateModelBuffer()
        {
            if(ModelBuffer == null)
            {
                CreateModelBuffer();
            }
            Renderer.GraphicsDevice.UpdateBuffer(ModelBuffer, 0, Transform.ToBytes());
        }

        private static Veldrid.ResourceLayoutDescription ModelTransformLayout = new Veldrid.ResourceLayoutDescription(
                new Veldrid.ResourceLayoutElementDescription("Model", Veldrid.ResourceKind.UniformBuffer, Veldrid.ShaderStages.Vertex));
       

        public static Veldrid.ResourceLayout ModelResourceLayout
        {
            get
            {
                if (_ModelResourceLayout == null)
                {
                    _ModelResourceLayout = Renderer.VeldridFactory.CreateResourceLayout(ModelTransformLayout);
                }
                return _ModelResourceLayout;
            }
        }
        static Veldrid.ResourceLayout _ModelResourceLayout;
    }
}

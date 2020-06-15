using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Filesystem;
using Watertight.Interfaces;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;

namespace Watertight.Framework.Components.Rendering
{
    public class CustomMeshComponent : SceneComponent, IRenderable
    {
        public IVertexBuffer SimpleVertexBuffer
        {
            get;
            set;
        }

        public ResourcePtr Texture
        {
            get;
            set;
        }

        public CustomMeshComponent()
        {
        }

        public CustomMeshComponent(Actor Owner) : base(Owner)
        {
        }

        public virtual void PreRender(Renderer renderer)
        {
            if(SimpleVertexBuffer == null)
            {
                return;
            }
            RenderingCommand cmd = CreateRenderingCommand(renderer);

            renderer.EnqueueRenderCommand(cmd);
        }

        private RenderingCommand cmd;
        protected virtual RenderingCommand CreateRenderingCommand(Renderer renderer)
        {
            if (cmd == null)
            {
                cmd = renderer.RendererResourceFactory.CreateRenderCommand();
            }
            cmd.WithVertexBuffer(SimpleVertexBuffer)
                .WithTexture(Texture.Get<ITexture>())
                .WithTransform(GetLocation_WorldSpace(), GetRotation_WorldSpace(), GetScale_WorldSpace())
                .WithDebugName(String.Format("CustomMeshComponent {0}", this.Name));

            return cmd;
        }

        public virtual void Render(Renderer renderer)
        {
            
        }
    }
}

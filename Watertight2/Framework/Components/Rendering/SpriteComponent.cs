using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using Watertight.Filesystem;
using Watertight.Interfaces;
using Watertight.Math;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;

namespace Watertight.Framework.Components.Rendering
{
    class SpriteComponent : SceneComponent, IHasResources, IRenderable
    {
        public ResourcePtr Texture
        {
            get => _Texture;
            set
            {
                _Texture = value;
                if(_Texture.Loaded)
                {
                    RebuildVertexBuffer();
                }
                else
                {
                    _Texture.OnLoaded += RebuildVertexBuffer;
                }
            }
        } 

        ResourcePtr _Texture;

        IVertexBuffer SpriteVertexBuffer;

     

        public SpriteComponent(Actor Owner) : base(Owner)
        {
        }

        public SpriteComponent()
            : base()
        {
        }

        protected void RebuildVertexBuffer()
        {
            Vector2 TextureSize = Texture.Get<ITexture>()?.Size ?? Vector2.One;

            SpriteVertexBuffer = IEngine.Instance.Renderer.RendererResourceFactory.CreateVertexBuffer();

            Vertex[] Verts = new Vertex[]
            {
                new Vertex
                {
                    Location = new Vector3(0, 0, 0),
                    UV = new Vector2(0, 0),
                    Color = Color.White
                },
                new Vertex
                {
                    Location = new Vector3(0, TextureSize.Y, 0),
                    UV = new Vector2(0, 1),
                    Color = Color.White
                },
                new Vertex
                {
                    Location = new Vector3(TextureSize.X, TextureSize.Y, 0),
                    UV = new Vector2(1, 1),
                    Color = Color.White
                },
                new Vertex
                {
                    Location = new Vector3(TextureSize.X, 0, 0),
                    UV = new Vector2(1, 0),
                    Color = Color.White
                },
            };
            ushort[] ind = new ushort[]
            {
                0, 3, 1,
                3, 2, 1
            };

            SpriteVertexBuffer.SetVertexData(Verts, ind);
        }

        public void CollectResources(IList<ResourcePtr> ResourceCollector)
        {
            ResourceCollector.Add(Texture);
        }

        RenderingCommand cmd;
        public void PreRender(Renderer renderer)
        {
            if(SpriteVertexBuffer == null)
            {
                return;
            }

            if (cmd == null)
            {
                cmd = renderer.RendererResourceFactory.CreateRenderCommand();
            }

            cmd.WithVertexBuffer(SpriteVertexBuffer)
                .WithDebugName(String.Format("SpriteComponent {0}", this.Name))
                .WithTexture(Texture.Get<ITexture>())
                .WithTransform(GetLocation_WorldSpace(), GetRotation_WorldSpace(), GetScale_WorldSpace());

            renderer.EnqueueRenderCommand(cmd);
        }

        public void Render(Renderer renderer)
        {
            
        }
    }
}

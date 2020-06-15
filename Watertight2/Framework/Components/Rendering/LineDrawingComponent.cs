using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Watertight.Interfaces;
using Watertight.Math;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;

namespace Watertight.Framework.Components.Rendering
{
    public class LineDrawingComponent : SceneComponent, IRenderable
    {
        public LineDrawingComponent()
        {
        }

        public LineDrawingComponent(Actor Owner) : base(Owner)
        {
        }

        public IEnumerable<Line> Lines
        {
            get => _Lines;
        } 

        List<Line> _Lines = new List<Line>();

        IVertexBuffer VertexBuffer;

        public void AddLine(Line line)
        {
            _Lines.Add(line);
            BuildLineVertexBuffer();
        }

        public void AddLines(IEnumerable<Line> lines)
        {
            _Lines.AddRange(lines);
            BuildLineVertexBuffer();
        }

        public void AddLines(params Line[] Lines)
        {
            AddLines((IEnumerable<Line>)Lines);
        }

        protected virtual void BuildLineVertexBuffer()
        {
            VertexBuffer = IEngine.Instance.Renderer.RendererResourceFactory.CreateVertexBuffer();

            List<Vertex> Verts = new List<Vertex>();
            List<ushort> Indices = new List<ushort>();
            for(uint i = 0; i < Lines.Count(); i++)
            {
                Line ln = _Lines[(int)i];

                Verts.Add(new Vertex
                {
                    Location = ln.Point1,
                    Color = Color.Red,
                });
                Verts.Add(new Vertex
                {
                    Location = ln.Point2,
                    Color = Color.Red,
                });

                Indices.Add((ushort)(i * 2 + 0));
                Indices.Add((ushort)(i * 2 + 1));
            }

            VertexBuffer.SetVertexData(Verts.ToArray(), Indices.ToArray());
        }

        public virtual void PreRender(Renderer renderer)
        {
            if (VertexBuffer == null)
            {
                BuildLineVertexBuffer();
            }

            RenderingCommand cmd = CreateRenderingCommand(renderer);
            renderer.EnqueueRenderCommand(cmd);
        }

        RenderingCommand cmd;
        protected virtual RenderingCommand CreateRenderingCommand(Renderer renderer)
        {
            if (cmd == null)
            {
                cmd = renderer.RendererResourceFactory.CreateRenderCommand();
            }
            cmd.WithVertexBuffer(VertexBuffer)
                .WithTransform(GetLocation_WorldSpace(), GetRotation_WorldSpace(), GetScale_WorldSpace())
                .WithMaterial(Renderer.DefaultWireframeMaterialPtr.Get<Material>())
                .WithDebugName(String.Format("LineDrawingComponent {0}", this.Name));

            return cmd;
        }

        public virtual void Render(Renderer renderer)
        {
            
        }
    }
}

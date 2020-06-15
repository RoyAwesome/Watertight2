using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Math;
using Watertight.Rendering.Interfaces;
using Watertight.Util;

namespace Watertight.Rendering.VeldridRendering
{
    class VeldridVertexBuffer : IVertexBuffer
    {
        static VeldridRenderer Renderer => IEngine.Instance.Renderer as VeldridRenderer;

        public Veldrid.DeviceBuffer VertexBuffer;
        public Veldrid.DeviceBuffer IndexBuffer;


        public int NumIndicies
        {
            get => CachedIndexData.Length / 2;
        }

        public int NumVerticies
        {
            get => CachedVertexData.Length / Vertex.Size;
        }

        public bool Bound
        {
            get;
            private set;
        }

        public VeldridVertexBuffer()
        {
           
        }

        ~VeldridVertexBuffer()
        {
            if(IndexBuffer != null)
            {
                IndexBuffer.Dispose();
            }
            if(VertexBuffer != null)
            {
                VertexBuffer.Dispose();
            }
        }

        public void Bind()
        {
            if(VertexBuffer == null || VertexBuffer.SizeInBytes < CachedVertexData.Length)
            {
                VertexBuffer?.Dispose();
                VertexBuffer = Renderer.VeldridFactory.CreateBuffer(new Veldrid.BufferDescription((uint)CachedVertexData.Length, Veldrid.BufferUsage.VertexBuffer | Veldrid.BufferUsage.Dynamic));
                VertexBuffer.Name = "VertexBuffer";
            }
            if(IndexBuffer == null || IndexBuffer.SizeInBytes < CachedIndexData.Length)
            {
                IndexBuffer?.Dispose();
                IndexBuffer = Renderer.VeldridFactory.CreateBuffer(new Veldrid.BufferDescription((uint)CachedIndexData.Length, Veldrid.BufferUsage.IndexBuffer | Veldrid.BufferUsage.Dynamic));
                IndexBuffer.Name = "IndexBuffer";
            }

            Renderer.GraphicsDevice.UpdateBuffer(VertexBuffer, 0, GetVertexData());
            Renderer.GraphicsDevice.UpdateBuffer(IndexBuffer, 0, GetIndexData());

            Bound = true;
        }

        byte[] CachedVertexData = new byte[] { };
        byte[] CachedIndexData = new byte[] { };

        public byte[] GetIndexData()
        {
            return CachedIndexData;
        }

        public byte[] GetVertexData()
        {
            return CachedVertexData;
        }

        public void SetVertexData(Vertex[] vertices, ushort[] indicies)
        {
            CachedVertexData = Utils.ConvertVertexArrayToByteBuffer(vertices);
            CachedIndexData = Utils.ConvertIndexArrayToByteBuffer(indicies);
            Bound = false;
        }


        public static Veldrid.VertexLayoutDescription VertexLayoutDescription = new Veldrid.VertexLayoutDescription(
                new Veldrid.VertexElementDescription("Position", Veldrid.VertexElementSemantic.Position, Veldrid.VertexElementFormat.Float3),
                new Veldrid.VertexElementDescription("TexCoord", Veldrid.VertexElementSemantic.TextureCoordinate, Veldrid.VertexElementFormat.Float2),
                new Veldrid.VertexElementDescription("Color", Veldrid.VertexElementSemantic.Color, Veldrid.VertexElementFormat.Float4)
               );
        
    }
}

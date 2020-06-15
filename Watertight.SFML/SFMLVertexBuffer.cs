using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watertight.Math;
using Watertight.Rendering.Interfaces;
using Watertight.Util;

namespace Watertight.SFML
{
    public class SFMLVertexBuffer : IVertexBuffer
    {
        public Vertex[] VertexBuffer
        {
            get
            {
                return vb;
            }
            set
            {
                vb = value;
                bytebuffer = null;
            }
        }

        public ushort[] IndexBuffer
        {
            get;
            set;
        }

        public int NumIndicies
        {
            get => IndexBuffer?.Length ?? 0;
        }


        public int NumVerticies
        {
            get => VertexBuffer?.Length ?? 0;
        }

        public bool Bound => true;

        private Vertex[] vb;
        private byte[] bytebuffer;

        public SFMLVertexBuffer()
        {

        }

        public SFMLVertexBuffer(int Verts)
        {
            VertexBuffer = new Vertex[Verts];
        }

        public byte[] GetVertexData()
        {
            if (VertexBuffer == null)
            {
                return null;
            }

            if (bytebuffer == null)
            {
                bytebuffer = Utils.ConvertVertexArrayToByteBuffer(VertexBuffer);
            }

            return bytebuffer;

        }

        public void SetVertexData(Vertex[] vertices)
        {
            this.VertexBuffer = vertices;
        }
                        

        public void Bind()
        {
            
        }

        public void SetVertexData(Vertex[] vertices, ushort[] indicies)
        {
            this.VertexBuffer = vertices;
            this.IndexBuffer = indicies;
        }

        public byte[] GetIndexData()
        {
            throw new NotImplementedException();
        }
    }
}

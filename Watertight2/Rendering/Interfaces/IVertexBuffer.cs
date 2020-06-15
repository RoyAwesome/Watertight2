using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Math;

namespace Watertight.Rendering.Interfaces
{
    public interface IVertexBuffer
    {
        public int NumIndicies
        {
            get;
        }

        public int NumVerticies
        {
            get;
        }

        public bool Bound
        {
            get;
        }

        public byte[] GetVertexData();

        public byte[] GetIndexData();

        public void SetVertexData(Vertex[] vertices, ushort[] indicies);

        public void Bind();

    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using Watertight.Math;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;

namespace Watertight.Rendering
{
   

    /// <summary>
    /// Base Class for building and submitting a render command to the renderer
    /// </summary>
    public abstract class RenderingCommand : IDisposable
    {
        public string Name
        {
            get;
            set;
        }

        public IVertexBuffer VertexBuffer
        {
            get;
            protected set;
        }

        public ITexture Texture
        {
            get;
            protected set;
        }

        public Matrix4x4 Transform
        {
            get;
            protected set;
        } = Matrix4x4.Identity;

       

        public Color? ClearColor
        {
            get;
            protected set;
        }
        

        public int StartVertex
        {
            get;
            protected set;
        } = 0;

        public int StartIndex
        {
            get;
            protected set;
        } = 0;

        public int? PrimitiveCount
        {
            get;
            protected set;
        }

        public int NumIndicies
        {
            get
            {
                return Material.Topology switch
                {
                    RenderTopology.Point_List => PrimitiveCount,
                    RenderTopology.Line_List => PrimitiveCount * 2,
                    RenderTopology.Line_Strip => PrimitiveCount + 1,
                    RenderTopology.Triangle_List => PrimitiveCount * 3,
                    RenderTopology.Triangle_Strip => PrimitiveCount + 2,
                    RenderTopology.Triangle_Fan => PrimitiveCount + 1,
                    RenderTopology.Quad_List => PrimitiveCount * 4,
                    _ => PrimitiveCount,
                } ?? VertexBuffer?.NumIndicies ?? 0;
            }
        }


        public Material Material
        {
            get
            {              
                return _Material ?? Renderer.DefaultMaterialPtr.Get<Material>();
            }
            protected set
            {
                _Material = value;
            }
        }
        private Material _Material;

        public ICamera Camera
        {
            get
            {
                if(_Camera == null)
                {
                    return IEngine.Instance.Renderer.MainCamera;
                }
                else
                {
                    return _Camera;
                }
            }
            protected set
            {
                _Camera = value;
            }
        }
        private ICamera _Camera;

        public virtual bool Dirty
        {
            get;
            protected set;
        } = true;

        

        protected void MarkDirty()
        {
            Dirty = true;
        }
                
        public RenderingCommand WithTexture(ITexture Texture )
        {
            this.Texture = Texture;
            MarkDirty();
            return this;
        }

        public RenderingCommand WithTransform(Vector3 Position, Quaternion Rotation, Vector3 Scale)
        {
            Transform = new Transform
            {
                Location = Position,
                Rotation = Rotation,
                Scale = Scale
            }.ToTransformMatrix();
            return this;
        }

        public RenderingCommand WithVertexBuffer(IVertexBuffer vertexBuffer)
        {
            VertexBuffer = vertexBuffer;
            MarkDirty();
            return this;
        }
              

        public RenderingCommand WithStartVertex(int StartVertex)
        {
            this.StartVertex = StartVertex;
            MarkDirty();
            return this;
        }

        public RenderingCommand WithStartIndex(int StartIndex)
        {
            this.StartIndex = StartIndex;
            MarkDirty();
            return this;
        }

        public RenderingCommand WithPrimitveCount(int PrimitiveCount)
        {
            this.PrimitiveCount = PrimitiveCount;
            MarkDirty();
            return this;
        }      

        public RenderingCommand WithClearColor(Color ClearColor)
        {
            this.ClearColor = ClearColor;
            MarkDirty();
            return this;
        }

        public RenderingCommand WithMaterial(Material Mat)
        {
            this.Material = Mat;
            MarkDirty();
            return this;
        }

        public RenderingCommand WithDebugName(string name)
        {
            this.Name = name;
            return this;
        }

        public RenderingCommand WithCamera(ICamera camera)
        {
            this.Camera = camera;
            MarkDirty();
            return this;
        }

        public virtual void Dispose()
        {

        }
    }
}

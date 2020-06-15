using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Watertight.Rendering.Materials
{
    public enum RenderTopology
    {
        Point_List,
        Line_List,
        Line_Strip,
        Triangle_List,
        Triangle_Strip,
        Triangle_Fan,
        Quad_List,
    }

    public abstract class Material : ICloneable
    {
        public IReadOnlyDictionary<Shader.Stage, Shader> Shaders
        {
            get
            {
                return _Shaders;
            }
        }

        internal protected Dictionary<Shader.Stage, Shader> _Shaders = new Dictionary<Shader.Stage, Shader>();

        public virtual RenderTopology Topology
        {
            get;
            set;
        } = RenderTopology.Triangle_List;

        public enum FillMode
        {
            Fill,
            Line,
            Points,
        }

        public FillMode Fill
        {
            get;
            set;
        } = FillMode.Fill;

        public bool DepthTest
        {
            get;
            set;
        }

        public abstract void PostLoad();

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

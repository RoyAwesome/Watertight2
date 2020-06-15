using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Rendering.Materials;

namespace Watertight.SFML
{
    class SFMLMaterial : Material
    {
        public PrimitiveType SFMLPrimitiveType
        {
            get
            {
                return Topology switch
                {
                    RenderTopology.Point_List => PrimitiveType.Points,
                    RenderTopology.Line_List => PrimitiveType.Lines,
                    RenderTopology.Line_Strip => PrimitiveType.LineStrip,
                    RenderTopology.Triangle_List => PrimitiveType.Triangles,
                    RenderTopology.Triangle_Strip => PrimitiveType.TriangleStrip,
                    RenderTopology.Triangle_Fan => PrimitiveType.TriangleFan,
                    RenderTopology.Quad_List => PrimitiveType.Quads,
                    _ => PrimitiveType.Points,
                };
            }
        }

        public override void PostLoad()
        {
           
        }
    }
}

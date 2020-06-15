using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Rendering;
using SFMLTransform = SFML.Graphics.Transform;
using SFMLColor = SFML.Graphics.Color;

namespace Watertight.SFML
{
    class SFMLRenderingCommand : RenderingCommand, Drawable
    {
       

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (ClearColor.HasValue)
            {
                target.Clear(new SFMLColor(ClearColor.Value.R, ClearColor.Value.G, ClearColor.Value.B, ClearColor.Value.A));
            }
            if(VertexBuffer == null)
            {
                return;
            }

            Vector2 Offset = Vector2.One;

            if(Texture != null && (Texture is Texture))
            {
                Texture SFMLTexture = Texture as Texture;

                Offset = new Vector2(SFMLTexture.Size.X, SFMLTexture.Size.Y);
                states.Texture = SFMLTexture;
            }

            SFMLVertexBuffer simpleVertexBuffer = VertexBuffer as SFMLVertexBuffer;
            SFMLMaterial sfmlMat = Material as SFMLMaterial;
                       
            List<Vertex> SFMLVerts = new List<Vertex>();
            int LastIndex = StartIndex + NumIndicies;
            for(int i = StartIndex; i < LastIndex; i++)
            {
                Watertight.Math.Vertex wtvert = simpleVertexBuffer.VertexBuffer[simpleVertexBuffer.IndexBuffer[i]];
                Vertex sfmlvert = new Vertex
                { 
                    Position = new Vector2f(wtvert.Location.X, wtvert.Location.Y),
                    TexCoords = new Vector2f(wtvert.UV.X * Offset.X, wtvert.UV.Y * Offset.Y),
                    Color = new Color(wtvert.Color.R, wtvert.Color.G, wtvert.Color.B, wtvert.Color.A)
                };

                SFMLVerts.Add(sfmlvert);

            }
            states.Transform = SFMLTransform.Identity;
            states.Transform.Translate(new Vector2f(Transform.Translation.X, Transform.Translation.Y));

            

            target.Draw(SFMLVerts.ToArray(), sfmlMat.SFMLPrimitiveType, states);
        }
    }
}

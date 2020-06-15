using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Framework.Components;
using Watertight.Interfaces;
using Watertight.Math;
using Watertight.Rendering.Interfaces;

namespace Watertight.SFML
{
    class SFMLCamera : View, ICamera
    {
        public CameraComponent Owner
        {
            get;
            internal set;
        }

        public Rectangle ViewRectangle
        {
            get
            {
                return new Rectangle(new System.Numerics.Vector2(Center.X, Center.Y), new System.Numerics.Vector2(Size.X, Size.Y));
            }
        }

        public Matrix4x4 Projection
        {
            get;
            set;
        } = Matrix4x4.Identity;

        public Matrix4x4 View
        {
            get => (Owner as ITransformable)?.GetTransform_WorldSpace().ToTransformMatrix() ?? Matrix4x4.Identity;
        }


        public void MakeActive()
        {
            SFMLRenderer.Instance.MainCamera = this;
        }

        internal void PreRender(float DeltaTime)
        {
            System.Numerics.Vector3 Location = Owner?.GetLocation_WorldSpace() ?? new Vector3(0, 0, 1);
            System.Numerics.Quaternion Rotation = Owner?.GetRotation_WorldSpace() ?? Quaternion.Identity; //TODO: Get the rotation out of this quat
            System.Numerics.Vector3 Scale = Owner?.GetScale_WorldSpace() ?? Vector3.One;

            Reset(new FloatRect(Location.X, -Location.Y, SFMLRenderer.Instance.ScreenSize.X, SFMLRenderer.Instance.ScreenSize.Y));

            //Center = new Vector2f(Location.X, -Location.Y);
            //Size = new Vector2f(SFMLEngine.SFMLInstance.ScreenSize.X, SFMLEngine.SFMLInstance.ScreenSize.Y);
            float ZoomLevel = MathF.Max(.01f, Location.Z);
            Zoom(ZoomLevel);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Watertight.Math
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Transform
    {
        public Vector3 Location;
        public Quaternion Rotation;
        public Vector3 Scale;

        public Matrix4x4 ToTransformMatrix()
        {
            return Matrix4x4.CreateTranslation(Location) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale);
        }

        public static Transform Identity
        {
            get
            {
                return new Transform
                { 
                    Location = Vector3.Zero,
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One,
                };
            }
        }
    }
}

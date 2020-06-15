using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Watertight.Math;
using System.Runtime.CompilerServices;

namespace Watertight.Interfaces
{
    public interface ITransformable
    {
        public Vector3 Location
        {
            get;
            set;
        }

        public Quaternion Rotation
        {
            get;
            set;
        }

        public Vector3 Scale
        {
            get;
            set;
        }

        public Vector3 GetLocation_WorldSpace();
        public Vector3 GetLocation_Relative();
        public Quaternion GetRotation_WorldSpace();
        public Quaternion GetRotation_Relative();
        public Vector3 GetScale_WorldSpace();
        public Vector3 GetScale_Relative();

      
    }

    public static class ITransformableUtil
    {
        public static Transform GetTransform_WorldSpace(this ITransformable transformable)
        {
            return new Transform
            {
                Location = transformable.GetLocation_WorldSpace(),
                Rotation = transformable.GetRotation_WorldSpace(),
                Scale = transformable.GetScale_WorldSpace(),
            };
        }

        public static Transform GetTransform_Relative(this ITransformable transformable)
        {
            return new Transform
            {
                Location = transformable.GetLocation_Relative(),
                Rotation = transformable.GetRotation_Relative(),
                Scale = transformable.GetScale_Relative(),
            };
        }
    }
}

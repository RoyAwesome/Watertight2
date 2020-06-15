using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Watertight.Math
{
    public static class FMath
    {
        public static float RoundToNearest(float value, float factor)
        {
           return (float)System.Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
        }

        public static Vector2 MaxVector(Vector2 A, Vector2 B)
        {
            return new Vector2(MathF.Max(A.X, B.X), MathF.Max(A.Y, B.Y));
        }

        public static Vector2 MinVector(Vector2 A, Vector2 B)
        {
            return new Vector2(MathF.Min(A.X, B.X), MathF.Min(A.Y, B.Y));
        }

        public static Vector3 MaxVector(Vector3 A, Vector3 B)
        {
            return new Vector3(MathF.Max(A.X, B.X), MathF.Max(A.Y, B.Y), MathF.Max(A.Z, B.Z));
        }

        public static Vector3 MinVector(Vector3 A, Vector3 B)
        {
            return new Vector3(MathF.Min(A.X, B.X), MathF.Min(A.Y, B.Y), MathF.Min(A.Z, B.Z));
        }

        public static float ToRadians(this float f)
        {
            return (System.MathF.PI / 180f) * f;
        }

        public static float ToDegrees(this float f)
        {
            return (180f / System.MathF.PI) * f;
        }

    }
}

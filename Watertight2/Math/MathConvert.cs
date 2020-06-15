using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Watertight.Math
{
    public static class MathConvert
    {
        public static byte[] GetBytes(params float[] floats)
        {
            byte[][] Arrays = new byte[floats.Length][];

            for (int i = 0; i < Arrays.Length; i++)
            {
                Arrays[i] = floats[i].ToBytes();
            }

            return CombineByteArrays(Arrays);
        }

        public static byte[] ToBytes(this float f)
        {
            return BitConverter.GetBytes(f);
        }

        public static byte[] ToBytes(this Vector2 vector)
        {
            return GetBytes(vector.X, vector.Y);
        }

        public static byte[] ToBytes(this Vector3 vector)
        {
            return GetBytes(vector.X, vector.Y, vector.Z);
        }

        public static byte[] ToBytes(this Vector4 vector)
        {
            return GetBytes(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static byte[] ToBytes(this Color color)
        {
            return GetBytes((color.R / 255f), color.G / 255f, color.B / 255f, color.A / 255.0f);
        }

        public static byte[] ToRawBytes(this Color color)
        {
            return new byte[] { color.R, color.G, color.B, color.A };
        }

        public static byte[] ToBytes(this Quaternion quat)
        {
            return GetBytes(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static byte[] ToBytes(this Transform transform)
        {
            return ToBytes(transform.ToTransformMatrix());
        }

        public static byte[] ToBytes(this Matrix4x4 mat4)
        {
            return GetBytes( mat4.M11, mat4.M12, mat4.M13, mat4.M14, 
                            mat4.M21, mat4.M22, mat4.M23, mat4.M24, 
                            mat4.M31, mat4.M32, mat4.M33, mat4.M34, 
                            mat4.M41, mat4.M42, mat4.M43, mat4.M44);
        }

        public static byte[] ToBytes(params Color[] colors)
        {
            byte[][] Arrays = new byte[colors.Length][];
            for(int i = 0; i < Arrays.Length; i++)
            {
                Arrays[i] = colors[i].ToBytes();
            }
            return CombineByteArrays(Arrays);
        }
        public static byte[] ToRawBytes(params Color[] colors)
        {
            byte[][] Arrays = new byte[colors.Length][];
            for (int i = 0; i < Arrays.Length; i++)
            {
                Arrays[i] = colors[i].ToRawBytes();
            }
            return CombineByteArrays(Arrays);
        }

        public static byte[] CombineByteArrays(params byte[][] arrays)
        {
            int totalSize = arrays.Sum(a => a.Length);
            
            byte[] output = new byte[totalSize];

            int offset = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                offset += i == 0 ? 0 : arrays[i - 1].Length;
                byte[] array = arrays[i];

                Buffer.BlockCopy(array, 0, output, offset, array.Length);
            }

            return output;
        }

    }
}

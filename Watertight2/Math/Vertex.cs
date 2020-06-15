using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Watertight.Math
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public const int Size_Location = (sizeof(float) * 3);
        public const int Size_UV = (sizeof(float) * 2);
        public const int Size_Color = (sizeof(float) * 4);
        public const int Stride = Size_Location + Size_UV + Size_Color;
        public const int Size = Stride;
        public const int Offset_Location = 0;
        public const int Offset_UV = Size_Location;
        public const int Offset_Color = Size_Location + Size_UV;

        public Vector3 Location;
        public Vector2 UV;
        public Color Color;
    }
}

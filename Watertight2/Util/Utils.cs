using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Watertight.Math;

namespace Watertight.Util
{
    public static class Utils
    {
        public static Type FindTypeFromString(string TypeName)
        {
            if(TypeName == null)
            {
                return null;
            }
            return System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(x => x.FullName == TypeName);
        }


        static Dictionary<string, int> NameTable = new Dictionary<string, int>();
        public static string GetNewNameForObject(object obj)
        {
            string TypeName = obj.GetType().Name;

            int val = NameTable.GetValueOrDefault(TypeName);
            val++;
            NameTable[TypeName] = val;

            return string.Format("{0}_{1}", TypeName, val);
        }

        public static byte[] GetBytes(this Color color)
        {
            return new byte[]
            {
                color.R,
                color.G,
                color.B,
                color.A,
            };
        }

        public static float[] GetFloats(this Color color)
        {
            return new float[]
            {
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f,
            };
        }

        public static byte[] ConvertVertexArrayToByteBuffer(Vertex[] VertexArray)
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter Writer = new BinaryWriter(MS))
            {               
                for (int i = 0; i < VertexArray.Length; i++)
                {
                    Vertex vtx = VertexArray[i];
                    Writer.Write(vtx.Location.X);
                    Writer.Write(vtx.Location.Y);
                    Writer.Write(vtx.Location.Z);
                    Writer.Write(vtx.UV.X);
                    Writer.Write(vtx.UV.Y);
                    var floats = vtx.Color.GetFloats();
                    for(int c = 0; c < floats.Length; c++)
                    {
                        Writer.Write(floats[c]);
                    }
                }
                return MS.ToArray();
            }        
        }

        public static byte[] ConvertIndexArrayToByteBuffer(ushort[] IndexArray)
        {
            byte[] Out = new byte[IndexArray.Length * 2];
            Buffer.BlockCopy(IndexArray, 0, Out, 0, IndexArray.Length * 2);

            return Out;
        }

        public static byte[] ReadToEnd(this System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Watertight.Scripts.Parsers
{
    class VectorParser : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            Type[] Types = new Type[]
            {
                typeof(Vector2),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Vector4),
            };

            return Types.Contains(objectType);

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            
            string Parsed = reader.Value as string;
            string[] Elements = Parsed.Split(new char[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
            float[] ParsedInts = Elements.Select(x =>
            {
                float i = 0;
                float.TryParse(x, out i);
                return i;
            }).ToArray();
            float[] VectorArray = new float[4];
            ParsedInts.CopyTo(VectorArray, 0);


            if(objectType == typeof(Vector2))
            {
                return new Vector2(VectorArray[0], VectorArray[1]);
            }

            if (objectType == typeof(Vector3))
            {
                return new Vector3(VectorArray[0], VectorArray[1], VectorArray[2]);
            }

            if(objectType == typeof(Vector4))
            {
                return new Vector4(VectorArray[0], VectorArray[1], VectorArray[2], VectorArray[4]);
            }

            if (objectType == typeof(Quaternion))
            {
                return new Quaternion(VectorArray[0], VectorArray[1], VectorArray[2], VectorArray[4]);
            }

            return Activator.CreateInstance(objectType);

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string objstring = value.ToString().Replace("<", " ").Replace(">", "").Replace(",", "");
            writer.WriteValue(objstring);
        }
    }
}

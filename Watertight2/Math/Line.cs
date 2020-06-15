using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Watertight.Math
{
    public struct Line : IEquatable<Line>
    {
        public Vector3 Point1
        {
            get;
            set;
        }

        public Vector3 Point2
        {
            get;
            set;
        }

        public Line(Vector2 Point1, Vector2 Point2)
            : this(new Vector3(Point1, 0), new Vector3(Point2, 0))
        {           
        }

        public Line(Vector3 Point1, Vector3 Point2)
        {
            this.Point1 = FMath.MinVector(Point1, Point2);
            this.Point2 = FMath.MaxVector(Point1, Point2);
        }

        public bool Equals([AllowNull] Line other)
        {
            throw new NotImplementedException();
        }
    }
}

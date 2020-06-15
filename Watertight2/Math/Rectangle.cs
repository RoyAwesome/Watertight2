using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Watertight.Math
{
    public struct Rectangle : IEquatable<Rectangle>
    {
        public enum CompareMode
        {
            Inclusive,
            Exclusive,
        }

        public static Rectangle Empty = new Rectangle(Vector2.Zero, Vector2.Zero);

        public Vector2 Center;
        public Vector2 Extent;

        public float Top
        {
            get
            {
                return Center.Y + Extent.Y / 2;
            }            
        }

        public float Bottom
        { 
            get
            {
                return Center.Y - Extent.Y / 2;
            }
        }

        public float Left
        {
            get
            {
                return Center.X - Extent.X / 2;
            }
        }

        public float Right
        {
            get
            {
                return Center.X + Extent.X / 2;
            }
        }

        public float Width
        {
            get
            {
                return Extent.X;
            }
        }

        public float Height
        {
            get
            {
                return Extent.Y;
            }
        }


        public Vector2 UpperLeft
        {
            get
            {
                return new Vector2(Left, Top);
            }
        }

        public Vector2 UpperRight
        {
            get
            {
                return new Vector2(Right, Top);
            }
        }

        public Vector2 LowerLeft
        {
            get
            {
                return new Vector2(Left, Bottom);
            }
        }

        public Vector2 LowerRight
        {
            get
            {
                return new Vector2(Right, Bottom);
            }
        }

        public Vector2 MinPoint
        {
            get
            {
                return LowerLeft;
            }
            set
            {
                Resize(value, MaxPoint);
            }
        }

        public Vector2 MaxPoint
        {
            get
            {
                return UpperRight;
            }
            set
            {
                Resize(MinPoint, value);
            }
        }

        public bool IsEmpty
        {
            get
            {                
                return Extent.X == 0 
                    || Extent.Y == 0;
            }
        }

        public IEnumerable<Vector2> Points
        {
            get
            {
                return new Vector2[]
                {
                    MinPoint,
                    MaxPoint,
                };
            }
        }

        public IEnumerable<Line> Lines
        {
            get
            {
                return new Line[]
                {
                    new Line(UpperLeft, UpperRight),
                    new Line(UpperRight, LowerRight),
                    new Line(LowerRight, LowerLeft),
                    new Line(LowerLeft, UpperLeft),
                };
            }
        }


        public Rectangle(Vector2 Center, Vector2 Extent)
        {
            this.Center = Center;
            this.Extent = Extent;
        }

        private bool IntersectsWith_Internal(Rectangle other, Func<float, float, float, bool> RangeCheckFunc)
        {
            bool xOverlap = RangeCheckFunc(this.Left, other.Left, other.Right)
              || RangeCheckFunc(other.Left, this.Left, this.Right);
            bool yOverlap = RangeCheckFunc(this.Bottom, other.Bottom, other.Top)
                || RangeCheckFunc(other.Bottom, this.Bottom, this.Top);

            return xOverlap || yOverlap;
        }

        private bool Contains_Internal(Vector2 Point, Func<float, float, float, bool> RangeCheckFunc)
        {
            bool xOverlap = RangeCheckFunc(Point.X, Left, Right);
            bool yOverlap = RangeCheckFunc(Point.Y, Bottom, Top);

            return xOverlap && yOverlap;
        }

        private bool InclusiveRange(float val, float min, float max)
        {
            return (val >= min) && (val <= max);
        }

        private bool ExclusiveRange(float val, float min, float max)
        {
            return (val > min) && (val < max);
        }

        #region Inclusive Operations
        /// <summary>
        /// Checks if another rectangle intersects with this one, including borders
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IntersectsWith(Rectangle other)
        {    
            return IntersectsWith_Internal(other, InclusiveRange);
        }      

        public bool Contains(Vector2 Point)
        {   
            return Contains_Internal(Point, InclusiveRange);
        }      

        public bool ContainsAll(params Vector2[] Points)
        {
            foreach(Vector2 vec in Points)
            {
                if(!Contains(vec))
                {
                    return false;
                }
            }
            return true;
        }       

        public bool ContainsAny(params Vector2[] Points)
        {
            foreach (Vector2 vec in Points)
            {
                if (Contains(vec))
                {
                    return true;
                }
            }
            return false;
        }      

        public bool Contains(Rectangle Other)
        {
            return ContainsAll(Other.MinPoint, Other.MaxPoint);
        }
        #endregion

        #region Exclusive Operations
        /// <summary>
        /// Checks if another rectangle intersects with this one, excluding borders
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IntersectsWithExclusive(Rectangle other)
        {           
            return IntersectsWith_Internal(other, ExclusiveRange);
        }

        public bool ContainsExclusive(Vector2 Point)
        {           
            return Contains_Internal(Point, ExclusiveRange);
        }

        public bool ContainsAllExclusive(params Vector2[] Points)
        {
            foreach (Vector2 vec in Points)
            {
                if (!ContainsExclusive(vec))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ContainsAnyExclusive(params Vector2[] Points)
        {
            foreach (Vector2 vec in Points)
            {
                if (ContainsExclusive(vec))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsExclusive(Rectangle Other)
        {
            return ContainsAllExclusive(Other.MinPoint, Other.MaxPoint);
        }
        #endregion

        #region Toggle Operations

        public bool IntersectsWith(Rectangle other, CompareMode CompareMode)
        {
            switch (CompareMode)
            {
                case CompareMode.Exclusive:
                    return IntersectsWithExclusive(other);
                case CompareMode.Inclusive:
                default:
                    return IntersectsWith(other);
            }
        }

        public bool Contains(Rectangle Other, CompareMode CompareMode)
        {
            switch (CompareMode)
            {
                case CompareMode.Exclusive:
                    return ContainsExclusive(Other);
                case CompareMode.Inclusive:
                default:
                    return Contains(Other);
            }
        }

        public bool Contains(Vector2 Point, CompareMode CompareMode)
        {
            switch (CompareMode)
            {
                case CompareMode.Exclusive:
                    return ContainsExclusive(Point);
                case CompareMode.Inclusive:
                default:
                    return Contains(Point);
            }
        }
        #endregion

        public void Resize(Vector2 MinPoint, Vector2 MaxPoint)
        {
            Center = (MinPoint + MaxPoint) / 2;
            Extent = (MaxPoint - MinPoint);
        }

        public static Rectangle FromMinMax(Vector2 MinPoint, Vector2 MaxPoint)
        {
            Rectangle Rect = new Rectangle();
            Rect.Resize(MinPoint, MaxPoint);
            return Rect;
        }       

        public static Rectangle FromMinAndSize(Vector2 MinPoint, Vector2 Size)
        {
            Rectangle Rect = new Rectangle();
            Rect.Resize(MinPoint, MinPoint + Size);
            return Rect;
        }
       

        public bool Equals([AllowNull] Rectangle other)
        {            
            return this.Center == other.Center
                 && this.Extent == other.Extent;
        }
    }
}

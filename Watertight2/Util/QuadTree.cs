using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using Watertight.Math;

namespace Watertight.Util
{
    public class QuadTree<T>
    {
        public Rectangle Bounds
        {
            get
            {
                return _Bounds;
            }
            protected set
            {
                _Bounds = value;
                ReIndex();
            }
        }
        private Rectangle _Bounds;

        public QuadrantNode Root
        {
            get;
            protected set;
        }

        public Rectangle.CompareMode CompareMode
        {
            get
            {
                return _exclusive;
            }
            set
            {
                _exclusive = value;
                ReIndex();
            }
        }
        private Rectangle.CompareMode _exclusive;

        IReadOnlyDictionary<T, QuadrantNode> Table
        {
            get
            {
                return _table;
            }
        }
        protected Dictionary<T, QuadrantNode> _table = new Dictionary<T, QuadrantNode>();

        public QuadTree(Rectangle Bounds)
        {
            if (Bounds.IsEmpty)
            {
                throw new ArgumentException(nameof(Bounds) + " cannot be empty");
            }

            this._Bounds = Bounds;
            this.Root = new QuadrantNode(null, Bounds, this);
        }

        public QuadTree(Rectangle Bounds, Rectangle.CompareMode CompareMode)
            : this(Bounds)
        {
            _exclusive = CompareMode;
        }

        public void Insert(T Node, Rectangle Bounds)
        {
            if (Bounds.IsEmpty)
            {
                throw new ArgumentException(nameof(Bounds) + " cannot be empty");
            }
            
            if(Root == null)
            {
                Root = new QuadrantNode(null, Bounds, this);
                _table = new Dictionary<T, QuadrantNode>();
            }

            QuadrantNode Parent = Root.Insert(Node, Bounds);
            _table[Node] = Parent;
        }

        public bool HasValuesInside(Rectangle Bounds)
        {
            return Root.HasIntersectingNodes(Bounds);
        }

        public IEnumerable<T> GetValues(Rectangle Bounds)
        {
            foreach(var Value in GetNodes(Bounds))
            {
                yield return Value.Item2;
            }
        }

        public bool Remove(T Node)
        {
            QuadrantNode Parent = null;
            if(Table.TryGetValue(Node, out Parent))
            {
                Parent.RemoveNode(Node);
                _table.Remove(Node);
                return true;
            }

            return false;
        }

        public void ReIndex()
        {
            var Nodes = GetNodes(Bounds);
            Root = null;
            foreach(var Node in Nodes)
            {
                Insert(Node.Item2, Node.Item1);
            }
        }


        IEnumerable<Tuple<Rectangle, T>> GetNodes(Rectangle Bounds)
        {
            List<Tuple<Rectangle, T>> Nodes = new List<Tuple<Rectangle, T>>();
            Root.GetIntersectingNodes(ref Nodes, Bounds);
            return Nodes;
        }


        public class QuadrantNode
        {
            protected internal QuadTree<T> Owner
            {
                get;
                private set;
            }

            protected internal QuadrantNode Parent
            {
                get;
                protected set;
            }

            protected internal bool IsRoot
            {
                get
                {
                    return Parent == null;
                }
            }

            protected internal Rectangle BoundingRect
            {
                get;
                set;
            }

            protected internal QuadrantNode UpperLeft
            {
                get;
                set;
            }
            protected internal QuadrantNode LowerLeft
            {
                get;
                set;
            }
            protected internal QuadrantNode UpperRight
            {
                get;
                set;
            }
            protected internal QuadrantNode LowerRight
            {
                get;
                set;
            }

            public List<Tuple<Rectangle, T>> Nodes
            {
                get;
                protected internal set;
            }

            public QuadrantNode(QuadrantNode Parent, Rectangle Bounds, QuadTree<T> Owner)
            {
                this.Parent = Parent;
                if(Bounds.IsEmpty)
                {
                    throw new ArgumentException(nameof(Bounds) + " cannot be empty");
                }
                this.BoundingRect = Bounds;

                this.Owner = Owner;
            }

            internal QuadrantNode Insert(T Node, Rectangle Bounds)
            {
                if (Bounds.IsEmpty)
                {
                    throw new ArgumentException(nameof(Bounds) + " cannot be empty");
                }

                QuadrantNode toInsert = this;
                while(true)
                {
                    Rectangle R0 = toInsert.BoundingRect;
                    float halfWidth = R0.Width / 2;
                    if(halfWidth < 1)
                    {
                        halfWidth = 1;
                    }
                    float halfHeight = R0.Height / 2;
                    if(halfHeight < 1)
                    {
                        halfHeight = 1;
                    }
                    Vector2 HalfExtent = new Vector2(halfWidth, halfHeight);
                   

                    Rectangle BottomLeft = Rectangle.FromMinAndSize(R0.LowerLeft, HalfExtent);
                    Rectangle TopLeft = Rectangle.FromMinAndSize(new Vector2(R0.Left, R0.Center.Y), HalfExtent);
                    Rectangle TopRight = Rectangle.FromMinAndSize(R0.Center, HalfExtent);
                    Rectangle BottomRight = Rectangle.FromMinAndSize(new Vector2(R0.Center.X, R0.Bottom), HalfExtent);

                    QuadrantNode child = null;

                    if(TopLeft.Contains(Bounds, Owner.CompareMode))
                    {
                        if(toInsert.UpperLeft == null)
                        {
                            toInsert.UpperLeft = new QuadrantNode(toInsert, TopLeft, Owner);
                        }
                        child = toInsert.UpperLeft;  
                    }
                    else if(TopRight.Contains(Bounds, Owner.CompareMode))
                    {
                        if (toInsert.UpperRight == null)
                        {
                            toInsert.UpperRight = new QuadrantNode(toInsert, TopRight, Owner);
                        }
                        child = toInsert.UpperRight;
                    }
                    else if (BottomLeft.Contains(Bounds, Owner.CompareMode))
                    {
                        if (toInsert.LowerLeft == null)
                        {
                            toInsert.LowerLeft = new QuadrantNode(toInsert, BottomLeft, Owner);
                        }
                        child = toInsert.LowerLeft;
                    }
                    else if (BottomRight.Contains(Bounds, Owner.CompareMode))
                    {
                        if (toInsert.LowerRight == null)
                        {
                            toInsert.LowerRight = new QuadrantNode(toInsert, BottomRight, Owner);
                        }
                        child = toInsert.LowerRight;
                    }

                    if(child != null)
                    {
                        toInsert = child;
                    }
                    else
                    {
                        //Insert the data
                        if(toInsert.Nodes == null)
                        {
                            toInsert.Nodes = new List<Tuple<Rectangle, T>>();
                        }
                        toInsert.Nodes.Add(new Tuple<Rectangle, T>(Bounds, Node));
                        return toInsert;
                    }
                }
            }

            internal void GetIntersectingValues(ref List<T> Nodes, Rectangle Bounds)
            {
                if (Bounds.IsEmpty)
                {
                    throw new ArgumentException(nameof(Bounds) + " cannot be empty");
                }

                List<Tuple<Rectangle, T>> ValueNodes = new List<Tuple<Rectangle, T>>();
                GetIntersectingNodes(ref ValueNodes, Bounds);

                Nodes.AddRange(ValueNodes.Select(x => x.Item2));              
            }

            internal void GetIntersectingNodes(ref List<Tuple<Rectangle, T>> Nodes, Rectangle Bounds)
            {
                if (Bounds.IsEmpty)
                {
                    throw new ArgumentException(nameof(Bounds) + " cannot be empty");
                }

                if (this.UpperLeft != null && Bounds.IntersectsWith(this.UpperLeft.BoundingRect, Owner.CompareMode))
                {
                    this.UpperLeft.GetIntersectingNodes(ref Nodes, Bounds);
                }
                if (this.UpperRight != null && Bounds.IntersectsWith(this.UpperRight.BoundingRect, Owner.CompareMode))
                {
                    this.UpperRight.GetIntersectingNodes(ref Nodes, Bounds);
                }
                if (this.LowerRight != null && Bounds.IntersectsWith(this.LowerRight.BoundingRect, Owner.CompareMode))
                {
                    this.LowerRight.GetIntersectingNodes(ref Nodes, Bounds);
                }
                if (this.LowerLeft != null && Bounds.IntersectsWith(this.LowerLeft.BoundingRect, Owner.CompareMode))
                {
                    this.LowerLeft.GetIntersectingNodes(ref Nodes, Bounds);
                }

                if (this.Nodes != null)
                {
                    Nodes.AddRange(this.Nodes.Where(x => x.Item1.IntersectsWith(Bounds, Owner.CompareMode)));
                }
            }

            internal bool HasIntersectingNodes(Rectangle Bounds)
            {
                if (Bounds.IsEmpty)
                {
                    throw new ArgumentException(nameof(Bounds) + " cannot be empty");
                }

                bool Found = false;

                if ( !Found && this.UpperLeft != null && Bounds.IntersectsWith(this.UpperLeft.BoundingRect, Owner.CompareMode))
                {
                   Found = this.UpperLeft.HasIntersectingNodes(Bounds);
                }
                if (!Found && this.UpperRight != null && Bounds.IntersectsWith(this.UpperRight.BoundingRect, Owner.CompareMode))
                {
                    Found = this.UpperRight.HasIntersectingNodes(Bounds);
                }
                if (!Found && this.LowerRight != null && Bounds.IntersectsWith(this.LowerRight.BoundingRect, Owner.CompareMode))
                {
                    Found = this.LowerRight.HasIntersectingNodes(Bounds);
                }
                if (!Found && this.LowerLeft != null && Bounds.IntersectsWith(this.LowerLeft.BoundingRect, Owner.CompareMode))
                {
                    Found = this.LowerLeft.HasIntersectingNodes(Bounds);
                }

                if (!Found && this.Nodes != null)
                {
                    Found = this.Nodes.Where(x => x.Item1.IntersectsWith(Bounds, Owner.CompareMode)).Count() > 0;
                }

                return Found;
            }

            internal bool RemoveNode(T Node)
            {
                bool RemovedItem = false;

                if(Nodes != null)
                {
                    var ToRemove = this.Nodes.Where(x => x.Item2.Equals(Node)).ToArray();
                    foreach(var Element in ToRemove)
                    {
                        RemovedItem |= this.Nodes.Remove(Element);
                    }
                }

                return RemovedItem;
            }

        }
    }
}

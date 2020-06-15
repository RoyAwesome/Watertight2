using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Watertight.Math;

namespace WatertightTests.Math
{
    [TestFixture]
    class RectangleTest
    {
        [Test]
        public void TestRectangle()
        {
            Vector2 Center = Vector2.Zero;
            Vector2 Extent = Vector2.One;

            Rectangle Rect = new Rectangle(Center, Extent);

            Assert.That(Rect.Center, Is.EqualTo(Center));
            Assert.That(Rect.Extent, Is.EqualTo(Extent));
        }

        [Test]
        public void TestRectangleMinMax()
        {
            Vector2 Min = Vector2.Zero;
            Vector2 Max = Vector2.One;

            Rectangle Rect = Rectangle.FromMinMax(Min, Max);

            Assert.That(Rect.Center, Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(Rect.Extent, Is.EqualTo(new Vector2(1, 1)));
        }

        [Test]
        public void TestRectangleMinAndSize()
        {
            Vector2 Min = Vector2.Zero;
            Vector2 Size = Vector2.One;

            Rectangle Rect = Rectangle.FromMinAndSize(Min, Size);

            Assert.That(Rect.Center, Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(Rect.Extent, Is.EqualTo(new Vector2(1, 1)));
        }

        [Test]
        public void TestSides()
        {
            Vector2 Center = Vector2.Zero;
            Vector2 Extent = Vector2.One;

            Rectangle Rect = new Rectangle(Center, Extent);

            Assert.That(Rect.Top, Is.EqualTo(0.5f));
            Assert.That(Rect.Bottom, Is.EqualTo(-0.5f));

            Assert.That(Rect.Left, Is.EqualTo(-0.5f));
            Assert.That(Rect.Right, Is.EqualTo(0.5f));
        }

        [Test]
        public void TestIntersection()
        {
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);
            Rectangle R1 = new Rectangle(new Vector2(.5f, 1), Vector2.One);

            Assert.That(R0.IntersectsWith(R1));
        }

        [Test]
        public void TestNoIntersection()
        {
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);
            Rectangle R1 = new Rectangle(new Vector2(2f, 2f), Vector2.One);

            Assert.That(!R0.IntersectsWith(R1));
        }

        [Test]
        public void TestContainsPoint()
        {
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);
            Vector2 InsidePoint = new Vector2(0.3f, 0.3f);

            Assert.That(R0.Contains(InsidePoint));
        }

        [Test]
        public void TestDoesntContainsPoint()
        {
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);
            Vector2 InsidePoint = new Vector2(10f, -30f);

            Assert.That(!R0.Contains(InsidePoint));
        }

        [Test]
        public void TestContainsRect()
        {
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);
            Rectangle R1 = new Rectangle(Vector2.Zero, new Vector2(0.5f, 0.5f));

            Assert.That(R0.Contains(R1));
        }

        [Test]
        public void TestContainsSelf()
        {
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);

            Assert.That(R0.Contains(R0));
        }

        [Test]
        public void TestContainsQuadrantsMinMax()
        {
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);
                     
            Rectangle BottomLeft = Rectangle.FromMinMax(R0.LowerLeft, R0.Center);
            Rectangle TopLeft = Rectangle.FromMinMax(new Vector2(R0.Left, R0.Center.Y), new Vector2(R0.Center.X, R0.Top));
            Rectangle TopRight = Rectangle.FromMinMax(R0.Center, R0.MaxPoint);
            Rectangle BottomRight = Rectangle.FromMinMax(new Vector2(R0.Center.X, R0.Bottom), new Vector2(R0.Right, R0.Center.Y));

            Assert.That(R0.Contains(BottomLeft), "R0 does not contain " + nameof(BottomLeft));
            Assert.That(R0.Contains(TopLeft), "R0 does not contain " + nameof(TopLeft));
            Assert.That(R0.Contains(TopRight), "R0 does not contain " + nameof(TopRight));
            Assert.That(R0.Contains(BottomRight), "R0 does not contain " + nameof(BottomRight));
        }

        [Test]
        public void TestContainsQuadrantsMinAndSize()
        {            
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);

            Vector2 HalfExtent = R0.Extent / 2;

            Rectangle BottomLeft = Rectangle.FromMinAndSize(R0.LowerLeft, HalfExtent);
            Rectangle TopLeft = Rectangle.FromMinAndSize(new Vector2(R0.Left, R0.Center.Y), HalfExtent);
            Rectangle TopRight = Rectangle.FromMinAndSize(R0.Center, HalfExtent);
            Rectangle BottomRight = Rectangle.FromMinAndSize(new Vector2(R0.Center.X, R0.Bottom), HalfExtent);

            Assert.That(R0.Contains(BottomLeft), "R0 does not contain " + nameof(BottomLeft));
            Assert.That(R0.Contains(TopLeft), "R0 does not contain " + nameof(TopLeft));
            Assert.That(R0.Contains(TopRight), "R0 does not contain " + nameof(TopRight));
            Assert.That(R0.Contains(BottomRight), "R0 does not contain " + nameof(BottomRight));
        }

        [Test]
        public void TestContainsQuadrantsMinAndSizeRandom()
        {
            System.Random RNG = new Random();

            for (int i =0; i < 100; i++)
            {
                Vector2 RandomCenter = new Vector2(RNG.Next() % 100, RNG.Next() % 100);
                Vector2 RandomExtent = new Vector2(RNG.Next() % 100, RNG.Next() % 100);
                Rectangle R0 = new Rectangle(RandomCenter, RandomExtent);

                Vector2 HalfExtent = R0.Extent / 2;

                Rectangle BottomLeft = Rectangle.FromMinAndSize(R0.LowerLeft, HalfExtent);
                Rectangle TopLeft = Rectangle.FromMinAndSize(new Vector2(R0.Left, R0.Center.Y), HalfExtent);
                Rectangle TopRight = Rectangle.FromMinAndSize(R0.Center, HalfExtent);
                Rectangle BottomRight = Rectangle.FromMinAndSize(new Vector2(R0.Center.X, R0.Bottom), HalfExtent);

                Assert.That(R0.Contains(BottomLeft), "R0 does not contain " + nameof(BottomLeft));
                Assert.That(R0.Contains(TopLeft), "R0 does not contain " + nameof(TopLeft));
                Assert.That(R0.Contains(TopRight), "R0 does not contain " + nameof(TopRight));
                Assert.That(R0.Contains(BottomRight), "R0 does not contain " + nameof(BottomRight));
            }          
        }

        [Test]
        public void TestContainsQuadrantsUnitFails()
        {
            Rectangle R0 = new Rectangle(Vector2.Zero, Vector2.One);

            float halfWidth = R0.Width / 2;
            if (halfWidth < 1)
            {
                halfWidth = 1;
            }
            float halfHeight = R0.Height / 2;
            if (halfHeight < 1)
            {
                halfHeight = 1;
            }
            Vector2 HalfExtent = new Vector2(halfWidth, halfHeight);

            //Bottom Left is R0 in this testcase. Ignore it for this testcase. 
            Rectangle TopLeft = Rectangle.FromMinAndSize(new Vector2(R0.Left, R0.Center.Y), HalfExtent);
            Rectangle TopRight = Rectangle.FromMinAndSize(R0.Center, HalfExtent);
            Rectangle BottomRight = Rectangle.FromMinAndSize(new Vector2(R0.Center.X, R0.Bottom), HalfExtent);

            Assert.That(!R0.Contains(TopLeft), "R0 does contain " + nameof(TopLeft));
            Assert.That(!R0.Contains(TopRight), "R0 does contain " + nameof(TopRight));
            Assert.That(!R0.Contains(BottomRight), "R0 does contain " + nameof(BottomRight));
        }
    }
}

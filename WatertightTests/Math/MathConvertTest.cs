using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Math;

namespace WatertightTests.Math
{
    [TestFixture]

    class MathConvertTest
    {

        [Test]
        public void TestArrayConcat()
        {
            byte[] test1 = new byte[] { 0, 1, 2, 3 };
            byte[] test2 = new byte[] { 5, 4, 3, 2, 1 };

            byte[] output = MathConvert.CombineByteArrays(test1, test2);

            Assert.That(output, Has.Length.EqualTo(test1.Length + test2.Length));
            Assert.AreEqual(output, new byte[] { 0, 1, 2, 3, 5, 4, 3, 2, 1 });
        }
    }
}

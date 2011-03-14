/*
 * Copyright (c) 2010 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using org.critterai.geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using System;

namespace org.critterai.geom
{
    [TestClass()]
    public class Triangle3Test
    {
        private const float TOLERANCE = MathUtil.TOLERANCE_STD;

        // Clockwise wrapped
        private const float AX = 3;
        private const float AY = 2;
        private const float AZ = -1;
        private const float BX = 2;
        private const float BY = -1;
        private const float BZ = 1;
        private const float CX = 0;
        private const float CY = -1;
        private const float CZ = 0;

        [TestMethod()]
        public void TestStaticGetArea()
        {
            float expected = GetHeronArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
            float actual = 
                Triangle3.GetArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
            Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE));
        }

        [TestMethod()]
        public void TestStaticGetAreaComp()
        {
            float expected = GetHeronArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
            float actual = (float)Math.Sqrt(
                Triangle3.GetAreaComp(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ)) / 2;
            Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, TOLERANCE));
        }

        [TestMethod()]
        public void TestStaticGetNormalFloatVector3()
        {
            Vector3 v = Triangle3.GetNormal(AX, AY, 0, BX, BY, 0, CX, CY, 0);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 0, 0, -1, TOLERANCE));
            v = Triangle3.GetNormal(AX, AY, 0, CX, CY, 0, BX, BY, 0);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 0, 0, 1, TOLERANCE));
            v = Triangle3.GetNormal(AX, 0, AZ, BX, 0, BZ, CX, 0, CZ);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 0, -1, 0, TOLERANCE));
            v = Triangle3.GetNormal(0, AY, AZ, 0, BY, BZ, 0, CY, CZ);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 1, 0, 0, TOLERANCE));
        }

        [TestMethod()]
        public void TestStaticGetNormalArrayVector3()
        {
            float[] vertices = {
                5, 5, 5
                , AX, 0, AZ
                , BX, 0, BZ
                , CX, 0, CZ
                , 9, 9, 9
            };
            Vector3 v = Triangle3.GetNormal(vertices, 1);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 0, -1, 0, TOLERANCE));
        }

        private float GetHeronArea(float ax, float ay, float az
                , float bx, float by, float bz
                , float cx, float cy, float cz)
        {
            double a = Math.Sqrt(
                Vector3Util.GetDistanceSq(AX, AY, AZ, BX, BY, BZ));
            double b = Math.Sqrt(
                Vector3Util.GetDistanceSq(AX, AY, AZ, CX, CY, CZ));
            double c = Math.Sqrt(
                Vector3Util.GetDistanceSq(CX, CY, CZ, BX, BY, BZ));
            double s = (a + b + c) / 2;
            return (float)Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }
    }
}

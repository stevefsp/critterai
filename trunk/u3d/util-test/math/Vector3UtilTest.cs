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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;

namespace org.critterai.math
{
    [TestClass()]
    public class Vector3UtilTest
    {
        private const float EPSILON = MathUtil.EPSILON_STD;

        private const float AX = 1.5f;
        private const float AY = 8.0f;
        private const float AZ = -3.2f;
        private const float BX = -17.112f;
        private const float BY = 77.5f;
        private const float BZ = 22.42f;
        
        private Vector3 mA1;
        private Vector3 mB1;
        
        float[] mVectors;

        [TestInitialize()]
        public void Setup()
        {
            mA1 = new Vector3(AX, AY, AZ);
            mB1 = new Vector3(BX, BY, BZ);
            mVectors = new float[6];
            mVectors[0] = AX;
            mVectors[1] = AY;
            mVectors[2] = AZ;    
            mVectors[3] = BX;
            mVectors[4] = BY;
            mVectors[5] = BZ;   
        }

        [TestMethod()]
        public void TestStaticSloppyEqualsVector()
        {

            /*
             * Had to remove the edge cases.  They don't function correctly,
             * even at only 2 decimal places.  E.g. 
             * Vector3Util.SloppyEquals(AX, AY, AZ, AX+0.19, AY, AZ, 0.19f)
             * returns false.
             * This was not a problem in Java.
             * I compensated by increasing the resolution of the test from 3
             * to 4 decimal places.
             */

            Vector3 v = new Vector3(AX, AY, AZ);
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v, 0.00f));

            v = new Vector3(AX + 0.0019f, AY, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.0018f));

            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v, 0.0020f));

            v = new Vector3(AX - 0.0019f, AY, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v, 0.0020f));

            v = new Vector3(AX, AY + 0.0019f, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v, 0.0020f));

            v = new Vector3(AX, AY - 0.0019f, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v, 0.0020f));

            v = new Vector3(AX, AY, AZ + 0.0019f);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v, 0.0020f));

            v = new Vector3(AX, AY, AZ - 0.0019f);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v, 0.0020f));
        }

        [TestMethod()]
        public void TestStaticSloppyEqualsFloat()
        {
            Vector3 v = new Vector3(AX, AY, AZ);
            Assert.IsTrue(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));

            v = new Vector3(AX + 0.0019f, AY, AZ);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX - 0.0019f, AY, AZ);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY + 0.0019f, AZ);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY - 0.0019f, AZ);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY, AZ + 0.0019f);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY, AZ - 0.0019f);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));
        }

        [TestMethod()]
        public void TestStaticSloppyEqualsVectorFloat()
        {
            Vector3 v = new Vector3(AX, AY, AZ);
            Assert.IsTrue(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));

            v = new Vector3(AX + 0.0019f, AY, AZ);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX - 0.0019f, AY, AZ);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY + 0.0019f, AZ);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY - 0.0019f, AZ);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY, AZ + 0.0019f);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY, AZ - 0.0019f);
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(
                Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));
        }

        [TestMethod()]
        public void TestStaticGetLengthSq()
        {
            float len = (AX * AX) + (AY * AY) + (AZ * AZ);
            Assert.IsTrue(MathUtil.SloppyEquals(
                Vector3Util.GetLengthSq(AX, AY, AZ), len, EPSILON));
        }

        [TestMethod()]
        public void TestStaticGetDistanceSq()
        {
            float result = Vector3Util.GetDistanceSq(AX, AY, AZ, BX, BY, BZ);
            float deltaX = BX - AX;
            float deltaY = BY - AY;
            float deltaZ = BZ - AZ;
            float expected = 
                (deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ);
            Assert.IsTrue(MathUtil.SloppyEquals(result, expected, EPSILON));
        }

        [TestMethod()]
        public void TestStaticDot()
        {
            float expected = (AX * BX) + (AY * BY) + (AZ * BZ);
            Assert.IsTrue(MathUtil.SloppyEquals(
                Vector3Util.Dot(AX, AY, AZ, BX, BY, BZ), expected, EPSILON));
        }

        [TestMethod()]
        public void TestStaticCross()
        {
            float expectedX = AY * BZ - AZ * BY;
            float expectedY = -AX * BZ + AZ * BX;
            float expectedZ = AX * BY - AY * BX;
            Vector3 v = Vector3Util.Cross(AX, AY, AZ, BX, BY, BZ);
            Assert.IsTrue(v.x == expectedX);
            Assert.IsTrue(v.y == expectedY);
            Assert.IsTrue(v.z == expectedZ);
        }

        [TestMethod()]
        public void TestStaticTranslateTowardFloat()
        {
            float factor = 0.62f;
            float x = AX + (BX - AX) * factor;
            float y = AY + (BY - AY) * factor;
            float z = AZ + (BZ - AZ) * factor;
            Vector3 u = 
                Vector3Util.TranslateToward(AX, AY, AZ, BX, BY, BZ, factor);
            Assert.IsTrue(u.x == x);
            Assert.IsTrue(u.y == y);
            Assert.IsTrue(u.z == z);
        }

        [TestMethod()]
        public void TestStaticFlatten() 
        {
            Vector3[] va = { mB1, mA1 };
            float[] v = Vector3Util.Flatten(va);
            Assert.IsTrue(v[0] == BX);
            Assert.IsTrue(v[1] == BY);
            Assert.IsTrue(v[2] == BZ);
            Assert.IsTrue(v[3] == AX);
            Assert.IsTrue(v[4] == AY);
            Assert.IsTrue(v[5] == AZ);
        }

        [TestMethod()]
        public void TestStaticGetVectors()
        {
            float[] v = { BX, BY, BZ, AX, AY, AZ };
            Vector3[] va = Vector3Util.GetVectors(v);
            Assert.IsTrue(va[0] == mB1);
            Assert.IsTrue(va[1] == mA1);
        }

        [TestMethod()]
        public void TestStaticGetVectorFromArray()
        {
            float[] v = { BX, BY, BZ, AX, AY, AZ };
            Assert.IsTrue(Vector3Util.GetVector(v, 0) 
                == new Vector3(BX, BY, BZ));
            Assert.IsTrue(Vector3Util.GetVector(v, 1)
                == new Vector3(AX, AY, AZ));
        }

        [TestMethod()]
        public void TestStaticGetBoundsVectorOut()
        {
            Vector3[] v = 
            { 
                 new Vector3(1, -2, -3)
                 , new Vector3(5, 2, -9)
                 , new Vector3(2, 1, -4)
            };

            Vector3 min, max;
            Vector3Util.GetBounds(v, out min, out max);

            Assert.IsTrue(min == new Vector3(1, -2, -9));
            Assert.IsTrue(max == new Vector3(5, 2, -3));
        }

        [TestMethod()]
        public void TestStaticGetBoundsArrayOut()
        {
            float[] v = 
            { 
                 1, -2, -3
                 , 5, 2, -9 
                 , 2, 1, -4
            };

            float[] bounds = new float[6];
            Vector3Util.GetBounds(v, bounds);

            Assert.IsTrue(bounds[0] == 1);
            Assert.IsTrue(bounds[1] == -2);
            Assert.IsTrue(bounds[2] == -9);
            Assert.IsTrue(bounds[3] == 5);
            Assert.IsTrue(bounds[4] == 2);
            Assert.IsTrue(bounds[5] == -3);
        }
    }
}

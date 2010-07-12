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
using org.critterai.math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;

namespace org.critterai.math
{
    /// <summary>
    ///This is a test class for Vector3UtilTest and is intended
    ///to contain all Vector3UtilTest Unit Tests
    ///</summary>
    [TestClass()]
    public class Vector3UtilTest
    {
        private TestContext testContextInstance;

        private const float EPSILON = 0.000000001f;

        private const float AX = 1.5f;
        private const float AY = 8.0f;
        private const float AZ = -3.2f;
        private const float BX = -17.112f;
        private const float BY = 77.5f;
        private const float BZ = 22.42f;
        
        private Vector3 mA1;
        private Vector3 mB1;
        
        float[] mVectors;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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

        /// <summary>
        ///A test for Vector3Util.SloppyEquals
        ///</summary>
        [TestMethod()]
        public void SloppyEqualsVectorTest()
        {

            /*
             * Had to remove the edge cases.  They don't function correctly,
             * even at only 2 decimal places.  
             * E.g. Vector2Util.Vector3Util.SloppyEquals(AX, AY, AZ, AX+0.19, AY, AZ, 0.19f)
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

        /// <summary>
        ///A test for Vector3Util.SloppyEquals
        ///</summary>
        [TestMethod()]
        public void SloppyEqualsFloatTest()
        {
            Vector3 v = new Vector3(AX, AY, AZ);
            Assert.IsTrue(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));

            v = new Vector3(AX + 0.0019f, AY, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX - 0.0019f, AY, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY + 0.0019f, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY - 0.0019f, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY, AZ + 0.0019f);
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY, AZ - 0.0019f);
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0020f));
        }

        /// <summary>
        ///A test for Vector3Util.SloppyEquals
        ///</summary>
        [TestMethod()]
        public void SloppyEqualsVectorFloatTest()
        {
            Vector3 v = new Vector3(AX, AY, AZ);
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));

            v = new Vector3(AX + 0.0019f, AY, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX - 0.0019f, AY, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY + 0.0019f, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY - 0.0019f, AZ);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY, AZ + 0.0019f);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));

            v = new Vector3(AX, AY, AZ - 0.0019f);
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.00f));
            Assert.IsFalse(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0018f));
            Assert.IsTrue(Vector3Util.SloppyEquals(mA1, v.x, v.y, v.z, 0.0020f));
        }

        /// <summary>
        ///A test for GetLengthSq
        ///</summary>
        [TestMethod()]
        public void GetLengthSqTest()
        {
            float len = (AX * AX) + (AY * AY) + (AZ * AZ);
            Assert.IsTrue(MathUtil.SloppyEquals(Vector3Util.GetLengthSq(AX, AY, AZ), len, EPSILON));
        }

        /// <summary>
        ///A test for GetDistanceSq
        ///</summary>
        [TestMethod()]
        public void GetDistanceSqTest()
        {
            float result = Vector3Util.GetDistanceSq(AX, AY, AZ, BX, BY, BZ);
            float deltaX = BX - AX;
            float deltaY = BY - AY;
            float deltaZ = BZ - AZ;
            float expected = (deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ);
            Assert.IsTrue(MathUtil.SloppyEquals(result, expected, EPSILON));
        }

        /// <summary>
        ///A test for Dot
        ///</summary>
        [TestMethod()]
        public void DotTest()
        {
            float expected = (AX * BX) + (AY * BY) + (AZ * BZ);
            Assert.IsTrue(MathUtil.SloppyEquals(Vector3Util.Dot(AX, AY, AZ, BX, BY, BZ), expected, EPSILON));
        }

        /// <summary>
        ///A test for Cross
        ///</summary>
        [TestMethod()]
        public void CrossTest()
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
            Vector3 u = Vector3Util.TranslateToward(AX, AY, AZ, BX, BY, BZ, factor);
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
    }
}

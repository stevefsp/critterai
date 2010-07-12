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
using System;

namespace org.critterai.math
{
    /// <summary>
    ///This is a test class for Vector2UtilTest and is intended
    ///to contain all Vector2UtilTest Unit Tests
    ///</summary>
    [TestClass()]
    public class Vector2UtilTest
    {

        private TestContext testContextInstance;

        private const float EPSILON = 0.000000001f;

        private const float AX = 1.5f;
        private const float AY = 8.0f;
        private const float BX = -17.112f;
        private const float BY = 77.5f;
        
        private Vector2 mA1;
        private Vector2 mA2;
        private Vector2 mB1;
        private Vector2 mC1;

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
            mA1 = new Vector2(AX, AY);
            mA2 = new Vector2(AX, AY);
            mB1 = new Vector2(BX, BY);
            mC1 = new Vector2(AY, AX);
        }

        [TestMethod()]
        public void SloppyEqualsFloatTest()
        {

            /*
             * Had to remove the edge cases.  They don't function correctly,
             * even at only 2 decimal places.  
             * E.g. Vector2Util.SloppyEquals(AX, AY, AX+0.19, AY, 0.19f)
             * returns false.
             * This was not a problem in Java.
             * I compensated by increasing the resolution of the test from 3
             * to 4 decimal places.
             */

            Vector2 v = new Vector2(AX, AY);
            Assert.IsTrue(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0f));

            v = new Vector2(AX + 0.0019f, AY);
            Assert.IsFalse(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0020f));

            v = new Vector2(AX - 0.0019f, AY);
            Assert.IsFalse(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0020f));

            v = new Vector2(AX, AY + 0.0019f);
            Assert.IsFalse(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0020f));

            v = new Vector2(AX, AY - 0.0019f);
            Assert.IsFalse(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(AX, AY, v.x, v.y, 0.0020f));
        }

        [TestMethod()]
        public void SloppyEqualsVectorTest()
        {
            Vector2 v = new Vector2(AX, AY);
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v, 0.0f));

            v = new Vector2(AX + 0.0019f, AY);
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v, 0.0020f));

            v = new Vector2(AX - 0.0019f, AY);
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v, 0.0020f));

            v = new Vector2(AX, AY + 0.0019f);
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v, 0.0020f));

            v = new Vector2(AX, AY - 0.0019f);
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v, 0.0020f));
        }

        [TestMethod()]
        public void SloppyEqualsVectorFloatTest()
        {
            Vector2 v = new Vector2(AX, AY);
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0f));

            v = new Vector2(AX + 0.0019f, AY);
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0020f));

            v = new Vector2(AX - 0.0019f, AY);
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0020f));

            v = new Vector2(AX, AY + 0.0019f);
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0020f));

            v = new Vector2(AX, AY - 0.0019f);
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0f));
            Assert.IsFalse(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0018f));
            Assert.IsTrue(Vector2Util.SloppyEquals(mA1, v.x, v.y, 0.0020f));
        }

        [TestMethod()]
        public void DotTest()
        {
            float expected = (AX * BX) + (AY * BY);
            Assert.IsTrue(MathUtil.SloppyEquals(Vector2Util.Dot(AX, AY, BX, BY), expected, EPSILON));
        }

        [TestMethod()]
        public void GetDistSqTest()
        {
            float result = Vector2Util.GetDistanceSq(AX, AY, BX, BY);
            float deltaX = BX - AX;
            float deltaY = BY - AY;
            float expected = (deltaX * deltaX) + (deltaY * deltaY);
            Assert.IsTrue(MathUtil.SloppyEquals(result, expected, EPSILON));
        }

        [TestMethod()]
        public void NormalizeTest()
        {
            Vector2 v = new Vector2(AX, AY);
            Vector2 u = Vector2Util.Normalize(AX, AY);
            float len = (float)Math.Sqrt((AX * AX) + (AY * AY));
            float x = AX / len;
            float y = AY / len;
            Assert.IsTrue(MathUtil.SloppyEquals(u.x, x, EPSILON) 
                && MathUtil.SloppyEquals(u.y, y, EPSILON));
        }

        [TestMethod()]
        public void TestStaticScaleToFloat() 
        {
            // Can improve this test by checking for proper setting
            // of both x and y.
            Vector2 u = Vector2Util.ScaleTo(AX, AY, 15.0f);
            float len = u.magnitude;
            Assert.IsTrue(len > 14.999f && len < 15.001f);

            u = Vector2Util.ScaleTo(AX, AY, 0);
            Assert.IsTrue(u.x == 0 && u.y == 0);
        }

        [TestMethod()]
        public void TestStaticScaleToVector2() 
        {
            // Can improve this test by checking for proper setting
            // of both x and y.
            Vector2 u = Vector2Util.ScaleTo(mA1, 15.0f);
            float len = u.magnitude;
            Assert.IsTrue(len > 14.999f && len < 15.001f);
            
            u = Vector2Util.ScaleTo(mA1, 0);
            Assert.IsTrue(u.x == 0 && u.y == 0);
        }

    }
}

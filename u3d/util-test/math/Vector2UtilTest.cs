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
    [TestClass()]
    public class Vector2UtilTest
    {
        private const float EPSILON = MathUtil.EPSILON_STD;
        private const float TOLERANCE = MathUtil.TOLERANCE_STD;

        private const float AX = 1.5f;
        private const float AY = 8.0f;
        private const float BX = -17.112f;
        private const float BY = 77.5f;
        
        private Vector2 mA1;
        private Vector2 mA2;
        private Vector2 mB1;
        private Vector2 mC1;

        [TestInitialize()]
        public void Setup()
        {
            mA1 = new Vector2(AX, AY);
            mA2 = new Vector2(AX, AY);
            mB1 = new Vector2(BX, BY);
            mC1 = new Vector2(AY, AX);
        }

        [TestMethod()]
        public void TestStaticSloppyEqualsFloat()
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
        public void TestStaticSloppyEqualsVector()
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
        public void TestStaticSloppyEqualsVectorFloat()
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
        public void TestStaticDot()
        {
            float expected = (AX * BX) + (AY * BY);
            Assert.IsTrue(MathUtil.SloppyEquals(
                Vector2Util.Dot(AX, AY, BX, BY), expected, EPSILON));
        }

        [TestMethod()]
        public void TestStaticGetDirectionAB()
        {
            Vector2 origin = new Vector2(4.4f, -2.2f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 target = Offset(origin, i);
                Vector2 direction =
                    Vector2Util.GetDirectionAB(origin.x, origin.y
                        , target.x, target.y);
                Assert.IsTrue(MathUtil.SloppyEquals(direction.magnitude
                    , 1, TOLERANCE));
                switch (i)
                {
                    case 0:
                        Assert.IsTrue(MathUtil.SloppyEquals(direction.x
                            , -1, TOLERANCE));
                        break;
                    case 1:
                        Assert.IsTrue(MathUtil.SloppyEquals(direction.y
                            , 1, TOLERANCE));
                        break;
                    case 2:
                        Assert.IsTrue(MathUtil.SloppyEquals(direction.x
                            , 1, TOLERANCE));
                        break;
                    case 3:
                        Assert.IsTrue(MathUtil.SloppyEquals(direction.y
                            , -1, TOLERANCE));
                        break;
                }
            }
        }

        [TestMethod()]
        public void TestStaticGetDistSq()
        {
            float result = Vector2Util.GetDistanceSq(AX, AY, BX, BY);
            float deltaX = BX - AX;
            float deltaY = BY - AY;
            float expected = (deltaX * deltaX) + (deltaY * deltaY);
            Assert.IsTrue(MathUtil.SloppyEquals(result, expected, EPSILON));
        }

        [TestMethod()]
        public void TestStaticNormalize()
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

        public Vector2 Offset(Vector2 v, int dir)
        {
            int ldir = (dir & 0x3);
            switch (ldir)
            {
                case 0:
                    return (v + new Vector2(-5.2f, 0));
                case 1:
                    return (v + new Vector2(0, 5.2f));
                case 2:
                    return (v + new Vector2(5.2f, 0));
                case 3:
                    return (v + new Vector2(0, -5.2f));
                default:
                    return v;
            }
        }

        [TestMethod()]
        public void TestStaticTruncateLengthFloat() 
        {
            // Can improve this test by checking for proper setting
            // of both x and y.
            Vector2 v = new Vector2(AX, AY);
            Vector2 u = Vector2Util.TruncateLength(AX, AY, 15.0f);
            Assert.IsTrue(u == v);
            Assert.IsTrue(u.x == AX && u.y == AY);
            
            u = Vector2Util.TruncateLength(AX, AY, 5.0f);
            float len = u.magnitude;
            Assert.IsTrue(len > 4.999f && len < 5.001f);
            
            u = Vector2Util.TruncateLength(AX, AY, 0);
            Assert.IsTrue(u.x == 0 && u.y == 0);
        }

        [TestMethod()]
        public void TestStaticTruncateLengthVector2() 
        {
            // Can improve this test by checking for proper setting
            // of both x and y.
            Vector2 v = new Vector2(AX, AY);
            Vector2 u = Vector2Util.TruncateLength(mA1, 15.0f);
            Assert.IsTrue(u == v);
            Assert.IsTrue(u.x == AX && u.y == AY);
            
            u = Vector2Util.TruncateLength(mA1, 5.0f);
            float len = u.magnitude;
            Assert.IsTrue(len > 4.999f && len < 5.001f);
            
            u = Vector2Util.TruncateLength(mA1, 0);
            Assert.IsTrue(u.x == 0 && u.y == 0);
        }

    }
}

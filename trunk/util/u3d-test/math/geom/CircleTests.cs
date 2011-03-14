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

namespace org.critterai.math.geom
{
    [TestClass()]
    public class CircleTests
    {
        public const float TOLERANCE = MathUtil.TOLERANCE_STD;
        public const float AX = 1;
        public const float AY = 2;
        public const float AR = 1.5f;
        public const float BR = 2.3f;

        [TestMethod()]
        public void TestStaticIntersectsXAxis()
        {
            Assert.IsTrue(Circle.Intersects(AX, AY, AR
                , AX - (BR + AR) + TOLERANCE, AY, BR));
            Assert.IsFalse(Circle.Intersects(AX, AY, AR
                , AX - (BR + AR) - TOLERANCE, AY, BR));

            Assert.IsTrue(Circle.Intersects(AX, AY, AR
                , AX + (BR + AR) - TOLERANCE, AY, BR));
            Assert.IsFalse(Circle.Intersects(AX, AY, AR
                , AX + (BR + AR) + TOLERANCE, AY, BR));
        }

        [TestMethod()]
        public void TestStaticIntersectsYAxis()
        {
            Assert.IsTrue(Circle.Intersects(AX, AY, AR
                , AX, AY - (BR + AR) + TOLERANCE, BR));
            Assert.IsFalse(Circle.Intersects(AX, AY, AR
                , AX, AY - (BR + AR) - TOLERANCE, BR));

            Assert.IsTrue(Circle.Intersects(AX, AY, AR
                , AX, AY + (BR + AR) - TOLERANCE, BR));
            Assert.IsFalse(Circle.Intersects(AX, AY, AR
                , AX, AY + (BR + AR) + TOLERANCE, BR));
        }

        [TestMethod()]
        public void TestStaticContainsXAxis()
        {
            Assert.IsTrue(Circle.Contains(AX - AR + TOLERANCE, AY
                , AX, AY, AR));
            Assert.IsFalse(Circle.Contains(AX - AR - TOLERANCE, AY
                , AX, AY, AR));

            Assert.IsTrue(Circle.Contains(AX + AR - TOLERANCE, AY
                , AX, AY, AR));
            Assert.IsFalse(Circle.Contains(AX + AR + TOLERANCE, AY
                , AX, AY, AR));
        }

        [TestMethod()]
        public void TestStaticContainsYAxis()
        {
            Assert.IsTrue(Circle.Contains(AX, AY - AR + TOLERANCE
                , AX, AY, AR));
            Assert.IsFalse(Circle.Contains(AX, AY - AR - TOLERANCE
                , AX, AY, AR));

            Assert.IsTrue(Circle.Contains(AX, AY + AR - TOLERANCE
                , AX, AY, AR));
            Assert.IsFalse(Circle.Contains(AX, AY + AR + TOLERANCE
                , AX, AY, AR));
        }
    }
}

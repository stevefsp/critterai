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
using org.critterai.math.geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace org.critterai.math.geom
{
    [TestClass()]
    public class Triangle2Test
    {
        // Clockwise wrapped
        private const float AX = 3;
        private const float AY = 2;
        private const float BX = 2;
        private const float BY = -1;
        private const float CX = 0;
        private const float CY = -1;
        
        // Clockwise Wrapped
        private const int AXI = 3;
        private const int AYI = 2;
        private const int BXI = 2;
        private const int BYI = -1;
        private const int CXI = 0;
        private const int CYI = -1;

        public const float TOLERANCE = MathUtil.TOLERANCE_STD;

        [TestMethod()]
        public void TestStaticGetSignedAreaX2Float()
        {
            float result = Triangle2.GetSignedAreaX2(AX, AY, BX, BY, CX, CY);
            Assert.IsTrue(result == -6);
            result = Triangle2.GetSignedAreaX2(AX, AY, CX, CY, BX, BY);
            Assert.IsTrue(result == 6);
        }

        [TestMethod()]
        public void TestStaticGetSignedAreaX2Int()
        {
            float result = 
                Triangle2.GetSignedAreaX2(AXI, AYI, BXI, BYI, CXI, CYI);
            Assert.IsTrue(result == -6);
            result = 
                Triangle2.GetSignedAreaX2(AXI, AYI, CXI, CYI, BXI, BYI);
            Assert.IsTrue(result == 6);
        }

        [TestMethod()]
        public void TestStaticContains()
        {
            // Vertex inclusion tests

            Assert.IsTrue(Triangle2.Contains(AX, AY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(BX, BY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(BX - TOLERANCE, BY + TOLERANCE
                , AX, AY, BX, BY, CX, CY));
            Assert.IsFalse(
                Triangle2.Contains(BX + TOLERANCE, BY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(CX, CY, AX, AY, BX, BY, CX, CY));

            // Wall inclusion tests

            float midpointX = AX + (BX - AX) / 2;
            float midpointY = AY + (BY - AY) / 2;
            Assert.IsTrue(Triangle2.Contains(
                midpointX, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(
                midpointX - TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsFalse(Triangle2.Contains
                (midpointX + TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
            midpointX = BX + (CX - BX) / 2;
            midpointY = BY + (CY - BY) / 2;
            Assert.IsTrue(Triangle2.Contains(
                midpointX, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(
                midpointX, midpointY + TOLERANCE, AX, AY, BX, BY, CX, CY));
            Assert.IsFalse(Triangle2.Contains(
                midpointX, midpointY - TOLERANCE, AX, AY, BX, BY, CX, CY));
            midpointX = CX + (AX - CX) / 2;
            midpointY = CY + (AY - CY) / 2;
            Assert.IsTrue(Triangle2.Contains(
                midpointX, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(
                midpointX + TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsFalse(Triangle2.Contains(
                midpointX - TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
        }
    }
}

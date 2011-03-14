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

namespace org.critterai.geom
{
    [TestClass()]
    public class Line2Test
    {
        private const float TOLERANCE = MathUtil.TOLERANCE_STD;

        private const float AX = -5;
        private const float AY = 3;
        private const float BX = 1;
        private const float BY = 1;
        private const float CX = -3;
        private const float CY = 0;
        private const float DX = -1;
        private const float DY = 4;
        private const float EX = -2;
        private const float EY = 2;
        private const float FX = 4;
        private const float FY = 0;
        private const float GX = 0;
        private const float GY = 3;
        private const float HX = 2;
        private const float HY = -1;
        private const float JX = -4;
        private const float JY = 1;
        private const float KX = -4;
        private const float KY = -2;
        
        private const int AXI = -5;
        private const int AYI = 3;
        private const int BXI = 1;
        private const int BYI = 1;
        private const int CXI = -3;
        private const int CYI = 0;
        private const int DXI = -1;
        private const int DYI = 4;
        private const int EXI = -2;
        private const int EYI = 2;
        private const int FXI = 4;
        private const int FYI = 0;
        private const int GXI = 0;
        private const int GYI = 3;

        [TestMethod()]
        public void TestStaticLinesIntersectInt()
        {
            // Standard. Cross within segments.
            Assert.IsTrue(Line2.LinesIntersect(
                AXI, AYI, BXI, BYI, CXI, CYI, DXI, DYI));

            // Standard. Cross outside segment.
            Assert.IsTrue(Line2.LinesIntersect(
                AXI, AYI, BXI, BYI, DXI, DYI, GXI, GYI));

            // Collinear
            Assert.IsTrue(Line2.LinesIntersect(
                AXI, AYI, BXI, BYI, EXI, EYI, FXI, FYI));

            // Parallel Diagonal
            Assert.IsFalse(Line2.LinesIntersect(
                AXI, AYI, BXI, BYI, EXI - 2, EYI, FXI - 2, FYI));

            // Parallel Vertical
            Assert.IsFalse(Line2.LinesIntersect(
                AXI, 5, BXI, 5, EXI, 3, FXI, 3));

            // Parallel Horizontal
            Assert.IsFalse(Line2.LinesIntersect(
                5, AYI, 5, BYI, 2, CYI, 2, DYI)); 
        }

        [TestMethod()]
        public void TestStaticLinesIntersectFloat()
        {
            // Standard. Cross within segments.
            Assert.IsTrue(Line2.LinesIntersect(AX, AY, BX, BY, CX, CY, DX, DY));
            Assert.IsTrue(Line2.LinesIntersect(BX, BY, AX, AY, CX, CY, DX, DY));
            Assert.IsTrue(Line2.LinesIntersect(BX, BY, AX, AY, DX, DY, CX, CY));
            Assert.IsTrue(Line2.LinesIntersect(AX, AY, BX, BY, DX, DY, CX, CY));

            // Standard. Cross outside segment.
            Assert.IsTrue(Line2.LinesIntersect(AX, AY, BX, BY, DX, DY, GX, GY));
            Assert.IsTrue(Line2.LinesIntersect(BX, BY, AX, AY, DX, DY, GX, GY));
            Assert.IsTrue(Line2.LinesIntersect(BX, BY, AX, AY, GX, GY, DX, DY));
            Assert.IsTrue(Line2.LinesIntersect(AX, AY, BX, BY, GX, GY, DX, DY));

            // Collinear
            Assert.IsTrue(Line2.LinesIntersect(AX, AY, BX, BY, EX, EY, FX, FY));
            Assert.IsTrue(Line2.LinesIntersect(BX, BY, AX, AY, EX, EY, FX, FY));
            Assert.IsTrue(Line2.LinesIntersect(BX, BY, AX, AY, FX, FY, EX, EY));
            Assert.IsTrue(Line2.LinesIntersect(AX, AY, BX, BY, FX, FY, EX, EY));

            // Parallel Diagonal
            Assert.IsFalse(
                Line2.LinesIntersect(AX, AY, BX, BY, EX - 2, EY, FX - 2, FY));
            Assert.IsFalse(
                Line2.LinesIntersect(BX, BY, AX, AY, EX - 2, EY, FX - 2, FY));
            Assert.IsFalse(
                Line2.LinesIntersect(BX, BY, AX, AY, FX - 2, FY, EX - 2, EY));
            Assert.IsFalse(
                Line2.LinesIntersect(AX, AY, BX, BY, FX - 2, FY, EX - 2, EY));

            // Parallel Vertical
            Assert.IsFalse(Line2.LinesIntersect(AX, 5, BX, 5, EX, 3, FX, 3));

            // Parallel Horizontal
            Assert.IsFalse(Line2.LinesIntersect(5, AY, 5, BY, 2, CY, 2, DY));
        }

        [TestMethod()]
        public void TestStaticGetPointSegmentDistanceSq()
        {
            // Closest to end point B.
            float expected = Vector2Util.GetDistanceSq(HX, HY, BX, BY);
            Assert.IsTrue(Line2.GetPointSegmentDistanceSq(
                HX, HY, EX, EY, BX, BY) == expected);

            // Closest to end point E.
            expected = Vector2Util.GetDistanceSq(JX, JY, EX, EY);
            Assert.IsTrue(Line2.GetPointSegmentDistanceSq(
                JX, JY, EX, EY, BX, BY) == expected);

            // Closest to mid-point of AB. (E)
            expected = Vector2Util.GetDistanceSq(-1, 5, EX, EY);
            Assert.IsTrue(Line2.GetPointSegmentDistanceSq(
                -1, 5, AX, AY, BX, BY) == expected);
            expected = Vector2Util.GetDistanceSq(-3, -1, EX, EY);
            Assert.IsTrue(Line2.GetPointSegmentDistanceSq(
                -3, -1, AX, AY, BX, BY) == expected);
        }

        [TestMethod()]
        public void TestStaticGetPointLineDistanceSq()
        {
            float expected = Vector2Util.GetDistanceSq(-3, -1, EX, EY);
            float actual = Line2.GetPointLineDistanceSq(-3, -1, AX, AY, BX, BY);
            Assert.IsTrue(actual == expected);
            expected = Vector2Util.GetDistanceSq(-1, 5, EX, EY);
            actual = Line2.GetPointLineDistanceSq(-1, 5, AX, AY, BX, BY);
            Assert.IsTrue(actual == expected);
            expected = 0;
            actual = Line2.GetPointLineDistanceSq(FX, FY, AX, AY, BX, BY);
            Assert.IsTrue(actual == expected);
        }

        [TestMethod()]
        public void TestStaticGetNormalAB()
        {
            // Diagonal
            Vector2 expected = Vector2Util.Normalize(-1, -3);
            Vector2 v = Line2.GetNormalAB(AX, AY, BX, BY);
            Assert.IsTrue(Vector2Util.SloppyEquals(v, expected, TOLERANCE));

            // Reversed Diagonal
            expected = Vector2Util.Normalize(1, 3);
            v = Line2.GetNormalAB(BX, BY, AX, AY);
            Assert.IsTrue(Vector2Util.SloppyEquals(v, expected, TOLERANCE));

            // Vertical
            expected = new Vector2(-1, 0);
            v = Line2.GetNormalAB(5, AY, 5, BY);
            Assert.IsTrue(Vector2Util.SloppyEquals(v, expected, TOLERANCE));

            // Horizontal
            expected = new Vector2(0, -1);
            v = Line2.GetNormalAB(AX, 5, BX, 5);
            Assert.IsTrue(Vector2Util.SloppyEquals(v, expected, TOLERANCE));

            // Not a line.
            v = Line2.GetNormalAB(AX, AY, AX, AY);
            Assert.IsTrue(v == Vector2.zero);
        }

        [TestMethod()]
        public void TestStaticGetRelationship()
        {
            Vector2 v = new Vector2();
            Assert.IsTrue(LineRelType.SegmentsIntersect
                    == Line2.GetRelationship(
                        AX, AY, BX, BY, CX, CY, DX, DY, out v));
            Assert.IsTrue(Vector2Util.SloppyEquals(v, EX, EY, TOLERANCE));

            Assert.IsTrue(LineRelType.ALineCrossesBSeg
                    == Line2.GetRelationship(
                        AX, AY, EX, EY, GX, GY, HX, HY, out v));
            Assert.IsTrue(Vector2Util.SloppyEquals(v, BX, BY, TOLERANCE));

            // Line reversal checks.
            v = new Vector2();
            Assert.IsTrue(LineRelType.ALineCrossesBSeg
                    == Line2.GetRelationship(
                        EX, EY, AX, AY, GX, GY, HX, HY, out v));
            Assert.IsTrue(Vector2Util.SloppyEquals(v, BX, BY, TOLERANCE));
            v = new Vector2();
            Assert.IsTrue(LineRelType.ALineCrossesBSeg
                    == Line2.GetRelationship(
                        EX, EY, AX, AY, HX, HY, GX, GY, out v));
            Assert.IsTrue(Vector2Util.SloppyEquals(v, BX, BY, TOLERANCE));
            v = new Vector2();
            Assert.IsTrue(LineRelType.ALineCrossesBSeg
                    == Line2.GetRelationship(
                        AX, AY, EX, EY, HX, HY, GX, GY, out v));
            Assert.IsTrue(Vector2Util.SloppyEquals(v, BX, BY, TOLERANCE));

            Assert.IsTrue(LineRelType.BLineCrossesASeg
                    == Line2.GetRelationship(
                        AX, AY, BX, BY, KX, KY, CX, CY, out v));
            Assert.IsTrue(Vector2Util.SloppyEquals(v, EX, EY, TOLERANCE));

            v = new Vector2();
            Assert.IsTrue(LineRelType.LinesIntersect
                    == Line2.GetRelationship(
                        KX, KY, CX, CY, FX, FY, BX, BY, out v));
            Assert.IsTrue(Vector2Util.SloppyEquals(v, EX, EY, TOLERANCE));

            v = new Vector2(JX, JY);
            Assert.IsTrue(LineRelType.Parallel
                    == Line2.GetRelationship(
                        AX, AY, BX, BY, EX - 2, EY, FX - 2, FY, out v));
            Assert.IsTrue(v == Vector2.zero);

            // Collinear - No segment overlap.
            v = new Vector2(JX, JY);
            Assert.IsTrue(LineRelType.Collinear
                    == Line2.GetRelationship(
                        AX, AY, EX, EY, BX, BY, FX, FY, out v));
            Assert.IsTrue(v == Vector2.zero);

            // Collinear - Segment overlap.
            v = new Vector2(JX, JY);
            Assert.IsTrue(LineRelType.Collinear
                    == Line2.GetRelationship(
                        AX, AY, BX, BY, EX, EY, FX, FY, out v));
            Assert.IsTrue(v == Vector2.zero);
        }
    }
}

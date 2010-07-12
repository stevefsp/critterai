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
using UnityEngine;

namespace org.critterai.math.geom
{
    /// <summary>
    ///This is a test class for Polygon3Test and is intended
    ///to contain all Polygon3Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Polygon3Test
    {

        private TestContext testContextInstance;

    private const float AX = -2;
    private const float AY = -2;
    private const float AZ = 1;
    private const float BX = -1;
    private const float BY = 0;
    private const float BZ = 2;
    private const float CX = 0;
    private const float CY = 2;
    private const float CZ = 2;
    private const float DX = 1;
    private const float DY = 4;
    private const float DZ = 1;
    private const float EX = 1;
    private const float EY = 4;
    private const float EZ = 0;
    private const float FX = 0;
    private const float FY = 2;
    private const float FZ = -1;
    private const float GX = -1;
    private const float GY = 0;
    private const float GZ = -1;
    private const float HX = -2;
    private const float HY = -2;
    private const float HZ = 0;
    private const float JX = 2;
    private const float JY = 6;
    private const float JZ = 1;
    private const float KX = -4;
    private const float KY = 0;
    private const float KZ = 2;
    
    private const float CENX = -0.5f;
    private const float CENY = 1.0f;
    private const float CENZ = 0.5f;
    
    private float[] mVerts;

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
            mVerts = new float[10 * 3];
            mVerts[0] = KX;    // Padding
            mVerts[1] = KY;
            mVerts[2] = KZ;
            mVerts[3] = AX;    // Start of poly
            mVerts[4] = AY;
            mVerts[5] = AZ;
            mVerts[6] = BX;
            mVerts[7] = BY;
            mVerts[8] = BZ;
            mVerts[9] = CX;
            mVerts[10] = CY;
            mVerts[11] = CZ;
            mVerts[12] = DX;
            mVerts[13] = DY;
            mVerts[14] = DZ;
            mVerts[15] = EX; // J insertion point.
            mVerts[16] = EY;
            mVerts[17] = EZ;
            mVerts[18] = FX;
            mVerts[19] = FY;
            mVerts[20] = FZ;
            mVerts[21] = GX;
            mVerts[22] = GY;
            mVerts[23] = GZ;
            mVerts[24] = HX;
            mVerts[25] = HY;
            mVerts[26] = HZ; // End of poly
            mVerts[27] = KX; // Padding and centroid storage location.
            mVerts[28] = KY;
            mVerts[29] = KZ;

        }

        /// <summary>
        ///A test for IsConvex
        ///</summary>
        [TestMethod()]
        public void IsConvexStandardTrueTest()
        {
            Assert.IsTrue(Polygon3.IsConvex(mVerts, 1, 8));
        }

        /// <summary>
        ///A test for IsConvex
        ///</summary>
        [TestMethod()]
        public void IsConvexStandardFalseTest()
        {
            mVerts[15] = JX;
            mVerts[16] = JY;
            mVerts[17] = JZ;
            Assert.IsFalse(Polygon3.IsConvex(mVerts, 1, 8));
        }

        /// <summary>
        ///A test for IsConvex
        ///</summary>
        [TestMethod()]
        public void IsConvexVerticalTrueTest()
        {
            for (int p = 1; p < mVerts.Length; p += 3)
            {
                mVerts[p] = mVerts[p + 1];
                mVerts[p + 1] = -2;
            }
            Assert.IsTrue(Polygon3.IsConvex(mVerts, 1, 8));
        }

        /// <summary>
        ///A test for IsConvex
        ///</summary>
        [TestMethod()]
        public void IsConvexVerticalFalseTest()
        {
            mVerts[15] = JX;
            mVerts[16] = JY;
            mVerts[17] = JZ;
            for (int p = 1; p < mVerts.Length; p += 3)
            {
                mVerts[p] = mVerts[p + 1];
                mVerts[p + 1] = -2;
            }
            Assert.IsFalse(Polygon3.IsConvex(mVerts, 1, 8));
        }

        /// <summary>
        ///A test for GetCentroid
        ///</summary>
        [TestMethod()]
        public void GetCentroidArrayTest()
        {
            Assert.IsTrue(mVerts == Polygon3.GetCentroid(mVerts, 1, 8, mVerts, 9));
            Assert.IsTrue(mVerts[27] == CENX);
            Assert.IsTrue(mVerts[28] == CENY);
            Assert.IsTrue(mVerts[29] == CENZ);
        }

        /// <summary>
        ///A test for GetCentroid
        ///</summary>
        [TestMethod()]
        public void GetCentroidVectorTest()
        {
            Vector3 v = Polygon3.GetCentroid(mVerts, 1, 8);
            Assert.IsTrue(v.x == CENX);
            Assert.IsTrue(v.y == CENY);
            Assert.IsTrue(v.z == CENZ);
        }

        /// <summary>
        ///A test for GetCentroid
        ///</summary>
        [TestMethod()]
        public void GetCentroidFloatListTest()
        {
            Vector3 v = Polygon3.GetCentroid(
                    AX, AY, AZ
                    , BX, BY, BZ
                    , CX, CY, CZ
                    , DX, DY, DZ
                    , EX, EY, EZ
                    , FX, FY, FZ
                    , GX, GY, GZ
                    , HX, HY, HZ);
            Assert.IsTrue(v.x == CENX);
            Assert.IsTrue(v.y == CENY);
            Assert.IsTrue(v.z == CENZ);
        }

    }
}

using org.critterai.math.geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using System;

namespace org.critterai.math.geom
{
    /// <summary>
    ///This is a test class for Triangle3Test and is intended
    ///to contain all Triangle3Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Triangle3Test
    {
        private TestContext testContextInstance;

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

        /// <summary>
        ///A test for GetNormal
        ///</summary>
        [TestMethod()]
        public void GetArea()
        {
            float expected = getHeronArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
            float actual = Triangle3.GetArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
            Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, 0.0001f));
        }

        /// <summary>
        ///A test for GetNormal
        ///</summary>
        [TestMethod()]
        public void GetAreaComp()
        {
            float expected = getHeronArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
            float actual = (float)Math.Sqrt(Triangle3.GetAreaComp(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ)) / 2;
            Assert.IsTrue(MathUtil.SloppyEquals(actual, expected, 0.0001f));
        }

        /// <summary>
        ///A test for GetAreaComp
        ///</summary>
        [TestMethod()]
        public void GetNormalFloatVector3()
        {
            Vector3 v = Triangle3.GetNormal(AX, AY, 0, BX, BY, 0, CX, CY, 0);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 0, 0, -1, 0.0001f));
            v = Triangle3.GetNormal(AX, AY, 0, CX, CY, 0, BX, BY, 0);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 0, 0, 1, 0.0001f));
            v = Triangle3.GetNormal(AX, 0, AZ, BX, 0, BZ, CX, 0, CZ);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 0, -1, 0, 0.0001f));
            v = Triangle3.GetNormal(0, AY, AZ, 0, BY, BZ, 0, CY, CZ);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 1, 0, 0, 0.0001f));
        }

        /// <summary>
        ///A test for GetArea
        ///</summary>
        [TestMethod()]
        public void GetNormalArrayVector3()
        {
            float[] vertices = {
                5, 5, 5
                , AX, 0, AZ
                , BX, 0, BZ
                , CX, 0, CZ
                , 9, 9, 9
            };
            Vector3 v = Triangle3.GetNormal(vertices, 1);
            Assert.IsTrue(Vector3Util.SloppyEquals(v, 0, -1, 0, 0.0001f));
        }

        private float getHeronArea(float ax, float ay, float az
                , float bx, float by, float bz
                , float cx, float cy, float cz)
        {
            double a = Math.Sqrt(Vector3Util.GetDistanceSq(AX, AY, AZ, BX, BY, BZ));
            double b = Math.Sqrt(Vector3Util.GetDistanceSq(AX, AY, AZ, CX, CY, CZ));
            double c = Math.Sqrt(Vector3Util.GetDistanceSq(CX, CY, CZ, BX, BY, BZ));
            double s = (a + b + c) / 2;
            return (float)Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }
    }
}

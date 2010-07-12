using org.critterai.math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace org.critterai.math
{
    /// <summary>
    ///This is a test class for MathUtilTest and is intended
    ///to contain all MathUtilTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MathUtilTest
    {
        private TestContext testContextInstance;

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
        ///A test for SloppyEquals
        ///</summary>
        [TestMethod()]
        public void SloppyEqualsTest()
        {
            float tol = 0.1f;
            Assert.IsTrue(MathUtil.SloppyEquals(5, 5.09f, tol));
            Assert.IsTrue(MathUtil.SloppyEquals(5, 4.91f, tol));
            Assert.IsTrue(MathUtil.SloppyEquals(5, 5.10f, tol));
            Assert.IsTrue(MathUtil.SloppyEquals(5, 4.90f, tol));
            Assert.IsFalse(MathUtil.SloppyEquals(5, 5.101f, tol));
            Assert.IsFalse(MathUtil.SloppyEquals(5, 4.899f, tol));
        }

        /// <summary>
        ///A test for Min
        ///</summary>
        [TestMethod()]
        public void MinTest()
        {
            Assert.IsTrue(MathUtil.Min(2) == 2);
            Assert.IsTrue(MathUtil.Min(-1, 0, 1, 2) == -1);
            Assert.IsTrue(MathUtil.Min(2, 2, -1, 0) == -1);
        }

        /// <summary>
        ///A test for Max
        ///</summary>
        [TestMethod()]
        public void MaxTest()
        {
            Assert.IsTrue(MathUtil.Max(2) == 2);
            Assert.IsTrue(MathUtil.Max(-1, 0, 1, 2) == 2);
            Assert.IsTrue(MathUtil.Max(-1, 2, -1, 0) == 2);
        }

        /// <summary>
        ///A test for ClampToPositiveNonZero
        ///</summary>
        [TestMethod()]
        public void ClampToPositiveNonZeroTest()
        {
            Assert.IsTrue(MathUtil.ClampToPositiveNonZero(0.1f) == 0.1f);
            Assert.IsTrue(MathUtil.ClampToPositiveNonZero(float.Epsilon) == float.Epsilon);
            Assert.IsTrue(MathUtil.ClampToPositiveNonZero(0) == float.Epsilon);
            Assert.IsTrue(MathUtil.ClampToPositiveNonZero(-float.Epsilon) == float.Epsilon);
        }

        /// <summary>
        ///A test for Clamp
        ///</summary>
        [TestMethod()]
        public void ClampIntTest()
        {
            Assert.IsTrue(MathUtil.Clamp(5, 4, 6) == 5);
            Assert.IsTrue(MathUtil.Clamp(4, 4, 6) == 4);
            Assert.IsTrue(MathUtil.Clamp(3, 4, 6) == 4);
            Assert.IsTrue(MathUtil.Clamp(6, 4, 6) == 6);
            Assert.IsTrue(MathUtil.Clamp(7, 4, 6) == 6);
        }
    }
}

using org.critterai.math.geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace org.critterai.math.geom
{
    
    
    /// <summary>
    ///This is a test class for Triangle2Test and is intended
    ///to contain all Triangle2Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Triangle2Test
    {
        private TestContext testContextInstance;

        // Clockwise wrapped
        private const float AX = 3;
        private const float AY = 2;
        private const float BX = 2;
        private const float BY = -1;
        private const float CX = 0;
        private const float CY = -1;
        
        // Clockwise Wrapped
        private const float AXI = 3;
        private const float AYI = 2;
        private const float BXI = 2;
        private const float BYI = -1;
        private const float CXI = 0;
        private const float CYI = -1;
        
        public const float TOLERANCE = 0.0001f;

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
        ///A test for GetSignedAreaX2 Float
        ///</summary>
        [TestMethod()]
        public void GetSignedAreaX2FloatTest()
        {
            float result = Triangle2.GetSignedAreaX2(AX, AY, BX, BY, CX, CY);
            Assert.IsTrue(result == -6);
            result = Triangle2.GetSignedAreaX2(AX, AY, CX, CY, BX, BY);
            Assert.IsTrue(result == 6);
        }

        /// <summary>
        ///A test for GetSignedAreaX2 Int
        ///</summary>
        [TestMethod()]
        public void GetSignedAreaX2IntTest()
        {
            float result = Triangle2.GetSignedAreaX2(AXI, AYI, BXI, BYI, CXI, CYI);
            Assert.IsTrue(result == -6);
            result = Triangle2.GetSignedAreaX2(AXI, AYI, CXI, CYI, BXI, BYI);
            Assert.IsTrue(result == 6);
        }

        /// <summary>
        ///A test for Contains
        ///</summary>
        [TestMethod()]
        public void ContainsTest()
        {
            // Vertex inclusion tests

            Assert.IsTrue(Triangle2.Contains(AX, AY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(BX, BY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(BX - TOLERANCE, BY + TOLERANCE, AX, AY, BX, BY, CX, CY));
            Assert.IsFalse(Triangle2.Contains(BX + TOLERANCE, BY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(CX, CY, AX, AY, BX, BY, CX, CY));

            // Wall inclusion tests

            float midpointX = AX + (BX - AX) / 2;
            float midpointY = AY + (BY - AY) / 2;
            Assert.IsTrue(Triangle2.Contains(midpointX, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(midpointX - TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsFalse(Triangle2.Contains(midpointX + TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
            midpointX = BX + (CX - BX) / 2;
            midpointY = BY + (CY - BY) / 2;
            Assert.IsTrue(Triangle2.Contains(midpointX, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(midpointX, midpointY + TOLERANCE, AX, AY, BX, BY, CX, CY));
            Assert.IsFalse(Triangle2.Contains(midpointX, midpointY - TOLERANCE, AX, AY, BX, BY, CX, CY));
            midpointX = CX + (AX - CX) / 2;
            midpointY = CY + (AY - CY) / 2;
            Assert.IsTrue(Triangle2.Contains(midpointX, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsTrue(Triangle2.Contains(midpointX + TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
            Assert.IsFalse(Triangle2.Contains(midpointX - TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
        }
    }
}

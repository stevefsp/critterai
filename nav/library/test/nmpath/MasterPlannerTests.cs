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

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Summary description for MasterNavigatorTest
    /// </summary>
    [TestClass]
    public class MasterNavigatorTest
    {
        /*
         * Design notes:
         * 
         * Only some very basic checks are performed by this suite.
         * Most validations have to wait until the Nav test suite.
         * 
         * Not validating the count getters.  They are validated all 
         * over the place in the Nav test suite.
         */

        private ITestMesh mMesh;
        private float[] verts;
        private int[] indices;
        private TriCell[] cells;
        private TriNavMesh mNavMesh;

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

        [TestInitialize()]
        public void SetUp()
        {
            mMesh = new CorridorMesh();
            verts = mMesh.GetVerts();
            indices = mMesh.GetIndices();
            cells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(cells);
            mNavMesh = TriNavMesh.Build(verts, indices, 5, 0.5f, 0.1f);
        }

        [TestMethod]
        public void TestMasterNavigator() 
        {
            MasterPlanner mn = new MasterPlanner(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , long.MaxValue
                    , 60000
                    , 4
                    , 2);
            Assert.IsTrue(mn.MaxPathAge == 60000);
            Assert.IsTrue(mn.MaxProcessingTimeslice == long.MaxValue);
            Assert.IsTrue(mn.RepairSearchDepth == 4);
            Assert.IsTrue(mn.IsDisposed == false);
        }

        [TestMethod]
        public void TestNavigator()
        {
            MasterPlanner mn = new MasterPlanner(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , 60000
                    , 4
                    , 2);
            Assert.IsTrue(mn.PathPlanner != null);
            Assert.IsTrue(mn.PathPlanner == mn.PathPlanner); // Same reference across multiple calls.
        }

        [TestMethod]
        public void TestDispose() 
        {
            MasterPlanner mn = new MasterPlanner(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , 60000
                    , 4, 2);
            mn.Dispose();
            Assert.IsTrue(mn.IsDisposed == true);
        }

        [TestMethod]
        public void TestProcess() 
        {
            MasterPlanner mn = new MasterPlanner(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , 60000
                    , 4
                    , 2);
            // Just make sure no exceptions are thrown
            // when there is nothing to do.
            mn.Process(false);
            mn.Process(true);
            mn.Dispose();
            mn.Process(false);
            mn.Process(true);
        }
        
        [TestMethod]
        public void TestProcessOnce() 
        {
            MasterPlanner mn = new MasterPlanner(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , 60000
                    , 4
                    , 2);
            // Just make sure no exceptions are thrown
            // when there is nothing to do.
            mn.ProcessOnce(false);
            mn.ProcessOnce(true);
            mn.Dispose();
            mn.ProcessOnce(false);
            mn.ProcessOnce(true);
        }
        
        [TestMethod]
        public void TestProcessAll() 
        {
            MasterPlanner mn = new MasterPlanner(mNavMesh
                    , DistanceHeuristicType.LongestAxis
                    , int.MaxValue
                    , 60000
                    , 4
                    , 2);
            // Just make sure no exceptions are thrown
            // when there is nothing to do.
            mn.ProcessAll(false);
            mn.ProcessAll(true);
            mn.Dispose();
            mn.ProcessAll(false);
            mn.ProcessAll(true);
        }
    }
}

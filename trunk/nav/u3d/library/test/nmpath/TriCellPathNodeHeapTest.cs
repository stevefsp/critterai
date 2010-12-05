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
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.critterai.math;
using org.critterai.math.geom;
using UnityEngine;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Summary description for TriCellPathNodeHeap
    /// </summary>
    [TestClass]
    public class TriCellPathNodeHeapTest
    {
        /*
        * Design notes:
        * 
        * There are a lot of dependencies.  The tests are ordered
        * from minimal to maximal dependence.
        * 
        * If the TriCellPathNode test suite fails, fix it before fixing any errors
        * from this test suite.
        */

        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;

        private TestContext testContextInstance;

        private ITestMesh mMesh;
        private float[] verts;
        private int[] indices;
        private TriCell[] cells;
        private Vector3[] cents;
        // Dev Note: Individual tests alter the content of the nodes list.
        List<TriCellPathNode> nodes;

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
            mMesh = new CorridorMesh();
            verts = mMesh.GetVerts();
            indices = mMesh.GetIndices();
            cells = TestUtil.GetAllCells(verts, indices);
            TestUtil.LinkAllCells(cells);
            cents = new Vector3[cells.Length];
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                int pCell = iCell * 3;
                Vector3 cent = Polygon3.GetCentroid(verts[indices[pCell] * 3]
                                                , verts[indices[pCell] * 3 + 1]
                                                , verts[indices[pCell] * 3 + 2]
                                                , verts[indices[pCell + 1] * 3]
                                                , verts[indices[pCell + 1] * 3 + 1]
                                                , verts[indices[pCell + 1] * 3 + 2]
                                                , verts[indices[pCell + 2] * 3]
                                                , verts[indices[pCell + 2] * 3 + 1]
                                                , verts[indices[pCell + 2] * 3 + 2]);
                cents[iCell] = cent;
            }
            nodes = new List<TriCellPathNode>();
            for (int iCell = 0; iCell < cells.Length; iCell++)
            {
                for (int iWall = 0; iWall < 3; iWall++)
                {
                    // Build a path 3 cells deep.
                    TriCell childCell = cells[iCell].GetLink(iWall);
                    if (childCell == null)
                        // Can't get to two cells deep.
                        break;
                    for (int iWallNext = 0; iWallNext < 3; iWallNext++)
                    {
                        TriCell childChildCell = childCell.GetLink(iWallNext);
                        if (childChildCell == null)
                            // Can't get to three cells deep.
                            break;
                        TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
                        Vector3 cent = cents[iCell];  // Parent's centroid.
                        TriCellPathNode child = new TriCellPathNode(childCell
                                , parent
                                , cent.x, cent.y, cent.z);
                        TriCellPathNode childChild = new TriCellPathNode(childChildCell
                                , child
                                , cent.x, cent.y, cent.z);
                        // Don't change the order the next adds.  It is important
                        // that the longer path be added before the shorter path.
                        nodes.Add(childChild);
                        nodes.Add(child);
                    }
                }
            }
        }

        [TestMethod]
        public void TestBasicHeap()
        {

            if (nodes.Count < 2)
                // Want at least 3 levels in the binary tree.
                Assert.Fail("Mesh doesn't support test.");

            // Basic tests.
            TriCellPathNodeHeap heap = new TriCellPathNodeHeap();
            Assert.IsTrue(heap.Count == 0);
            Assert.IsTrue(heap.Peek() == null);
            Assert.IsTrue(heap.Poll() == null);

            foreach (TriCellPathNode node in nodes)
            {
                heap.Add(node);
            }

            Assert.IsTrue(heap.Count == nodes.Count);
            Assert.IsTrue(heap.Peek() != null);
            Assert.IsTrue(heap.Poll() != null);

            heap.Clear();
            Assert.IsTrue(heap.Count == 0);
            Assert.IsTrue(heap.Peek() == null);
            Assert.IsTrue(heap.Poll() == null);

        }

        [TestMethod]
        public void TestAddPeek()
        {
            if (nodes.Count < 7)
                // Want at least 3 levels in the binary tree.
                Assert.Fail("Mesh doesn't support test.");

            TriCellPathNodeHeap heap = new TriCellPathNodeHeap();

            TriCellPathNode expectedRoot = nodes[0];
            for (int i = 0; i < nodes.Count; i++)
            {
                heap.Add(nodes[i]);
                Assert.IsTrue(heap.Count == i + 1);
                if (nodes[i].F < expectedRoot.F)
                    expectedRoot = nodes[i];
                Assert.IsTrue(heap.Peek() == expectedRoot);
            }
        }

        [TestMethod]
        public void TestPoll()
        {
            if (nodes.Count < 7)
                // Want at least 3 levels in the binary tree.
                Assert.Fail("Mesh doesn't support test.");

            TriCellPathNodeHeap heap = new TriCellPathNodeHeap();
            foreach (TriCellPathNode node in nodes)
            {
                heap.Add(node);
            }

            // Dev Note: It is part of the test to not base loop on heap Size
            // since we expect each node to be foundStart in the heap's root at some point.
            while (nodes.Count > 0)
            {
                List<TriCellPathNode> expectedRoots = GetLowestF(nodes);
                TriCellPathNode rootNode = heap.Poll();
                Assert.IsTrue(expectedRoots.Contains(rootNode));
                nodes.Remove(rootNode);
                Assert.IsTrue(heap.Count == nodes.Count);
            }
        }

        [TestMethod]
        public void TestRestack()
        {
            /*
             * Changes to the F-value is simulated by changing the node's H-value based
             * on the minimum/maximum F-values of the test nodes.
             * 
             * Each node has its value changed in a random manner 5 times.
             * The restack is checked each change.  So this results in 5*nodeCount
             * tests.
             */

            if (nodes.Count < 7)
                // Want at least 3 levels in the binary tree.
                Assert.Fail("Mesh doesn't support test.");

            TriCellPathNodeHeap heap = new TriCellPathNodeHeap();
            float maxF = 0;
            float minF = float.MaxValue;
            foreach (TriCellPathNode node in nodes)
            {
                heap.Add(node);
                maxF = Math.Max(maxF, node.F);
                minF = Math.Min(minF, node.F);
            }

            // Expance the range slightly.
            maxF += 0.1f * (maxF - minF);
            minF -= 0.1f * (maxF - minF);
            float rangeF = maxF - minF;

            System.Random r = new System.Random(29487);

            foreach (TriCellPathNode node in nodes)
            {
                for (int i = 0; i < 5; i++)
                {
                    List<TriCellPathNode> tnodes = new List<TriCellPathNode>();
                    tnodes.AddRange(nodes);

                    float desiredF = minF + (float)(r.NextDouble() * rangeF);
                    node.H = (desiredF - node.F);
                    heap.Restack(node);

                    // Test to make sure heap is sorted correctly. 
                    while (tnodes.Count > 0)
                    {
                        List<TriCellPathNode> expectedRoots = GetLowestF(tnodes);
                        TriCellPathNode rootNode = heap.Poll();
                        Assert.IsTrue(expectedRoots.Contains(rootNode));
                        tnodes.Remove(rootNode);
                        Assert.IsTrue(heap.Count == tnodes.Count);
                    }

                    foreach (TriCellPathNode nnode in nodes)
                    {
                        heap.Add(nnode);
                    }
                }
            }
        }

        private List<TriCellPathNode> GetLowestF(List<TriCellPathNode> nodeList)
        {
            List<TriCellPathNode> result = new List<TriCellPathNode>();

            float selectedF = float.MaxValue;
            foreach (TriCellPathNode node in nodeList)
            {
                if (node.F < selectedF)
                {
                    selectedF = node.F;
                    result.Clear();
                    result.Add(node);
                }
                else if (node.F == selectedF)
                    result.Add(node);
            }
            return result;
        }
    }
}

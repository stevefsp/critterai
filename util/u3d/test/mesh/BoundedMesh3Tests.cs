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

namespace org.critterai.mesh
{
    [TestClass]
    public sealed class BaseMesh3Tests
    {
        [TestMethod]
        public void TestConstructionInt()
        {
            BoundedMesh3 mesh = new BoundedMesh3(5);
            Assert.IsTrue(mesh.vertsPerPolygon == 5);
            Assert.IsTrue(mesh.indices == null);
            Assert.IsTrue(mesh.vertices == null);
            Assert.IsTrue(mesh.bounds.Length == 6);
        }

        [TestMethod]
        public void TestConstructionFull()
        {
            float[] verts = { 1, 2, 3, -1, -2, -3 };
            int[] indices = { 1, 2, 3, 6, 5, 4, 1, 2 };

            BoundedMesh3 mesh = new BoundedMesh3(4, verts, indices);
            Assert.IsTrue(mesh.vertsPerPolygon == 4);
            Assert.IsTrue(mesh.indices == indices);
            Assert.IsTrue(mesh.vertices == verts);
            Assert.IsTrue(mesh.bounds[0] == -1);
            Assert.IsTrue(mesh.bounds[1] == -2);
            Assert.IsTrue(mesh.bounds[2] == -3);
            Assert.IsTrue(mesh.bounds[3] == 1);
            Assert.IsTrue(mesh.bounds[4] == 2);
            Assert.IsTrue(mesh.bounds[5] == 3);
        }

        [TestMethod]
        public void TestRebuildBounds()
        {
            float[] verts = { 1, 2, 3, -1, -2, -3 };
            int[] indices = { 1, 2, 3, 6, 5, 4, 1, 2 };

            BoundedMesh3 mesh = new BoundedMesh3(4, verts, indices);
            
            verts[0] = -14;
            verts[1] = -15;
            verts[2] = -16;
            verts[3] = -4;
            verts[4] = -5;
            verts[5] = -6;

            mesh.RebuildBounds();

            Assert.IsTrue(mesh.bounds[0] == -14);
            Assert.IsTrue(mesh.bounds[1] == -15);
            Assert.IsTrue(mesh.bounds[2] == -16);
            Assert.IsTrue(mesh.bounds[3] == -4);
            Assert.IsTrue(mesh.bounds[4] == -5);
            Assert.IsTrue(mesh.bounds[5] == -6);
        }
    }
}

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
    public sealed class Mesh3Tests
    {
        [TestMethod]
        public void TestConstructionInt()
        {
            Mesh3 mesh = new Mesh3(5);
            Assert.IsTrue(mesh.vertsPerPolygon == 5);
            Assert.IsTrue(mesh.indices == null);
            Assert.IsTrue(mesh.vertices == null);

            mesh = new Mesh3(7);
            Assert.IsTrue(mesh.vertsPerPolygon == 7);

            mesh = new Mesh3(2);
            Assert.IsTrue(mesh.vertsPerPolygon == 3);
        }

        [TestMethod]
        public void TestConstructionIntIntInt()
        {
            Mesh3 mesh = new Mesh3(5, 8, 9);
            Assert.IsTrue(mesh.vertsPerPolygon == 5);
            Assert.IsTrue(mesh.indices.Length == 9 * 5);
            Assert.IsTrue(mesh.vertices.Length == 8 * 3);

            mesh = new Mesh3(2, 8, 9);
            Assert.IsTrue(mesh.vertsPerPolygon == 3);
            Assert.IsTrue(mesh.indices.Length == 9 * 3);
            Assert.IsTrue(mesh.vertices.Length == 8 * 3);
        }

        [TestMethod]
        public void TestConstructionFull()
        {
            float[] verts = { 1, 2, 3, 4, 5, 6 };
            int[] indices = { 1, 2, 3, 6, 5, 4, 1, 2 };

            Mesh3 mesh = new Mesh3(4, verts, indices);
            Assert.IsTrue(mesh.vertsPerPolygon == 4);
            Assert.IsTrue(mesh.indices == indices);
            Assert.IsTrue(mesh.vertices == verts);

            mesh = new Mesh3(2, verts, indices);
            Assert.IsTrue(mesh.vertsPerPolygon == 3);
        }

        [TestMethod]
        public void TestProperties()
        {
            float[] verts = { 1, 2, 3, 4, 5, 6 };
            int[] indices = { 1, 2, 3, 6 };

            Mesh3 mesh = new Mesh3(4, verts, indices);
            Assert.IsTrue(mesh.PolyCount == 1);
            Assert.IsTrue(mesh.VertexCount == 2);
        }
    }
}

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
using org.critterai.geom;

namespace org.critterai.geom
{
    [TestClass]
    public sealed class Mesh3Tests
    {

        private float[] verts = new float[12];
        private int[] tris = new int[6];

        [TestInitialize]
        public void Setup()
        {
            verts[0] = 0.01f;
            verts[1] = 0.1f;
            verts[2] = 0.0f;
            verts[3] = 0.02f;
            verts[4] = 0.003f;
            verts[5] = 1.0f;
            verts[6] = 1.02f;
            verts[7] = 1.0f;
            verts[8] = 1.01f;
            verts[9] = 1.0f;
            verts[10] = -1.03f;
            verts[11] = 0.0f;

            tris[0] = 0;
            tris[1] = 3;
            tris[2] = 2;
            tris[3] = 0;
            tris[4] = 2;
            tris[5] = 1;
        }

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

        [TestMethod]
        public void TestStaticInvertXAxis()
        {
            Mesh3.InvertAxis(verts, Axis.X);

            Assert.IsTrue(verts[0] == -0.01f);
            Assert.IsTrue(verts[1] == 0.1f);
            Assert.IsTrue(verts[2] == 0.0f);
            Assert.IsTrue(verts[3] == -0.02f);
            Assert.IsTrue(verts[4] == 0.003f);
            Assert.IsTrue(verts[5] == 1.0f);
            Assert.IsTrue(verts[6] == -1.02f);
            Assert.IsTrue(verts[7] == 1.0f);
            Assert.IsTrue(verts[8] == 1.01f);
            Assert.IsTrue(verts[9] == -1.0f);
            Assert.IsTrue(verts[10] == -1.03f);
            Assert.IsTrue(verts[11] == 0.0f);
        }

        [TestMethod]
        public void TestStaticInvertYAxis()
        {
            Mesh3.InvertAxis(verts, Axis.Y);

            Assert.IsTrue(verts[0] == 0.01f);
            Assert.IsTrue(verts[1] == -0.1f);
            Assert.IsTrue(verts[2] == 0.0f);
            Assert.IsTrue(verts[3] == 0.02f);
            Assert.IsTrue(verts[4] == -0.003f);
            Assert.IsTrue(verts[5] == 1.0f);
            Assert.IsTrue(verts[6] == 1.02f);
            Assert.IsTrue(verts[7] == -1.0f);
            Assert.IsTrue(verts[8] == 1.01f);
            Assert.IsTrue(verts[9] == 1.0f);
            Assert.IsTrue(verts[10] == 1.03f);
            Assert.IsTrue(verts[11] == 0.0f);
        }

        [TestMethod]
        public void TestStaticInvertZAxis()
        {
            Mesh3.InvertAxis(verts, Axis.Z);

            Assert.IsTrue(verts[0] == 0.01f);
            Assert.IsTrue(verts[1] == 0.1f);
            Assert.IsTrue(verts[2] == 0.0f);
            Assert.IsTrue(verts[3] == 0.02f);
            Assert.IsTrue(verts[4] == 0.003f);
            Assert.IsTrue(verts[5] == -1.0f);
            Assert.IsTrue(verts[6] == 1.02f);
            Assert.IsTrue(verts[7] == 1.0f);
            Assert.IsTrue(verts[8] == -1.01f);
            Assert.IsTrue(verts[9] == 1.0f);
            Assert.IsTrue(verts[10] == -1.03f);
            Assert.IsTrue(verts[11] == 0.0f);
        }

        [TestMethod]
        public void TestStaticReverseWrapDirection()
        {

            Mesh3.ReverseWrapDirection(tris);

            Assert.IsTrue(tris[0] == 0);
            Assert.IsTrue(tris[1] == 2);
            Assert.IsTrue(tris[2] == 3);
            Assert.IsTrue(tris[3] == 0);
            Assert.IsTrue(tris[4] == 1);
            Assert.IsTrue(tris[5] == 2);
        }

        [TestMethod]
        public void TestStaticGetWrapStats()
        {
            float[] va = 
            {
               -3, 1, -1
               , -2, 1.2f, 3
               , 2, 0.8f, 2
               , 1, 1, -2
               , 0, 8, 0.8f
            };
            int[] ta =
            {
                0, 1, 2     // cw
                , 0, 2, 3   // cw
                , 0, 2, 1   // ccw
                , 0, 4, 2   // vert
                , 0, 3, 2   // ccw
                , 0, 1, 3   // cw
            };

            int cw, ccw, vertical;
            Mesh3.GetWrapStatisicsXZ(va, ta
                , out cw, out ccw, out vertical);

            Assert.IsTrue(cw == 3);
            Assert.IsTrue(ccw == 2);
            Assert.IsTrue(vertical == 1);
        }
    }
}

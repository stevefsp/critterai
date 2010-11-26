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

namespace org.critterai.nav.nmgen
{
    [TestClass]
    public sealed class TriangleMeshTestsCLI
    {
        [TestMethod]
        public void ConstructionTests()
        {
            const int vertCount = 10;
            const int triangleCount = 5;
            TriangleMesh mesh = new TriangleMesh(vertCount * 3
                , triangleCount * 3);
            Assert.IsTrue(mesh.vertices.Length == vertCount * 3);
            Assert.IsTrue(mesh.triangles.Length == triangleCount * 3);
            Assert.IsTrue(mesh.TriangleCount == triangleCount);
            Assert.IsTrue(mesh.VertCount == vertCount);

        }

        [TestMethod]
        public void GetVerticesTests()
        {
            const int vertCount = 4;
            const int triangleCount = 2;
            TriangleMesh mesh = new TriangleMesh(vertCount * 3
                , triangleCount * 3);
            for (int i = 0; i < vertCount; i++)
            {
                int p = i * 3;
                mesh.vertices[p + 0] = p + 0;
                mesh.vertices[p + 1] = p + 1;
                mesh.vertices[p + 2] = p + 2;
            }
            mesh.triangles[0] = 3;
            mesh.triangles[1] = 1;
            mesh.triangles[2] = 0;
            mesh.triangles[3] = 2;
            mesh.triangles[4] = 1;
            mesh.triangles[5] = 3;

            float[] result = mesh.GetTriangleVerts(0);
            Assert.IsTrue(result.Length == 9);
            Assert.IsTrue(result[0] == mesh.triangles[0] * 3 + 0);
            Assert.IsTrue(result[1] == mesh.triangles[0] * 3 + 1);
            Assert.IsTrue(result[2] == mesh.triangles[0] * 3 + 2);
            Assert.IsTrue(result[3] == mesh.triangles[1] * 3 + 0);
            Assert.IsTrue(result[4] == mesh.triangles[1] * 3 + 1);
            Assert.IsTrue(result[5] == mesh.triangles[1] * 3 + 2);
            Assert.IsTrue(result[6] == mesh.triangles[2] * 3 + 0);
            Assert.IsTrue(result[7] == mesh.triangles[2] * 3 + 1);
            Assert.IsTrue(result[8] == mesh.triangles[2] * 3 + 2);

            result = mesh.GetTriangleVerts(1);
            Assert.IsTrue(result.Length == 9);
            Assert.IsTrue(result[0] == mesh.triangles[3] * 3 + 0);
            Assert.IsTrue(result[1] == mesh.triangles[3] * 3 + 1);
            Assert.IsTrue(result[2] == mesh.triangles[3] * 3 + 2);
            Assert.IsTrue(result[3] == mesh.triangles[4] * 3 + 0);
            Assert.IsTrue(result[4] == mesh.triangles[4] * 3 + 1);
            Assert.IsTrue(result[5] == mesh.triangles[4] * 3 + 2);
            Assert.IsTrue(result[6] == mesh.triangles[5] * 3 + 0);
            Assert.IsTrue(result[7] == mesh.triangles[5] * 3 + 1);
            Assert.IsTrue(result[8] == mesh.triangles[5] * 3 + 2);
        }

        [TestMethod]
        public void GetVerticesFailTests()
        {
            const int vertCount = 10;
            const int triangleCount = 5;
            TriangleMesh mesh = new TriangleMesh(vertCount * 3
                , triangleCount * 3);
            Assert.IsTrue(mesh.GetTriangleVerts(-1) == null);
            Assert.IsTrue(mesh.GetTriangleVerts(5) == null);
        }
    }
}

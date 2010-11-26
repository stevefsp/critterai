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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace org.critterai.nav.nmgen
{
    [TestClass]
    public sealed class MeshBuilderTestsCLI
    {
        [TestMethod]
        public void SmokeTest()
        {
            BuildConfig config = SmokeTestMesh.GetStandardBuildConfig();

            float[] sourceVertices = SmokeTestMesh.GetVerts();
            int[] sourceTriangles = SmokeTestMesh.GetTriangles();

            List<string> messages = new List<string>();
            TriangleMesh mesh = MeshBuilder.BuildSimpleMesh(sourceVertices
                , sourceTriangles
                , config
                , messages
                , 3);

            Assert.IsTrue(mesh != null);

            Assert.IsTrue(
                SmokeTestMesh.PartialResultVertCheckOK(mesh.vertices));
            Assert.IsTrue(
                SmokeTestMesh.PartialResultTrianglesCheckOK(mesh.triangles));

            Assert.IsTrue(messages.Count >= 4);  // 4 is min expected.
        }

        [TestMethod]
        public void SmokeTestMemLeak()
        {
            BuildConfig config = SmokeTestMesh.GetStandardBuildConfig();

            float[] sourceVertices = SmokeTestMesh.GetVerts();
            int[] sourceTriangles = SmokeTestMesh.GetTriangles();

            List<string> messages = new List<string>();

            for (int i = 0; i < 10000; i++)
            {
                TriangleMesh mesh = MeshBuilder.BuildSimpleMesh(sourceVertices
                    , sourceTriangles
                    , config
                    , messages
                    , 3);
                Assert.IsTrue(mesh != null);
                if (i % 1000 == 0)
                    Debug.WriteLine("Iteration: " + i);
            }
        }
    }
}

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
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace org.critterai.mesh
{
    [TestClass]
    public sealed class WavefrontTests
    {

        private const String TEST_FILE_NAME = 
            "org.critterai.assets.quickmesh.txt";

        private static string source;

        [ClassInitialize()]
        public static void SetupOnce(TestContext testContext)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            StreamReader reader = 
                new StreamReader(asm.GetManifestResourceStream(TEST_FILE_NAME));

            source = reader.ReadToEnd();
        }

        [TestMethod]
        public void TestStaticTranslateFrom()
        {
            float[] verts;
            int[] tris;
            float[] bounds = new float[6];
            Wavefront.TranslateFrom(source, bounds, out verts, out tris);

            Assert.IsTrue(tris.Length == 6);
            Assert.IsTrue(verts.Length == 4 * 3);

            Assert.IsTrue(verts[0] == 0.01f);
            Assert.IsTrue(verts[1] == 0.1f);
            Assert.IsTrue(verts[2] == 0.0f);
            Assert.IsTrue(verts[3] == 0.02f);
            Assert.IsTrue(verts[4] == 0.003f);
            Assert.IsTrue(verts[5] == 1.0f);
            Assert.IsTrue(verts[6] == 1.02f);
            Assert.IsTrue(verts[7] == 1.0f);
            Assert.IsTrue(verts[8] == 1.01f);
            Assert.IsTrue(verts[9] == 1.0f);
            Assert.IsTrue(verts[10] == -1.03f);
            Assert.IsTrue(verts[11] == 0.0f);

            Assert.IsTrue(tris[0] == 0);
            Assert.IsTrue(tris[1] == 3);
            Assert.IsTrue(tris[2] == 2);
            Assert.IsTrue(tris[3] == 0);
            Assert.IsTrue(tris[4] == 2);
            Assert.IsTrue(tris[5] == 1);
        }

        [TestMethod]
        public void TestStaticTranslateToPlain()
        {
            float[] verts = new float[12];
            int[] tris = new int[6];

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

            // Cheating a bit here.  OK...a lot.
            string result = Wavefront.TranslateTo(verts, tris, false, false);

            verts = null;
            tris = null;
            Wavefront.TranslateFrom(result, null, out verts, out tris);

            Assert.IsTrue(verts[0] == 0.01f);
            Assert.IsTrue(verts[1] == 0.1f);
            Assert.IsTrue(verts[2] == 0.0f);
            Assert.IsTrue(verts[3] == 0.02f);
            Assert.IsTrue(verts[4] == 0.003f);
            Assert.IsTrue(verts[5] == 1.0f);
            Assert.IsTrue(verts[6] == 1.02f);
            Assert.IsTrue(verts[7] == 1.0f);
            Assert.IsTrue(verts[8] == 1.01f);
            Assert.IsTrue(verts[9] == 1.0f);
            Assert.IsTrue(verts[10] == -1.03f);
            Assert.IsTrue(verts[11] == 0.0f);

            Assert.IsTrue(tris[0] == 0);
            Assert.IsTrue(tris[1] == 3);
            Assert.IsTrue(tris[2] == 2);
            Assert.IsTrue(tris[3] == 0);
            Assert.IsTrue(tris[4] == 2);
            Assert.IsTrue(tris[5] == 1);
        }

        [TestMethod]
        public void TestStaticTranslateToAlter()
        {
            float[] verts = new float[12];
            int[] tris = new int[6];

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

            // Cheating a bit here.  OK...a lot.
            string result = Wavefront.TranslateTo(verts, tris, true, true);

            verts = null;
            tris = null;
            Wavefront.TranslateFrom(result, null, out verts, out tris);

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

            Assert.IsTrue(tris[0] == 0);
            Assert.IsTrue(tris[1] == 2);
            Assert.IsTrue(tris[2] == 3);
            Assert.IsTrue(tris[3] == 0);
            Assert.IsTrue(tris[4] == 1);
            Assert.IsTrue(tris[5] == 2);
        }
    }
}

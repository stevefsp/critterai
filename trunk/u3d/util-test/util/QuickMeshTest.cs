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

namespace org.critterai.util
{
    /// <summary>
    /// Summary description for QuickMeshTest
    /// </summary>
    [TestClass]
    public class QuickMeshTest
    {

        /*
         * Design Notes:
         * 
         * This test suite does not currently test exception handling.
         */

        private const String TEST_FILE_NAME = "org.critterai.assets.quickmesh.txt";

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

        [ClassInitialize()]
        public static void SetupOnce(TestContext testContext)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            //String[] rns = asm.GetManifestResourceNames();
            //if (rns.Length == 0)
            //    rns = null;
            StreamReader reader = new StreamReader(asm.GetManifestResourceStream(TEST_FILE_NAME));

            StreamWriter writer = new StreamWriter(TEST_FILE_NAME);
            writer.Write(reader.ReadToEnd());
            writer.Close();
        }

        [TestMethod]
        public void TestConstructorA()
        {
            QuickMesh m = new QuickMesh(TEST_FILE_NAME, false);

            Assert.IsTrue(m.indices.Length == 6);
            Assert.IsTrue(m.verts.Length == 4 * 3);

            Assert.IsTrue(m.verts[0] == 0.01f);
            Assert.IsTrue(m.verts[1] == 0.1f);
            Assert.IsTrue(m.verts[2] == 0.0f);
            Assert.IsTrue(m.verts[3] == 0.02f);
            Assert.IsTrue(m.verts[4] == 0.003f);
            Assert.IsTrue(m.verts[5] == 1.0f);
            Assert.IsTrue(m.verts[6] == 1.02f);
            Assert.IsTrue(m.verts[7] == 1.0f);
            Assert.IsTrue(m.verts[8] == 1.01f);
            Assert.IsTrue(m.verts[9] == 1.0f);
            Assert.IsTrue(m.verts[10] == -1.03f);
            Assert.IsTrue(m.verts[11] == 0.0f);

            Assert.IsTrue(m.indices[0] == 0);
            Assert.IsTrue(m.indices[1] == 2);
            Assert.IsTrue(m.indices[2] == 3);
            Assert.IsTrue(m.indices[3] == 0);
            Assert.IsTrue(m.indices[4] == 1);
            Assert.IsTrue(m.indices[5] == 2);
        }

        [TestMethod]
        public void TestConstructorB()
        {
            QuickMesh m = new QuickMesh(TEST_FILE_NAME, true);

            Assert.IsTrue(m.indices.Length == 6);
            Assert.IsTrue(m.verts.Length == 4 * 3);

            Assert.IsTrue(m.verts[0] == 0.01f);
            Assert.IsTrue(m.verts[1] == 0.1f);
            Assert.IsTrue(m.verts[2] == 0.0f);
            Assert.IsTrue(m.verts[3] == 0.02f);
            Assert.IsTrue(m.verts[4] == 0.003f);
            Assert.IsTrue(m.verts[5] == 1.0f);
            Assert.IsTrue(m.verts[6] == 1.02f);
            Assert.IsTrue(m.verts[7] == 1.0f);
            Assert.IsTrue(m.verts[8] == 1.01f);
            Assert.IsTrue(m.verts[9] == 1.0f);
            Assert.IsTrue(m.verts[10] == -1.03f);
            Assert.IsTrue(m.verts[11] == 0.0f);

            Assert.IsTrue(m.indices[0] == 0);
            Assert.IsTrue(m.indices[1] == 3);
            Assert.IsTrue(m.indices[2] == 2);
            Assert.IsTrue(m.indices[3] == 0);
            Assert.IsTrue(m.indices[4] == 2);
            Assert.IsTrue(m.indices[5] == 1);
        }
    }
}

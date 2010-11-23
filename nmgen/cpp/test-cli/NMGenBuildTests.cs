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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace org.critterai.nmgen
{
    [TestClass]
    public sealed class NMGenBuildTests
    {
        [TestMethod]
        public void SmokeTestBuild()
        {
            Configuration config = SmokeTestMesh.GetStandardConfig();
            
            float[] sourceVertices = SmokeTestMesh.GetVerts();
            int[] sourceTriangles = SmokeTestMesh.GetTriangles();

            IntPtr ptrResultTriangles = IntPtr.Zero;
            IntPtr ptrResultVerts = IntPtr.Zero;
            int resultVertLength = 0;
            int resultTrianglesLength = 0;

            char[] charMsgArray = new char[10000];

            bool success = Extern.buildSimpleMesh(config
                , sourceVertices
                , sourceVertices.Length
                , sourceTriangles
                , sourceTriangles.Length
                , ref ptrResultVerts
                , ref resultVertLength
                , ref ptrResultTriangles
                , ref resultTrianglesLength
                , charMsgArray
                , charMsgArray.Length
                , 3);

            string aggregateMsg = new string(charMsgArray);
            char[] delim = { '\0' };
            string[] messages =
                aggregateMsg.Split(delim, StringSplitOptions.RemoveEmptyEntries);

            Assert.IsTrue(success);
            Assert.IsTrue(messages.Length > 4);  // 4 is min expected.

            Assert.IsTrue(resultVertLength 
                == SmokeTestMesh.ExpectedResultVertLength);
            Assert.IsTrue(resultTrianglesLength
                == SmokeTestMesh.ExpectedResultTrianglesLength);
            Assert.IsTrue(ptrResultVerts != IntPtr.Zero);
            Assert.IsTrue(ptrResultTriangles != IntPtr.Zero);

            float[] resultVertices = new float[resultVertLength];
            Marshal.Copy(ptrResultVerts, resultVertices, 0, resultVertLength);
            int[] resultTriangles = new int[resultTrianglesLength];
            Marshal.Copy(ptrResultTriangles, resultTriangles, 0, resultTrianglesLength);

            Extern.freeMesh(ref ptrResultVerts, ref ptrResultTriangles);

            Assert.IsTrue(
                SmokeTestMesh.PartialResultVertCheckOK(resultVertices));
            Assert.IsTrue(
                SmokeTestMesh.PartialResultTrianglesCheckOK(resultTriangles));

            Assert.IsTrue(ptrResultVerts == IntPtr.Zero);
            Assert.IsTrue(ptrResultTriangles == IntPtr.Zero);
        }

        [TestMethod]
        public void SmokeTestNullMessageArray()
        {
            // Just making sure it doesn't crash or fail due to
            // theme message argument being null.

            Configuration config = SmokeTestMesh.GetStandardConfig();

            float[] sourceVertices = SmokeTestMesh.GetVerts();
            int[] sourceTriangles = SmokeTestMesh.GetTriangles();

            IntPtr ptrResultTriangles = IntPtr.Zero;
            IntPtr ptrResultVerts = IntPtr.Zero;
            int resultVertLength = 0;
            int resultTrianglesLength = 0;

            bool success = Extern.buildSimpleMesh(config
                , sourceVertices
                , sourceVertices.Length
                , sourceTriangles
                , sourceTriangles.Length
                , ref ptrResultVerts
                , ref resultVertLength
                , ref ptrResultTriangles
                , ref resultTrianglesLength
                , null
                , 0
                , 3);

            Assert.IsTrue(success);
        }

        [TestMethod]
        public void SmokeTestMemLeak()
        {
            Configuration config = SmokeTestMesh.GetStandardConfig();

            float[] sourceVertices = SmokeTestMesh.GetVerts();
            int[] sourceTriangles = SmokeTestMesh.GetTriangles();

            IntPtr ptrResultTriangles = IntPtr.Zero;
            IntPtr ptrResultVerts = IntPtr.Zero;
            int resultVertLength = 0;
            int resultTrianglesLength = 0;

            char[] charMsgArray = new char[10000];

            for (int i = 0; i < 10000; i++)
            {
                if (Extern.buildSimpleMesh(config
                    , sourceVertices
                    , sourceVertices.Length
                    , sourceTriangles
                    , sourceTriangles.Length
                    , ref ptrResultVerts
                    , ref resultVertLength
                    , ref ptrResultTriangles
                    , ref resultTrianglesLength
                    , charMsgArray
                    , charMsgArray.Length
                    , 3))
                {
                    Extern.freeMesh(ref ptrResultVerts, ref ptrResultTriangles);
                }
                else
                    Assert.Fail("Build fail on loop {0}", i);
                if (i % 1000 == 0)
                    Debug.WriteLine("Iteration: " + i);
            }
        }
    }
}

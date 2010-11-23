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
using System.IO;
using org.critterai.mesh;
using org.critterai.math;

namespace org.critterai.nmgen
{
    /*
     * This test class does not contain any real tests.  It is used to aid
     * debug troubleshooting.
     */

    //[TestClass]
    //public sealed class TroubleshootingTests
    //{
    //    [TestMethod]
    //    public void NaughtyTroubleshooting()
    //    {
    //        Configuration config = new Configuration();
    //        config.clipLedges = false;
    //        config.contourMaxDeviation = 0.2f;
    //        config.contourSampleDistance = 0.9f;
    //        config.edgeMaxDeviation = 1;
    //        config.heightfieldBorderSize = 0;
    //        config.maxEdgeLength = 0.2f;
    //        config.maxTraversableSlope = 45.5f;
    //        config.maxTraversableStep = 0.25f;
    //        config.maxVertsPerPoly = 6;
    //        config.mergeRegionSize = 0;
    //        config.minIslandRegionSize = 0;
    //        config.minTraversableHeight = 1.2f;
    //        config.smoothingThreshold = 1;
    //        config.traversableAreaBorderSize = 0.2f;
    //        config.xzCellSize = 0.1f;
    //        config.yCellSize = 0.05f;

    //        string path = @"C:\Dev\cai-working\nmgen\u3d\project\ArraySourceNavmesh-p16642-source.obj";
    //        StreamReader r = new StreamReader(path);

    //        float[] sourceVertices = null;
    //        int[] sourceTriangles = null;

    //        Wavefront.TranslateFrom(r.ReadToEnd()
    //            , null
    //            , out sourceVertices
    //            , out sourceTriangles);
    //        r.Close();

    //        IntPtr ptrResultTriangles = IntPtr.Zero;
    //        IntPtr ptrResultVerts = IntPtr.Zero;
    //        int resultVertLength = 0;
    //        int resultTrianglesLength = 0;

    //        char[] charMsgArray = new char[10000];

    //        bool success = Extern.buildSimpleMesh(config
    //            , sourceVertices
    //            , sourceVertices.Length
    //            , sourceTriangles
    //            , sourceTriangles.Length
    //            , ref ptrResultVerts
    //            , ref resultVertLength
    //            , ref ptrResultTriangles
    //            , ref resultTrianglesLength
    //            , charMsgArray
    //            , charMsgArray.Length
    //            , 3);

    //        string aggregateMsg = new string(charMsgArray);
    //        char[] delim = { '\0' };
    //        string[] messages =
    //            aggregateMsg.Split(delim, StringSplitOptions.RemoveEmptyEntries);

    //        if (success)
    //        {
    //            float[] resultVertices = new float[resultVertLength];
    //            Marshal.Copy(ptrResultVerts, resultVertices, 0, resultVertLength);
    //            int[] resultTriangles = new int[resultTrianglesLength];
    //            Marshal.Copy(ptrResultTriangles, resultTriangles, 0, resultTrianglesLength);

    //            Extern.freeMesh(ref ptrResultVerts, ref ptrResultTriangles);

    //            float[] bounds = 
    //                Vector3Util.GetBounds(resultVertices, new float[6]);
    //            Assert.IsFalse(bounds[4] > 4.5f);
                
    //        }

    //        Assert.IsTrue(success);
    //    }
    //}
}

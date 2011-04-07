/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using System.Text;
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    public static class NMGenUtil
    {
        public static bool BuildMesh(NMGenParams config
            , float[] sourceVertices
            , int[] sourceIndices
            , byte[] triangleAreas
            , MessageStyle messageStyle
            , List<string> resultMessages
            , out PolyMesh resultPolyMesh
            , out PolyMeshDetail resultDetailMesh)
        {
            if (resultMessages == null)
                messageStyle = MessageStyle.None;
            else
                resultMessages.Clear();

            TriMesh3Ex sourceMesh =
                new TriMesh3Ex(sourceVertices, sourceIndices);
            if (sourceMesh.triangleCount < 1)
            {
                resultPolyMesh = null;
                resultDetailMesh = null;
                if (messageStyle != MessageStyle.None)
                    resultMessages.Add("Invalid source mesh data.");
                TriMesh3Ex.Free(ref sourceMesh);
                return false;
            }

            if (triangleAreas != null 
                && triangleAreas.Length < sourceMesh.triangleCount)
            {
                resultPolyMesh = null;
                resultDetailMesh = null;
                if (messageStyle != MessageStyle.None)
                    resultMessages.Add("Triangle area array is too small.");
                TriMesh3Ex.Free(ref sourceMesh);
                return false;
            }

            // RCConfigEx rcConfig = Utils.ToConfigStructure(config);
            MessageBufferEx messageBuffer =
                new MessageBufferEx((int)messageStyle);
            PolyMeshEx polyMesh = new PolyMeshEx();
            PolyMeshDetailEx detailMesh = new PolyMeshDetailEx();

            bool success = NMGenUtilEx.BuildMesh(config.root
                , ref sourceMesh
                , triangleAreas
                , ref messageBuffer
                , ref polyMesh
                , ref detailMesh);

            TriMesh3Ex.Free(ref sourceMesh);

            if (messageStyle != MessageStyle.None)
                resultMessages.AddRange(messageBuffer.GetMessages());
            MessageBufferEx.FreeEx(ref messageBuffer);

            if (success)
            {
                resultPolyMesh = new PolyMesh(polyMesh
                    , config.MinTraversableHeight
                    , config.MaxTraversableStep
                    , config.TraversableAreaBorderSize);
                resultDetailMesh = new PolyMeshDetail(detailMesh);
            }
            else
            {
                resultPolyMesh = null;
                resultDetailMesh = null;
            }

            return success;
        }

        public static bool BuildMesh(NMGenParams config
            , float[] sourceVertices
            , int[] sourceIndices
            , MessageStyle messageStyle
            , List<string> resultMessages
            , out float[] resultVertices
            , out int[] resultTriangles)
        {
            if (resultMessages == null)
                messageStyle = MessageStyle.None;
            else
                resultMessages.Clear();

            TriMesh3Ex sourceMesh =
                new TriMesh3Ex(sourceVertices, sourceIndices);
            if (sourceMesh.triangleCount < 1)
            {
                resultVertices = null;
                resultTriangles = null;
                if (messageStyle != MessageStyle.None)
                    resultMessages.Add("Invalid source mesh data.");
                TriMesh3Ex.Free(ref sourceMesh);
                return false;
            }

            // RCConfigEx rcConfig = Utils.ToConfigStructure(config);
            MessageBufferEx messageBuffer =
                new MessageBufferEx((int)messageStyle);
            PolyMeshEx polyMesh = new PolyMeshEx();
            PolyMeshDetailEx detailMesh = new PolyMeshDetailEx();

            bool success = NMGenUtilEx.BuildMesh(config.root
                , ref sourceMesh
                , null
                , ref messageBuffer
                , ref polyMesh
                , ref detailMesh);

            TriMesh3Ex.Free(ref sourceMesh);

            if (messageStyle != MessageStyle.None)
                resultMessages.AddRange(messageBuffer.GetMessages());
            MessageBufferEx.FreeEx(ref messageBuffer);

            if (!success)
            {
                resultVertices = null;
                resultTriangles = null;
                return false;
            }

            PolyMeshEx.FreeEx(ref polyMesh);

            TriMesh3Ex resultMesh = new TriMesh3Ex();

            success = NMGenUtilEx.FlattenDetailMesh(ref detailMesh, ref resultMesh);

            PolyMeshDetailEx.FreeEx(ref detailMesh);

            if (success)
            {
                resultVertices = resultMesh.GetVertices();
                resultTriangles = resultMesh.GetTriangles();
                TriMesh3Ex.FreeEx(ref resultMesh);
            }
            else
            {
                resultVertices = null;
                resultTriangles = null;
                if (messageStyle != MessageStyle.None)
                    resultMessages.Add("Failure while flattening detail mesh.");
            }

            return success;
        }
    }
}

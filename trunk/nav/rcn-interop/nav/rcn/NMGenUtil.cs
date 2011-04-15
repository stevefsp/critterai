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
using System.Text;
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Provides utilities related to generating navigation meshes.
    /// </summary>
    public static class NMGenUtil
    {
        private static string[] mTraceMessages = null;
        private static byte[] mMessageBuffer = null;
        private static bool mTraceMessagesEnabled = false;

        /// <summary>
        /// The trace messages for the last build operation. (Null if no trace
        /// messages are available.)
        /// </summary>
        public static string[] TraceMessages { get { return mTraceMessages; } }

        /// <summary>
        /// TRUE if trace messages should be gathered during build operations.
        /// </summary>
        public static bool TraceMessagesEnabled
        {
            get { return mTraceMessagesEnabled; }
            set 
            { 
                mTraceMessagesEnabled = value;
                if (!mTraceMessagesEnabled)
                    ClearTraceMessages();
            }
        }

        /// <summary>
        /// Clear all trace messages.
        /// </summary>
        public static void ClearTraceMessages()
        {
            mTraceMessages = null;
            mMessageBuffer = null;
        }

        private static void InitializeMessageBuffer()
        {
            if (mTraceMessagesEnabled)
                mMessageBuffer = new byte[10000];
        }

        private static void PostSingleMessage(string message)
        {
            if (mTraceMessagesEnabled)
            {
                mTraceMessages = new string[1];
                mTraceMessages[0] = message;
            }
        }

        private static void TransferMessages()
        {
            if (mTraceMessagesEnabled)
            {
                string aggregateMsg =
                    ASCIIEncoding.ASCII.GetString(mMessageBuffer);
                char[] delim = { '\0' };
                mTraceMessages = aggregateMsg.Split(delim
                    , StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Generates navigation mesh data that can be used to create
        /// a <see cref="Navmesh"/> object.
        /// </summary>
        /// <remarks>
        /// This method will generate trace messages if trace messages are
        /// enabled.</remarks>
        /// <param name="config">The configuration parameters to use
        /// during the build.</param>
        /// <param name="sourceVertices">The source geometry vertices in the
        /// form (x, y, z).</param>
        /// <param name="sourceTriangles">The source geometry triangles
        /// in the form (vertAIndex, vertBIndex, vertCIndex).</param>
        /// <param name="triangleAreas">The area ids for the source
        /// geometry triangles.  (Optional)</param>
        /// <param name="resultPolyMesh">The resulting polygon mesh,
        /// or null if the build failed.</param>
        /// <param name="resultDetailMesh">The resulting detail mesh, or
        /// null if the build failed.</param>
        /// <returns>TRUE if the build succeeded.</returns>
        public static bool BuildMesh(NMGenParams config
            , float[] sourceVertices
            , int[] sourceTriangles
            , byte[] triangleAreas
            , out PolyMesh resultPolyMesh
            , out PolyMeshDetail resultDetailMesh)
        {
            TriMesh3Ex sourceMesh =
                new TriMesh3Ex(sourceVertices, sourceTriangles);
            if (sourceMesh.triangleCount < 1)
            {
                resultPolyMesh = null;
                resultDetailMesh = null;
                PostSingleMessage("Invalid source mesh data.");
                TriMesh3Ex.Free(ref sourceMesh);
                return false;
            }

            if (triangleAreas != null 
                && triangleAreas.Length < sourceMesh.triangleCount)
            {
                resultPolyMesh = null;
                resultDetailMesh = null;
                PostSingleMessage("Triangle area array is too small.");
                TriMesh3Ex.Free(ref sourceMesh);
                return false;
            }


            InitializeMessageBuffer();
            PolyMeshEx polyMesh = new PolyMeshEx();
            PolyMeshDetailEx detailMesh = new PolyMeshDetailEx();

            bool success = NMGenUtilEx.BuildMesh(config
                , ref sourceMesh
                , triangleAreas
                , ref polyMesh
                , ref detailMesh
                , mMessageBuffer
                , (mMessageBuffer == null ? 0 : mMessageBuffer.Length));

            TriMesh3Ex.Free(ref sourceMesh);

            TransferMessages();

            if (success)
            {
                resultPolyMesh = new PolyMesh(polyMesh
                    , config.MinTraversableHeight
                    , config.MaxTraversableStep
                    , config.TraversableAreaBorderSize
                    , AllocType.External);
                resultDetailMesh = new PolyMeshDetail(detailMesh
                    , AllocType.External);
            }
            else
            {
                resultPolyMesh = null;
                resultDetailMesh = null;
            }

            return success;
        }

        /// <summary>
        /// Generates a triangle navigation mesh data from the source geometry.
        /// </summary>
        /// <remarks>
        /// This method will generate trace messages if trace message are
        /// enabled.</remarks>
        /// <param name="config">The configuration parameters to use
        /// during the build.</param>
        /// <param name="sourceVertices">The source geometry vertices in the
        /// form (x, y, z).</param>
        /// <param name="sourceTriangles">The source geometry triangles
        /// in the form (vertAIndex, vertBIndex, vertCIndex).</param>
        /// <param name="resultVertices">The result vertices in the form
        /// (x, y, z), or null if the build failed.</param>
        /// <param name="resultTriangles">The result triangles in the form
        /// (vertAIndex, vertBIndex, vertCIndex), or null if the build
        /// failed.</param>
        /// <returns>TRUE if the build succeeded.</returns>
        public static bool BuildMesh(NMGenParams config
            , float[] sourceVertices
            , int[] sourceTriangles
            , out float[] resultVertices
            , out int[] resultTriangles)
        {
            TriMesh3Ex sourceMesh =
                new TriMesh3Ex(sourceVertices, sourceTriangles);
            if (sourceMesh.triangleCount < 1)
            {
                resultVertices = null;
                resultTriangles = null;
                PostSingleMessage("Invalid source mesh data.");
                TriMesh3Ex.Free(ref sourceMesh);
                return false;
            }

            InitializeMessageBuffer();
            PolyMeshEx polyMesh = new PolyMeshEx();
            PolyMeshDetailEx detailMesh = new PolyMeshDetailEx();

            bool success = NMGenUtilEx.BuildMesh(config
                , ref sourceMesh
                , null
                , ref polyMesh
                , ref detailMesh
                , mMessageBuffer
                , (mMessageBuffer == null ? 0 : mMessageBuffer.Length));

            TriMesh3Ex.Free(ref sourceMesh);

            TransferMessages();

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
            }

            return success;
        }
    }
}

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
using System.Runtime.InteropServices;
using org.critterai.interop;

namespace org.critterai.nmgen.rcn
{
    internal static class PolyMeshDetailEx
    {
        /*
         * Design note:
         * 
         * This class will have to be converted to a structure when the
         * merge mesh functionality is implemented.
         * 
         */

        [DllImport("cai-nmgen-rcn", EntryPoint = "rcpdFreeMeshData")]
        public static extern bool FreeEx([In, Out] PolyMeshDetail detailMesh);

        [DllImport("cai-nmgen-rcn", EntryPoint = "rcpdGetSerializedData")]
        public static extern bool GetSerializedData(
            [In] PolyMeshDetail detailMesh
                , bool includeBuffer
                , ref IntPtr resultData
                , ref int dataSize);

        [DllImport("cai-nmgen-rcn", EntryPoint = "rcpdBuildFromMeshData")]
        public static extern bool Build([In] byte[] meshData
        , int dataSize
        , [In, Out] PolyMeshDetail detailMesh);

        [DllImport("cai-nmgen-rcn", EntryPoint = "nmgFreeSerializationData")]
        public static extern void FreeSerializationData(ref IntPtr data);

        [DllImport("cai-nmgen-rcn", EntryPoint = "rcdtFlattenMesh")]
        public static extern bool FlattenMesh([In] PolyMeshDetail detailMesh
            , [In, Out] float[] verts
            , ref int vertCount
            , int vertsSize
            , [In, Out] int[] tris
            , ref int triCount
            , int trisSize);

        [DllImport("cai-nmgen-rcn", EntryPoint = "rcdtBuildPolyMeshDetail")]
        public static extern bool Build(IntPtr context
            , ref PolyMeshEx polyMesh
            , [In] CompactHeightfield chf
            , float sampleDist
            , float sampleMaxError
            , [In, Out] PolyMeshDetail detailMesh);
    }
}

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
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class is not directly serializable.  If you need serialization,
    /// then you'll need to save the the source's used to create the mesh.
    /// E.g. The RCPolyMesh and RCPolyMeshDetail.
    /// </remarks>
    public sealed class DTNavmesh 
        : IDisposable
    {
        public const int MaxVertsPerPolygon = DTNavmeshEx.MaxVertsPerPolygon;
        public const int MaxAreas = DTNavmeshEx.MaxAreas;

        internal IntPtr root;

        internal DTNavmesh(IntPtr mesh)
        {
            root = mesh;
        }

        ~DTNavmesh()
        {
            Dispose();
        }

        public DTNavMeshParams GetParams()
        {
            DTNavMeshParams result = DTNavMeshParams.Initialized;
            DTNavmeshEx.GetParams(root, ref result);
            return result;
        }

        public int GetMaxTiles()
        {
            return DTNavmeshEx.GetMaxTiles(root);
        }

        public bool IsValidPolyId(uint polyId)
        {
            return DTNavmeshEx.IsValidPolyId(root, polyId);
        }

        public DTStatus GetTileInfo(int tileIndex, out DTTileInfo info)
        {
            info = DTTileInfo.Initialized;
            return (DTStatus)DTNavmeshEx.GetTileInfo(root, tileIndex, ref info);
        }

        public DTStatus GetPolyInfo(uint polyId, out DTPolyInfo info)
        {
            info = DTPolyInfo.Initialized;
            return (DTStatus)DTNavmeshEx.GetPolyInfo(root, polyId, ref info);
        }

        public DTStatus GetPolyFlags(uint polyId, out ushort flags)
        {
            flags = 0;
            return (DTStatus)DTNavmeshEx.GetPolyFlags(root, polyId, ref flags);
        }

        public DTStatus SetPolyFlags(uint polyId, ushort flags)
        {
            return (DTStatus)DTNavmeshEx.SetPolyFlags(root, polyId, flags);
        }

        public DTStatus GetPolyArea(uint polyId, out byte flags)
        {
            flags = 0;
            return (DTStatus)DTNavmeshEx.GetPolyArea(root, polyId, ref flags);
        }

        public DTStatus SetPolyArea(uint polyId, byte flags)
        {
            return (DTStatus)DTNavmeshEx.SetPolyArea(root, polyId, flags);
        }

        //public DTStatus GetOffMeshConnectionEndPoints(uint prevPolyId
        //    , uint polyID
        //    , ref float[] startPos
        //    , ref float[] endPos)
        //{
        //    return (DTStatus)DTNavmeshEx.GetOffMeshConnectionPolyEndPoints(root
        //        , prevPolyId
        //        , polyID
        //        , ref startPos
        //        , ref endPos);
        //}

        public bool IsDisposed()
        {
            return (root == IntPtr.Zero);
        }

        public void Dispose()
        {
            if (root != IntPtr.Zero)
            {
                DTNavmeshEx.FreeNavmesh(ref root);
                root = IntPtr.Zero;
            }
        }

        public static uint GetPolyId(DTTileInfo tileInfo
            , int polyIndex)
        {
            return (tileInfo.basePolyId | (uint)polyIndex);
        }
    }
}

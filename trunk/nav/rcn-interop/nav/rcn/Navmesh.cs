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
    public sealed class Navmesh 
        : IDisposable
    {
        public const int MaxVertsPerPolygon = NavmeshEx.MaxVertsPerPolygon;
        public const int MaxAreas = NavmeshEx.MaxAreas;

        internal IntPtr root;

        internal Navmesh(IntPtr mesh)
        {
            root = mesh;
        }

        ~Navmesh()
        {
            Dispose();
        }

        public bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        public void Dispose()
        {
            if (root != IntPtr.Zero)
            {
                NavmeshEx.FreeNavmesh(ref root);
                root = IntPtr.Zero;
            }
        }

        public NavmeshParams GetParams()
        {
            NavmeshParams result = NavmeshParams.Initialized;
            NavmeshEx.GetParams(root, ref result);
            return result;
        }

        public int GetMaxTiles()
        {
            return NavmeshEx.GetMaxTiles(root);
        }

        public bool IsValidPolyId(uint polyId)
        {
            return NavmeshEx.IsValidPolyId(root, polyId);
        }

        public NavmeshStatus GetTileInfo(int tileIndex, out NavmeshTileData info)
        {
            info = NavmeshTileData.Initialized;
            return NavmeshEx.GetTileInfo(root, tileIndex, ref info);
        }

        public NavmeshStatus GetTilePolys(NavmeshTileData tile, out NavmeshPoly[] polys)
        {
            polys = NavmeshPoly.GetInitializedArray(tile.polygonCount);

            NavmeshStatus status = NavmeshEx.GetTilePolys(root
                , tile.tileIndex
                , polys
                , polys.Length);

            if (NavmeshUtil.Failed(status))
                polys = null;

            return status;
        }

        public NavmeshStatus GetTileVerts(NavmeshTileData tile, out float[] verts)
        {
            verts = new float[tile.vertexCount * 3];

            NavmeshStatus status = NavmeshEx.GetTileVerts(root
                , tile.tileIndex
                , verts
                , verts.Length);

            if (NavmeshUtil.Failed(status))
                verts = null;

            return status;
        }

        public NavmeshStatus GetTileDetailVerts(NavmeshTileData tile, out float[] verts)
        {
            verts = new float[tile.detailVertCount * 3];

            NavmeshStatus status = NavmeshEx.GetTileDetailVerts(root
                , tile.tileIndex
                , verts
                , verts.Length);

            if (NavmeshUtil.Failed(status))
                verts = null;

            return status;
        }

        public NavmeshStatus GetTileDetailTris(NavmeshTileData tile, out byte[] triangles)
        {
            triangles = new byte[tile.detailTriCount * 4];

            NavmeshStatus status = NavmeshEx.GetTileDetailTris(root
                , tile.tileIndex
                , triangles
                , triangles.Length);

            if (NavmeshUtil.Failed(status))
                triangles = null;

            return status;
        }

        public NavmeshStatus GetTileLinks(NavmeshTileData tile, out NavmeshLink[] links)
        {
            links = new NavmeshLink[tile.maxLinkCount];

            NavmeshStatus status = NavmeshEx.GetTileLinks(root
                , tile.tileIndex
                , links
                , links.Length);

            if (NavmeshUtil.Failed(status))
                links = null;

            return status;
        }

        public NavmeshStatus GetTileBVTree(NavmeshTileData tile, out BVNode[] nodes)
        {
            nodes = BVNode.GetInitializedArray(tile.bvNodeCount);

            NavmeshStatus status = NavmeshEx.GetTileBVTree(root
                , tile.tileIndex
                , nodes
                , nodes.Length);

            if (NavmeshUtil.Failed(status))
                nodes = null;

            return status;
        }

        public NavmeshStatus GetTileOffMeshCons(NavmeshTileData tile
            , out NavmeshConnection[] nodes)
        {
            nodes = 
                NavmeshConnection.GetInitializedArray(tile.offMeshConCount);

            NavmeshStatus status = NavmeshEx.GetTileOffMeshCons(root
                , tile.tileIndex
                , nodes
                , nodes.Length);

            if (NavmeshUtil.Failed(status))
                nodes = null;

            return status;
        }

        public NavmeshStatus GetTileDetailMeshes(NavmeshTileData tile
            , out NavmeshPolyDetail[] detailMeshes)
        {
            detailMeshes = new NavmeshPolyDetail[tile.detailMeshCount];

            NavmeshStatus status = NavmeshEx.GetTileDetailMeshes(root
                , tile.tileIndex
                , detailMeshes
                , detailMeshes.Length);

            if (NavmeshUtil.Failed(status))
                detailMeshes = null;

            return status;
        }

        public NavmeshStatus GetPolyFlags(uint polyId, out ushort flags)
        {
            flags = 0;
            return NavmeshEx.GetPolyFlags(root, polyId, ref flags);
        }

        public NavmeshStatus SetPolyFlags(uint polyId, ushort flags)
        {
            return NavmeshEx.SetPolyFlags(root, polyId, flags);
        }

        public NavmeshStatus GetPolyArea(uint polyId, out byte flags)
        {
            flags = 0;
            return NavmeshEx.GetPolyArea(root, polyId, ref flags);
        }

        public NavmeshStatus SetPolyArea(uint polyId, byte flags)
        {
            return NavmeshEx.SetPolyArea(root, polyId, flags);
        }

        public NavmeshStatus GetOffMeshConnEndpoints(uint previousPolyId
            , uint polyId
            , float[] startPosition
            , float[] endPosition)
        {
            return NavmeshEx.GetOffMeshConEndpoints(root
                , previousPolyId
                , polyId
                , startPosition
                , endPosition);
        }

        public static uint GetPolyId(NavmeshTileData tileInfo
            , int polyIndex)
        {
            return (tileInfo.basePolyId | (uint)polyIndex);
        }

        public static NavmeshStatus BuildStaticMesh(PolyMesh polyMesh
            , PolyMeshDetail detailMesh
            , NavWaypoints offMeshConnections
            , out Navmesh resultMesh)
        {
            IntPtr navMesh = IntPtr.Zero;

            NavmeshStatus status = NavmeshEx.BuildMesh(
                ref polyMesh.root
                , ref detailMesh.root
                , polyMesh.MinTraversableHeight
                , polyMesh.TraversableAreaBorderSize
                , polyMesh.MaxTraversableStep
                , ref offMeshConnections.root
                , ref navMesh);

            if (NavmeshUtil.Succeeded(status))
                resultMesh = new Navmesh(navMesh);
            else
                resultMesh = null;

            return status;
        }
    }
}

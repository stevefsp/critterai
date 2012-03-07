/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using org.critterai.nmgen;
using org.critterai.nav;
using System.Collections.Generic;
using Math = System.Math;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
	public static class NMBuild
	{
        public static NavmeshTileBuildData GetBuildData(int tx, int tz
            , PolyMeshData polyMesh
            , PolyMeshDetailData detailMesh
            , ConnectionSet connections
            , BuildContext context)
        {
            if (context == null)
                // Silent.
                return null;

            Vector3[] verts;
            float[] radii;
            byte[] dirs;
            byte[] areas;
            ushort[] flags;
            uint[] userIds;

            int connCount = connections.GetConnections(
                polyMesh.boundsMin, polyMesh.boundsMax
                , out verts, out radii, out dirs, out areas, out flags, out userIds);

            NavmeshTileBuildData result = new NavmeshTileBuildData(
                    polyMesh.vertCount
                    , polyMesh.polyCount
                    , polyMesh.maxVertsPerPoly
                    , (detailMesh == null ? 0 : detailMesh.vertCount)
                    , (detailMesh == null ? 0 : detailMesh.triCount)
                    , connCount);

            if (!result.LoadBase(tx, tz, 0, 0
                , polyMesh.boundsMin
                , polyMesh.boundsMax
                , polyMesh.xzCellSize
                , polyMesh.yCellSize
                , polyMesh.walkableHeight
                , polyMesh.walkableRadius
                , polyMesh.walkableStep
                , true))  // TODO: Parameterize
            {
                context.LogError("Base data load failed. Bad configuration data or internal error."
                    , null);
                return null;
            }

            if (!result.LoadPolys(polyMesh.verts
                , polyMesh.vertCount
                , polyMesh.polys
                , polyMesh.flags
                , polyMesh.areas
                , polyMesh.polyCount))
            {
                context.LogError("Polygon load failed. Bad mesh data or internal error.", null);
                return null;
            }

            if (detailMesh != null)
            {
                if (!result.LoadDetail(detailMesh.verts
                    , detailMesh.vertCount
                    , detailMesh.tris
                    , detailMesh.triCount
                    , detailMesh.meshes
                    , detailMesh.meshCount))
                {
                    context.LogError("Detail load failed. Bad mesh data or internal error.", null);
                    return null;
                }
            }

            if (connCount > 0)
            {
                if (!result.LoadConns(verts, radii, dirs, areas, flags, userIds, connCount))
                {
                    context.LogError("Off-mesh connection load failed. Bad data or internal error."
                        , null);
                    return null;
                }
            }

            return result;
        }
	}
}

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
using System;
using org.critterai.geom;
using org.critterai.nmgen.rcn;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// Provides various constants and utility methods related to generating
    /// navigation mesh data.
    /// </summary>
    public static class NMGen
    {
        /// <summary>
        /// The default area id used to indicate a walkable polygon.
        /// </summary>
        /// <remarks>
        /// <para>This is also the maximum value that can be used as an area id.</para>
        /// </remarks>
        public const byte WalkableArea = 63;

        /// <summary>
        /// The default flag applied to polygons during the build process
        /// if the <see cref="BuildFlags.ApplyPolyFlags"/> is set.
        /// </summary>
        public const ushort DefaultFlag = 0x01;

        /// <summary>
        /// Represents the null region.
        /// </summary>
        /// <remarks>
        /// <para>When a data item is given this region it is considered
        /// to have been removed from the the data set.</para>
        /// <para>Examples: When applied to a poygon, it indicates the polygon 
        /// should be culled from the final mesh. When applied to an edge,
        /// it means the edge is a solid wall.</para>
        /// </remarks>
        public const byte NullRegion = 0;

        /// <summary>
        /// Represents a null area.
        /// </summary>
        /// <remarks>
        /// <para>When a data item is given this value it is considered to 
        /// no longer be assigned to a usable area.</para>
        /// </remarks>
        [System.Obsolete("Use Unwalkable area. Will be removed in v0.5")]
        public const byte NullArea = 0;

        /// <summary>
        /// Represents an unwalkable area.
        /// </summary>
        /// <remarks>
        /// <para>When a data item is given this value it is considered to 
        /// no longer be assigned to a usable area.</para>
        /// <para>This is also the minimum value that can be used as an area id.</para>
        /// </remarks>
        public const byte UnwalkableArea = 0;

        /// <summary>
        /// The minimum allowed value for cells size parameters.
        /// </summary>
        public const float MinCellSize = 0.01f;

        /// <summary>
        /// The maximum allowed value for parameters that define maximum 
        /// vertices per polygon.
        /// </summary>
        public const int MaxAllowedVertsPerPoly = 6;

        /// <summary>
        /// The minimum value for parameters that define walkable height.
        /// </summary>
        /// <remarks>
        /// Dependencies between parameters may limit the minimum value 
        /// to a higher value.
        /// </remarks>
        public const int MinWalkableHeight = 3;

        /// <summary>
        /// The maximum allowed value for parameters that define slope.
        /// </summary>
        public const float MaxAllowedSlope = 85.0f;

        public static void DeriveSizeOfTileGrid(Vector3 boundsMin
            , Vector3 boundsMax
            , float xzCellSize
            , int tileSize
            , out int width
            , out int depth)
        {
            if (tileSize < 1)
            {
                width = 1;
                depth = 1;
                return;
            }

            int cellGridWidth = 
                (int)((boundsMax.x - boundsMin.x) / xzCellSize + 0.5f);
            int cellGridDepth = 
                (int)((boundsMax.z - boundsMin.z) / xzCellSize + 0.5f);

            width = (cellGridWidth + tileSize - 1) / tileSize;
            depth = (cellGridDepth + tileSize - 1) / tileSize;
        }

        /// <summary>
        /// Derive the width/depth values from the bounds and cell size values.
        /// </summary>
        public static void DeriveSizeOfCellGrid(Vector3 boundsMin
            , Vector3 boundsMax
            , float xzCellSize
            , out int width
            , out int depth)
        {
            width = (int)((boundsMax.x - boundsMin.x) / xzCellSize + 0.5f);
            depth = (int)((boundsMax.z - boundsMin.z) / xzCellSize + 0.5f);
        }

        /// <summary>
        /// Returns an array with all values set to <see cref="WalkableArea"/>.
        /// </summary>
        /// <param name="size">The length of the returned array.</param>
        /// <returns>An array with all values set to <see cref="WalkableArea"/>.
        /// </returns>
        public static byte[] CreateWalkableAreaBuffer(int size)
        {
            return CreateAreaBuffer(size, WalkableArea);
        }

        public static byte[] CreateAreaBuffer(int size, byte fillValue)
        {
            byte[] result = new byte[size];
            fillValue = Math.Min(WalkableArea, fillValue);
            for (int i = 0; i < size; i++)
            {
                result[i] = fillValue;
            }
            return result;
        }

        /// <summary>
        /// Validates the content of the area buffer.
        /// </summary>
        /// <remarks><para>The validation checks for an undersized buffer.
        /// It doesn't check for an oversized buffer.</para></remarks>
        /// <param name="areas"></param>
        /// <param name="areaCount"></param>
        /// <returns></returns>
        public static bool ValidateAreaBuffer(byte[] areas, int areaCount)
        {
            if (areas.Length < areaCount)
                return false;

            for (int i = 0; i < areaCount; i++)
            {
                if (areas[i] > WalkableArea)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Set the area id of all triangles with a slope below the specified
        /// value to <see cref="WalkableArea"/>.
        /// </summary>
        /// <param name="context">The context to use duing the operation.
        /// </param>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="walkableSlope">The maximum walkable slope.
        /// </param>
        /// <param name="areas">The area ids associated with each triangle.
        /// [Size: >= mesh.triCount].</param>
        /// <returns>TRUE if the operation was successful.</returns>
        public static bool MarkWalkableTriangles(BuildContext context
            , TriangleMesh mesh
            , float walkableSlope
            , byte[] areas)
        {
            if (mesh == null
                || context == null
                || areas == null || areas.Length < mesh.triCount)
            {
                return false;
            }

            NMGenEx.nmgMarkWalkableTriangles(context.root
                , walkableSlope
                , mesh.verts
                , mesh.vertCount
                , mesh.tris
                , mesh.triCount
                , areas);

            return true;
        }

        /// <summary>
        /// Set the area id of all triangles with a slope above the specified
        /// value to <see cref="NullArea"/>.
        /// </summary>
        /// <param name="context">The context to use duing the operation.
        /// </param>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="walkableSlope">The maximum walkable slope.
        /// </param>
        /// <param name="areas">The area ids associated with each triangle.
        /// [Size: >= mesh.triCount].</param>
        /// <returns>TRUE if the operation was successful.</returns>
        public static bool ClearUnwalkableTriangles(BuildContext context
            , TriangleMesh mesh
            , float walkableSlope
            , byte[] areas)
        {
            if (mesh == null
                || context == null
                || areas == null || areas.Length < mesh.triCount)
            {
                return false;
            }

            NMGenEx.nmgClearUnwalkableTriangles(context.root
                , walkableSlope
                , mesh.verts
                , mesh.vertCount
                , mesh.tris
                , mesh.triCount
                , areas);

            return true;
        }

        //public static bool ClearUnwalkableTriangles(BuildContext context
        //    , ChunkyTriMesh mesh
        //    , Vector3 bmin
        //    , Vector3 bmax
        //    , out byte[] areas)
        //{
        //    if (mesh == null || mesh.IsDisposed)
        //    {
        //        areas = null;
        //        return false;
        //    }

        //    List<ChunkyTriMeshNode> nodeList = new List<ChunkyTriMeshNode>();
        //    int triCount;

        //    mesh.GetChunks(bmin.x, bmin.z, bmax.x, bmax.z, nodeList, out triCount);

        //    areas = new byte[triCount];

        //    if (triCount == 0)
        //        return true;

        //    NMGenEx.nmgClearUnwalkableTrianglesAlt(context.root
        //        , mesh.root
        //        , nodeList.ToArray()
        //        , nodeList.Count
        //        , walkableSlope
        //        , areas);
        //}

        /// <summary>
        /// Builds an aggregate triangle mesh from a detail mesh.
        /// </summary>
        /// <remarks>
        /// <para>All duplicate vertices are merged.</para>
        /// </remarks>
        /// <param name="source">The detail mesh to extract the triangle mesh
        /// from.</param>
        /// <param name="verts">The result vertices. [Form: (x, y, z) * vertCount]
        /// </param>
        /// <param name="tris">The result triangles.
        /// [Form: (vertAIndex, vertBIndex, vertCIndex) * triCount]</param>
        /// <returns>TRUE if the operation completed successfully.
        /// </returns>
        public static bool ExtractTriMesh(PolyMeshDetail source
            , out Vector3[] verts
            , out int[] tris)
        {
            // TODO: EVAL: Inefficient.

            verts = null;
            tris = null;
            if (source == null || source.IsDisposed || source.TriCount == 0)
                return false;

            // Assume no duplicate verts.
            Vector3[] tverts = new Vector3[source.VertCount];
            tris = new int[source.TriCount * 3];
            int vertCount = 0;
            int triCount = 0;

            if (PolyMeshDetailEx.rcpdFlattenMesh(source
                , tverts
                , ref vertCount
                , source.VertCount
                , tris
                , ref triCount
                , source.TriCount))
            {
                verts = new Vector3[vertCount];
                for (int i = 0; i < vertCount; i++)
                {
                    verts[i] = tverts[i];
                }
                return true;
            }

            tris = null;
            return false;
        }



        public static byte ClampArea(byte value)
        {
            return Math.Min(WalkableArea, value);
        }

        public static byte ClampArea(int value)
        {
            return (byte)Math.Min(NMGen.WalkableArea, Math.Max(0, value));
        }
    }
}

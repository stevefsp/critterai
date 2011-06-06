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
using org.critterai.geom;
using org.critterai.nmgen.rcn;

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
        /// This is the only recognized non-zero area id recognized by some 
        /// steps in the mesh build process.
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
        public const byte NullArea = 0;

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

        /// <summary>
        /// Returns an array with all values set to <see cref="WalkableArea"/>.
        /// </summary>
        /// <param name="size">The length of the returned array.</param>
        /// <returns>An array with all values set to <see cref="WalkableArea"/>.
        /// </returns>
        public static byte[] BuildWalkableAreaBuffer(int size)
        {
            byte[] result = new byte[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = WalkableArea;
            }
            return result;
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

            NMGenEx.MarkWalkableTriangles(context.root
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
        public static bool ClearWalkableTriangles(BuildContext context
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

            NMGenEx.ClearUnwalkableTriangles(context.root
                , walkableSlope
                , mesh.verts
                , mesh.vertCount
                , mesh.tris
                , mesh.triCount
                , areas);

            return true;
        }

        /// <summary>
        /// Creates <see cref="PolyMesh"/> and <see cref="PolyMeshDetail"/>
        /// data from the specified source geometry.
        /// </summary>
        /// <param name="config">The configuration to use for the build.</param>
        /// <param name="buildFlags">Flags indicating the optional build
        /// steps to peform.</param>
        /// <param name="sourceMesh">The source geometry.</param>
        /// <param name="polyMesh">The resulting polygon mesh.</param>
        /// <param name="detailMesh">The resulting detail mesh.</param>
        /// <param name="messages">The messages generated by the build.</param>
        /// <param name="trace">TRUE if detailed trace messages should
        /// be generated.</param>
        /// <returns>TRUE if the build produced valid polygon and detail 
        /// meshes.</returns>
        public static bool BuildPolyMesh(NMGenParams config
            , BuildFlags buildFlags
            , TriangleMesh sourceMesh
            , out PolyMesh polyMesh
            , out PolyMeshDetail detailMesh
            , out string[] messages
            , bool trace)
        {

            IncrementalBuilder builder = new IncrementalBuilder(trace
                , config
                , buildFlags
                , sourceMesh);

            while (!builder.IsFinished)
            {
                builder.Build();
            }

            if (builder.State == BuildState.Complete)
            {
                polyMesh = builder.PolyMesh;
                detailMesh = builder.DetailMesh;
                messages = builder.GetMessages();
                return true;
            }

            polyMesh = null;
            detailMesh = null;
            messages = builder.GetMessages();

            return false;
        }

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
            , out float[] verts
            , out int[] tris)
        {
            // TODO: EVAL: Inefficient.

            verts = null;
            tris = null;
            if (source == null || source.IsDisposed || source.TriCount == 0)
                return false;

            // Assume no duplicate verts.
            float[] tverts = new float[source.VertCount * 3];
            tris = new int[source.TriCount * 3];
            int vertCount = 0;
            int triCount = 0;

            if (PolyMeshDetailEx.FlattenMesh(source
                , tverts
                , ref vertCount
                , source.VertCount
                , tris
                , ref triCount
                , source.TriCount))
            {
                verts = new float[vertCount * 3];
                Array.Copy(tverts, verts, verts.Length);
                return true;
            }

            tris = null;
            return false;
        }
    }
}

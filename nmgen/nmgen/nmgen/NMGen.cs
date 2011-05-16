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
    /// Provides various utility methods and constants related to generating
    /// navigation mesh data.
    /// </summary>
    public static class NMGen
    {
        /// <summary>
        /// The default area id used to indicate a walkable polygon.
        /// </summary>
        /// <remarks>
        /// This value is the only recognized non-zero id for most stages of 
        /// the mesh build process.
        /// </remarks>
        public const byte WalkableArea = 63;

        /// <summary>
        /// Represents the null region.
        /// </summary>
        /// <remarks>
        /// <p>When a data item is given this region it is not considered
        /// to have been removed from the the data set.
        /// <p>Examples: When applied to a poygon, it indicates the polygon 
        /// should be culled from the final mesh. When applied to an edge,
        /// it means the edge is a solid boundary.
        /// <p>
        /// </remarks>
        public const byte NullRegion = 0;

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
        /// Dependancies between parameters may limit the minimum value 
        /// to a higher value.
        /// </remarks>
        public const int MinWalkableHeight = 3;

        /// <summary>
        /// The maximum allowed value for <see cref="MaxTraversableSlope"/>.
        /// </summary>
        public const float MaxAllowedSlope = 85.0f;

        /// <summary>
        /// Returns an array with all values set to <see cref="WalkableArea"/>.
        /// </summary>
        /// <param name="length">The length of the array.</param>
        /// <returns>An array with all values set to <see cref="WalkableArea"/>.
        /// </returns>
        public static byte[] BuildWalkableAreaBuffer(int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = WalkableArea;
            }
            return result;
        }

        public static bool MarkWalkableTriangles(BuildContext context
            , TriangleMesh mesh
            , float walkableSlope
            , byte[] areas)
        {
            // TODO: Needs test.

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

        public static bool ClearWalkableTriangles(BuildContext context
            , TriangleMesh mesh
            , float walkableSlope
            , byte[] areas)
        {
            // TODO: Needs test.

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

            //// Design note: The return boolean on methods are only checked when 
            //// there is a possibility of content validation errors.  
            //// (Rather than just null errors and such.)

            //const string pre = "BuildPolyMesh: ";
            //const string pret = pre + "Trace: ";

            //polyMesh = null;
            //detailMesh = null;

            //if (context == null || context.IsDisposed)
            //    return false;

            //if (config == null
            //    || sourceMesh == null)
            //{
            //    context.Log(pre + "Aborted at parameter null check.");
            //    return false;
            //}

            //config = config.Clone();
            //config.DerivedGridSize();

            //// Applying this change based on recast sample code
            //// and experience. The naming doesn't seem to be acruate.
            //// config.minRegionArea 
            ////     = config.minRegionArea * config.minRegionArea;
            //// config.mergeRegionArea 
            ////     = config.mergeRegionArea * config.mergeRegionArea;

            //byte[] areas = new byte[sourceMesh.triCount];

            //MarkWalkableTriangles(context
            //    , sourceMesh
            //    , config.walkableSlope
            //    , areas);

            //if (trace)
            //    context.Log(pret + "Marked walkable triangles");

            //Heightfield hf = new Heightfield(config.width
            //    , config.depth
            //    , config.boundsMin
            //    , config.boundsMax
            //    , config.xzCellSize
            //    , config.yCellSize);

            //hf.AddTriangles(context
            //    , sourceMesh
            //    , areas
            //    , config.walkableStep);  // Merge for any spans less than step.

            //if (trace)
            //    context.Log(pret + "Voxelized triangles.");

            //areas = null;

            //if ((buildFlags & BuildFlags.LowObstaclesWalkable) != 0
            //    && config.walkableStep > 0)
            //{
            //    hf.MarkLowObstaclesWalkable(context, config.walkableStep);
            //    if (trace)
            //        context.Log(pret + "Flagged low obstacles as walkable.");

            //}

            //if ((buildFlags & BuildFlags.LedgeSpansNotWalkable) != 0)
            //{
            //    hf.MarkLedgeSpansNotWalkable(context
            //        , config.walkableHeight
            //        , config.walkableStep);
            //    if (trace)
            //        context.Log(pret + "Flagged ledge spans as not walklable");
            //}

            //if ((buildFlags & BuildFlags.LowHeightSpansNotWalkable) != 0)
            //{
            //    hf.MarkLowHeightSpansNotWalkable(context
            //        , config.walkableHeight);
            //    if (trace)
            //        context.Log(pret 
            //            + "Flagged low height spans as not walkable.");
            //}

            //CompactHeightfield chf = CompactHeightfield.Build(context
            //    , hf
            //    , config.walkableHeight
            //    , config.walkableStep);

            //hf.RequestDisposal();
            //hf = null;

            //if (chf == null)
            //{
            //    context.Log(pre + "Aborted at compact heightfield build.");
            //    return false;
            //}

            //if (trace)
            //    context.Log(pret + "Built compact heightfield.");

            //if (config.walkableRadius > 0)
            //{
            //    chf.ErodeWalkableArea(context, config.walkableRadius);
            //    if (trace)
            //        context.Log(pret + "Eroded walkable area by radius.");
            //}

            //// TODO: EVAL: The Recast sample code skips this step
            //// when building monotone regions.  But I get errors.
            //// Problem with this design?  Problem with sample?
            //chf.BuildDistanceField(context);
            //if (trace)
            //    context.Log(pret + "Built distance field.");

            //if ((buildFlags & BuildFlags.UseMonotonePartitioning) != 0)
            //{
            //    chf.BuildRegionsMonotone(context
            //        , config.borderSize
            //        , config.minRegionArea
            //        , config.mergeRegionArea);
            //    if (trace)
            //        context.Log(pret + "Built monotone regions.");
            //}
            //else
            //{

            //    chf.BuildRegions(context
            //        , config.borderSize
            //        , config.minRegionArea
            //        , config.mergeRegionArea);
            //    if (trace)
            //        context.Log(pret + "Built regions.");
            //}

            //ContourBuildFlags cflags = 
            //    (ContourBuildFlags)((int)buildFlags & 0x03);

            //ContourSet cset = ContourSet.Build(context
            //    , chf
            //    , config.edgeMaxDeviation
            //    , config.maxEdgeLength
            //    , cflags);

            //if (cset == null)
            //{
            //    context.Log(pre + "Aborted at contour set build.");
            //    return false;
            //}

            //if (trace)
            //    context.Log(pret + "Build contour set.");

            //polyMesh = PolyMesh.Build(context
            //    , cset
            //    , config.maxVertsPerPoly
            //    , config.walkableHeight
            //    , config.walkableRadius
            //    , config.walkableStep);

            //cset.RequestDisposal();
            //cset = null;

            //if (polyMesh == null)
            //{
            //    context.Log(pre + "Aborted at poly mesh build.");
            //    return false;
            //}

            //if (trace)
            //    context.Log(pret + "Built poly mesh.");

            //detailMesh = PolyMeshDetail.Build(context
            //    , polyMesh
            //    , chf
            //    , config.detailSampleDistance
            //    , config.detailMaxDeviation);

            //chf.RequestDisposal();
            //chf = null;

            //if (detailMesh == null)
            //{
            //    context.Log(pre + "Aborted at detail mesh build.");
            //    polyMesh.RequestDisposal();
            //    polyMesh = null;
            //    return false;
            //}

            //if (trace)
            //    context.Log(pret + "Built detail mesh.");
            
            //return true;
        }

        /// <summary>
        /// Builds a standard triangle mesh from the detail mesh data.
        /// </summary>
        /// <remarks>
        /// All duplicate vertices are merged.
        /// </remarks>
        /// <param name="vertices">The result vertices. (x, y, z) * vertexCount
        /// </param>
        /// <param name="triangles">The result triangles.
        /// (vertAIndex, vertBIndex, vertCIndex) * triangleCount</param>
        /// <returns></returns>
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

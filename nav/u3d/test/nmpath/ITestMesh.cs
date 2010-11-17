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
namespace org.critterai.nav.nmpath
{
    public interface ITestMesh
    {
        ///
        /// The number of polygons in the mesh.
        /// @return The number of polygons in the mesh.
        ///
        int PolyCount { get; }

        ///
        /// A value that, if used to offset a point from a polygon centroid
        /// or edge midpoint will not cause the point to exit the polygon.
        /// <para>E.G. If a point is offset from edge0midpoint toward edge1midpoint
        /// of any polygon in the mesh, the resulting point will still be within
        /// the polygon.</para> 
        ///
        float Offset { get; }

        ///
        /// Returns the minimum vertex in the mesh such that movement in direction (-1, -1, -1)
        /// will always result in a point outside of the mesh.
        /// @return The minimum vertex in the mesh in the form (x, y, z).
        ///
        float[] MinVertex { get; }

        ///
        /// Returns the indices of all polygons that share the mesh's minimum vertex.
        /// @return The indices of all polygons that share the mesh's minimum vertex.
        /// @see ITestMesh#MinVertex()
        ///
        int[] GetMinVertexPolys();

        ///
        /// The mesh indices in the form (vertAIndex, vertBIndex, vertCIndex) * polygonCount
        /// @return The mesh indices.
        ///
        int[] GetIndices();

        ///
        /// The mesh vertices in the form (x, y, z) * vertCount.
        /// @return The mesh vertices.
        ///
        float[] GetVerts();

        ///
        /// An array containing information on the number of polygons adjacent to each polygon in the
        /// mesh, in the form (linkCountPoly0, linkCountPoly1, ..., linkCountPolyN).
        /// @return  An array containing information on the number of polygons adjacent to each polygon in the
        /// mesh.
        ///
        int[] GetLinkCounts();

        ///
        /// An array containing information on the neighbor polygon walls linked to each polygon in the mesh,
        /// in the form (link0NeighborWall, link1NeighborWall, link2NeighborWall) * polyCount.
        /// <para>If a wall has no adjacent polygon, the value for the wall will be -1.</para>
        /// <para>Example:  For the polygon entry (2, -1, 1).
        /// Wall 2 of an adjacent polygon is linked to wall 0 of this polygon.
        /// There is no polygon adjacent to wall 1 of this polygon.
        /// Wall 1 of an adjacent polygon is linked to wall 2 of this polygon.<para>
        /// @return An array containing information on the polygon walls linked to each polygon in the mesh.
        ///
        int[] GetLinkWalls();

        ///
        /// An array containing information on the polygons linked to each polygon in the mesh,
        /// in the form (link0AdjacentPoly, link1AdjacentPoly, link2AdjacentPoly) * polyCount.
        /// <para>If a wall has no adjacent polygon, the value for the wall will be -1.</para>
        /// <para>Example:  For the polygon entry (14, -1, 8).
        /// polygon14 is linked to wall 0 of this polygon.
        /// There is no polygon linked to wall 1 of this polygon.
        /// polygon8 is linked to to wall 2 of this polygon.<para>
        /// @return An array containing information on the polygons linked to each polygon in the mesh.
        ///
        int[] GetLinkPolys();

        ///
        /// An array containing information on start and end polygons for valid LOS positions
        /// in the form (startPolyIndex, endPolyIndex).
        /// Used in conjunction with {@Link #GetLOSPointsTrue()}.
        /// @return An array containing information on start and end polygons for valid 
        /// LOS positions.
        ///
        int[] GetLOSPolysTrue();

        ///
        /// An array containing information on start and end points that have valid LOS in the
        /// form (startX, startZ, endX, endZ).
        /// Used in conjunction with {@Link #GetLOSPolysTrue()}.
        /// @return An array containing information on start and end points that have valid LOS.
        ///
        float[] GetLOSPointsTrue();

        ///
        /// An array containing information on start and end polygons for invalid LOS positions
        /// in the form (startPolyIndex, endPolyIndex).
        /// Used in conjunction with {@Link #GetLOSPointsFalse()}.
        /// @return An array containing information on start and end polygons for invalid 
        /// LOS positions.
        ///
        int[] GetLOSPolysFalse();

        ///
        /// An array containing information on start and end points that do not have LOS in the
        /// form (startX, startZ, endX, endZ).
        /// Used in conjunction with {@Link #GetLOSPolysFalse()}.
        /// @return An array containing information on start and end points that no not have LOS.
        ///
        float[] GetLOSPointsFalse();

        ///
        /// The number of tests paths.
        /// @return
        ///
        int GetPathCount { get; }

        ///
        /// An array of polygon indices for a valid path in the form 
        /// (polyIndex1, polyIndex2, ..., polyIndexN).
        /// Used in conjunction with {@Link #GetPathPoints(int)}.
        /// <para>Guarantee: If paths are supported, the path length will never be less than 2.</para>
        /// @return An array of polygon indices for a valid path.
        ///
        int[] GetPathPolys(int index);

        ///
        /// An array containing information the start and goal point for a valid path in
        /// the form (startX, startY, startZ, GoalX, GoalY, GoalZ).
        /// <para>Guarantee: If paths are supported, all goal points will be unique.</para>
        /// Used in conjunction with {@Link #GetPathPolys(int)}.
        /// @return An array containing information the start and goal point for a valid path.
        ///
        float[] GetPathPoints(int index);

        ///
        /// Represents a value that is safe to use for plane offset tests.  Specifically,
        /// the value is less than 50% of the minimum y-axis distance between two overlapping cells.
        /// @return
        ///
        float PlaneTolerance { get; }

        ///
        /// The number of paths in the multi-path. (Single start point, multiple goal points.)
        /// <para>Guarantee: If supported, will provide at least two unique paths.</para>
        /// @return The number of paths in the multi-path. 
        ///
        int MultiPathCount { get; }

        ///
        /// The start point of the multi-path in the form (x, y, z).
        /// @return The start point of the multi-path.
        ///
        float[] MultiPathStartPoint { get; }

        ///
        /// A goal point for the multi-path in the form (x, y, z).
        /// Used in conjunction with {@Link #GetMultiPathPolys(int)}.
        /// @param index The path index for the goal.
        /// @return A goal point for the multi-path
        ///
        float[] GetMultiPathGoalPoint(int index);

        ///
        /// A path for the each goal point in the multi-path in the form
        /// (polyIndex1, polyIndex2, ..., polyIndexN).
        /// Used in conjunction with {@Link #GetMultiPathGoalPoint(int)}.
        /// <para>Guarantee: If supported, will provide at two paths of unique mDepth, and
        /// all paths with a mDepth greater than one.</para>
        /// @param index The path index.
        /// @return A path for the each goal point in the multi-path
        ///
        int[] GetMultiPathPolys(int index);

        ///
        /// The index of the shortest multi-path.
        /// @return The index of the shortest multi-path.
        ///
        int GetShortestMultiPath();
    }
}

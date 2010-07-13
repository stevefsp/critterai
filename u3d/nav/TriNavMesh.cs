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
using System;
using System.Collections.Generic;
using org.critterai.math;
using UnityEngine;

namespace org.critterai.nav
{
    /// <summary>
    /// Represents a navigation mesh comprised of triangle cells.
    /// </summary>
    /// <remarks>
    /// Instances of this class are not thread-safe. Static operations are thread-safe.
    /// </remarks>
    public sealed class TriNavMesh
    {
        /// <summary>
        /// The tolerance used for various operations which are susceptible to
        /// floating point errors.
        /// </summary>
        public const float TOLERANCE = MathUtil.TOLERANCE_STD;

        private readonly TriCellQuadTree mCells;
        private readonly float mOffsetScale;
        private readonly float mPlaneTolerance;

        /// <summary>
        /// A value to use when offsetting waypoints from the edge of
        /// cells.
        /// </summary>
        /// <remarks>This information is for use by clients. 
        /// It does not have any side effects on the mesh.
        /// </remarks>
        public float OffsetScale { get { return mOffsetScale; } }

        /// <summary>
        /// The tolerance to use when evaluating whether a point is on the
        /// surface of the navigation mesh.
        /// </summary>
        /// <remarks>This information is for use by clients. 
        /// It does not have any side effects on the mesh.
        /// </remarks>
        public float PlaneTolerance { get { return mPlaneTolerance; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="minX">The minimum x-bound of the of the mesh's AABB.</param>
        /// <param name="minZ">The minimum z-bound of the of the mesh's AABB.</param>
        /// <param name="maxX">The maximum x-bound of the of the mesh's AABB.</param>
        /// <param name="maxZ">The maximum z-bound of the of the mesh's AABB.</param>
        /// <param name="maxDepth">The allowed quad tree depth to use when storing
        /// navigation mesh cells. Higher values will tend to improve cell
        /// search speed at the cost of more memory use.</param>
        /// <param name="planeTolerance">The tolerance to use when evaluating whether a point is on the
        /// surface of the navigation mesh.  This value should be set above the maximum
        /// the source geometry deviates from the navigation mesh and below 0.5 times the minimum
        /// distance between overlapping navigation mesh polygons. 
        /// This information is for use by clients. It does not have any side effects on the mesh.</param>
        /// <param name="offsetScale">A value to use when offsetting waypoints from the edge of
        /// cells. Values between 0.05 and 0.1 are normal. Values above 0.5 are not appropriate for normal
        /// pathfinding.  This information is for use by clients. It does not have any side effects
        /// on the mesh.</param>
        private TriNavMesh(float minX, float minZ
                , float maxX, float maxZ
                , int maxDepth
                , float planeTolerance
                , float offsetScale)
        {
            mCells = new TriCellQuadTree(minX, minZ, maxX, maxZ, maxDepth);
            mPlaneTolerance = Math.Max(float.Epsilon, planeTolerance);
            mOffsetScale = Math.Max(0, offsetScale);
        }
        
        /// <summary>
        /// Returns the cell that best matches the provided point.
        /// </summary>
        /// <remarks>
        /// <para>Setting mustBeInColumn to TRUE is much more efficient since it allows culling of cells.
        /// If the (x, z) location of the point is expected to reside within the xz-column of
        /// the mesh and the y-value is expected to be close to the mesh surface, then first search
        /// using mustBeInColumn=TRUE.  If NULL is returned, then an exhaustive search can be 
        /// performed.</para>
        /// <para>The mesh column is the area that extends above and below the xz-plane projection 
        /// of the mesh.</para>
        /// <para>If a point lies on a vertex or wall shared by more than one cell, or the point is
        /// equidistant to more than one cell, then which cell is chosen is arbitrary.</para>
        /// </remarks>
        /// <param name="x">The x-value of the point (x, y, z).</param>
        /// <param name="y">The y-value of the point (x, y, z).</param>
        /// <param name="z">The z-value of the point (x, y, z).</param>
        /// <param name="mustBeInColumn">If TRUE, then only cells in whose column the point resides will be considered. 
        /// If FALSE, an exhaustive search will be performed.</param>
        /// <param name="outClosestPointOnMesh">If provided, will be populated with the point snapped to the cell.
        /// If the point is already within the cell's column, the (x, z) values will not be altered.  
        /// In this case only the y-value will be updated.</param>
        /// <returns>The cell that is closest to the provided point, or NULL if mustBeInColumn=TRUE
        /// and the provided point does not lie in the xz-column the mesh.</returns>
        public TriCell GetClosestCell(float x, float y, float z
                , Boolean mustBeInColumn
                , out Vector3 outClosestPointOnMesh) 
        {
            return mCells.GetClosestCell(x, y, z, mustBeInColumn, out outClosestPointOnMesh);
        }

        /// <summary>
        ///
        /// Indicates whether or not the point is within the xz-column of the mesh
        /// and within the specified tolerance of the mesh surface. Only the y-axis
        /// is considered during the tolerance test.
        /// </summary>
        /// <param name="x">The x-value of the point (x, y, z).</param>
        /// <param name="y">The y-value of the point (x, y, z).</param>
        /// <param name="z">The z-value of the point (x, y, z).</param>
        /// <param name="yTolerance">The y-axis tolerance.  (How close the point must
        /// be to the surface of the mesh.)</param>
        /// <returns>True if the point is within the xz-column of the mesh
        /// and within the specified tolerance of the mesh surface.</returns>
        public Boolean IsValidPosition(float x, float y, float z, float yTolerance)
        {
            Vector3 v;
            if (mCells.GetClosestCell(x, y, z, true, out v) == null)
                return false;
            if (y < v.y - yTolerance 
                    || y > v.y + yTolerance)
                return false;
            return true;
        }

        /// <summary>
        /// Build a navigation mesh from the provided data.
        /// </summary>
        /// <param name="verts">The navigation mesh vertices in the form (x, y, z).</param>
        /// <param name="indices">The navigation mesh triangles in the form (vertAIndex, vertBIndex, vertCIndex)</param>
        /// <param name="spacialDepth">The allowed quad tree depth to use when storing
        /// navigation mesh cells. Higher values will tend to improve cell
        ///  search speed at the cost of more memory use.</param>
        /// <param name="planeTolerance">The tolerance to use when evaluating whether a point is on the
        /// surface of the navigation mesh.  This value should be set above the maximum
        /// the source geometry deviates from the navigation mesh and below 0.5 times the minimum
        /// distance between overlapping navigation mesh polygons. 
        /// This information is for use by clients. It does not have any side effects on the mesh.</param>
        /// <param name="offsetScale">A value to use when offsetting waypoints from the edge of
        /// cells. Values between 0.05 and 0.1 are normal. Values above 0.5 are not appropriate for normal
        /// pathfinding.  This information is for use by clients. It does not have any side effects on the mesh.</param>
        /// <returns>A navigation mesh.</returns>
        public static TriNavMesh Build(float[] verts
                , int[] indices
                , int spacialDepth
                , float planeTolerance
                , float offsetScale)
        {
            if (indices.Length % 3 != 0
                    || verts.Length % 3 != 0)
                return null;
            
            float[] cverts = new float[verts.Length];
            Array.Copy(verts, 0, cverts, 0, verts.Length);
            
            // Find the 2D AABB of the mesh.
            float minX = cverts[0];
            float minZ = cverts[2];
            float maxX = cverts[0];
            float maxZ = cverts[2];
            for (int pVert = 3; pVert < verts.Length; pVert += 3)
            {
                minX = Math.Min(minX, cverts[pVert]);
                minZ = Math.Min(minZ, cverts[pVert+2]);
                maxX = Math.Max(maxX, cverts[pVert]);
                maxZ = Math.Max(maxZ, cverts[pVert+2]);
            }
            
            TriNavMesh result = new TriNavMesh(minX
                    , minZ
                    , maxX
                    , maxZ
                    , spacialDepth
                    , planeTolerance
                    , offsetScale);
            
            List<TriCell> cells = new List<TriCell>();
            
            // Generate cell data.
            for (int pPoly = 0; pPoly < indices.Length; pPoly += 3)
            {
                TriCell cell = new TriCell(cverts
                        , indices[pPoly]
                        , indices[pPoly+1]
                        , indices[pPoly+2]);
                cells.Add(cell);
                result.mCells.Add(cell);
            }

            List<TriCell> neighborCells = new List<TriCell>();
            
            // Generate cell links.
            foreach (TriCell cell in cells)
            {
                int maxLinks = cell.MaxLinks;
                if (maxLinks == cell.LinkCount)
                    // All links already assigned.
                    continue;
                // Get all cells that intersect with current cell's slightly
                // expanded AABB.
                result.mCells.GetCellsInColumn(cell.BoundsMinX - TOLERANCE
                          , cell.BoundsMinZ - TOLERANCE
                          , cell.BoundsMaxX + TOLERANCE
                          , cell.BoundsMaxZ + TOLERANCE
                          , neighborCells);

                // Loop through all Link locations and check to see if
                // any of the neighbors can be linked.
	            foreach (TriCell nCell in neighborCells)
	            {
	                if (cell.Link(nCell, true) != TriCell.NULL_INDEX)
	                {
	            	    // Successful link.
	            	    if (cell.LinkCount == maxLinks)
	            		    break;
	                    continue;
	                }
	            }
            }
            
            return result;
        }

        /// <summary>
        /// Indicates whether or not the end point is visible from the start point.
        /// Only the xz-plane projection of the mesh is considered.
        /// </summary>
        /// <remarks>
        /// <para>False negatives can result if the path intersects a vertex.
        /// Starting or ending on a vertex will trigger special handling designed to minimize
        /// false negatives.  But they may still occur.</para>
        /// <para>Assumptions:</para>
        /// <ul>
        /// <li>The start cell contains the start position.</li>
        /// <li>The end cell contains the end position.</li>
        /// <li>The start and end cells are part of a properly constructed and linked
        /// mesh of <see cref="TriCell">TriCell</see> objects.</li>
        /// </ul>
        /// <para>Behavior is undefined if assumptions are not met.</para>
        /// </remarks>
        /// <param name="startX">The x-value of the start point. (startX, startZ)</param>
        /// <param name="startZ">The z-value of the start point. (startX, startZ)</param>
        /// <param name="endX">The x-value of the end point. (endX, endZ)</param>
        /// <param name="endZ">The z-value of the end point. (endX, endZ)</param>
        /// <param name="startCell">The cell that contains the start point.</param>
        /// <param name="endCell">The cell that contains the end point.</param>
        /// <param name="offsetScale">The scale to offset the start and end points if they
        /// lie on a vertex of their respective cells.  If this occurs, the point is offset toward
        /// the center of the cell by  this percentage of the vertex to centroid distance.
        /// A value of between 0.01 and 0.1 is normal.</param>
        /// <returns>TRUE if the two points have line of sight.  Otherwise FALSE.</returns>
        public static Boolean HasLOS(float startX, float startZ
                ,  float endX, float endZ
                , TriCell startCell
                , TriCell endCell
                , float offsetScale)
        {
            
            // This is the return result.
            Boolean losSucceeded = false;
            TriCell nextCell = null;
            int exitWallIndex;
            
            // Handle special case: Start and/or end on a vertex.
            Vector2 workingVector = startCell.GetSafePoint(startX, startZ, offsetScale);
            startX = workingVector.x;
            startZ = workingVector.y;
            workingVector = endCell.GetSafePoint(endX, endZ, offsetScale);
            endX = workingVector.x;
            endZ = workingVector.y;
            
            // Loop through the cells along the path until one of the following conditions is met:
            // - The end cell (which contains the end position) is intersected.
            // - A cell which has a vertex which matches the end point is intersected.
            // - The path hits a wall without a Link. (LOS failure.)
            PathRelType pathResult = startCell.GetPathRelationship(startX
                    , startZ
                    , endX
                    , endZ
                    , out nextCell
                    , out exitWallIndex
                    , out workingVector);
            while(pathResult == PathRelType.ExitingCell)
            {
                if (nextCell == null) 
                    break;  // Hit a non-linked wall.
                
                // Perform some pre-tests to see if the next cell contains the end position.
                
                /*
                 * Besides efficiency, this next check is necessary for situations
                 * where the end of the path is on a wall. In such cases, math inaccuracies
                 * can result in the following:
                 * Cell A indicates path entry to Cell B.
                 * Cell B indicates no entry. (E.G. No relationship.)
                 * Path check falsely fails.
                 */
                if (nextCell == endCell) 
                {
                    losSucceeded = true;
                    break;
                }
                
                // Next cell is not the end cell.  So continue more expensive checks.
                pathResult = nextCell.GetPathRelationship(startX
                        , startZ
                        , endX
                        , endZ
                        , out nextCell
                        , out exitWallIndex
                        , out workingVector);
            }
        
            if (pathResult == PathRelType.EndingCell) 
                losSucceeded = true;    
            
            return losSucceeded;
        
        }
        
    }
}

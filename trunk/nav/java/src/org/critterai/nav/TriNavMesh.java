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
package org.critterai.nav;

import java.util.ArrayList;

import org.critterai.math.MathUtil;
import org.critterai.math.Vector2;
import org.critterai.math.Vector3;

/**
 * Represents a navigation mesh comprised of triangle cells.
 * <p>Instances of this class are not thread-safe. Static operations are thread-safe.</p>
 */
public final class TriNavMesh
{

    /**
     * The tolerance used for various operations which are susceptible to
     * floating point errors.
     */
    public static final float TOLERANCE = MathUtil.TOLERANCE_STD;
    
    private final TriCellQuadTree mCells;
    
    private final float mOffsetScale;
    
    private final float mPlaneTolerance;
    
    private final Vector3 mtVec3A = new Vector3();
    
    /**
     * Constructor
     * @param minX The minimum x-bound of the of the mesh's AABB.
     * @param minZ The minimum z-bound of the of the mesh's AABB.
     * @param maxX The maximum x-bound of the of the mesh's AABB.
     * @param maxZ The maximum z-bound of the of the mesh's AABB.
     * @param maxDepth The allowed quad tree depth to use when storing
     * navigation mesh cells. Higher values will tend to improve cell
     *  search speed at the cost of more memory use.
     * @param planeTolerance The tolerance to use when evaluating whether a point is on the
     * surface of the navigation mesh.  This value should be set above the maximum
     * the source geometry deviates from the navigation mesh and below 0.5 times the minimum
     * distance between overlapping navigation mesh polygons. 
     * This information is for use by clients. It does not have any side effects on the mesh.
     * @param offsetScale A value to use when offsetting waypoints from the edge of
     * cells. Values between 0.05 and 0.1 are normal. Values above 0.5 are not appropriate for normal
     * pathfinding.  This information is for use by clients. It does not have any side effects on the mesh.
     * @throws IllegalArgumentException If the bounds is invalid.
     */
    private TriNavMesh(float minX, float minZ
            , float maxX, float maxZ
            , int maxDepth
            , float planeTolerance
            , float offsetScale) 
        throws IllegalArgumentException
    {
        mCells = new TriCellQuadTree(minX, minZ, maxX, maxZ, maxDepth);
        mPlaneTolerance = Math.max(Float.MIN_VALUE, planeTolerance);
        mOffsetScale = Math.max(0, offsetScale);
    }
    
    /**
     * Returns the cell that best matches the provided point.
     * <p>Setting mustBeInColumn to TRUE is much more efficient since it allows culling of cells.
     * If the (x, z) location of the point is expected to reside within the xz-column of
     * the mesh and the y-value is expected to be close to the mesh surface, then first search
     * using mustBeInColumn=TRUE.  If NULL is returned, then an exhaustive search can be performed.</p>
     * <p>The mesh column is the area that extends above and below the xz-plane projection of the mesh.</p>
     * <p>If a point lies on a vertex or wall shared by more than one cell, or the point is equidistant
     * to more than one cell, then which cell is chosen is arbitrary.</p>
     * @param x The x-value of the point (x, y, z).
     * @param y The y-value of the point (x, y, z).
     * @param z The z-value of the point (x, y, z).
     * @param mustBeInColumn If TRUE, then only cells in whose column the point resides will be considered. 
     * If FALSE, an exhaustive search will be performed.
     * @param outClosestPointOnMesh If provided, will be populated with the point snapped to the cell.
     * If the point is already within the cell's column, the (x, z) values will not be altered.  
     * In this case only the y-value will be updated.
     * @return The cell that is closest to the provided point, or NULL if mustBeInColumn=TRUE
     * and the provided point does not lie in the xz-column the mesh..
     */
    public TriCell getClosestCell(float x, float y, float z
            , boolean mustBeInColumn
            , Vector3 outClosestPointOnMesh) 
    {
        return mCells.getClosestCell(x, y, z, mustBeInColumn, outClosestPointOnMesh);
    }

    /**
     * Indicates whether or not the point is within the xz-column of the mesh
     * and within the specified tolerance of the mesh surface. Only the y-axis
     * is considered during the tolerance test.
     * @param x The x-value of the point (x, y, z).
     * @param y The y-value of the point (x, y, z).
     * @param z The z-value of the point (x, y, z).
     * @param yTolerance The y-axis tolerance.  (How close the point must
     * be to the surface of the mesh.)
     * @return True If the point is within the xz-column of the mesh
     * and within the specified tolerance of the mesh surface.
     */
    public boolean isValidPosition(float x, float y, float z, float yTolerance)
    {
        if (mCells.getClosestCell(x, y, z, true, mtVec3A) == null)
            return false;
        if (y < mtVec3A.y - yTolerance 
                || y > mtVec3A.y + yTolerance)
            return false;
        return true;
    }
    
    /**
     * A value to use when offsetting waypoints from the edge of
     * cells.
     * <p>This information is for use by clients. It does not have any side effects on the mesh.</p>
     * @return A value to use when offsetting waypoints from the edge of
     * cells.
     */
    public float offsetScale() { return mOffsetScale; }
    
    /**
     * The tolerance to use when evaluating whether a point is on the
     * surface of the navigation mesh.
     * <p>This information is for use by clients. It does not have any side effects on the mesh.</p>
     * @return The tolerance to use when evaluating whether a point is on the
     * surface of the navigation mesh.
     */
    public float planeTolerance() { return mPlaneTolerance; }
    /**
     * Build a navigation mesh from the provided data.
     * @param verts The navigation mesh vertices in the form (x, y, z).
     * @param indices The navigation mesh triangles in the form (vertAIndex, vertBIndex, vertCIndex)
     * @param spacialDepth The allowed quad tree depth to use when storing
     * navigation mesh cells. Higher values will tend to improve cell
     *  search speed at the cost of more memory use.
     * @param planeTolerance The tolerance to use when evaluating whether a point is on the
     * surface of the navigation mesh.  This value should be set above the maximum
     * the source geometry deviates from the navigation mesh and below 0.5 times the minimum
     * distance between overlapping navigation mesh polygons. 
     * This information is for use by clients. It does not have any side effects on the mesh.
     * @param offsetScale A value to use when offsetting waypoints from the edge of
     * cells. Values between 0.05 and 0.1 are normal. Values above 0.5 are not appropriate for normal
     * pathfinding.  This information is for use by clients. It does not have any side effects on the mesh.
     * @return A navigation mesh.
     */
    public static TriNavMesh build(float[] verts
            , int[] indices
            , int spacialDepth
            , float planeTolerance
            , float offsetScale)
    {
        if (indices.length % 3 != 0
                || verts.length % 3 != 0)
            return null;
        
        float[] cverts = new float[verts.length];
        System.arraycopy(verts, 0, cverts, 0, verts.length);
        
        // Find the 2D AABB of the mesh.
        float minX = cverts[0];
        float minZ = cverts[2];
        float maxX = cverts[0];
        float maxZ = cverts[2];
        for (int pVert = 3; pVert < verts.length; pVert += 3)
        {
            minX = Math.min(minX, cverts[pVert]);
            minZ = Math.min(minZ, cverts[pVert+2]);
            maxX = Math.max(maxX, cverts[pVert]);
            maxZ = Math.max(maxZ, cverts[pVert+2]);
        }
        
        final TriNavMesh result = new TriNavMesh(minX
                , minZ
                , maxX
                , maxZ
                , spacialDepth
                , planeTolerance
                , offsetScale);
        
        final ArrayList<TriCell> cells = new ArrayList<TriCell>();
        
        // Generate polygon information.
        for (int pPoly = 0; pPoly < indices.length; pPoly += 3)
        {
            TriCell cell = new TriCell(cverts
                    , indices[pPoly]
                    , indices[pPoly+1]
                    , indices[pPoly+2]);
            cells.add(cell);
            result.mCells.add(cell);
        }
        
        final ArrayList<TriCell> neighborCells = new ArrayList<TriCell>();
        
        // Generate cell links.
        for (TriCell cell : cells)
        {
            final int maxLinks = cell.maxLinks();
            if (maxLinks == cell.linkCount())
                // All links already assigned.
                continue;
            // Get all cells that intersect with current cell's slightly
            // expanded AABB.
            result.mCells.getCellsInColumn(cell.boundsMinX - TOLERANCE
                      , cell.boundsMinZ - TOLERANCE
                      , cell.boundsMaxX + TOLERANCE
                      , cell.boundsMaxZ + TOLERANCE
                      , neighborCells);
            // Attempt to link neighbors.
	        for (TriCell nCell : neighborCells)
	        {
	            if (cell.link(nCell, true) != TriCell.NULL_INDEX)
	            {
	            	// Successful link.
	            	if (cell.linkCount() == maxLinks)
	            		break;
	                continue;
	            }
	        }
        }
        
        return result;
    }

    /**
     * Indicates whether or not the end point is visible from the start point.
     * Only the xz-plane projection of the mesh is considered.
     * <p>False negatives can result if the path intersects a vertex.
     * Starting or ending on a vertex will trigger special handling designed to minimize
     * false negatives.  But they may still occur.</p>
     * <p>Assumptions:
     * The start cell contains the start position.
     * The end cell contains the end position.
     * The start and end cells are part of a properly constructed and linked
     * mesh of {@link TriCell} objects.
     * Behavior is undefined if assumptions are not met.</p>
     * @param startX The x-value of the start point. (startX, startZ)
     * @param startZ The z-value of the start point. (startX, startZ)
     * @param endX The x-value of the end point. (endX, endZ)
     * @param endZ The z-value of the end point. (endX, endZ)
     * @param startCell The cell that contains the start point.
     * @param endCell The cell that contains the end point.
     * @param offsetScale The scale to offset the start and end points if they
     * lie on a vertex of their respective cells.  If this occurs, the point is offset toward
     * the center of the cell by  this percentage of the vertex to centroid distance.
     * A value of between 0.01 and 0.1 is normal.
     * @param workingCell A value used during processing of the algorithm.  It's content is undefined
     * on return.
     * @return TRUE if the two points have line of sight.  Otherwise FALSE.
     */
    public static boolean hasLOS(float startX, float startZ
            ,  float endX, float endZ
            , TriCell startCell
            , TriCell endCell
            , float offsetScale
            , Vector2 workingVector
            , TriCell[] workingCell)
    {
        
        // This is the return result.
        boolean losSucceeded = false;
        
        // Handle special case: Start and/or end on a vertex.
        startCell.getSafePoint(startX, startZ, offsetScale, workingVector);
        startX = workingVector.x;
        startZ = workingVector.y;
        endCell.getSafePoint(endX, endZ, offsetScale, workingVector);
        endX = workingVector.x;
        endZ = workingVector.y;
        
        // Loop through the cells along the path until one of the following conditions is met:
        // - The end cell (which contains the end position) is intersected.
        // - A cell which has a vertex which matches the end point is intersected.
        // - The path hits a wall without a link. (LOS failure.)
        PathRelType pathResult = startCell.getPathRelationship(startX
                , startZ
                , endX
                , endZ
                , workingCell
                , null
                , null
                , workingVector);
        while(pathResult == PathRelType.EXITING_CELL)
        {
            if (workingCell[0] == null) 
                break;  // Hit a non-linked wall.
            
            // Perform some pre-tests to see if the next cell contains the end position.
            
            /*
             * Besides efficiency, this next check is necessary for situations
             * where the end of the path is on a wall. In such cases, math inaccuracies
             * can result in the following:
             * Cell A indicates path entry to cell B.
             * Cell B indicates no entry. (E.g. No relationship.)
             * Path check falsely fails.
             */
            if (workingCell[0].equals(endCell)) 
            {
                losSucceeded = true;
                break;
            }
            
            // Next cell is not the end cell.  So continue more expensive checks.
            pathResult = workingCell[0].getPathRelationship(startX
                    , startZ
                    , endX
                    , endZ
                    , workingCell
                    , null
                    , null
                    , workingVector);
        }
    
        if (pathResult == PathRelType.ENDING_CELL) 
            losSucceeded = true;    
        
        workingCell[0] = null;
        return losSucceeded;
    
    }
    
}

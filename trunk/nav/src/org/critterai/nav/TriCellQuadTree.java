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
import java.util.List;

import org.critterai.math.Vector2;
import org.critterai.math.Vector3;
import org.critterai.math.geom.Rectangle2;

/**
 * Provides quad tree storage and search for triangle navigation cells.
 * <p>Instances of this class are not thread-safe.</p>
 */
public final class TriCellQuadTree
{
	
	/*
	 * Design notes:
	 * 
	 * As it currently stands, all edges are considered inclusive in the tree node.  So
	 * if a point lies on the edge of a node's AABB, then that node will be included for
	 * all searches related to the point.  This results in more accurate searches at the
	 * cost of larger search spaces. An example of the impact:  If the search point lies
	 * on the centroid of the node's AABB, then the node and all of its children will be searched.
	 */
	
	private static final int FITS_HERE = -1;
	private static final int NO_FIT = -2;
	
	/**
	 * The maximum x-bounds of the node's AABB.
	 */
	public final float boundsMaxX;
	
	/**
	 * The maximum z-bounds of the node's AABB.
	 */
	public final float boundsMaxZ;
	
	/**
	 * The minimum x-bounds of the node's AABB.
	 */
	public final float boundsMinX;
	
	/**
	 * The minimum z-bounds of the node's AABB.
	 */
	public final float boundsMinZ;

	/**
	 * The maximum allowed depth of children below the
	 * current node. (A value of zero indicates that this node
	 * cannot have children.)
	 */
	public final int maxDepth;

    /**
	 * The current depth of the node within the tree.
	 * A value of zero indicates the node is the root node.
	 */
	private final int depth;
	/**
     * Cells with this node.
     * The list is not created until it is needed.
     */
    private ArrayList<TriCell> mCells = null;

    /**
	 * The child nodes of this node.
	 * The array is not created until it is needed.
	 */
    private TriCellQuadTree[] mChildNodes = null;
    private final ArrayList<TriCell> mTmpPolys = new ArrayList<TriCell>();
    
    private final Vector2 mTmpVec2A = new Vector2();

    /**
     * Constructor
     * @param minX The minimum x-bound of the of the tree's AABB.
     * @param minZ The minimum z-bound of the of the tree's AABB.
     * @param maxX The maximum x-bound of the of the tree's AABB.
     * @param maxZ The maximum z-bound of the of the tree's AABB.
     * @param maxDepth The maximum depth of the tree.
     * @throws IllegalArgumentException If the bounds are invalid.
     */
    public TriCellQuadTree(float minX, float minZ, float maxX, float maxZ, int maxDepth)
    	throws IllegalArgumentException
    {

    	if (minX >= maxX || minZ >= maxZ)
    		throw new IllegalArgumentException("Invalid bounds.");
    	
    	this.boundsMinX = minX;
    	this.boundsMinZ = minZ;
    	this.boundsMaxX = maxX;
    	this.boundsMaxZ = maxZ;

        this.maxDepth = Math.max(1, maxDepth);
        depth = 0;

    }

    private TriCellQuadTree(float[] bounds, TriCellQuadTree parent)
    {
    	this.boundsMinX = bounds[0];
    	this.boundsMinZ = bounds[1];
    	this.boundsMaxX = bounds[2];
    	this.boundsMaxZ = bounds[3];
    	
        maxDepth = parent.maxDepth - 1;
        depth = parent.depth + 1;
    }
    
    /**
     * Adds a cell to the quad tree.
     * <p>The addition will fail if the cell is not fully encompassed by the tree bounds
     * or is a duplicate.</p>
     * @param cell The cell to add to the tree.
     * @return TRUE if the cell was successfully added.  Otherwise FALSE.
     */
    public boolean add(TriCell cell)
    {
 
    	// Where should the 
        final int iTarget = getDestinationIndex(cell);

        if (iTarget == NO_FIT) 
            // The cell can't fit in this node or any of its children.
            return false;        	

        if (iTarget != FITS_HERE)
        {
            // Cell belongs in one of the children.
            if (mChildNodes == null)
            	// This is the first time children have been needed.
            	// Create them.
            	prepareChildren();
            // Add to the chosen child.
            return mChildNodes[iTarget].add(cell);
        }
   
        // Should be added to this node.
    	if (mCells == null)
    		// First cell added to this node.  Create the cell list.
    		mCells = new ArrayList<TriCell>();
    	else if (mCells.contains(cell))
    		// The cell has already been added.  Duplicates are not allowed.
    		return false;
    	// Add the cell to this node.
    	return mCells.add(cell);

    }
    
    /**
     * Returns a list of all cells within the tree.
     * <p>Ordering is undefined.<p>
     * @param out This list will be cleared, loaded with the results, and its reference
     * returned.
     * @return A reference to the out argument.
     */
    public List<TriCell> getCells(List<TriCell> out)
    {
    	out.clear();
    	getCellsInternal(out);
    	return out;
    }
    
    /**
	 * Returns a list of cells in whose xz-column the the provided point lies.
	 * <p>The cell column is the area that extends above and below the xz-plane projection of the cell.</p>
	 * <p>The list will be empty if the point is outside the column of all cells.</p>
	 * @param x The x-value of the point (x, z).
	 * @param z The z-value of the point (x, z).
     * @param out This list will be cleared, loaded with the results, and its reference
     * returned.
     * @return A reference to the out argument.
	 */
	public List<TriCell> getCellsForPoint(float x, float z, List<TriCell> out)
	{
		out.clear();
		return getCellsForPointInternal(x, z, out);
	}

	/**
	 * Returns a list of cells that intersect the column of the AABB.
	 * @param minX The minimum x-bound of the of the tree's AABB.
	 * @param minZ The minimum z-bound of the of the tree's AABB.
	 * @param maxX The maximum x-bound of the of the tree's AABB.
	 * @param maxZ The maximum z-bound of the of the tree's AABB.
	 * @param out This list will be cleared, loaded with the results, and its reference
	 * returned.
	 * @return A reference to the out argument.
	 */
	public List<TriCell> getCellsInColumn(float minX
			, float minZ
			, float maxX
			, float maxZ
			, List<TriCell> out)
	{
		out.clear();
	   	getCellsInColumnInternal(minX, minZ, maxX, maxZ, out);
	   	return out;
	}

	/**
	 * Returns the cell that best matches the provided point.
	 * <p>Setting mustBeInColumn to TRUE is much more efficient since it allows culling of cells.
	 * If the (x, z) location of the point is expected to reside within the xz-column of
	 * one or more cells and the y-value is expected to be close to the plane of a cell, then first search
	 * using mustBeInColumn=TRUE.  If NULL is returned, then an exhaustive search can be performed.</p>
	 * <p>The cell column is the area that extends above and below the xz-plane projection of the cell.</p>
	 * <p>If a point lies on a vertex or wall shared by more than one cell, or the point is equidistant
	 * to more than one cell, then which cell is chosen is arbitrary.</p>
	 * @param x The x-value of the point (x, y, z).
	 * @param y The y-value of the point (x, y, z).
	 * @param z The z-value of the point (x, y, z).
	 * @param mustBeInColumn If TRUE, then only cells in whose column the point resides will be considered. 
	 * If FALSE, an exhaustive search will be performed.
	 * @param outPointOnPoly If provided, will be populated with the point snapped to the cell.
	 * If the point is already within the cell's column, the (x, z) values will not be altered.  
	 * In this case only the y-value will be updated.
	 * @return The cell that is closest to the provided point, or NULL if mustBeInColumn=TRUE
	 * and the provided point does not lie in the xz-column of any cell.
	 */
	public TriCell getClosestCell(float x, float y, float z
			, boolean mustBeInColumn
			, Vector3 outPointOnPoly)
	{
		
		if (mustBeInColumn)
			// Perform the efficient strict search.  This may return null.
			return getClosestCellStrict(x, y, z, outPointOnPoly);
		
		// Perform an exhaustive search.  Will never return null.
		return getClosestCell(x, y, z, outPointOnPoly);
		
	}

	/**
	 * Recursively searches for cells whose columns contain the point.
	 * (NULL is not allowed for the out parameter.)
	 */
    private List<TriCell> getCellsForPointInternal(float x, float z, List<TriCell> out)
    {
    	if (!Rectangle2.contains(boundsMinX, boundsMinZ, boundsMaxX, boundsMaxZ, x, z))
    		return out;
    	if (mChildNodes != null)
    	{
    		for (int i = 0; i < 4; i++)
    		{
    			mChildNodes[i].getCellsForPointInternal(x, z, out);
    		}
    	}
    	if (mCells != null)
    	{
    		for (TriCell element : mCells)
    		{
    			if (element.isInColumn(x, z))
    				out.add(element);
    		}
    	}
    	return out;
    }
    
    private void getCellsInColumnInternal(float xmin
			, float zmin
			, float xmax
			, float zmax
			, List<TriCell> out)
	{
    	// Note that an inclusive test is performed so that edge cases are caught.
	  	if (!Rectangle2.intersectsAABB(boundsMinX, boundsMinZ
	  			, boundsMaxX, boundsMaxZ
	  			, xmin, zmin
	  			, xmax, zmax))
	  		// This node does not overlap the reference column.
			return;
		if (mChildNodes != null)
		{
			for (int i = 0; i < 4; i++)
			{
				mChildNodes[i].getCellsInColumnInternal(xmin, zmin
						, xmax, zmax
						, out);
			}
		}
		if (mCells != null)
		{
			for (TriCell cell : mCells)
			{
				if (cell.intersects(xmin, zmin, xmax, zmax))
					out.add(cell);
			}
		}
	}

	/**
     * Iterates through all nodes and adds the node's cells to the list.
     */
	private void getCellsInternal(List<TriCell> out)
    {
    	if (mChildNodes != null)
    	{
    		for (TriCellQuadTree child : mChildNodes)
    		{
    			child.getCellsInternal(out);
    		}
    	}
    	if (mCells != null)
    		out.addAll(mCells);
    }   
    
	/**
     * Returns the min/max bounds of the child in the form: (minX, minY, maxX, maxY) x 1
     */
    private float[] getChildBounds(int childIndex, float[] outBounds)
    {

        float halfWidth = (boundsMaxX - boundsMinX) * 0.5f;

        // Set the minimums for each child.
        switch (childIndex)
        {
            case 0:
                outBounds[0] = boundsMinX;
                outBounds[1] = boundsMinZ;
                break;
            case 1:
                outBounds[0] = boundsMinX;
                outBounds[1] = boundsMinZ + halfWidth;
                break;
            case 2:
                outBounds[0] = boundsMinX + halfWidth;
                outBounds[1] = boundsMinZ + halfWidth;
                break;
            case 3:
                outBounds[0] = boundsMinX + halfWidth;
                outBounds[1] = boundsMinZ;
                break;
            default:
                return null;
        }

        // Determine the maximums by adding a half width to the minimums.
        outBounds[2] = outBounds[0] + halfWidth;
        outBounds[3] = outBounds[1] + halfWidth;
        
        return outBounds;

    }
    
	/**
	 * Performs an exhaustive search of all cells and finds the cell that is closest
	 * to the point.
	 */
	private TriCell getClosestCell(float x, float y, float z, Vector3 outPointOnCell)
	{
		
 		// Get a list of all cells in the tree.
		mTmpPolys.clear();
		getCellsInternal(mTmpPolys);
		
		TriCell result = TriCell.getClosestCell(x, y, z, mTmpPolys, outPointOnCell, mTmpVec2A);
		mTmpPolys.clear();
		return result;
    }

    /**
     * Gets the closest cell whose column contains the point. 
     */
    private TriCell getClosestCellStrict(float x, float y, float z, Vector3 outPointOnPoly)
	{
		
		// Get all potential cells for this point.  (Point is in cell column.)
		getCellsForPoint(x, z, mTmpPolys);
		
		TriCell selectedPoly = null;
		float selectedDeltaY = Float.MAX_VALUE;
		float selectedY = Float.MAX_VALUE;
		
		// Loop through all potential cells.
		for (TriCell candidate : mTmpPolys)
		{
			float currentY = candidate.getPlaneY(x, z);
			float currentDeltaY = Math.abs(currentY - y);
			if (currentDeltaY < selectedDeltaY)
			{
				// The point is closer to this cell than any others so far. 
				// Select this cell.
				selectedDeltaY = currentDeltaY;
				selectedPoly = candidate;
				selectedY = currentY;
			}
		}
		if (outPointOnPoly != null)
			// Shift this input point's y to the plane of this cell.
			outPointOnPoly.set(x, selectedY, z);
		
		mTmpPolys.clear();
		return selectedPoly;
	}

    /**
     * Returns the index of the child node the cell should be added to.
     * <p>Takes into account maximum allowed depth.</p> 
     * <p>If the cell can't fit in any children, then {@link #FITS_HERE} or {@link #NO_FIT}
     * is returned.</p>
     */
    private int getDestinationIndex(TriCell cell)
    {
    	// Default to no fit.
        int result = NO_FIT;

        if (Rectangle2.contains(boundsMinX, boundsMinZ, boundsMaxX, boundsMaxZ
                , cell.boundsMinX, cell.boundsMinZ, cell.boundsMaxX, cell.boundsMaxZ))
        {
        	// The cell will fit in this node or a child.
        	float[] childBounds = new float[4];
            result = FITS_HERE;
            if (depth < maxDepth)
            {
                // Children are permitted. (This node isn't at the maximum depth.)
            	// Check each child in order.
                getChildBounds(0, childBounds);
                if (fits(childBounds, cell))
                	// The cell will fit in this child.
                	return 0;
                getChildBounds(1, childBounds);
                if (fits(childBounds, cell))
                	// The cell will fit in this child.
                	return 1;
                getChildBounds(2, childBounds);
                if (fits(childBounds, cell))
                	// The cell will fit in this child.
                	return 2;
                getChildBounds(3, childBounds);
                if (fits(childBounds, cell))
                	// The cell will fit in this child.
                	return 3;
            }
        }

        // The cell fits in this node, but not any of its children,
        //or doesn't fit in this node at all.
        return result;
    }

	/**
     * Populates the child array.
     * (No validations performed.)
     */
    private void prepareChildren()
    {
        // Need to create the children.
    	float[] workingBounds = new float[4];
        mChildNodes = new TriCellQuadTree[4];
        mChildNodes[0] = new TriCellQuadTree(getChildBounds(0, workingBounds), this);
        mChildNodes[1] = new TriCellQuadTree(getChildBounds(1, workingBounds), this);
        mChildNodes[2] = new TriCellQuadTree(getChildBounds(2, workingBounds), this);
        mChildNodes[3] = new TriCellQuadTree(getChildBounds(3, workingBounds), this);
    }

	/**
	 * Returns TRUE if the cell fits within the specified bounds.  The check is inclusive of the
	 * all edges.
	 */
	private static boolean fits(float[] bounds, TriCell cell)
	{
		return Rectangle2.contains(bounds[0], bounds[1]
		                              , bounds[2], bounds[3]
		                              , cell.boundsMinX, cell.boundsMinZ
		                              , cell.boundsMaxX, cell.boundsMaxZ);
	}
	
}

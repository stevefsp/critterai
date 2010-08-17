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
using org.critterai.math.geom;
using UnityEngine;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Provides quad tree storage and search for triangle navigation cells.
    /// </summary>
    /// <remarks>Instances of this class are not thread-safe.</remarks>
    public sealed class TriCellQuadTree
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
        
        private const int FITS_HERE = -1;
        private const int NO_FIT = -2;
        
        private readonly float mBoundsMaxX;
        private readonly float mBoundsMaxZ;
        private readonly float mBoundsMinX;
        private readonly float mBoundsMinZ;
        private readonly int mMaxDepth;

        /// <summary>
        /// The current mDepth of the node within the tree.
        /// A value of zero indicates the node is the root node.
        /// </summary>
        private readonly int mDepth;

        /// <summary>
        /// Cells with this node.
        /// The list is not created until it is needed.
        /// </summary>
        private List<TriCell> mCells = null;

        /// <summary>
        /// The child nodes of this node.
        /// The array is not created until it is needed.
        /// </summary>
        private TriCellQuadTree[] mChildNodes = null;

        private readonly List<TriCell> mTmpPolys = new List<TriCell>();

        /// <summary>
        /// The maximum x-bounds of the node's AABB.
        /// </summary>
        public float BoundsMaxX
        {
            get { return mBoundsMaxX; }
        } 

        /// <summary>
        /// The maximum z-bounds of the node's AABB.
        /// </summary>
        public float BoundsMaxZ
        {
            get { return mBoundsMaxZ; }
        } 

        /// <summary>
        /// The minimum x-bounds of the node's AABB.
        /// </summary>
        public float BoundsMinX
        {
            get { return mBoundsMinX; }
        }

        /// <summary>
        /// The minimum z-bounds of the node's AABB.
        /// </summary>
        public float BoundsMinZ
        {
            get { return mBoundsMinZ; }
        } 

        /// <summary>
        /// The maximum allowed depth of children below the
        /// current node. (A value of zero indicates that this node
        /// cannot have children.)
        /// </summary>
        public int MaxDepth
        {
            get { return mMaxDepth; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="minX">The minimum x-bound of the of the tree's AABB.</param>
        /// <param name="minZ">The minimum z-bound of the of the tree's AABB.</param>
        /// <param name="maxX">The maximum x-bound of the of the tree's AABB.</param>
        /// <param name="maxZ">The maximum z-bound of the of the tree's AABB.</param>
        /// <param name="maxDepth">The maximum mDepth of the tree.</param>
        public TriCellQuadTree(float minX, float minZ, float maxX, float maxZ, int maxDepth)
        {

            if (minX >= maxX || minZ >= maxZ)
                throw new ArgumentException("bounds", "Invalid bounds.");
            
            this.mBoundsMinX = minX;
            this.mBoundsMinZ = minZ;
            this.mBoundsMaxX = maxX;
            this.mBoundsMaxZ = maxZ;

            this.mMaxDepth = Math.Max(1, maxDepth);
            mDepth = 0;

        }

        private TriCellQuadTree(float[] bounds, TriCellQuadTree parent)
        {
            this.mBoundsMinX = bounds[0];
            this.mBoundsMinZ = bounds[1];
            this.mBoundsMaxX = bounds[2];
            this.mBoundsMaxZ = bounds[3];
            
            mMaxDepth = parent.mMaxDepth - 1;
            mDepth = parent.mDepth + 1;
        }
        
        /// <summary>
        /// Adds a cell to the quad tree.
        /// </summary>
        /// <remarks>The addition will fail if the cell is not fully encompassed by the tree bounds
        /// or is a duplicate.</remarks>
        /// <param name="cell">The cell to Add to the tree.</param>
        /// <returns>TRUE if the cell was successfully added.  Otherwise FALSE.</returns>
        public Boolean Add(TriCell cell)
        {
     
            // Where should the 
            int iTarget = GetDestinationIndex(cell);

            if (iTarget == NO_FIT) 
                // The cell can't fit in this node or any of its children.
                return false;            

            if (iTarget != FITS_HERE)
            {
                // cell belongs in one of the children.
                if (mChildNodes == null)
                    // This is the first time children have been needed.
                    // Create them.
                    PrepareChildren();
                // Add to the chosen child.
                return mChildNodes[iTarget].Add(cell);
            }
       
            // Should be added to this node.
            if (mCells == null)
                // First cell added to this node.  Create the cell list.
                mCells = new List<TriCell>();
            else if (mCells.Contains(cell))
                // The cell has already been added.  Duplicates are not allowed.
                return false;
            // Add the cell to this node.
            mCells.Add(cell);
            return true;
        }
        
        /// <summary>
        /// Returns a list of all cells within the tree.
        /// <para>Ordering is undefined.</para>
        /// </summary>
        /// <param name="outList">This list will be cleared, loaded with the results, and its reference
        /// returned.</param>
        /// <returns>A reference to the <paramref name="outList"/> argument.</returns>
        public List<TriCell> GetCells(List<TriCell> outList)
        {
            outList.Clear();
            GetCellsInternal(outList);
            return outList;
        }
        
        /// <summary>
        /// Returns a list of cells in whose xz-column the the provided point lies.
        /// </summary>
        /// <remarks>
        /// The cell column is the area that extends above and below the xz-plane projection of the cell.
        /// <para>The list will be empty if the point is outside the column of all cells.</para>
        /// </remarks>
        /// <param name="x">The x-value of the point (x, z).</param>
        /// <param name="z">The z-value of the point (x, z).</param>
        /// <param name="outList">This list will be cleared, loaded with the results, and its reference
        /// returned.</param>
        /// <returns>A reference to the out argument.</returns>
        public List<TriCell> GetCellsForPoint(float x, float z, List<TriCell> outList)
        {
            outList.Clear();
            return GetCellsForPointInternal(x, z, outList);
        }

        /// <summary>
        /// Returns a list of cells that intersect the column of the AABB.
        /// @param minX 
        /// @param minZ 
        /// @param maxX 
        /// @param maxZ 
        /// @param out 
        /// @return 
        /// </summary>
        /// <param name="minX">The minimum x-bound of the of the tree's AABB.</param>
        /// <param name="minZ">The minimum z-bound of the of the tree's AABB.</param>
        /// <param name="maxX">The maximum x-bound of the of the tree's AABB.</param>
        /// <param name="maxZ">The maximum z-bound of the of the tree's AABB.</param>
        /// <param name="outList">This list will be cleared, loaded with the results, and its reference
        /// returned.</param>
        /// <returns>A reference to the out argument.</returns>
        public List<TriCell> GetCellsInColumn(float minX
                , float minZ
                , float maxX
                , float maxZ
                , List<TriCell> outList)
        {
            outList.Clear();
               GetCellsInColumnInternal(minX, minZ, maxX, maxZ, outList);
               return outList;
        }

        /// <summary>
        /// Returns the cell that best matches the provided point.
        /// </summary>
        /// <remarks>
        /// Setting mustBeInColumn to TRUE is much more efficient since it allows culling of cells.
        /// If the (x, z) location of the point is expected to reside within the xz-column of
        /// one or more cells and the y-value is expected to be close to the plane of a cell, then first search
        /// using mustBeInColumn=TRUE.  If NULL is returned, then an exhaustive search can be performed.
        /// <para>The cell column is the area that extends above and below the xz-plane projection of the cell.</para>
        /// <para>If a point lies on a vertex or wall shared by more than one cell, or the point is equidistant
        /// to more than one cell, then which cell is chosen is arbitrary.</para>
        /// </remarks>
        /// <param name="x">The x-value of the point (x, y, z).</param>
        /// <param name="y">The y-value of the point (x, y, z).</param>
        /// <param name="z">The z-value of the point (x, y, z).</param>
        /// <param name="mustBeInColumn">If TRUE, then only cells in whose column the point resides will be considered. 
        /// If FALSE, an exhaustive search will be performed.</param>
        /// <param name="outPointOnPoly">If provided, will be populated with the point snapped to the cell.
        /// If the point is already within the cell's column, the (x, z) values will not be altered.  
        /// In this case only the y-value will be updated.</param>
        /// <returns>The cell that is closest to the provided point, or NULL if mustBeInColumn=TRUE
        /// and the provided point does not lie in the xz-column of any cell.</returns>
        public TriCell GetClosestCell(float x, float y, float z
                , Boolean mustBeInColumn
                , out Vector3 outPointOnPoly)
        {
            
            if (mustBeInColumn)
                // Perform the efficient strict search.  This may return null.
                return GetClosestCellStrict(x, y, z, out outPointOnPoly);
            
            // Perform an exhaustive search.  Will never return null.
            return GetClosestCell(x, y, z, out outPointOnPoly);
            
        }

        /// <summary>
        /// Recursively searches for cells whose columns contain the point.
        /// (NULL is not allowed for the out parameter.)
        /// </summary>
        private List<TriCell> GetCellsForPointInternal(float x, float z, List<TriCell> outList)
        {
            if (!Rectangle2.Contains(mBoundsMinX, mBoundsMinZ, mBoundsMaxX, mBoundsMaxZ, x, z))
                return outList;
            if (mChildNodes != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    mChildNodes[i].GetCellsForPointInternal(x, z, outList);
                }
            }
            if (mCells != null)
            {
                foreach (TriCell element in mCells)
                {
                    if (element.IsInColumn(x, z))
                        outList.Add(element);
                }
            }
            return outList;
        }
        
        private void GetCellsInColumnInternal(float xmin
                , float zmin
                , float xmax
                , float zmax
                , List<TriCell> outList)
        {
            // Note that an inclusive test is performed so that edge cases are caught.
              if (!Rectangle2.IntersectsAABB(mBoundsMinX, mBoundsMinZ
                      , mBoundsMaxX, mBoundsMaxZ
                      , xmin, zmin
                      , xmax, zmax))
                  // This node does not overlap the reference column.
                return;
            if (mChildNodes != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    mChildNodes[i].GetCellsInColumnInternal(xmin, zmin
                            , xmax, zmax
                            , outList);
                }
            }
            if (mCells != null)
            {
                foreach (TriCell cell in mCells)
                {
                    if (cell.Intersects(xmin, zmin, xmax, zmax))
                        outList.Add(cell);
                }
            }
        }

        /// <summary>
        /// Iterates through all nodes and adds the node's cells to the list.
        /// </summary>
        private void GetCellsInternal(List<TriCell> outList)
        {
            if (mChildNodes != null)
            {
                foreach (TriCellQuadTree child in mChildNodes)
                {
                    child.GetCellsInternal(outList);
                }
            }
            if (mCells != null)
                outList.AddRange(mCells);
        }   
        
        /// <summary>
        /// Returns the Min/Max bounds of the child in the form: (minX, minY, maxX, maxY) x 1
        /// </summary>
        private float[] GetChildBounds(int childIndex, float[] outBounds)
        {

            float halfWidth = (mBoundsMaxX - mBoundsMinX) * 0.5f;

            // Set the minimums for each child.
            switch (childIndex)
            {
                case 0:
                    outBounds[0] = mBoundsMinX;
                    outBounds[1] = mBoundsMinZ;
                    break;
                case 1:
                    outBounds[0] = mBoundsMinX;
                    outBounds[1] = mBoundsMinZ + halfWidth;
                    break;
                case 2:
                    outBounds[0] = mBoundsMinX + halfWidth;
                    outBounds[1] = mBoundsMinZ + halfWidth;
                    break;
                case 3:
                    outBounds[0] = mBoundsMinX + halfWidth;
                    outBounds[1] = mBoundsMinZ;
                    break;
                default:
                    return null;
            }

            // Determine the maximums by adding a half width to the minimums.
            outBounds[2] = outBounds[0] + halfWidth;
            outBounds[3] = outBounds[1] + halfWidth;
            
            return outBounds;

        }

        /// <summary>
        /// Performs an exhaustive search of all cells and finds the cell that is closest
        /// to the point.
        /// </summary>
        private TriCell GetClosestCell(float x, float y, float z, out Vector3 outPointOnCell)
        {
            
             // Get a list of all cells in the tree.
            mTmpPolys.Clear();
            GetCellsInternal(mTmpPolys);
            
            TriCell result = TriCell.GetClosestCell(x, y, z, mTmpPolys, out outPointOnCell);
            mTmpPolys.Clear();
            return result;
        }

        /// <summary>
        /// Gets the closest cell whose column Contains the point. 
        /// </summary>
        private TriCell GetClosestCellStrict(float x, float y, float z, out Vector3 outPointOnPoly)
        {
            
            // Get all potential cells for this point.  (Point is in cell column.)
            GetCellsForPoint(x, z, mTmpPolys);
            
            TriCell selectedPoly = null;
            float selectedDeltaY = float.MaxValue;
            float selectedY = float.MaxValue;
            
            // Loop through all potential cells.
            foreach (TriCell candidate in mTmpPolys)
            {
                float currentY = candidate.GetPlaneY(x, z);
                float currentDeltaY = Math.Abs(currentY - y);
                if (currentDeltaY < selectedDeltaY)
                {
                    // The point is closer to this cell than any others so far. 
                    // Select this cell.
                    selectedDeltaY = currentDeltaY;
                    selectedPoly = candidate;
                    selectedY = currentY;
                }
            }
            // Shift this input point's y to the plane of this cell.
            outPointOnPoly = new Vector3(x, selectedY, z);
            
            mTmpPolys.Clear();
            return selectedPoly;
        }

        /// <summary>
        /// Returns the index of the child node the cell should be added to.
        /// <para>Takes into account maximum allowed mDepth.</para> 
        /// <para>If the cell can't fit in any children, then FITS_HERE or NO_FIT
        /// is returned.</para>
        /// </summary>
        private int GetDestinationIndex(TriCell cell)
        {
            // Default to no fit.
            int result = NO_FIT;

            if (Rectangle2.Contains(mBoundsMinX, mBoundsMinZ, mBoundsMaxX, mBoundsMaxZ
                    , cell.BoundsMinX, cell.BoundsMinZ, cell.BoundsMaxX, cell.BoundsMaxZ))
            {
                // The cell will fit in this node or a child.
                float[] childBounds = new float[4];
                result = FITS_HERE;
                if (mDepth < mMaxDepth)
                {
                    // Children are permitted. (This node isn't at the maximum mDepth.)
                    // Check each child in order.
                    GetChildBounds(0, childBounds);
                    if (Fits(childBounds, cell))
                        // The cell will fit in this child.
                        return 0;
                    GetChildBounds(1, childBounds);
                    if (Fits(childBounds, cell))
                        // The cell will fit in this child.
                        return 1;
                    GetChildBounds(2, childBounds);
                    if (Fits(childBounds, cell))
                        // The cell will fit in this child.
                        return 2;
                    GetChildBounds(3, childBounds);
                    if (Fits(childBounds, cell))
                        // The cell will fit in this child.
                        return 3;
                }
            }

            // The cell fits in this node, but not any of its children,
            //or doesn't fit in this node at all.
            return result;
        }

        /// <summary>
        /// Populates the child array.
        /// (No validations performed.)
        /// </summary>
        private void PrepareChildren()
        {
            // Need to create the children.
            float[] workingBounds = new float[4];
            mChildNodes = new TriCellQuadTree[4];
            mChildNodes[0] = new TriCellQuadTree(GetChildBounds(0, workingBounds), this);
            mChildNodes[1] = new TriCellQuadTree(GetChildBounds(1, workingBounds), this);
            mChildNodes[2] = new TriCellQuadTree(GetChildBounds(2, workingBounds), this);
            mChildNodes[3] = new TriCellQuadTree(GetChildBounds(3, workingBounds), this);
        }

        /// <summary>
        /// Returns TRUE if the cell fits within the specified bounds.  The check is inclusive of the
        /// all edges.
        /// </summary>
        private static Boolean Fits(float[] bounds, TriCell cell)
        {
            return Rectangle2.Contains(bounds[0], bounds[1]
                                          , bounds[2], bounds[3]
                                          , cell.BoundsMinX, cell.BoundsMinZ
                                          , cell.BoundsMaxX, cell.BoundsMaxZ);
        }
    }

}

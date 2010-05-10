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

/**
 * Represents a tip of an ordered list of cells that form the shortest known path
 * back to a start point.
 * <p>From a graph standpoint, the node is the midpoint on the cell wall connecting
 * the {@link #cell()} to its {@link #parent()} cell.  It there is no parent, then
 * the node is the start point.</p>
 * <p>Instances of this class are not thread-safe.</p>
 */
public final class TriCellPathNode
{

    private final TriCell mCell;    
    private float mH = 0;
    private float mLocalG = 0;
    private TriCellPathNode mParent;
    
    /**
     * Constructor for the root node in a node graph.
     * @param cell The cell that backs the node.
     * @throws IllegalArgumentException If the cell is null.
     */
    public TriCellPathNode(TriCell cell)
        throws IllegalArgumentException
    {
        if (cell == null)
            throw new IllegalArgumentException("Cell is null.");
        mCell = cell;
        mParent = null;
    }
    
    /**
     * Constructor for a node that has a parent.
     * @param cell The cell that backs the node.
     * @param parent The parent node.  The parent cell must share a wall with the
     * cell.
     * @param startX The x-value of the path starting point. (startX, startY, startZ)
     * @param startY The y-value of the path starting point. (startX, startY, startZ)
     * @param startZ The z-value of the path starting point. (startX, startY, startZ)
     * @throws IllegalArgumentException If the cell or parent is null.
     */
    public TriCellPathNode(TriCell cell, TriCellPathNode parent, float startX, float startY, float startZ)
        throws IllegalArgumentException
    {
        if (cell == null || parent == null) 
            throw new IllegalArgumentException("Cell and/or parent is null.");
        mCell = cell;
        mParent = parent;
        mLocalG = calculateLocalG(mParent, startX, startY, startZ);
    }
    
    /**
     * The navigation cell backing this path node.
     * @return The navigation cell backing this path node.
     */
    public TriCell cell() { return mCell; }
    
    /**
     * Determines the path cost from the search starting point to this path node
     * if the potential parent is assigned.
     * <p>In the standard use case, if this operation returns a lower value than the
     * current value of {@link #g()}, then the potential parent is a better parent than the
     * existing parent.</p>
     * @param potentialParent The parent to test.
     * @param startX The x-value of the path starting point. (startX, startY, startZ)
     * @param startY The y-value of the path starting point. (startX, startY, startZ)
     * @param startZ The z-value of the path starting point. (startX, startY, startZ)
     * @return The path cost form the search starting point to this path node if the test parent
     * were the parent of this node.
     */
    public float estimateNewG(TriCellPathNode potentialParent, float startX, float startY, float startZ)
    {
        // Get an estimate of the new local G if this parent is used.
        return potentialParent.g() + calculateLocalG(potentialParent, startX, startY, startZ);
    }
    
    /**
     * Total estimated cost of the path that includes this node. (F = G + H)
     * @return Total estimated cost of the path.
     */
    public float f() 
    {
        float f = mLocalG + mH;
        if (mParent != null) f += mParent.g();
        return f; 
    }
    
    /**
     * The path cost from the starting point to the current path node.
     * @return Path cost from search starting point to the current path node.
     */
    public float g() 
    { 
        if (mParent == null) 
            return mLocalG;
        return mLocalG + mParent.g(); 
    }
    
    /**
     * Heuristic value. Estimated cost to move from the current node to the goal point.
     * @return Estimated movement cost to move from the current node to goal point.
     */
    public float h() { return mH; }
    
    /**
     * Gets an ordered list of cells that represent the path from the root node to 
     * the current node.
     * @param outCells The array to load the result into. Length of the array must be at
     * least {@link #pathSize()}
     */
    public void loadPath(TriCell[] outCells)
    {
        loadPath(outCells, 0);
    }
    
    /**
     * The path cost of the edge connecting this path node to its parent.
     * <p>Value will be zero if {@link #parent()} is null.  Otherwise it will
     * be the distance from parent node to the midpoint of the wall connecting 
     * the {@link #cell()} to its {@link #parent()} cell.</p>
     * @return The path cost of the edge connecting this path node to its parent.
     */
    public float localG() { return mLocalG; }
    
    /**
     * The path node that is immediately before this node in the graph.
     * If null, then this is the root path node.
     * @return The path node that is immediately before this node in the graph.
     */
    public TriCellPathNode parent() { return mParent; }
    
    /**
     * The number of path nodes from the root to this node.
     * @return The number of path nodes from the root to this node.
     */
    public int pathSize()
    {
        if (mParent == null)
            return 1;
        else
            return mParent.pathSize() + 1;
    }

    /**
     * Sets the heuristic value, the estimated cost to move from the current
     * node to the goal point.
     * @param value The new heuristic value.
     */
    public void setH(float value) { mH = value; }
    
    /**
     * Sets the parent to this path node.
     * <p>The use case is to decide at construction whether the node has a parent
     * and leave it that way, only assigning a new parent to replace an existing parent.</p>
     * <p>Behavior is undefined if this use case is violated.</p>
     * @param parent The new parent of this path node.
     */
    public void setParent(TriCellPathNode parent, float startX, float startY, float startZ) 
    { 
        mParent = parent;
        if (mParent == null)
            mLocalG = 0;
        else
            mLocalG = calculateLocalG(mParent, startX, startY, startZ);
    }

    private float calculateLocalG(TriCellPathNode parent, float startX, float startY, float startZ)
    {
        
        int linkIndex = parent.cell().getLinkIndex(mCell);
        
        if (parent.parent() == null)
        {
            // The parent is the starting cell.  So get the distance from the starting
            // position to the connecting wall.
            return (float)Math.sqrt(parent.cell().getLinkPointDistanceSq(startX, startY, startZ
                    , linkIndex));
        }
        
        // Get the distance from the connecting wall of my parent's parent to my connecting wall.
        // Basically, this is getting the distance from my parent's parent to me.
        
        int otherConnectingWall = parent.cell().getLinkIndex(parent.parent().cell());
        return parent.cell().getLinkPointDistance(otherConnectingWall, linkIndex);
        
    }

    private int loadPath(TriCell[] outCells, int index)
    {
        int myIndex = index;
        if (mParent != null)
            // Insert parent information first.
            myIndex = mParent.loadPath(outCells, index);
        if (myIndex >= outCells.length)
            // Can't fit anything more into this path.
            return myIndex;
        // Add my information.
        outCells[myIndex] = mCell;
        return myIndex+1;
    }
    
}

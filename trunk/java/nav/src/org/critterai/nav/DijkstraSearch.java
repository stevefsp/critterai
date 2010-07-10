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

import static org.critterai.nav.SearchState.COMPLETE;
import static org.critterai.nav.SearchState.FAILED;
import static org.critterai.nav.SearchState.INITIALIZED;
import static org.critterai.nav.SearchState.PROCESSING;
import static org.critterai.nav.SearchState.UNINITIALIZED;

import java.util.ArrayList;
import java.util.Hashtable;
import java.util.PriorityQueue;

/**
 * Provides a shortest path search, start point to goal cell, using the Dijskstra search algorithm.
 * When the search completes, provides one or more ordered lists of cells that represent corridors
 * from the start point to goal cells.
 * <p>Features:<p>
 * <ul>
 * <li>Complete when the first shortest path is found. (Single path returned.)</li>
 * <li>Complete when all goal points are found. (Multiple paths returned.)</li>
 * <li>Limit the search depth. (Only paths found within the search depth are returned.)</li>
 * </ul>
 * <p>Suitable for re-use across multiple searches.</p>
 * <p>Primary use case:</p>
 * <ol>
 * <li>Perform the 
 * {@link #initialize(float, float, float, TriCell, TriCell[], int, boolean)}
 * operation to set up the search.</li>
 * <li>Perform the {@link #process()} operation until the search is complete.</li>
 * <li>If the search completes successfully, use the {@link #pathCount()} and {@link #getPathCells(int)}
 * operations to obtain the search results.</li>
 * <li>Repeat as needed.</li>
 * </ol>
 * <p>Instances of this class are not thread safe.</p>
 */
public final class DijkstraSearch 
{
    
    private final PriorityQueue<TriCellPathNode> mOpenHeap 
        = new PriorityQueue<TriCellPathNode>(10, new TriCellPathComparator());
    
    /**
     * Cells that have already been investigated.
     */
    private final ArrayList<TriCell> mClosedCells = new ArrayList<TriCell>();
    
    private SearchState mState = UNINITIALIZED;
    
    private final ArrayList<TriCell[]> mPathCells = new ArrayList<TriCell[]>();
    
    private boolean mSelectFirst;
    
    private float mStartX;
    private float mStartY;
    private float mStartZ;
    
    private float[] mGoals;
    
    private TriCell mStartCell = null;
    private final ArrayList<TriCell> mGoalCells = new ArrayList<TriCell>();
    
    private final Hashtable<TriCell, TriCellPathNode> mOpenCellMap 
        = new Hashtable<TriCell, TriCellPathNode>();
    
    private int mMaxSearchDepth;
    
    /**
     * An array of ordered of cells representing a path corridor
     * from the start point to one of the goal points.
     * Will contain one path for each goal successfully found.
     * <p>Will only contain information if the {@link #state()} of the search
     * is {@link SearchState#COMPLETE}.  Otherwise it will be null.</p>
     * <p>A successful search only guarantees that at least one goal point has
     * been found, not that all goal points have been found</p>
     * <p>The order of the results is undefined.  The goal cell of each path
     * can be used to determine which goal point is associated with which path.<p> 
     * <p>For performance reasons, this operation provides a reference
     * to the internal data array.  It is not a copy.</p>
     * @param index The index of the path to retrieve.  The index will be between
     * zero and {@link #pathCount()}.
     * @return An array of ordered cells representing a path corridor from the
     * start point to a goal point.
     */
    public TriCell[] getPathCells(int index) { return mPathCells.get(index); }
    
    /**
     * The goal points of the search in the form (x, y, z) * numberOfGoals.
     * @return The goal points of the search.
     */
    public float[] goals() { return mGoals; }
    
    /**
     * Prepares the instance for a new search.
     * <p>Calling this operation while a search is in progress will clear
     * the existing search.</p>
     * <p>There is no need to call {@link #reset()} when re-using a search instance.
     * Just use this operation to re-initialize the search.</p>
     * <p>Behavior of the search is undefined if any of the the start and/or goal
     * position is not within the start/goal cell columns.</p>
     * @param startX The x-value of the start location (startX, startY, startZ).
     * @param startY The y-value of the start location (startX, startY, startZ).
     * @param startZ The z-value of the start location (startX, startY, startZ).
     * @param goals The goal positions of the search in the form (x, y, z) * numberOfGoals.
     * A value of null is valid.
     * @param startCell The start cell.  (In which the start position exists.)
     * @param goalCells The cells to search for.
     * @param maxSearchDepth  The maximum depth to search.  The search will terminate,
     * whether successful or not, at this search depth.  A value of 
     * {@link Integer#MAX_VALUE} will result in an exhaustive search.
     * The maximum possible path length will be search depth + 1.
     * @param selectFirst If set to TRUE, the search will complete as soon as
     * the first (shortest) path is found.
     * @return The state of the search after initialization.  
     * Usually this is {@link SearchState#INITIALIZED}.  But other values may be returned if
     * there is a failure.
     */
    public SearchState initialize(float startX, float startY, float startZ
            , float[] goals
            , TriCell startCell
            , TriCell[] goalCells
            , int maxSearchDepth
            , boolean selectFirst)
    {
        
        reset();
        
        mMaxSearchDepth = Math.max(1, maxSearchDepth);
        
        mStartX = startX;
        mStartY = startY;
        mStartZ = startZ;
        
        mGoals = goals;
        
        mSelectFirst = selectFirst;
        
        mStartCell = startCell;
        mGoalCells.ensureCapacity(goalCells.length);
        for (TriCell cell : goalCells)
            mGoalCells.add(cell);
        
        // Add the starting point to the open heap as a root entry.
        TriCellPathNode current = new TriCellPathNode(mStartCell);
        mOpenHeap.add(current);
        
        mState = INITIALIZED;
        
        return mState;
        
    }
    
    /**
     * Prepares the instance for a new search.
     * Useful when searching for cells rather than goal points.
     * <p>Calling this operation while a search is in progress will clear
     * the existing search.</p>
     * <p>There is no need to call {@link #reset()} when re-using a search instance.
     * Just use this operation to re-initialize the search.</p>
     * <p>Behavior of the search is undefined if the start position is not within
     * the start cell column.</p>
     * @param startX The x-value of the start location (startX, startY, startZ).
     * @param startY The y-value of the start location (startX, startY, startZ).
     * @param startZ The z-value of the start location (startX, startY, startZ).
     * @param startCell The start cell.  (In which the start position exists.)
     * @param goalCells The cells to search for.
     * @param maxSearchDepth  The maximum depth to search.  The search will terminate,
     * whether successful or not, at this search depth.  A value of 
     * {@link Integer#MAX_VALUE} will result in an exhaustive search.
     * The maximum possible path length will be search depth + 1.
     * @param selectFirst If set to TRUE, the search will complete as soon as
     * the first (shortest) path is found.
     * @return The state of the search after initialization.  
     * Usually this is {@link SearchState#INITIALIZED}.  But other values may be returned if
     * there is a failure.
     */
    public SearchState initialize(float startX, float startY, float startZ
            , TriCell startCell
            , TriCell[] goalCells
            , int maxSearchDepth
            , boolean selectFirst)
    {
        return initialize(startX, startY, startZ
                , null
                , startCell
                , goalCells
                , maxSearchDepth
                , selectFirst);
    }
    
    /**
     * TRUE if a search is in progress.  Otherwise the search is finished or
     * uninitialized.
     * @return TRUE if a search is in progress.  Otherwise the search is finished or
     * uninitialized.
     */
    public boolean isActive()
    {
        return (mState == PROCESSING || mState == INITIALIZED ? true : false);
    }
    
    /**
     * The number of paths available from {@link #getPathCells(int)}.
     * @return The number of paths available from {@link #getPathCells(int)}.
     */
    public int pathCount() { return mPathCells.size(); }
    
    /**
     * Performs a single iteration of the Dijkstra search.  Must be repeated as 
     * needed to complete the search.
     * <p>While it is safe to call this operation during any state, actual
     * processing will only occur if the state is either 
     * {@link SearchState#INITIALIZED} or {@link SearchState#PROCESSING}.</p>
     * @return The state of the search after the search iteration.
     */
    public SearchState process() 
    {
        
        if (mState == INITIALIZED)
            mState = PROCESSING;
        else if (mState != PROCESSING)
            return mState;
        
        TriCellPathNode current = mOpenHeap.poll();
        mClosedCells.add(current.cell());
        mOpenCellMap.remove(current.cell());
        
        int maxLinks = current.cell().maxLinks();
        for (int iLink = 0; iLink < maxLinks; iLink++)
        {
            TriCell linkedCell = current.cell().getLink(iLink);
            if (linkedCell == null)
                continue;
            if (mClosedCells.contains(linkedCell)) 
                continue;
            if (mGoalCells.contains(linkedCell))
            {
                // Finished one of the searches.
                TriCell[] cells = new TriCell[current.pathSize() + 1];
                current.loadPath(cells);
                cells[cells.length-1] = linkedCell;
                mPathCells.add(cells);
                if (mSelectFirst || mGoalCells.size() == 1)
                {
                    // We can stop the search.
                    cleanup();
                    mState = COMPLETE;
                    return mState;
                }
                mGoalCells.remove(linkedCell);
            }
            if (linkedCell.linkCount() == 1)
            {
                // Cell is a dead end, so we don't care about it.
                mClosedCells.add(linkedCell);
                continue;
            }
            if (current.pathSize() >= mMaxSearchDepth)
                // The cells beyond this cell are
                // beyond the search depth.  So we are done.
                continue;
            TriCellPathNode entry = mOpenCellMap.get(linkedCell);
            if (entry != null)
            {
                // Cell is in the open heap via another path.
                // Is this path better?
                float newG = entry.estimateNewG(current, mStartX, mStartY, mStartZ);
                if (newG < entry.g())
                {
                    // The priority queue assumes static sort values.
                    // So need to remove and re-add to the queue.
                    mOpenHeap.remove(entry);
                    entry.setParent(current, mStartX, mStartY, mStartZ);
                    mOpenHeap.add(entry);
                }
            }
            else
            {
                // This is a new cell.  Set things up.
                entry = new TriCellPathNode(linkedCell
                        , current
                        , mStartX
                        , mStartY
                        , mStartZ);
                mOpenHeap.add(entry);
                mOpenCellMap.put(linkedCell, entry);
            }
        }
        
        if (mOpenHeap.size() == 0)
        {
            // Search is complete.
            cleanup();
            if (mPathCells.size() == 0)
                mState = FAILED;
            else
                mState = COMPLETE;
            return mState;
        }
        
        return mState;
        
    }

    /**
     * Resets the object to its state at construction, releasing
     * any internal references held by the search.
     */
    public void reset()
    {
        
        cleanup();
        mState = UNINITIALIZED;
        mStartX = 0;
        mStartY = 0;
        mStartZ = 0;
        mGoals = null;
        mStartCell = null;
        mGoalCells.clear();
        mPathCells.clear();
        mSelectFirst = false;
    }
    
    /**
     * The current state of the search.
     * @return The current state of the search.
     */
    public SearchState state() { return mState; }

    /**
     * Cleans up after a search is complete, but leaves the search/path/state information
     * intact.
     */
    private void cleanup()
    {
        mOpenHeap.clear();
        mClosedCells.clear();
        mOpenCellMap.clear();
    }
}

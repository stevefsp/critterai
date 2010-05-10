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
 * Provides a shortest path search, start location to goal cell, using the A* search algorithm.  
 * When the search completes, provides a an ordered list of cells that represent a corridor
 * from the start point to the goal cell.
 * <p>Suitable for re-use across multiple searches.</p>
 * <p>Primary use case:</p>
 * <ol>
 * <li>Perform the 
 * {@link #initialize(float, float, float, float, float, float, TriCell, TriCell) initialize()}
 * operation to set up the search.</li>
 * <li>Perform the {@link #process()} operation until the search is complete.</li>
 * <li>If the search complete successfully, use the {@link #pathCells()} operation to obtain
 * the path corridor.</li>
 * <li>Repeat as needed.</li>
 * </ol>
 * <p>Instances of this class are not thread safe.</p>
 */
public final class AStarSearch
{
    
    private final DistanceHeuristicType mHeuristic;
    
    private final PriorityQueue<TriCellPathNode> mOpenHeap 
        = new PriorityQueue<TriCellPathNode>(10, new TriCellPathComparator());
    
    private final ArrayList<TriCell> mClosedCells = new ArrayList<TriCell>();
    
    private SearchState mState = UNINITIALIZED;
    
    private TriCell[] mPathCells;
    
    private float mStartX;
    private float mStartY;
    private float mStartZ;
    
    /**
     * Goal data is not required by the search.  But it is required
     * by clients of the search.
     */
    private float mGoalX;
    private float mGoalY;
    private float mGoalZ;
    
    private TriCell mStartCell = null;
    private TriCell mGoalCell = null;
    
    private final Hashtable<TriCell, TriCellPathNode> mOpenCellMap 
        = new Hashtable<TriCell, TriCellPathNode>();
    
    /**
     * Constructor
     * @param heuristic  The distance to goal heuristic to use for the search.
     * @throws IllegalArgumentException If the heuristic is null or invalid.
     */
    public AStarSearch(DistanceHeuristicType heuristic)
        throws IllegalArgumentException
    {
        mHeuristic = heuristic;
    }

    /**
     * Evaluates a list of paths.  If any provide a valid path
     * for the current search, the path will be returned.
     * If multiple paths in the list are valid for the current
     * search, the first one found is returned.
     * @param paths A list of paths to evaluate.
     * @return A reference to a path that is valid for
     * the current search parameters.  Or NULL if no paths are valid.
     */
    public MasterPath evaluate(ArrayList<MasterPath> paths)
    {
        if (!isActive())
            return null;
        for (MasterPath path : paths)
        {
            if (path.startCell() == mStartCell
                    && path.goalCell() == mGoalCell)
                return path;
        }
        return null;
    }
    
    /**
     * The x-value of the goal of the search. (x, y, z)
     * @return The x-value of the goal of the search. 
     */
    public float goalX() { return mGoalX; }
    
    /**
     * The y-value of the goal of the search. (x, y, z)
     * @return The y-value of the goal of the search. 
     */
    public float goalY() { return mGoalY; }

    /**
     * The z-value of the goal of the search. (x, y, z)
     * @return The z-value of the goal of the search. 
     */
    public float goalZ() { return mGoalZ; }

    /**
     * Prepares the instance for a new search.
     * <p>Calling this operation while a search is in progress will clear
     * the existing search.</p>
     * <p>There is no need to call {@link #reset()} when re-using a search instance.
     * Just use this operation to re-initialize the search.</p>
     * <p>Behavior of the search is undefined if the start and/or goal
     * positions are not within their respective start and goal cell columns.</p>
     * @param startX The x-value of the start location (startX, startY, startZ).
     * @param startY The y-value of the start location (startX, startY, startZ).
     * @param startZ The z-value of the start location (startX, startY, startZ).
     * @param goalX The x-value of the goal location (goalX, goalY, goalZ).
     * @param goalY The y-value of the goal location (goalX, goalY, goalZ).
     * @param goalZ The z-value of the goal location (goalX, goalY, goalZ).
     * @param startCell The start cell.  (In which the start position exists.)
     * @param goalCell The goal cell.  (In which the goal position exists.)
     * @return The state of the search after initialization.  
     * Usually this is {@link SearchState#INITIALIZED}.  But other values may be returned if
     * there is a failure.
     */
    public SearchState initialize(float startX, float startY, float startZ
            , float goalX, float goalY, float goalZ
            , TriCell startCell
            , TriCell goalCell)
    {
        
        reset();
        
        mStartX = startX;
        mStartY = startY;
        mStartZ = startZ;
        
        mGoalX = goalX;
        mGoalY = goalY;
        mGoalZ = goalZ;
        
        mStartCell = startCell;
        mGoalCell = goalCell;
        
        // Add the starting point to the open heap as the root entry.
        TriCellPathNode current = new TriCellPathNode(mStartCell);
        mOpenHeap.add(current);
        
        mState = INITIALIZED;
        
        return mState;
        
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
     * An ordered list of cells representing the path corridor
     * from the start to the goal point.
     * Will only contain a value if the {@link #state()} of the search
     * is {@link SearchState#COMPLETE}.  Otherwise it will be null.
     * <p>For performance reasons, this operation provides a reference
     * to the internal data array.  It is not a copy.</p>
     * @return An ordered list of cells representing the the corridor
     * from the start to the goal point.
     */
    public TriCell[] pathCells() { return mPathCells; }
    
    /**
     * Performs a single iteration of the A* search.  Must be repeated as 
     * needed to complete the search.
     * <p>While it is safe to call this operation during any state, actual
     * processing will only occur if the state is either 
     * {@link SearchState#INITIALIZED} or {@link SearchState#PROCESSING}.</p>
     * @return The state of the search after the search iteration.
     */
    public SearchState process() 
    {
        
        if (mState == INITIALIZED)
        {
            mState = PROCESSING;
            if (mStartCell == mGoalCell)
            {
                // Special Case: Start and goal are in the same cell.
                mPathCells = new TriCell[1];
                mPathCells[0] = mStartCell;
                mState = COMPLETE;
            }
        }

        if (mState != PROCESSING)
            return mState;
        
        // Get the top of the heap.
        TriCellPathNode current = mOpenHeap.poll();
        mClosedCells.add(current.cell());
        mOpenCellMap.remove(current.cell());
        
        if (current.cell() == mGoalCell)
        {
            // Finished the search.
            mPathCells = new TriCell[current.pathSize()];
            current.loadPath(mPathCells);
            cleanup();
            mState = COMPLETE;
            return mState;
        }        

        // Process this cell.
        
        int linkCount = current.cell().maxLinks();
        
        for (int iLink = 0; iLink < linkCount; iLink++)
        {
            TriCell linkedCell = current.cell().getLink(iLink);
            if (linkedCell == null)
                continue;
            if (mClosedCells.contains(linkedCell)) 
                continue;
            if (linkedCell.linkCount() == 1)
            {
                if (linkedCell == mGoalCell)
                {
                    // Special case, can complete immediately
                    mPathCells = new TriCell[current.pathSize()+1];
                    current.loadPath(mPathCells);
                    mPathCells[mPathCells.length-1] = linkedCell;
                    cleanup();
                    mState = COMPLETE;
                    return mState;
                }
                // This cell is a dead end.  Ignore it.
                continue;
            }
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
                entry.setH(DistanceHeuristic.getHeuristicValue(mHeuristic
                        , linkedCell.centroidX
                        , linkedCell.centroidY
                        , linkedCell.centroidZ
                        , mGoalX
                        , mGoalY
                        , mGoalZ));
                mOpenHeap.add(entry);
                mOpenCellMap.put(linkedCell, entry);
            }
        }
        
        if (mOpenHeap.size() == 0)
        {
            // The search is not complete, but there is nothing in the open
            // heap.  Search has failed.
            mState = FAILED;
            cleanup();
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
        mGoalX = 0;
        mGoalY = 0;
        mGoalZ = 0;
        mStartCell = null;
        mGoalCell = null;
        mPathCells = null;
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
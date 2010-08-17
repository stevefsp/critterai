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

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Provides a shortest path search from the start location to the goal Cell, using the A* search algorithm.
    /// </summary>
    /// <remarks>
    /// When the search completes, provides an ordered list of cells that represent a corridor
    /// from the start point to the goal Cell.
    /// <para>Suitable for re-use across multiple searches.</para>
    /// <para>Primary use case:</para>
    /// <ol>
    /// <li>Perform the 
    /// <see cref="Initialize">Initialize</see> operation to set up the search.</li>
    /// <li>Perform the <see cref="Process">Process</see> operation until the search is complete.</li>
    /// <li>If the search complete successfully, use <see cref="PathCells">PathCells</see> to obtain
    /// the path corridor.</li>
    /// <li>Repeat as needed.</li>
    /// </ol>
    /// <para>Instances of this class are not thread safe.</para>
    /// </remarks>
    public sealed class AStarSearch
    {
        
        private readonly DistanceHeuristicType mHeuristic;
        private readonly TriCellPathNodeHeap mOpenHeap = new TriCellPathNodeHeap();
        private readonly List<TriCell> mClosedCells = new List<TriCell>();
        private SearchState mState = SearchState.Uninitialized;
        
        private TriCell[] mPathCells;
        
        private float mStartX;
        private float mStartY;
        private float mStartZ;
        
        /*
         * Goal Data is not required by the search.  But it is required
         * by clients of the search.
         */
        private float mGoalX;
        private float mGoalY;
        private float mGoalZ;
        
        private TriCell mStartCell = null;
        private TriCell mGoalCell = null;

        private readonly Dictionary<TriCell, TriCellPathNode> mOpenCellMap
            = new Dictionary<TriCell, TriCellPathNode>();

        /// <summary>
        /// The x-value of the goal of the search. (x, y, z)
        /// </summary>
        public float GoalX { get { return mGoalX; } }

        /// <summary>
        /// The y-value of the goal of the search. (x, y, z)
        /// </summary>
        public float GoalY { get { return mGoalY; } }

        /// <summary>
        /// The z-value of the goal of the search. (x, y, z)
        /// </summary>
        public float GoalZ { get { return mGoalZ; } }

        /// <summary>
        /// TRUE if a search is in progress.  Otherwise the search is finished or
        /// uninitialized.
        /// </summary>
        public Boolean IsActive
        {
            get
            {
                return (mState == SearchState.Processing
                    || mState == SearchState.Initialized ? true : false);
            }
        }

        /// <summary>
        /// The current state of the search.
        /// </summary>
        public SearchState State { get { return mState; } }

        /// <summary>
        /// An ordered list of cells representing the path corridor
        /// from the start to the goal point.
        /// </summary>
        /// <remarks>
        /// Will only contain a value if the <see cref="State">State</see> of the search
        /// is <see cref="SearchState.Complete">Complete</see>.  Otherwise it will be null.
        /// <para>For performance reasons, this operation provides a reference
        /// to the internal Data array.  It is not a copy.</para></remarks>
        public TriCell[] PathCells { get { return mPathCells; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="heuristic">The heuristic to use to estimate the distance to goal.</param>
        public AStarSearch(DistanceHeuristicType heuristic)
        {
            mHeuristic = heuristic;
        }

        /// <summary>
        /// Evaluates a list of paths.  If any provide a valid path
        /// for the current search, the path will be returned. If multiple paths in the list are valid for the current
        /// search, the first one found is returned.
        /// </summary>
        /// <param name="paths">A list of paths to Evaluate.</param>
        /// <returns>A reference to a path that is valid for
        /// the current search parameters.  Or NULL if no paths are valid.</returns>
        public MasterPath Evaluate(List<MasterPath> paths)
        {
            if (!IsActive)
                return null;
            foreach (MasterPath path in paths)
            {
                if (path.StartCell == mStartCell
                        && path.GoalCell == mGoalCell)
                    return path;
            }
            return null;
        }

        /// <summary>
        /// Prepares the instance for a new search.
        /// </summary>
        /// <remarks>
        /// Calling this operation while a search is in progress will Clear
        /// the existing search.
        /// <para>There is no need to call <see cref="Reset">Reset</see> when re-using a search instance.
        /// Just use this operation to re-initialize the search.</para>
        /// <para>Behavior of the search is undefined if the start and/or goal
        /// positions are not within their respective start and goal cell columns.</para>
        /// </remarks>
        /// <param name="startX">The x-value of the start location (startX, startY, startZ).</param>
        /// <param name="startY">The y-value of the start location (startX, startY, startZ).</param>
        /// <param name="startZ">The z-value of the start location (startX, startY, startZ).</param>
        /// <param name="goalX">The x-value of the goal location (goalX, goalY, goalZ).</param>
        /// <param name="goalY">The y-value of the goal location (goalX, goalY, goalZ).</param>
        /// <param name="goalZ">The z-value of the goal location (goalX, goalY, goalZ).</param>
        /// <param name="startCell">The start Cell.  (In which the start position exists.)</param>
        /// <param name="goalCell">The goal Cell.  (In which the goal position exists.)</param>
        /// <returns>The State of the search after initialization. Usually this is 
        /// <see cref="SearchState.Initialized">Initialized</see>.  But other values may be returned if
        /// there is a failure.</returns>
        public SearchState Initialize(float startX, float startY, float startZ
                , float goalX, float goalY, float goalZ
                , TriCell startCell
                , TriCell goalCell)
        {
            Reset();
            
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
            mOpenHeap.Add(current);
            
            mState = SearchState.Initialized;
            
            return mState;
            
        }
        
        /// <summary>
        /// Performs a single iteration of the A* search.  Must be repeated as 
        /// needed to complete the search.
        /// </summary>
        /// <remarks>While it is safe to call this operation during any <see cref="State">State</see>, actual
        /// processing will only occur if the state is either 
        /// <see cref="SearchState.Initialized">Initialized</see> or 
        /// <see cref="SearchState.Processing">Processing</see>.</remarks>
        /// <returns>The state of the search after the search iteration.</returns>
        public SearchState Process() 
        {
            
            if (mState == SearchState.Initialized)
            {
                mState = SearchState.Processing;
                if (mStartCell == mGoalCell)
                {
                    // Special Case: Start and goal are in the same Cell.
                    mPathCells = new TriCell[1];
                    mPathCells[0] = mStartCell;
                    mState = SearchState.Complete;
                }
            }

            if (mState != SearchState.Processing)
                return mState;
            
            // Get the top of the heap.
            TriCellPathNode current = mOpenHeap.Poll();
            mClosedCells.Add(current.Cell);
            mOpenCellMap.Remove(current.Cell);
            
            if (current.Cell == mGoalCell)
            {
                // Finished the search.
                mPathCells = new TriCell[current.PathSize];
                current.LoadPath(mPathCells);
                Cleanup();
                mState = SearchState.Complete;
                return mState;
            }        

            // Process this Cell.
            
            int linkCount = current.Cell.MaxLinks;
            
            for (int iLink = 0; iLink < linkCount; iLink++)
            {
                TriCell linkedCell = current.Cell.GetLink(iLink);
                if (linkedCell == null)
                    continue;
                if (mClosedCells.Contains(linkedCell)) 
                    continue;
                if (linkedCell.LinkCount == 1)
                {
                    if (linkedCell == mGoalCell)
                    {
                        // Special case, can complete immediately
                        mPathCells = new TriCell[current.PathSize+1];
                        current.LoadPath(mPathCells);
                        mPathCells[mPathCells.Length-1] = linkedCell;
                        Cleanup();
                        mState = SearchState.Complete;
                        return mState;
                    }
                    // This Cell is a dead end.  Ignore it.
                    continue;
                }
                TriCellPathNode entry;
                if (mOpenCellMap.TryGetValue(linkedCell, out entry))
                {
                    // Cell is in the open heap via another path.
                    // Is this path better?
                    
                    float newG = entry.EstimateNewG(current, mStartX, mStartY, mStartZ);
                    if (newG < entry.G)
                    {
                        // Update entry and re-stack it in the heap.
                        entry.SetParent(current, mStartX, mStartY, mStartZ);
                        mOpenHeap.Restack(entry);
                    }
                }
                else
                {
                    // This is a new Cell.  Set things up.
                    entry = new TriCellPathNode(linkedCell
                            , current
                            , mStartX
                            , mStartY
                            , mStartZ);
                    entry.H = DistanceHeuristic.GetHeuristicValue(mHeuristic
                            , linkedCell.CentroidX
                            , linkedCell.CentroidY
                            , linkedCell.CentroidZ
                            , mGoalX
                            , mGoalY
                            , mGoalZ);
                    mOpenHeap.Add(entry);
                    mOpenCellMap.Add(linkedCell, entry);
                }
            }
            
            if (mOpenHeap.Count == 0)
            {
                // The search is not complete, but there is nothing in the open
                // heap.  Search has failed.
                // UnityEngine.Debug.LogWarning("Failed");
                mState = SearchState.Failed;
                Cleanup();
            }
            
            return mState;
            
        }

        /// <summary>
        /// Resets the object to its state at construction, releasing
        /// any internal references held by the search.
        /// </summary>
        public void Reset()
        {
            Cleanup();
            mState = SearchState.Uninitialized;
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

        /// <summary>
        /// Cleans up after a search is complete, but leaves the search/path/state information
        /// intact.
        /// </summary>
        private void Cleanup()
        {
            mOpenHeap.Clear();
            mClosedCells.Clear();
            mOpenCellMap.Clear();
        }
    }
}

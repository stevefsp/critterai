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

namespace org.critterai.nav
{
    /// <summary>
    /// Provides a shortest path search from the start point to the goal Cell, using the 
    /// Dijskstra search algorithm. When the search completes, provides one or more ordered
    /// lists of cells that represent corridors from the start point to goal cells.
    /// </summary>
    /// <remarks>
    /// <para>Features:</para>
    /// <ul>
    /// <li>Complete when the first shortest path is found. (Single path returned.)</li>
    /// <li>Complete when all goal points are found. (Multiple paths returned.)</li>
    /// <li>Limit the search mDepth. (Only paths found within the search mDepth are returned.)</li>
    /// </ul>
    /// <para>Suitable for re-use across multiple searches.</para>
    /// <para>Primary use case:</para>
    /// <ol>
    /// <li>Perform the 
    /// <see cref="Initialize(float, float, float, float[], TriCell, TriCell[], int, Boolean)">
    /// Initialize</see> operation to set up the search.</li>
    /// <li>Perform the <see cref="Process">Process</see> operation until the search is complete.</li>
    /// <li>If the search completes successfully, use <see cref="PathCount">PathCount</see> 
    /// and the <see cref="GetPathCells">GetPathCells</see> operation to obtain the search results.</li>
    /// <li>Repeat as needed.</li>
    /// </ol>
    /// <para>Instances of this class are not thread safe.</para>
    /// </remarks>
    public sealed class DijkstraSearch 
    {
        
        private readonly TriCellPathNodeHeap mOpenHeap = new TriCellPathNodeHeap();
        
        /// <summary>
        /// Cells that have already been investigated.
        /// </summary>
        private readonly List<TriCell> mClosedCells = new List<TriCell>();
        
        private SearchState mState = SearchState.Uninitialized;
        
        private readonly List<TriCell[]> mPathCells = new List<TriCell[]>();
        
        private Boolean mSelectFirst;
        
        private float mStartX;
        private float mStartY;
        private float mStartZ;
        
        private float[] mGoals;
        
        private TriCell mStartCell = null;
        private readonly List<TriCell> mGoalCells = new List<TriCell>();

        private readonly Dictionary<TriCell, TriCellPathNode> mOpenCellMap 
            = new Dictionary<TriCell, TriCellPathNode>();
        
        private int mMaxSearchDepth;

        /// <summary>
        /// The goal points of the search in the form (x, y, z) * numberOfGoals.
        /// </summary>
        public float[] Goals { get { return mGoals; } }

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
        /// The number of paths available from <see cref="GetPathCells(int)">GetPathCells</see>
        /// </summary>
        public int PathCount { get { return mPathCells.Count; } }

        /// <summary>
        /// The current State of the search.
        /// </summary>
        public SearchState State { get { return mState; } }

        /// <summary>
        /// An array of ordered of cells representing a path corridor
        /// from the start point to one of the goal points.
        /// Will contain one path for each goal successfully found.
        /// </summary>
        /// <remarks>
        /// Will only contain information if the <see cref="State">State</see> of the search
        /// is <see cref="SearchState.Complete">Complete</see>  Otherwise it will be null.
        /// <para>A successful search only guarantees that at least one goal point has
        /// been found, not that all goal points have been found</para>
        /// <para>The order of the results is undefined.  The goal Cell of each path
        /// can be used to determine which goal point is associated with which path.</para> 
        /// <para>For performance reasons, this operation provides a reference
        /// to the internal data array.  It is not a copy.</para>
        /// @param index 
        /// @return An array of ordered cells representing a path corridor from the
        /// start point to a goal point.
        /// </remarks>
        /// <param name="index">The index of the path to retrieve.  The index will be between
        /// zero and <see cref="PathCount">PathCount</see>.</param>
        /// <returns>A reference to an array of ordered cells representing a path corridor.  Or NULL
        /// if there are no paths.</returns>
        public TriCell[] GetPathCells(int index) { return mPathCells[index]; }
        
        /// <summary>
        /// Prepares the instance for a new search.
        /// </summary>
        /// <remarks>
        /// Calling this operation while a search is in progress will Clear
        /// the existing search.
        /// <para>There is no need to call <see cref="Reset">Reset</see> when re-using a search instance.
        /// Just use this operation to re-Initialize the search.</para>
        /// <para>Behavior of the search is undefined if any of the the start and/or goal
        /// position is not within the start/goal Cell columns.</para>
        /// </remarks>
        /// <param name="startX">The x-value of the start location (startX, startY, startZ).</param>
        /// <param name="startY">The y-value of the start location (startX, startY, startZ).</param>
        /// <param name="startZ">The z-value of the start location (startX, startY, startZ).</param>
        /// <param name="goals">The goal positions of the search in the form (x, y, z) * numberOfGoals.
        /// A value of null is valid.</param>
        /// <param name="startCell">The start Cell.  (In which the start position exists.)</param>
        /// <param name="goalCells">The cells to search for.</param>
        /// <param name="maxSearchDepth">The maximum depth to search.  The search will terminate,
        /// whether successful or not, at this search depth.  A value of 
        /// <see cref="int.MaxValue">int.MaxValue</see> will result in an exhaustive search.
        /// The maximum possible path length will be search depth + 1.</param>
        /// <param name="selectFirst">If set to TRUE, the search will complete as soon as
        /// the first (shortest) path is found.</param>
        /// <returns>The State of the search after initialization.  
        /// Usually this is <see cref="SearchState.Initialized">Initialized</see>.  But other values may be returned if
        /// there is a failure.</returns>
        public SearchState Initialize(float startX, float startY, float startZ
                , float[] goals
                , TriCell startCell
                , TriCell[] goalCells
                , int maxSearchDepth
                , Boolean selectFirst)
        {
            
            Reset();
            
            mMaxSearchDepth = Math.Max(1, maxSearchDepth);
            
            mStartX = startX;
            mStartY = startY;
            mStartZ = startZ;
            
            mGoals = goals;
            
            mSelectFirst = selectFirst;
            
            mStartCell = startCell;
            if (mGoalCells.Capacity < goalCells.Length)
                mGoalCells.Capacity = goalCells.Length;
            mGoalCells.AddRange(goalCells);
            
            // Add the starting point to the open heap as a root entry.
            TriCellPathNode current = new TriCellPathNode(mStartCell);
            mOpenHeap.Add(current);
            
            mState = SearchState.Initialized;
            
            return mState;
            
        }
        
        /// <summary>
        /// Prepares the instance for a new search.
        /// Useful when searching for cells rather than goal points.
        /// </summary>
        /// <remarks>
        /// Calling this operation while a search is in progress will Clear
        /// the existing search.
        /// <para>There is no need to call <see cref="Reset">Reset</see> when re-using a search instance.
        /// Just use this operation to re-Initialize the search.</para>
        /// <para>Behavior of the search is undefined if any of the the start and/or goal
        /// position is not within the start/goal Cell columns.</para>
        /// </remarks>
        /// <param name="startX">The x-value of the start location (startX, startY, startZ).</param>
        /// <param name="startY">The y-value of the start location (startX, startY, startZ).</param>
        /// <param name="startZ">The z-value of the start location (startX, startY, startZ).</param>
        /// <param name="startCell">The start Cell.  (In which the start position exists.)</param>
        /// <param name="goalCells">The cells to search for.</param>
        /// <param name="maxSearchDepth">The maximum depth to search.  The search will terminate,
        /// whether successful or not, at this search depth.  A value of 
        /// <see cref="int.MaxValue">int.MaxValue</see> will result in an exhaustive search.
        /// The maximum possible path length will be search depth + 1.</param>
        /// <param name="selectFirst">If set to TRUE, the search will complete as soon as
        /// the first (shortest) path is found.</param>
        /// <returns>The State of the search after initialization.  
        /// Usually this is <see cref="SearchState.Initialized">Initialized</see>.  But other values may be returned if
        /// there is a failure.</returns>
        public SearchState Initialize(float startX, float startY, float startZ
                , TriCell startCell
                , TriCell[] goalCells
                , int maxSearchDepth
                , Boolean selectFirst)
        {
            return Initialize(startX, startY, startZ
                    , null
                    , startCell
                    , goalCells
                    , maxSearchDepth
                    , selectFirst);
        }
        
        /// <summary>
        /// Performs a single iteration of the Dijkstra search.  Must be repeated as 
        /// needed to complete the search.
        /// </summary>
        /// <remarks>
        /// While it is safe to call this operation during any State, actual processing will only occur
        /// if the State is either <see cref="SearchState.Initialized">Initialized</see> or 
        /// <see cref="SearchState.Processing">Processing</see>.
        /// </remarks>
        /// <returns>The state of the search after the search iteration.</returns>
        public SearchState Process() 
        {
            
            if (mState == SearchState.Initialized)
                mState = SearchState.Processing;
            else if (mState != SearchState.Processing)
                return mState;
            
            TriCellPathNode current = mOpenHeap.Poll();
            mClosedCells.Add(current.Cell);
            mOpenCellMap.Remove(current.Cell);
            
            int maxLinks = current.Cell.MaxLinks;
            for (int iLink = 0; iLink < maxLinks; iLink++)
            {
                TriCell linkedCell = current.Cell.GetLink(iLink);
                if (linkedCell == null)
                    continue;
                if (mClosedCells.Contains(linkedCell)) 
                    continue;
                if (mGoalCells.Contains(linkedCell))
                {
                    // Finished one of the searches.
                    TriCell[] cells = new TriCell[current.PathSize + 1];
                    current.LoadPath(cells);
                    cells[cells.Length-1] = linkedCell;
                    mPathCells.Add(cells);
                    if (mSelectFirst || mGoalCells.Count == 1)
                    {
                        // We can stop the search.
                        Cleanup();
                        mState = SearchState.Complete;
                        return mState;
                    }
                    mGoalCells.Remove(linkedCell);
                }
                if (linkedCell.LinkCount == 1)
                {
                    // Cell is a dead end, so we don't care about it.
                    mClosedCells.Add(linkedCell);
                    continue;
                }
                if (current.PathSize >= mMaxSearchDepth)
                    // The cells beyond this Cell are
                    // beyond the search mDepth.  So we are done.
                    continue;
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
                    mOpenHeap.Add(entry);
                    mOpenCellMap.Add(linkedCell, entry);
                }
            }
            
            if (mOpenHeap.Count == 0)
            {
                // Search is complete.
                Cleanup();
                if (mPathCells.Count == 0)
                    mState = SearchState.Failed;
                else
                    mState = SearchState.Complete;
                return mState;
            }
            
            return mState;
            
        }

        /// <summary>
        /// Resets the object to its State at construction, releasing
        /// any internal references held by the search.
        /// </summary>
        public void Reset()
        {
            
            Cleanup();
            mState = SearchState.Uninitialized;
            mStartX = 0;
            mStartY = 0;
            mStartZ = 0;
            mGoals = null;
            mStartCell = null;
            mGoalCells.Clear();
            mPathCells.Clear();
            mSelectFirst = false;
        }


        /// <summary>
        /// Cleans up after a search is complete, but leaves the search/path/dtate information
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

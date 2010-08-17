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

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Represents a tip of an ordered list of cells that form the shortest known path
    /// back to a start point.
    /// </summary>
    /// <remarks>From a graph standpoint, the node is the midpoint on the cell wall connecting
    /// the <see cref="Cell">Cell</see> to its <see cref="Parent">Parent</see> cell.  It there is no parent, then
    /// the node is the start point.
    /// <para>Instances of this class are not thread-safe.</para>
    /// </remarks>
    public sealed class TriCellPathNode
    {
        private readonly TriCell mCell;    
        private float mH = 0;
        private float mLocalG = 0;
        private TriCellPathNode mParent;

        /// <summary>
        /// The navigation cell backing this path node.
        /// </summary>
        public TriCell Cell { get { return mCell; } }



        /// <summary>
        /// Constructor for the root node in a node graph.
        /// </summary>
        /// <param name="cell">The cell that backs the node.</param>
        public TriCellPathNode(TriCell cell)
        {
            if (cell == null)
                throw new ArgumentNullException("cell");
            mCell = cell;
            mParent = null;
        }

        /// <summary>
        /// Total estimated cost of the path that includes this node. (F = G + H)
        /// </summary>
        public float F
        {
            get
            {
                float f = mLocalG + mH;
                if (mParent != null) f += mParent.G;
                return f;
            }
        }

        /// <summary>
        /// The path cost from the starting point to the current path node.
        /// </summary>
        public float G
        {
            get
            {
                if (mParent == null)
                    return mLocalG;
                return mLocalG + mParent.G;
            }
        }

        /// <summary>
        /// Heuristic value. Estimated cost to move from the current node to the goal point.
        /// </summary>
        public float H { get { return mH; } set { mH = value; } }

        /// <summary>
        /// The path cost of the edge connecting this path node to its parent.
        /// <para>Value will be zero if <see cref="Parent">Parent</see> is null.  Otherwise it will
        /// be the distance from parent node to the midpoint of the wall connecting 
        /// the <see cref="Cell">Cell</see> to its <see cref="Parent">Parent</see> cell.</para>
        /// </summary>
        public float LocalG { get { return mLocalG; } }

        /// <summary>
        /// The path node that is immediately before this node in the graph.
        /// If null, then this is the root path node.
        /// </summary>
        public TriCellPathNode Parent { get { return mParent; } }

        /// <summary>
        /// The number of path nodes from the root to this node.
        /// </summary>
        public int PathSize
        {
            get
            {
                if (mParent == null)
                    return 1;
                else
                    return mParent.PathSize + 1;
            }
        }

        /// <summary>
        /// Constructor for a node that has a parent.
        /// </summary>
        /// <param name="cell">The cell that backs the node.</param>
        /// <param name="parent">The parent node.  The parent cell must share a wall with the cell.</param>
        /// <param name="startX">The x-value of the path starting point. (startX, startY, startZ)</param>
        /// <param name="startY">The y-value of the path starting point. (startX, startY, startZ)</param>
        /// <param name="startZ">The z-value of the path starting point. (startX, startY, startZ)</param>
        public TriCellPathNode(TriCell cell, TriCellPathNode parent, float startX, float startY, float startZ)
        {
            if (cell == null || parent == null) 
                throw new ArgumentNullException("cell and/or parent");
            mCell = cell;
            mParent = parent;
            mLocalG = CalculateLocalG(mParent, startX, startY, startZ);
        }
        
        /// <summary>
        /// Determines the path cost from the search starting point to this path node
        /// if the potential parent is assigned.
        /// </summary>
        /// <remarks>
        /// In the standard use case, if this operation returns a lower value than the
        /// current value of <see cref="G">G</see>, then the potential parent is a better parent than the
        /// existing parent.
        /// </remarks>
        /// <param name="potentialParent">The parent to test.</param>
        /// <param name="startX">The x-value of the path starting point. (startX, startY, startZ)</param>
        /// <param name="startY">The y-value of the path starting point. (startX, startY, startZ)</param>
        /// <param name="startZ">The z-value of the path starting point. (startX, startY, startZ)</param>
        /// <returns>The path cost form the search starting point to this path node if the test parent
        /// were the parent of this node.</returns>
        public float EstimateNewG(TriCellPathNode potentialParent, float startX, float startY, float startZ)
        {
            // Get an estimate of the new local G if this parent is used.
            return potentialParent.G + CalculateLocalG(potentialParent, startX, startY, startZ);
        }
        
        /// <summary>
        /// Gets an ordered list of cells that represent the path from the root node to 
        /// the current node.
        /// </summary>
        /// <param name="outCells"> outCells The array to load the result into. Length of the array must be at
        /// least <see cref="PathSize">PathSize</see>.</param>
        public void LoadPath(TriCell[] outCells)
        {
            LoadPath(outCells, 0);
        }
        
        /// <summary>
        /// Sets the parent to this path node.
        /// <para>The use case is to decide at construction whether the node has a parent
        /// and leave it that way, only assigning a new parent to replace an existing parent.</para>
        /// <para>Behavior is undefined if this use case is violated.</para>
        /// @param 
        /// </summary>
        /// <param name="parent">Parent The new parent of this path node.</param>
        /// <param name="startX">The x-value of the path starting point. (startX, startY, startZ)</param>
        /// <param name="startY">The y-value of the path starting point. (startX, startY, startZ)</param>
        /// <param name="startZ">The z-value of the path starting point. (startX, startY, startZ)</param>
        public void SetParent(TriCellPathNode parent, float startX, float startY, float startZ) 
        { 
            mParent = parent;
            if (mParent == null)
                mLocalG = 0;
            else
                mLocalG = CalculateLocalG(mParent, startX, startY, startZ);
        }

        private float CalculateLocalG(TriCellPathNode parent, float startX, float startY, float startZ)
        {
            
            int linkIndex = parent.Cell.GetLinkIndex(mCell);
            
            if (parent.Parent == null)
            {
                // The parent is the starting cell.  So get the distance from the starting
                // position to the connecting wall.
                return (float)Math.Sqrt(parent.Cell.GetLinkPointDistanceSq(startX, startY, startZ
                        , linkIndex));
            }
            
            // Get the distance from the connecting wall of my parent's parent to my connecting wall.
            // Basically, this is getting the distance from my parent's parent to me.
            
            int otherConnectingWall = parent.Cell.GetLinkIndex(parent.Parent.Cell);
            return parent.Cell.GetLinkPointDistance(otherConnectingWall, linkIndex);
            
        }

        private int LoadPath(TriCell[] outCells, int index)
        {
            int myIndex = index;
            if (mParent != null)
                // Insert parent information first.
                myIndex = mParent.LoadPath(outCells, index);
            if (myIndex >= outCells.Length)
                // Can't fit anything more into this path.
                return myIndex;
            // Add my information.
            outCells[myIndex] = mCell;
            return myIndex+1;
        }
    }
}

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
using System.Collections.Generic;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// A simple ordered heap for <see cref="TriCellPathNode">TriCellPathNodes</see>.
    /// The heap is ordered such that the node with the lowest F-value is at the top
    /// of the heap.
    /// <remarks>If a node's F-value changes, the <see cref="Restack">Restack</see>
    /// operation must be performed.</remarks>
    /// </summary>
    public sealed class TriCellPathNodeHeap
    {
        private const int NULL_INDEX = -1;

        private readonly List<TriCellPathNode> mHeap = new List<TriCellPathNode>();

        /// <summary>
        /// The number of nodes in the heap.
        /// </summary>
        public int Count { get { return mHeap.Count; } }

        /// <summary>
        /// Adds a node to the heap.
        /// </summary>
        /// <param name="node">The node to add to the heap.</param>
        public void Add(TriCellPathNode node)
        {
            float f = node.F;
            mHeap.Add(node);
            int loc = RestackTowardRoot(mHeap.Count - 1);
        }

        /// <summary>
        /// Gets the node with the lowest F-value without removing it from the heap.
        /// </summary>
        /// <returns>The node with the lowest F-value, or NULL if the heap is empty.</returns>
        public TriCellPathNode Peek()
        {
            return (mHeap.Count == 0 ? null : mHeap[0]);
        }

        /// <summary>
        /// Gets the node with the lowest F-value and removes it from the heap.
        /// </summary>
        /// <returns>The node with the lowest F-value, or NULL if the heap is empty.</returns>
        public TriCellPathNode Poll()
        {
            if (mHeap.Count == 0)
                return null;
            TriCellPathNode result = mHeap[0];
            if (mHeap.Count == 1)
            {
                // The last entry has been extracted.
                mHeap.Clear();
                return result;
            }
            // Move the last entry to the root and restack.
            mHeap[0] = mHeap[mHeap.Count - 1];
            mHeap.RemoveAt(mHeap.Count - 1);
            RestackTowardLeaf(0);
            return result;
        }

        /// <summary>
        /// Re-evaluates a node whose F-value has changed and re-ordered it within
        /// the stack as needed.
        /// </summary>
        /// <remarks>If the F-value of a node changes and a restack is not performed,
        /// the order of the heap will no longer be valid.
        /// <para>This operation cannot be used to add nodes to the stack.</para></remarks>
        /// <param name="node">The node whose F-value has changed.</param>
        public void Restack(TriCellPathNode node)
        {
            int index = mHeap.IndexOf(node);
            if (index < 0 || RestackTowardRoot(index) != index)
                return;
            RestackTowardLeaf(index);
        }

        /// <summary>
        /// Empties the heap.
        /// </summary>
        public void Clear() { mHeap.Clear(); }

        private int GetParentIndex(int index) { return (index - 1) / 2; }
        private int GetLeftIndex(int index) { return index * 2 + 1; }
        private int GetRightIndex(int index) { return index * 2 + 2; }

        private int RestackTowardRoot(int index)
        {
            int parentIndex = GetParentIndex(index);
            while (index > 0 && mHeap[index].F < mHeap[parentIndex].F)
            {
                TriCellPathNode parent = mHeap[parentIndex];
                mHeap[parentIndex] = mHeap[index];
                mHeap[index] = parent;
                index = parentIndex;
                parentIndex = GetParentIndex(index);
            }
            return index;
        }

        private void RestackTowardLeaf(int index)
        {
            int selectedIndex;
            int leftIndex;
            int rightIndex;
            while (true)
            {
                leftIndex = GetLeftIndex(index);
                rightIndex = GetRightIndex(index);
                selectedIndex = index;
                if (leftIndex < mHeap.Count && mHeap[index].F > mHeap[leftIndex].F)
                    selectedIndex = leftIndex;
                if (rightIndex < mHeap.Count && mHeap[selectedIndex].F > mHeap[rightIndex].F)
                    selectedIndex = rightIndex;
                if (selectedIndex == index)
                    break;
                TriCellPathNode child = mHeap[selectedIndex];
                mHeap[selectedIndex] = mHeap[index];
                mHeap[index] = child;
                index = selectedIndex;
            }
        }
    }
}

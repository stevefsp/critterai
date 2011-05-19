/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using org.critterai.interop;
using org.critterai.nmgen.rcn;

namespace org.critterai.nmgen
{
    /// <summary>
    /// A heightfield layer set built from a 
    /// <see cref="CompactHeightfield"/>.
    /// </summary>
    /// <remarks>
    /// <p>Undocumented: Equivalent to Recast: rcHeightfieldLayerSet.</p>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    public sealed class HeightFieldLayerSet
        : IManagedObject
    {
        private IntPtr root;
        private HeightFieldLayer[] mLayers;
        private int mLayerCount;

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (root == IntPtr.Zero); } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// The number of layers in the set.
        /// </summary>
        public int LayerCount { get { return mLayerCount; } }

        private HeightFieldLayerSet(IntPtr root, int layerCount)
        {
            this.root = root;
            mLayerCount = layerCount;
            mLayers = new HeightFieldLayer[layerCount];

            for (int i = 0; i < mLayerCount; i++)
            {
                HeightFieldLayer layer = new HeightFieldLayer();
                HeightfieldLayserSetEx.GetLayer(root, i, layer);
                mLayers[i] = layer;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~HeightFieldLayerSet()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Frees and marks as disposed all resoures associated with the
        /// object.  (Including its <see cref="HeightfieldLayer"/> objects.)
        /// </summary>
        public void RequestDisposal()
        {
            if (!IsDisposed)
            {
                HeightfieldLayserSetEx.FreeEx(root);
                root = IntPtr.Zero;
                mLayerCount = 0;
                for (int i = 0; i < mLayers.Length; i++)
                {
                    mLayers[i].Reset();
                }
            }
        }

        /// <summary>
        /// Gets the specified layser.
        /// </summary>
        /// <param name="index">The layer. 
        /// [Limit: 0 &lt;= value &lt; LayerCount]</param>
        /// <returns></returns>
        public HeightFieldLayer GetLayer(int index)
        {
            if (IsDisposed || index < 0 || index >= mLayerCount)
                return null;

            return mLayers[index];
        }

        /// <summary>
        /// Builds a layer set from the <see cref="CompactHeightfield"/>.
        /// </summary>
        /// <param name="context">The context to use duing the operation.
        /// </param>
        /// <param name="field">The source field.</param>
        /// <returns>The resulting layer set, or null on failure.</returns>
        public static HeightFieldLayerSet Build(BuildContext context
            , CompactHeightfield field)
        {
            if (context == null)
                return null;

            IntPtr ptr = IntPtr.Zero;
            
            int layerCount = HeightfieldLayserSetEx.Build(context.root
                , field
                , field.BorderSize
                , field.WalkableHeight
                , ref ptr);

            if (ptr == IntPtr.Zero)
                return null;

            return new HeightFieldLayerSet(ptr, layerCount);
        }
    }
}

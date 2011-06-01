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
using System.Runtime.InteropServices;
using org.critterai.geom;
using org.critterai.interop;
using org.critterai.nmgen.rcn;

namespace org.critterai.nmgen
{
    /// <summary>
    /// A heightfield representing obstructed space.
    /// </summary>
    /// <remarks>
    /// <p>When used in the context of a heighfield, the term voxel
    /// refers to an area <see cref="XZCellSize"/> in width, 
    /// <see cref="XZCellSize"/> in depth, and <see cref="YCellSize"/>
    /// in height.</p>
    /// <p>TODO: Link to detailed discussion of heightfields.</p>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class Heightfield
        : IManagedObject
    {
        /*
         * Design notes:
         * 
         * I ran into complications with implementing this class with a data
         * layout matching the native structure. The cause appears to be the 
         * pointer to a pointer field in the native structure. So I switched
         * to the root pattern with some duplication of data on this size
         * of the boundary for performance reasons.
         * 
         * The AddSpan method is not supported yet because of a bug in Recast.
         * http://code.google.com/p/recastnavigation/issues/detail?id=167
         * 
         */

        private int mWidth = 0;
        private int mDepth = 0;

        private float[] mBoundsMin = new float[3];
        private float[] mBoundsMax = new float[3];

        private float mXZCellSize = 0;
        private float mYCellSize = 0;

        internal IntPtr root;

        /// <summary>
        /// The width of the heightfield. (Along the x-axis in cell units.)
        /// </summary>
        public int Width { get { return mWidth; } }

        /// <summary>
        /// The depth of the heighfield. (Along the z-axis in cell units.)
        /// </summary>
        public int Depth { get { return mDepth; } }

        /// <summary>
        /// The minimum bounds of the heightfield in world space. 
        /// [Form: (x, y, z)]
        /// </summary>
        /// <returns>The minimum bounds of the heighfield.
        /// </returns>
        public float[] GetBoundsMin() { return (float[])mBoundsMin.Clone(); }

        /// <summary>
        /// The maximum bounds of the heightfield in world space. 
        /// [Form: (x, y, z)]
        /// </summary>
        /// <returns>The maximum bounds of the heightfield.</returns>
        public float[] GetBoundsMax() { return (float[])mBoundsMax.Clone(); }

        /// <summary>
        /// The width/depth size of each cell. (On the xz-plane.)
        /// </summary>
        /// <remarks>
        /// <p>The smallest span can be 
        /// XZCellSize width * XZCellSize depth * YCellSize height.</p>
        /// <p>A width or depth value within the field can be converted
        /// to world units as follows:<br/>
        /// boundsMin[0] + (width * XZCellSize)<br/>
        /// boundsMin[2] + (depth * XZCellSize)</p>
        /// </remarks>
        /// 
        public float XZCellSize { get { return mXZCellSize; } }

        /// <summary>
        /// The height increments for span data.  (On the y-axis.)
        /// </summary>
        /// <remarks>
        /// <p>The smallest span can be 
        /// XZCellSize width * XZCellSize depth * YCellSize height.</p>
        /// <p>A height within the field is converted to world units
        /// as follows: boundsMin[1] + (height * YCellSize)</p>
        /// </remarks>
        public float YCellSize { get { return mYCellSize; } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return root == IntPtr.Zero; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the field. 
        /// [Limits: >= 1] [Units: Cells]
        /// </param>
        /// <param name="depth">The depth of the field.
        /// [Limits: >= 1] [Units: Cells]
        /// </param>
        /// <param name="boundsMin">The minimum bounds of the field's AABB.
        /// [Form: (x, y, z)] [Units: World]
        /// </param>
        /// <param name="boundsMax">The maximum bounds of the field's AABB.
        /// [Form: (x, y, z)] [Units: World]
        /// </param>
        /// <param name="xzCellSize">The xz-plane cell size. [Units: World]
        /// (>= <see cref="NMGen.MinCellSize"/>)</param>
        /// <param name="yCellSize">The y-axis span increments. [Units: World]
        /// (>= <see cref="NMGen.MinCellSize"/>)</param>
        public Heightfield(int width
            , int depth
            , float[] boundsMin
            , float[] boundsMax
            , float xzCellSize
            , float yCellSize)
        {
            root = HeightfieldEx.Alloc(width
                , depth
                , boundsMin
                , boundsMax
                , xzCellSize
                , yCellSize);

            if (root == IntPtr.Zero)
                return;

            mWidth = width;
            mDepth = depth;
            mBoundsMin = (float[])boundsMin.Clone();
            mBoundsMax = (float[])boundsMax.Clone();
            mYCellSize = yCellSize;
            mXZCellSize = xzCellSize;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Heightfield()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Frees all resources and marks object as disposed.
        /// </summary>
        public void RequestDisposal() 
        {
            if (!IsDisposed)
            {
                HeightfieldEx.FreeEx(root);
                root = IntPtr.Zero;
                mWidth = 0;
                mDepth = 0;
                mXZCellSize = 0;
                mYCellSize = 0;
                Array.Clear(mBoundsMin, 0, 3);
                Array.Clear(mBoundsMax, 0, 3);
            }
        }

        /// <summary>
        /// The number of spans in the field.
        /// </summary>
        /// <remarks>This is a non-trivial method call.  Cache the result
        /// when possible.</remarks>
        /// <returns>The number of spans in the field.</returns>
        public int GetSpanCount()
        {
            if (IsDisposed)
                return 0;
            return HeightfieldEx.GetSpanCount(root);
        }

        /// <summary>
        /// Gets an buffer that is sized to fit the maximum
        /// number of spans within a column of the field.
        /// </summary>
        /// <returns>A buffer that is sized to fit the maximum
        /// spans within a column.</returns>
        public HeightfieldSpan[] GetSpanBuffer()
        {
            if (IsDisposed)
                return null;

            int size = HeightfieldEx.GetMaxSpansInColumn(root);
            return new HeightfieldSpan[size];
        }

        /// <summary>
        /// Gets the spans within the specified column.
        /// </summary>
        /// <remarks>
        /// <p>The spans will be ordered from lowest height to highest.</p>
        /// <p>The <see cref="GetSpanBuffer"/> method can be used to
        /// get a properly sized buffer.</p>
        /// </remarks>
        /// <param name="widthIndex">The width index. 
        /// [Limits: 0 &lt;= value &lt; <see cref="Width"/>]</param>
        /// <param name="depthIndex">The depth index. 
        /// [Limits: 0 &lt;= value &lt; <see cref="Depth"/>]</param>
        /// <param name="buffer">The buffer to load the result into. 
        /// [Size: Maximum spans in a column]</param>
        /// <returns>The number of spans returned.</returns>
        public int GetSpans(int widthIndex
            , int depthIndex
            , HeightfieldSpan[] buffer)
        {
            if (IsDisposed)
                return -1;

            return HeightfieldEx.GetSpans(root
                , widthIndex
                , depthIndex
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Marks non-walkable spans as walkable if their maximum is
        /// within walkableStep of a walkable neihbor.
        /// </summary>
        /// <remarks>
        /// <p>Example of test: 
        /// Math.Abs(currentSpan.Max - neighborSpan.Max) &lt; walkableStep
        /// </p>
        /// <p>Allows the formation of walkable regions that will flow over low
        /// lying objects such as curbs, or up structures such as stairways.</p>
        /// </remarks>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="walkableStep">The maximum allowed difference between
        /// span maximum's for the step to be considered waklable.
        /// [Limit: > 0]</param>
        /// <returns>TRUE if the operation was successful.</returns>
        public bool MarkLowObstaclesWalkable(BuildContext context
            , int walkableStep)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.FlagLowObstaclesWalkable(context.root
                , walkableStep
                , root);
        }

        /// <summary>
        /// Marks spans that are ledges as not-walkable.
        /// </summary>
        /// <remarks>
        /// <p>A ledge is a span with neighbor whose maximum further away than
        /// walkableStep.  Example: 
        /// Math.Abs(currentSpan.Max - neighborSpan.Max) > walkableStep
        /// </p>
        /// <p>This method removes the impact of the overestimation of
        /// conservative voxelization so the resulting mesh will not have 
        /// regions hanging in the air over ledges.</p>
        /// </remarks>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="walkableHeight">The maximum floor to ceiling height
        /// that is considered still walkable. 
        /// [Limit: > <see cref="NMGen.MinWalkableHeight"/>]</param>
        /// <param name="walkableStep">The maximum allowed difference between
        /// span maximum's for the step to be considered walkable. 
        /// [Limit: > 0]
        /// </param>
        /// <returns>TRUE if the operation was successful.</returns>
        public bool MarkLedgeSpansNotWalkable(BuildContext context
            , int walkableHeight
            , int walkableStep)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.FlagLedgeSpansNotWalkable(context.root
                , walkableHeight
                , walkableStep
                , root);
        }

        /// <summary>
        /// Marks walkable spans as not walkable if the clearence above the
        /// span is less than the specified height.
        /// </summary>
        /// <remarks>
        /// <p>For this method, the clearance above the span is the distance
        /// from the span's maximum to the next higher span's minimum.
        /// (Same column.)</p>
        /// </remarks>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="walkableHeight">The maximum allowed floor to ceiling
        /// height that is considered still walkable.
        /// [Limit: > <see cref="NMGen.MinWalkableHeight"/>]</param>
        /// <returns>TRUE if the operation was successful.</returns>
        public bool MarkLowHeightSpansNotWalkable(BuildContext context
            , int walkableHeight)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.FlagLowHeightSpansNotWalkable(context.root
                , walkableHeight
                , root);
        }

        /// <summary>
        /// Voxelizes a triangle into the heightfield.
        /// </summary>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="verts">The triangle vertices.
        /// [Form: (ax, ay, ax, bx, by, bz, cx, cy, cz)]</param>
        /// <param name="area">The id of the area the triangle belongs to.
        /// (&lt;= <see cref="NMGen.WalkableArea"/>)</param>
        /// <param name="flagMergeThreshold">The distance where the
        /// walkable flag is favored over the non-walkable flag. [Limit: >= 0]
        /// [Normal: 1]</param>
        /// <returns>TRUE if the operation was successful.</returns>
        public bool AddTriangle(BuildContext context
            , float[] verts
            , byte area
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.AddTriangle(context.root
                , verts
                , area
                , root
                , flagMergeThreshold);
        }

        /// <summary>
        /// Voxelizes the triangles in the provided mesh into the heightfield.
        /// </summary>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="mesh"></param>
        /// <param name="areas">The ids of the areas the triangles belong to.
        /// [Limit: &lt;= <see cref="NMGen.WalkableArea"/>] 
        /// [Size: >= mesh.triCount]
        /// </param>
        /// <param name="flagMergeThreshold">The distance where the
        /// walkable flag is favored over the non-walkable flag. [Limit: >= 0]
        /// [Normal: 1]</param>
        /// <returns>TRUE if the operation was successful.</returns>
        public bool AddTriangles(BuildContext context
            , TriangleMesh mesh
            , byte[] areas
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.AddTriangles(context.root
                , mesh.verts
                , mesh.vertCount
                , mesh.tris
                , areas
                , mesh.triCount
                , root
                , flagMergeThreshold);
        }

        /// <summary>
        /// Voxelizes the provided triangles into the heightfield.
        /// </summary>
        /// <remarks>
        /// <p>Unlike many other methods in the library, the arrays must
        /// be sized exactly to the content.  If you need to pass buffers,
        /// use the method that takes a <see cref="TriangleMesh"/> object.</p>
        /// </remarks>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="verts">The vertices. [Form: (x, y, z) * vertCount]
        /// (No buffering allowed.)</param>
        /// <param name="tris">The triangles. 
        /// [Form: vertAIndex, vertBIndex, vertCIndex) * triCount] 
        /// (No buffering allowed.)</param>
        /// <param name="areas">The ids of the areas the triangles belong to.
        /// (&lt;= <see cref="NMGen.WalkableArea"/>) (Size: >= triCount)
        /// </param>
        /// <param name="flagMergeThreshold">The distance where the
        /// walkable flag is favored over the non-walkable flag. [Limit: >= 0]
        /// [Normal: 1]</param>
        /// <returns>TRUE if the operation was successful.</returns>
        public bool AddTriangles(BuildContext context
            , float[] verts
            , ushort[] tris
            , byte[] areas
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.AddTriangles(context.root
                , verts
                , verts.Length / 3
                , tris
                , areas
                , tris.Length / 3
                , root
                , flagMergeThreshold);
        }

        /// <summary>
        /// Voxelizes the provided triangles into the heightfield.
        /// </summary>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="verts">The triangles.
        /// [Form: (ax, ay, az, bx, by, bz, cx, by, cx) * triCount]</param>
        /// <param name="areas">The ids of the areas the triangles belong to.
        /// (&lt;= <see cref="NMGen.WalkableArea"/>) [Size: >= triCount]</param>
        /// <param name="triCount">The number of triangles in the vertex
        /// array.</param>
        /// <param name="flagMergeThreshold">The distance where the
        /// walkable flag is favored over the non-walkable flag. [Limit: >= 0]
        /// [Normal: 1]</param>
        /// <returns>TRUE if the operation was successful.</returns>
        public bool AddTriangles(BuildContext context
            , float[] verts
            , byte[] areas
            , int triCount
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.AddTriangles(context.root
                , verts
                , areas
                , triCount
                , root
                , flagMergeThreshold);
        }
    }
}

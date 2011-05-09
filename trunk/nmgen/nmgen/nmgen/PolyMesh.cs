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
using System.Runtime.Serialization;
using org.critterai.interop;
using org.critterai.nmgen.rcn;

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents a polygon mesh suitable for use in building a 
    /// <see cref="Navmesh"/> object.
    /// </summary>
    /// <remarks>
    /// <p>The polygon mesh is made up of convex, potentailly overlapping
    /// polygons of varying sizes.  Each polygon can have extra meta-data
    /// representing region and area membership, as well as user defined flags.
    /// </p>
    /// <p>This object is not normally built manually.  Instead it is built
    /// using one of the methods in the <see cref="NMGenUtil"/> class.</p>
    /// <p>This class is not compatible with Unity serialization.</p>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    [Serializable]
    public sealed class PolyMesh
        : ManagedObject, ISerializable
    {
        /// <summary>
        /// Represents an index that does not point to anything.
        /// </summary>
        public const ushort NullIndex = 0xffff;

        // Serialization key.
        private const string DataKey = "d";

        internal PolyMeshEx root = new PolyMeshEx();

        private int mMaxVerts = 0;
        private float mWalkableHeight = 0;
        private float mWalkableStep = 0;

        /// <summary>
        /// The number of vertices in the vertex array.
        /// </summary>
        public int VertCount { get { return root.vertCount; } }

        /// <summary>
        /// The number of polygons defined by the mesh.
        /// </summary>
        public int PolyCount { get { return root.polyCount; } }

        /// <summary>
        /// The maximum number of vertices per polygon.  Individual polygons
        /// can vary in size from three to this number of vertices.
        /// </summary>
        public int MaxVertsPerPoly { get { return root.maxVertsPerPoly; } }

        /// <summary>
        /// The world space minimum bounds of the mesh's axis-aligned
        /// bounding box. (x, y, z)
        /// </summary>
        public float[] GetBoundsMin() 
        { 
            return (float[])root.boundsMin.Clone(); 
        }

        /// <summary>
        /// The world space maximum bounds of the mesh's axis-aligned bounding
        /// box. (x, y, z)
        /// </summary>
        public float[] GetBoundsMax() 
        { 
            return (float[])root.boundsMax.Clone(); 
        }

        /// <summary>
        /// The xz-plane size of the cells that form the mesh field.
        /// </summary>
        /// <remarks>
        /// <p>See the <see cref="GetVetices"/> method for details.
        /// </p>
        /// </remarks>
        public float XZCellSize { get { return root.xzCellSize; } }

        /// <summary>
        /// The y-axis size of the cells that form the mesh field.
        /// </summary>
        /// <remarks>
        /// <p>See the <see cref="GetVetices"/> method for details.
        /// </p>
        /// </remarks>
        public float YCellSize { get { return root.yCellSize; } }

        /// <summary>
        /// The Minimum floor to 'ceiling' height used to build the polygon
        /// mesh.
        /// </summary>
        public float WalkableHeight
        {
            get { return mWalkableHeight; }
        }

        /// <summary>
        /// The maximum traversable ledge height used to build the polygon
        /// mesh.
        /// </summary>
        public float WalkableStep { get { return mWalkableStep; } }

        public int MaxVerts { get { return mMaxVerts; } }

        public int MaxPolys { get { return root.maxPolys; } }

        /// <summary>
        /// The the closest any part of the polygon mesh gets to obstructions 
        /// in the source geometry.
        /// </summary>
        public int BorderSize { get { return root.borderSize; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed 
        { 
            get { return (root.polys == IntPtr.Zero); } 
        }

        private PolyMesh(AllocType resourceType, bool initializeRoot)
            : base(resourceType)
        {
            if (initializeRoot)
                root.Initialize();
        }

        private PolyMesh(SerializationInfo info, StreamingContext context)
            : base(AllocType.External)
        {
            // Note: Version compatability is handled by the interop call;

            byte[] rawData = (byte[])info.GetValue(DataKey, typeof(byte[]));

            if (!PolyMeshEx.Build(rawData
                , rawData.Length
                , ref root
                , ref mMaxVerts
                , ref mWalkableHeight
                , ref mWalkableStep))
            {
                root.Initialize();
            }
        }

        /// <summary>
        /// Creates a polygon mesh with all buffers allocated and ready
        /// to load with data. (See: <see cref="Load"/>)
        /// </summary>
        /// <param name="maxVerts"></param>
        /// <param name="maxPolys"></param>
        /// <param name="maxVertsPerPoly"></param>
        /// <returns></returns>
        public PolyMesh(int maxVerts, int maxPolys, int maxVertsPerPoly)
            : base(AllocType.Local)
        {
            if (maxVerts < 3
                || maxPolys < 1
                || maxVertsPerPoly < 3
                || maxVertsPerPoly > NMGen.MaxAllowedVertsPerPoly)
            {
                return;
            }

            root.Initialize();

            mMaxVerts = maxVerts;
            root.maxPolys = maxPolys;
            root.maxVertsPerPoly = maxVertsPerPoly;

            int size = sizeof(ushort) * mMaxVerts * 3;
            root.verts = UtilEx.GetBuffer(size, true);

            size = sizeof(ushort) * root.maxPolys * 2
                * root.maxVertsPerPoly;
            root.polys = UtilEx.GetBuffer(size, true);

            size = sizeof(ushort) * root.maxPolys;
            root.regions = UtilEx.GetBuffer(size, true);
            root.flags = UtilEx.GetBuffer(size, true);

            size = sizeof(byte) * root.maxPolys;
            root.areas = UtilEx.GetBuffer(size, true);
        }

        public PolyMesh(byte[] serializedMesh)
            : base(AllocType.External)
        {
            if (!PolyMeshEx.Build(serializedMesh
                , serializedMesh.Length
                , ref root
                , ref mMaxVerts
                , ref mWalkableHeight
                , ref mWalkableStep))
            {
                root.Initialize();
            }
        }

        ~PolyMesh()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Frees all resources and marks the object as disposed.
        /// </summary>
        public override void RequestDisposal()
        {
            if (!IsDisposed)
            {
                if (ResourceType == AllocType.Local)
                {
                    Marshal.FreeHGlobal(root.areas);
                    Marshal.FreeHGlobal(root.flags);
                    Marshal.FreeHGlobal(root.polys);
                    Marshal.FreeHGlobal(root.regions);
                    Marshal.FreeHGlobal(root.verts);
                }
                else if (ResourceType == AllocType.External)
                    PolyMeshEx.FreeEx(ref root);

                root.Reset();
                mMaxVerts = 0;
                mWalkableHeight = 0;
                mWalkableStep = 0;

            }
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <remarks>If an optional parameter is not included the applicable 
        /// content of the associated array will be initailized to zero values.
        /// </remarks>
        /// <param name="xzCellSize">The xz-plane size of the cells that form 
        /// the mesh field.</param>
        /// <param name="yCellSize">The y-axis size of the cells that form 
        /// the mesh field.</param>
        /// <param name="maxVertsPerPoly">The maximum number of vertices 
        /// per polygon.  Individual polygons can vary in size from three 
        /// to this number of vertices.</param>
        /// <param name="minTraversableHeight">The Minimum floor to 'ceiling' 
        /// height used to build the polygon mesh.</param>
        /// <param name="maxTraversableStep">The maximum traversable ledge 
        /// height used to build the polygon mesh.</param>
        /// <param name="traversableAreaBorderSize">The the closest any part 
        /// of the polygon mesh gets to obstructions in the source geometry.
        /// </param>
        /// <param name="allocType">The resource allocation type of
        /// the polygon mesh structure.</param>
        /// <param name="boundsMin">The world space minimum bounds of the 
        /// mesh's axis-aligned bounding box. (x, y, z)</param>
        /// <param name="boundsMax">The world space maximum bounds of the 
        /// mesh's axis-aligned bounding box. (x, y, z)</param>
        /// <param name="vertices">The mesh vertices. (x, y, z) * VertexCount
        /// </param>
        /// <param name="polys">The polygon and neighbor data.
        /// (See <see cref="GetPolys"/> for details.)</param>
        /// <param name="regions">Region id for each polygon. 
        /// (Size: PolyCount) (Optional)</param>
        /// <param name="flags">User defined flags for each polygon. 
        /// (Size: PolyCount) (Optional)</param>
        /// <param name="areas">User defined area id for each polygon. 
        /// (Size: PolyCount)(Optional)</param>
        public bool Load(PolyMeshData data)
        {
            root.vertCount = (data.verts == null || data.vertCount < 3 ?
                0 : data.vertCount);
            root.polyCount = (data.polys == null || data.polyCount < 1 ?
                 0 : data.polyCount);

            if (root.polyCount == 0 || root.polyCount > root.maxPolys
                || root.vertCount == 0 || root.vertCount > mMaxVerts
                || data.xzCellSize < NMGen.MinCellSize
                || data.yCellSize < NMGen.MinCellSize
                || data.walkableHeight 
                    < NMGen.MinWalkableHeight * NMGen.MinCellSize
                || data.walkableStep < 0
                || data.borderSize < 0
                || data.polys.Length 
                    < (root.polyCount * 2 * root.maxVertsPerPoly)
                || data.verts.Length < (root.vertCount * 3)
                || data.boundsMin == null || data.boundsMin.Length < 3
                || data.boundsMax == null || data.boundsMax.Length < 3
                || (data.areas != null && data.areas.Length < root.polyCount)
                || (data.regions != null 
                    && data.regions.Length < root.polyCount)
                || (data.flags != null && data.flags.Length < root.polyCount))
            {
                return false;
            }

            root.xzCellSize = data.xzCellSize;
            root.yCellSize = data.yCellSize;
            mWalkableHeight = data.walkableHeight;
            mWalkableStep = data.walkableStep;
            root.borderSize = data.borderSize;

            for (int i = 0; i < 3; i++)
            {
                root.boundsMin[i] = data.boundsMin[i];
                root.boundsMax[i] = data.boundsMax[i];
            }

            UtilEx.Copy(data.verts, 0, root.verts, root.vertCount * 3);

            UtilEx.Copy(data.polys
                , 0
                , root.polys
                , root.polyCount * 2 * root.maxVertsPerPoly);

            if (data.regions == null)
                UtilEx.ZeroMemory(root.regions, sizeof(ushort) * root.polyCount);
            else
                UtilEx.Copy(data.regions, 0, root.regions, root.polyCount);

            if (data.flags == null)
                UtilEx.ZeroMemory(root.flags, sizeof(ushort) * root.polyCount);
            else
                UtilEx.Copy(data.flags, 0, root.flags, root.polyCount);

            if (data.areas == null)
                UtilEx.ZeroMemory(root.areas, sizeof(byte) * root.polyCount);
            else
                Marshal.Copy(data.areas, 0, root.areas, root.polyCount);

            return true;
        }

        public PolyMeshData GetData(bool includeBuffer)
        {
            int mp = (includeBuffer ? root.maxPolys : root.polyCount);
            int mv = (includeBuffer ? mMaxVerts : root.vertCount);

            PolyMeshData buffer = new PolyMeshData(mv
                , mp
                , root.maxVertsPerPoly);

            FillData(buffer);

            return buffer;
        }

        public PolyMeshData GetData(PolyMeshData buffer)
        {
            if (IsDisposed)
                return null;
            if (buffer == null)
                return GetData(true);

            if (!buffer.CanFit(root.vertCount
                , root.polyCount
                , root.maxVertsPerPoly))
            {
                buffer.Reset(mMaxVerts, root.maxPolys, root.maxVertsPerPoly);
            }

            FillData(buffer);

            return buffer;
        }

        private void FillData(PolyMeshData buffer)
        {
            buffer.maxVertsPerPoly = root.maxVertsPerPoly;
            buffer.boundsMax = GetBoundsMax();   // Auto-clone.
            buffer.boundsMin = GetBoundsMin();   // Auto-clone.
            buffer.yCellSize = root.yCellSize;
            buffer.xzCellSize = root.xzCellSize;
            buffer.polyCount = root.polyCount;
            buffer.vertCount = root.vertCount;
            buffer.walkableStep = mWalkableStep;
            buffer.walkableHeight = mWalkableHeight;
            buffer.borderSize = root.borderSize;

            int count = root.polyCount;

            UtilEx.Copy(root.polys
                , buffer.polys
                , root.polyCount * 2 * root.maxVertsPerPoly);
            Marshal.Copy(root.areas, buffer.areas, 0, root.polyCount);
            UtilEx.Copy(root.flags, buffer.flags, root.polyCount);
            UtilEx.Copy(root.regions, buffer.regions, root.polyCount);

            UtilEx.Copy(root.verts, buffer.verts, root.vertCount * 3);
        }

        public byte[] GetSerializedData(bool includeBuffer)
        {
            if (IsDisposed)
                return null;

            // Design note:  This is implemented using an interop call
            // rather than local code bacause it is much more easier to 
            // serialize in C++ than it is in C#.

            IntPtr ptr = IntPtr.Zero;
            int dataSize = 0;

            if (!PolyMeshEx.GetSerializedData(ref root
                , mMaxVerts
                , mWalkableHeight
                , mWalkableStep
                , includeBuffer
                , ref ptr
                , ref dataSize))
            {
                return null;
            }

            byte[] result = UtilEx.ExtractArrayByte(ptr, dataSize);

            PolyMeshEx.FreeSerializationData(ref ptr);

            return result;
        }

        /// <summary>
        /// Gets serialization data for the object.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        public void GetObjectData(SerializationInfo info
            , StreamingContext context)
        {
            /*
             * Design Notes:
             * 
             * Default serialization security is OK.
             * Validation and versioning is handled by GetSerializedData().
             */

            byte[] rawData = GetSerializedData(true);

            if (rawData == null)
                return;

            info.AddValue(DataKey, rawData);
        }

        ///// <summary>
        ///// Creates an empty mesh with no buffers.
        ///// </summary>
        ///// <remarks>
        ///// <p>The object is for use with external method calls.  
        ///// The external method creates and fills the object buffers.</p>
        ///// <p>The object will show as disposed until it is filled by
        ///// a method call.</p>
        ///// </remarks>
        //public static PolyMesh AllocEmpty()
        //{
        //    // The interop call will change this value as appropriate.
        //    return new PolyMesh(AllocType.Local, true);
        //}

        public static PolyMesh Build(BuildContext context
            , ContourSet contours
            , int maxVertsPerPoly
            , int walkableHeight
            , int walkableStep)
        {
            if (context == null || contours == null)
                return null;

            PolyMesh result = new PolyMesh(AllocType.External, false);

            if (!PolyMeshEx.Build(context.root
                , contours.root
                , maxVertsPerPoly
                , ref result.root
                , ref result.mMaxVerts))
            {
                return null;
            }

            result.mWalkableHeight = walkableHeight * contours.XZCellSize;
            result.mWalkableStep = walkableStep * contours.YCellSize;

            return result;
        }

        // TODO: EVAL: v0.3: Add functionality?
        // This functionality WILL be added.  The only question is when.
        // Testing is what is holding it up.

        //public static PolyMesh Build(BuildContext context
        //    , PolyMesh[] meshes
        //    , float walkableHeight
        //    , float walkableStep)
        //{
        //    if (context == null || meshes == null)
        //        return null;

        //    PolyMeshEx[] ms = new PolyMeshEx[meshes.Length];

        //    for (int i = 0; i < meshes.Length; i++)
        //    {
        //        ms[i] = meshes[i].root;
        //    }

        //    PolyMesh result = new PolyMesh(AllocType.External, false);

        //    if (!PolyMeshEx.MergeMeshes(context.root
        //        , ms
        //        , ms.Length
        //        , ref result.root
        //        , ref result.mMaxVerts))
        //    {
        //        return null;
        //    }

        //    result.mWalkableHeight = walkableHeight;
        //    result.mWalkableStep = walkableStep;

        //    return result;
        //}

        #region Removed Features

        // TODO: EVAL: v1.0 Remove or return to service.

        ///// <summary>
        ///// Returns a copy of the vertex information. (x, y, z) *
        ///// VertexCount
        ///// </summary>
        ///// <returns>
        ///// <p>Values are in voxel space.  They can be converted to world space
        ///// as follows:</p>
        ///// <p>
        ///// worldX = boundsMin[0] + x * XZCellSize
        ///// worldY = boundsMin[1] + y * YCellSize
        ///// worldZ = boundsMin[2] + z * XZCellSize
        ///// </p></returns>
        //public int GetVerts(ushort[] buffer) 
        //{
        //    if (root.vertCount > 0)
        //        UtilEx.Copy(root.verts, buffer, root.vertCount * 3);
        //    return root.vertCount;
        //}

        ///// <summary>
        ///// Returns a copy of polygon and neighbor information. 
        ///// </summary>
        ///// <remarks>
        ///// <p>Each entry is 2 * MaxVertsPerPoly in length.</p>
        ///// <p>The first half of the entry contains the indices of the polygon.
        ///// The first instance of <see cref="NullIndex"/> indicates the end of 
        ///// the indices for the entry.</p>
        ///// <p>The second half contains indices to neighbor polygons.  A
        ///// value of <see cref="NullIndex"/> indicates no connection for the 
        ///// associated edge. </p>
        ///// <p><b>Example:</b></p>
        ///// <p>
        ///// MaxVertsPerPoly = 6<br/>
        ///// For the entry: (1, 3, 4, 8, NullIndex, NullIndex, 18, NullIndex
        ///// , 21, NullIndex, NullIndex, NullIndex)</p>
        ///// <p>
        ///// (1, 3, 4, 8) defines a polygon with 4 vertices.<br />
        ///// Edge 1->3 is shared with polygon 18.<br />
        ///// Edge 4->8 is shared with polygon 21.<br />
        ///// Edges 3->4 and 4->8 are border edges not shared with any other
        ///// polygon.
        ///// </p>
        ///// </remarks>
        ///// <returns>A copy of polygon and neighbor information.</returns>
        //public int GetPolys(ushort[] buffer) 
        //{
        //    if (root.polyCount > 0)
        //    UtilEx.Copy(root.polys
        //        , buffer
        //        , root.polyCount * 2 * root.maxVertsPerPoly);
        //    return root.polyCount;
        //}

        ///// <summary>
        ///// Returns a copy of the region id for each polygon.
        ///// </summary>
        ///// <remarks>
        ///// This information is generated during the mesh build process.
        ///// </remarks>
        ///// <param name="includeBuffer">If TRUE, the entire array, including
        ///// unused buffer space, is returned.</param>
        ///// <returns>A copy of region id for each polygon.
        ///// </returns>
        //public int GetRegions(ushort[] buffer) 
        //{
        //    if (root.polyCount > 0)
        //        UtilEx.Copy(root.regions, buffer, root.polyCount);
        //    return root.polyCount;
        //}

        ///// <summary>
        ///// Returns a copy of the user-defined flags for each polygon.
        ///// </summary>
        ///// <param name="includeBuffer">If TRUE, the entire array, including
        ///// unused buffer space, is returned.</param>
        ///// <returns>A copy of the flags for each polygon.</returns>
        //public int GetFlags(ushort[] buffer) 
        //{
        //    if (root.polyCount > 0)
        //        UtilEx.Copy(root.flags, buffer, root.polyCount);
        //    return root.polyCount;
        //}

        ///// <summary>
        ///// Returns a copy of the area id for each polygon.
        ///// </summary>
        ///// <remarks>
        ///// <p>During the standard build process, all walkable polygons
        ///// get the default value of <see cref="WalkableArea"/>.  
        ///// This value can then be changed to meet user requirements.</p>
        ///// </remarks>
        ///// <param name="includeBuffer">If TRUE, the entire array, including
        ///// unused buffer space, is returned.</param>
        ///// <returns>A copy of area id for each polygon.</returns>
        //public int GetAreas(byte[] buffer)
        //{
        //    if (root.polyCount > 0)
        //        Marshal.Copy(root.areas, buffer, 0, root.polyCount);
        //    return root.polyCount;
        //}

        #endregion

    }
}

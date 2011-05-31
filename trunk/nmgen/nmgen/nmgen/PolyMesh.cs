﻿/*
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
    /// a navigation mesh.
    /// </summary>
    /// <remarks>
    /// <p>The polygon mesh is made up of convex, potentailly overlapping
    /// polygons of with varying vertex counts.  Each polygon can have extra 
    /// meta-data representing region and area membership, as well as user 
    /// defined flags.
    /// </p>
    /// <p>This object is not normally built manually.  Instead it is built
    /// using one of the standard build processes.</p>
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
        private float mWalkableRadius = 0;

        /// <summary>
        /// The number of vertices in the vertex array.
        /// </summary>
        public int VertCount { get { return root.vertCount; } }

        /// <summary>
        /// The number of polygons defined by the mesh.
        /// </summary>
        public int PolyCount { get { return root.polyCount; } }

        /// <summary>
        /// The maximum number of vertices per polygon.  
        /// </summary>
        /// <remarks>
        /// <p>Individual polygons can vary in size from three to this number
        /// of vertices.</p>
        /// </remarks>
        public int MaxVertsPerPoly { get { return root.maxVertsPerPoly; } }

        /// <summary>
        /// The world space minimum bounds of the mesh's AABB. 
        /// [Form: (x, y, z)]
        /// </summary>
        public float[] GetBoundsMin() 
        { 
            return (float[])root.boundsMin.Clone(); 
        }

        /// <summary>
        /// The world space maximum bounds of the mesh's AABB.
        /// [Form: (x, y, z)]
        /// </summary>
        public float[] GetBoundsMax() 
        { 
            return (float[])root.boundsMax.Clone(); 
        }

        /// <summary>
        /// The xz-plane size of the cells that form the mesh field.
        /// </summary>
        public float XZCellSize { get { return root.xzCellSize; } }

        /// <summary>
        /// The y-axis size of the cells that form the mesh field.
        /// </summary>
        public float YCellSize { get { return root.yCellSize; } }

        /// <summary>
        /// The minimum floor to 'ceiling' height used to build the polygon
        /// mesh.
        /// </summary>
        public float WalkableHeight { get { return mWalkableHeight; } }

        /// <summary>
        /// The radius used to erode the walkable area of the mesh.
        /// </summary>
        public float WalkableRadius { get { return mWalkableRadius; } }

        /// <summary>
        /// The maximum traversable ledge height used to build the polygon
        /// mesh.
        /// </summary>
        public float WalkableStep { get { return mWalkableStep; } }

        /// <summary>
        /// The maximum number of vertices the vertex buffer can hold.
        /// </summary>
        public int MaxVerts { get { return mMaxVerts; } }

        /// <summary>
        /// The maximum number of polygons the polygon buffer can hold.
        /// </summary>
        public int MaxPolys { get { return root.maxPolys; } }

        /// <summary>
        /// The AABB border size applied during the build of the mesh.
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
                , ref mWalkableRadius
                , ref mWalkableStep))
            {
                root.Initialize();
            }
        }

        /// <summary>
        /// Constructs an object with all buffers allocated and 
        /// ready to load with data. (See: <see cref="Load"/>)
        /// </summary>
        /// <param name="maxVerts">The maximum veritices the vertex buffer
        /// can hold.</param>
        /// <param name="maxPolys">The maximum polygons the polygon buffer
        /// can hold.</param>
        /// <param name="maxVertsPerPoly">The maximum allowed vertices for a
        /// polygon.</param>
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

        /// <summary>
        /// Constructs a polygon mesh from the data generated by
        /// the <see cref="GetSerializedData"/> method.
        /// </summary>
        /// <param name="serializedMesh">The source data.</param>
        public PolyMesh(byte[] serializedMesh)
            : base(AllocType.External)
        {
            if (!PolyMeshEx.Build(serializedMesh
                , serializedMesh.Length
                , ref root
                , ref mMaxVerts
                , ref mWalkableHeight
                , ref mWalkableRadius
                , ref mWalkableStep))
            {
                root.Initialize();
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
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
        /// Loads the data into the mesh buffers, overwriting existing content.
        /// </summary>
        /// <param name="data">The data to load.</param>
        /// <remarks>TRUE if the load was successful.</remarks>
        public bool Load(PolyMeshData data)
        {
            int vcount = (data.verts == null || data.vertCount < 3 ?
                0 : data.vertCount);
            int pcount = (data.polys == null || data.polyCount < 1 ?
                 0 : data.polyCount);

            if (pcount == 0 || pcount > root.maxPolys
                || vcount == 0 || vcount > mMaxVerts
                || data.xzCellSize < NMGen.MinCellSize
                || data.yCellSize < NMGen.MinCellSize
                || data.walkableHeight 
                    < NMGen.MinWalkableHeight * NMGen.MinCellSize
                || data.walkableStep < 0
                || data.walkableRadius < 0
                || data.borderSize < 0
                || data.polys.Length 
                    < (pcount * 2 * root.maxVertsPerPoly)
                || data.verts.Length < (vcount * 3)
                || data.boundsMin == null || data.boundsMin.Length < 3
                || data.boundsMax == null || data.boundsMax.Length < 3
                || (data.areas != null && data.areas.Length < pcount)
                || (data.regions != null 
                    && data.regions.Length < pcount)
                || (data.flags != null && data.flags.Length < pcount))
            {
                return false;
            }

            root.polyCount = pcount;
            root.vertCount = vcount;
            root.xzCellSize = data.xzCellSize;
            root.yCellSize = data.yCellSize;
            mWalkableHeight = data.walkableHeight;
            mWalkableRadius = data.walkableRadius;
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

        /// <summary>
        /// Gets the data from the mesh buffers.
        /// </summary>
        /// <remarks>This method is useful for extracting mesh data so it
        /// can be inspected or altered and reloaded.</remarks>
        /// <param name="includeBuffer">If TRUE, includes the unused
        /// buffer space.  Otherwise only the used buffer data is returned.
        /// </param>
        /// <returns>The data from the mesh buffers.</returns>
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

        /// <summary>
        /// Loads the data from the mesh buffers into the data object.
        /// </summary>
        /// <remarks>
        /// <p>If the buffer argument is null, a new buffer will be returned.
        /// If the buffer is too small it will be resized.</p>
        /// <p>Only the used portions of the mesh buffers are copied.</p>
        /// </remarks>
        /// <param name="buffer">A buffer to load the data into.</param>
        /// <returns>A reference to the mesh data.</returns>
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
                buffer.Resize(mMaxVerts, root.maxPolys, root.maxVertsPerPoly);
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
            buffer.walkableRadius = mWalkableRadius;
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

        /// <summary>
        /// Gets a serialized version of the mesh that can be used to
        /// recreate it later.
        /// </summary>
        /// <param name="includeBuffer">TRUE if serialized data should
        /// include the full buffer size.  Otherwise the buffers will
        /// be stripped and the smallest possible serialized data returned.
        /// </param>
        /// <returns>A serialized version of the mesh.</returns>
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
                , mWalkableRadius
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
        /// <remarks>Will always include the unused buffer space.
        /// (No compression.)</remarks>
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

        /// <summary>
        /// Builds polygon mesh from the provided contours.
        /// </summary>
        /// <remarks>
        /// <p>The values of the CellSize-based parameters will be converted
        /// to world units.</p>
        /// </remarks>
        /// <param name="context">The context to use for the operation.</param>
        /// <param name="contours">The contours to use to build the mesh.
        /// </param>
        /// <param name="maxVertsPerPoly">The maximum allowed vertices for
        /// a polygon.</param>
        /// <param name="walkableHeight">The walkable height used to build
        /// the contour data. [Units: CellSize (Y)]</param>
        /// <param name="walkableRadius">The radius used to erode the
        /// walkable area covered by the contours. [Units: CellSize (XZ)]</param>
        /// <param name="walkableStep">The walkable step used to build
        /// the contour data. [Units: CellSize (Y)]</param>
        /// <returns>The generated polygon mesh, or null if there were errors.
        /// </returns>
        public static PolyMesh Build(BuildContext context
            , ContourSet contours
            , int maxVertsPerPoly
            , int walkableHeight
            , int walkableRadius
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

            result.mWalkableHeight = walkableHeight * contours.YCellSize;
            result.mWalkableRadius = walkableRadius * contours.XZCellSize;
            result.mWalkableStep = walkableStep * contours.YCellSize;
            
            return result;
        }

        // TODO: EVAL: v0.3: Add functionality now?
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

    }
}

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
using org.critterai.nav.rcn.externs;
using System.Runtime.Serialization;

namespace org.critterai.nav.rcn
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
    /// <p><b>Unity Note:</b> The serialization method used by this class is not 
    /// compatible with Unity serialization.</p>
    /// <p>Behavior is undefined if an object is used after 
    /// disposal.</p>
    /// </remarks>
    [Serializable]
    public sealed class PolyMesh
        : ManagedObject, ISerializable
    {
        /// <summary>
        /// Represents an index that does not point to anything.
        /// </summary>
        public const ushort NullIndex = PolyMeshEx.NullIndex;

        /// <summary>
        /// Represents the null region, a region which is not part of the
        /// polygon mesh.  (E.g. An edge on the boundary of the mesh connects
        /// to the null region.)
        /// </summary>
        public const byte NullRegion = PolyMeshEx.NullRegion;

        /// <summary>
        /// The default area for externally built polygon meshes, indicating
        /// a walkable polygon.
        /// </summary>
        public const byte WalkableArea = PolyMeshEx.WalkableArea;

        private const string VertKey = "v";
        private const string PolyKey = "p";
        private const string RegionKey = "r";
        private const string FlagsKey = "f";
        private const string AreasKey = "a";
        private const string MaxVertPerPolyKey = "mv";
        private const string BoundsMinKey = "bi";
        private const string BoundsMaxKey = "ba";
        private const string XZCellSizeKey = "s";
        private const string YCellSizeKey = "h";
        private const string HeightKey = "th";
        private const string BorderKey = "tb";
        private const string StepKey = "ts";

        /// <summary>
        /// The root polygon mesh structure.
        /// </summary>
        internal PolyMeshEx root;

        private bool mIsDisposed = false;
        private float mMinTraversableHeight;
        private float mTraversableAreaBorderSize;
        private float mMaxTraversableStep;

        /// <summary>
        /// The Minimum floor to 'ceiling' height used to build the polygon
        /// mesh.
        /// </summary>
        public float MinTraversableHeight
        {
            get { return mMinTraversableHeight; }
            set 
            { 
                mMinTraversableHeight = 
                    Math.Max(NMGenParams.LimitTraversableHeight, value); 
            }
        }

        /// <summary>
        /// The maximum traversable ledge height used to build the polygon
        /// mesh.
        /// </summary>
        public float MaxTraversableStep
        {
            get { return mMaxTraversableStep; }
            set
            {
                mMaxTraversableStep =
                    Math.Max(NMGenParams.LimitTraversableStep, value);
            }
        }
        
        /// <summary>
        /// The the closest any part of the polygon mesh gets to obstructions 
        /// in the source geometry.
        /// </summary>
        public float TraversableAreaBorderSize
        {
            get { return mTraversableAreaBorderSize; }
            set
            { 
                mTraversableAreaBorderSize =
                    Math.Max(NMGenParams.MinBorderSize, value);
            }
        }

        /// <summary>
        /// Unsafe constructor.
        /// </summary>
        /// <param name="polyMesh">An allocated polygon mesh structure.</param>
        /// <param name="minTraversableHeight">The Minimum floor to 'ceiling' 
        /// height used to build the polygon mesh.</param>
        /// <param name="maxTraversableStep">The maximum traversable ledge 
        /// height used to build the polygon mesh.</param>
        /// <param name="traversableAreaBorderSize">The the closest any part 
        /// of the polygon mesh gets to obstructions in the source geometry.
        /// </param>
        /// <param name="allocType">The resource allocation type of
        /// the polygon mesh structure.</param>
        internal PolyMesh(PolyMeshEx polyMesh
            , float minTraversableHeight
            , float maxTraversableStep
            , float traversableAreaBorderSize
            , AllocType allocType) 
            : base(allocType)
        {
            root = polyMesh;
            MinTraversableHeight = minTraversableHeight;
            MaxTraversableStep = maxTraversableStep;
            TraversableAreaBorderSize = TraversableAreaBorderSize;
        }

        /// <summary>
        /// Manual Constructor
        /// </summary>
        /// <remarks>If optional parameters are not included the content of
        /// their associated array will be initailized with zero values.
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
        /// <param name="polygons">The polygon and neighbor data.
        /// (See <see cref="GetPolygons"/> for details.)</param>
        /// <param name="regions">Region id for each polygon. 
        /// (Size: PolygonCount) (Optional)</param>
        /// <param name="flags">User defined flags for each polygon. 
        /// (Size: PolygonCount) (Optional)</param>
        /// <param name="areas">User defined area id for each polygon. 
        /// (Size: PolygonCount)(Optional)</param>
        public PolyMesh(float xzCellSize
                , float yCellSize
                , int maxVertsPerPoly
                , float minTraversableHeight
                , float maxTraversableStep
                , float traversableAreaBorderSize
                , float[] boundsMin
                , float[] boundsMax
                , ushort[] vertices
                , ushort[] polygons
                , ushort[] regions
                , ushort[] flags
                , byte[] areaIds)
            : base(AllocType.Local)
        {
            // Extra data padding is not supported.  So polyCount
            // and maxPolyCount are set to the same value.

            int polyCount = (polygons == null ? 
                0 : polygons.Length / (maxVertsPerPoly * 2));

            root = new PolyMeshEx(xzCellSize
                , yCellSize
                , polyCount
                , polyCount
                , maxVertsPerPoly
                , boundsMin
                , boundsMax
                , vertices
                , polygons
                , regions
                , flags
                , areaIds);

            MinTraversableHeight = minTraversableHeight;
            MaxTraversableStep = maxTraversableStep;
            TraversableAreaBorderSize = traversableAreaBorderSize;
        }

        private PolyMesh(SerializationInfo info, StreamingContext context)
            : base(AllocType.Local)
        {
            if (info.MemberCount != 13)
            {
                root = new PolyMeshEx();
                mIsDisposed = true;
                return;
            }

            float xzCellSize = info.GetSingle(XZCellSizeKey);
            float yCellSize = info.GetSingle(YCellSizeKey);
            int maxVertsPerPoly = info.GetInt32(MaxVertPerPolyKey);
            mMinTraversableHeight = info.GetSingle(HeightKey);
            mMaxTraversableStep = info.GetSingle(StepKey);
            mTraversableAreaBorderSize = info.GetSingle(BorderKey);

            float[] boundsMin =
                (float[])info.GetValue(BoundsMinKey, typeof(float[]));
            float[] boundsMax =
                (float[])info.GetValue(BoundsMaxKey, typeof(float[]));

            ushort[] vertices = 
                (ushort[])info.GetValue(VertKey, typeof(ushort[]));
            ushort[] polygons = 
                (ushort[])info.GetValue(PolyKey, typeof(ushort[]));
            ushort[] regions = 
                (ushort[])info.GetValue(RegionKey, typeof(ushort[]));
            ushort[] flags = 
                (ushort[])info.GetValue(FlagsKey, typeof(ushort[]));
            byte[] areas = 
                (byte[])info.GetValue(AreasKey, typeof(byte[]));

            int polyCount = polygons.Length / (maxVertsPerPoly * 2);

            root = new PolyMeshEx(xzCellSize
                , yCellSize
                , polyCount
                , polyCount
                , maxVertsPerPoly
                , boundsMin
                , boundsMax
                , vertices
                , polygons
                , regions
                , flags
                , areas);
        }

        ~PolyMesh()
        {
            RequestDisposal();
        }

        /// <summary>
        /// The number of vertices in the vertex array.
        /// </summary>
        public int VertexCount { get { return root.VertexCount; } }

        /// <summary>
        /// The number of polygons defined by the mesh.
        /// </summary>
        public int PolygonCount { get { return root.PolygonCount; } }

        /// <summary>
        /// The maximum number of vertices per polygon.  Individual polygons
        /// can vary in size from three to this number of vertices.
        /// </summary>
        public int MaxVertsPerPoly { get { return root.MaxVertsPerPoly; } }

        /// <summary>
        /// The world space minimum bounds of the mesh's axis-aligned
        /// bounding box. (x, y, z)
        /// </summary>
        public float[] BoundsMin { get { return root.BoundsMin; } }

        /// <summary>
        /// The world space maximum bounds of the mesh's axis-aligned bounding
        /// box. (x, y, z)
        /// </summary>
        public float[] BoundsMax { get { return root.BoundsMax; } }

        /// <summary>
        /// The xz-plane size of the cells that form the mesh field.
        /// </summary>
        /// <remarks>
        /// <p>See the <see cref="GetVetices"/> method for details.
        /// </p>
        /// </remarks>
        public float XZCellSize { get { return root.XZCellSize; } }

        /// <summary>
        /// The y-axis size of the cells that form the mesh field.
        /// </summary>
        /// <remarks>
        /// <p>See the <see cref="GetVetices"/> method for details.
        /// </p>
        /// </remarks>
        public float YCellSize { get { return root.YCellSize; } }

        /// <summary>
        /// Returns a copy of the vertex information. (x, y, z) *
        /// VertexCount
        /// </summary>
        /// <returns>
        /// <p>Values are in voxel space.  They can be converted to world space
        /// as follows:</p>
        /// <p>
        /// worldX = boundsMin[0] + x * XZCellSize
        /// worldY = boundsMin[1] + y * YCellSize
        /// worldZ = boundsMin[2] + z * XZCellSize
        /// </p></returns>
        public ushort[] GetVertices() { return root.GetVertices(); }

        /// <summary>
        /// Returns a copy of polygon and neighbor information. 
        /// </summary>
        /// <remarks>
        /// <p>Each entry is 2 * MaxVertsPerPoly in length.</p>
        /// <p>The first half of the entry contains the indices of the polygon.
        /// The first instance of <see cref="NullIndex"/> indicates the end of 
        /// the indices for the entry.</p>
        /// <p>The second half contains indices to neighbor polygons.  A
        /// value of <see cref="NullIndex"/> indicates no connection for the 
        /// associated edge. </p>
        /// <p><b>Example:</b></p>
        /// <p>
        /// MaxVertsPerPoly = 6<br/>
        /// For the entry: (1, 3, 4, 8, NullIndex, NullIndex, 18, NullIndex
        /// , 21, NullIndex, NullIndex, NullIndex)</p>
        /// <p>
        /// (1, 3, 4, 8) defines a polygon with 4 vertices.<br />
        /// Edge 1->3 is shared with polygon 18.<br />
        /// Edge 4->8 is shared with polygon 21.<br />
        /// Edges 3->4 and 4->8 are border edges not shared with any other
        /// polygon.
        /// </p>
        /// </remarks>
        /// <returns>A copy of polygon and neighbor information.</returns>
        public ushort[] GetPolygons() { return root.GetPolygons(false); }

        /// <summary>
        /// Returns a copy of the region id for each polygon.
        /// </summary>
        /// <remarks>
        /// This information is generated during the mesh build process.
        /// </remarks>
        /// <param name="includeBuffer">If TRUE, the entire array, including
        /// unused buffer space, is returned.</param>
        /// <returns>A copy of region id for each polygon.
        /// </returns>
        public ushort[] GetRegions() { return root.GetRegions(false); }

        /// <summary>
        /// Returns a copy of the user-defined flags for each polygon.
        /// </summary>
        /// <param name="includeBuffer">If TRUE, the entire array, including
        /// unused buffer space, is returned.</param>
        /// <returns>A copy of the flags for each polygon.</returns>
        public ushort[] GetFlags() { return root.GetFlags(false); }

        /// <summary>
        /// Returns a copy of the area id for each polygon.
        /// </summary>
        /// <remarks>
        /// <p>During the standard build process, all walkable polygons
        /// get the default value of <see cref="WalkableArea"/>.  
        /// This value can then be changed to meet user requirements.</p>
        /// </remarks>
        /// <param name="includeBuffer">If TRUE, the entire array, including
        /// unused buffer space, is returned.</param>
        /// <returns>A copy of area id for each polygon.</returns>
        public byte[] GetAreas() { return root.GetAreas(false); }

        /// <summary>
        /// Sets the user-defined area id for each polygon.
        /// </summary>
        /// <remarks>The size of the area array may be larger than
        /// <see cref="MaxPolygons"/>.  Only the data up to MaxPolygons
        /// will be copied.</remarks>
        /// <param name="areas">The area id for each polygon. 
        /// (Size: >= PolygonCount)</param>
        public void SetAreas(byte[] areas)
        {
            root.SetAreas(areas);
        }

        /// <summary>
        /// Sets the user-defined flags for each polygon.
        /// </summary>
        /// <remarks>The size of the flags array may be larger than
        /// <see cref="MaxPolygons"/>.  Only the data up to MaxPolygons
        /// will be copied.</remarks>
        /// <param name="flags">The flags for each polygon. 
        /// (Size: >= PolygonCount)</param>
        public void SetFlags(ushort[] flags)
        {
            root.SetFlags(flags);
        }

        /// <summary>
        /// Gets a copy of the entire data structure of the mesh.
        /// </summary>
        /// <returns>A copy of the entire data structure of the mesh.
        /// </returns>
        public PolyMeshData GetData()
        {
            PolyMeshData result = new PolyMeshData();

            result.areaIds = GetAreas();
            result.boundsMax = BoundsMax;
            result.boundsMin = BoundsMin;
            result.yCellSize = YCellSize;
            result.xzCellSize = XZCellSize;
            result.flags = GetFlags();
            result.maxPolygons = PolygonCount;
            result.maxVertsPerPoly = MaxVertsPerPoly;
            result.polygonCount = PolygonCount;
            result.polygons = GetPolygons();
            result.regions = GetRegions();
            result.vertexCount = VertexCount;
            result.vertices = GetVertices();

            return result;
        }

        /// <summary>
        /// Request all unmanaged resources controlled by the object be 
        /// immediately freed and the object marked as disposed.
        /// </summary>
        /// <remarks>
        /// If the object was created using a public constructor the
        /// resources will be freed immediately.
        /// </remarks>
        public override void  RequestDisposal()
        {
            if (!mIsDisposed)
            {
                if (ResourceType == AllocType.Local)
                    PolyMeshEx.Free(ref root);
                else if (ResourceType == AllocType.External)
                    PolyMeshEx.FreeEx(ref root);
                mIsDisposed = true;
            }
        }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed { get { return mIsDisposed; } }

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
             * 
             * The serialization is not necessarily an exact duplicate of the
             * root structure.  All padding is discarded resulting in
             * root.polygonCount and root.maxPolygons becoming equal.
             * 
             * The following fields are not serialized since they can
             * be derived from other data:
             *    polygons
             *    maxPolygons
             *    vertexCount
             */

            if (mIsDisposed)
                return;

            info.AddValue(XZCellSizeKey, root.XZCellSize);
            info.AddValue(YCellSizeKey, root.YCellSize);
            info.AddValue(BoundsMinKey, root.BoundsMin);
            info.AddValue(BoundsMaxKey, root.BoundsMax);
            info.AddValue(VertKey, GetVertices());
            info.AddValue(PolyKey, GetPolygons());
            info.AddValue(RegionKey, GetRegions());
            info.AddValue(FlagsKey, GetFlags());
            info.AddValue(AreasKey, GetAreas());
            info.AddValue(MaxVertPerPolyKey, root.MaxVertsPerPoly);
            info.AddValue(HeightKey, mMinTraversableHeight);
            info.AddValue(StepKey, mMaxTraversableStep);
            info.AddValue(BorderKey, mTraversableAreaBorderSize);
        }
    }
}

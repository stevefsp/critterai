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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class does not support the extra data padding supported by
    /// RecastPolyMesh.  So maximum polygon count is always the same
    /// as the actual polygon count.
    /// </remarks>
    [Serializable]
    public sealed class PolyMesh
        : IDisposable, ISerializable
    {
        public const ushort NullIndex = PolyMeshEx.NullIndex;
        public const byte NullRegion = PolyMeshEx.NullRegion;
        public const byte WalkableArea = PolyMeshEx.WalkableArea;

        private const string VertKey = "v";
        private const string PolyKey = "p";
        private const string RegionKey = "r";
        private const string FlagsKey = "f";
        private const string AreasKey = "a";
        private const string MaxVertPerPolyKey = "mv";
        private const string BoundsMinKey = "bi";
        private const string BoundsMaxKey = "ba";
        private const string CellSizeKey = "s";
        private const string CellHeightKey = "h";
        private const string HeightKey = "th";
        private const string BorderKey = "tb";
        private const string StepKey = "ts";

        internal PolyMeshEx root;
        private bool mIsDisposed = false;
        private readonly bool mIsLocal;
        private float mMinTraversableHeight;
        private float mTraversableAreaBorderSize;
        private float mMaxTraversableStep;

        public float MinTraversableHeight
        {
            get { return mMinTraversableHeight; }
            set 
            { 
                mMinTraversableHeight = 
                    Math.Max(NMGenParams.LimitTraversableHeight, value); 
            }
        }

        public float MaxTraversableStep
        {
            get { return mMaxTraversableStep; }
            set
            {
                mMaxTraversableStep =
                    Math.Max(NMGenParams.LimitTraversableStep, value);
            }
        }

        public float TraversableAreaBorderSize
        {
            get { return mTraversableAreaBorderSize; }
            set
            {
                mTraversableAreaBorderSize =
                    Math.Max(NMGenParams.LimitBorderSize, value);
            }
        }

        internal PolyMesh(PolyMeshEx polyMesh
            , float minTraversableHeight
            , float maxTraversableStep
            , float traversableAreaBorderSize)
        {
            root = polyMesh;
            MinTraversableHeight = minTraversableHeight;
            MaxTraversableStep = maxTraversableStep;
            TraversableAreaBorderSize = TraversableAreaBorderSize;
            mIsLocal = false;
        }

        public PolyMesh(float cellSize
                , float cellHeight
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
                , byte[] areas)
        {
            // Extra data padding is not supported.  So polyCount
            // and maxPolyCount are set to the same value.

            int polyCount = (polygons == null ? 
                0 : polygons.Length / (maxVertsPerPoly * 2));

            root = new PolyMeshEx(cellSize
                , cellHeight
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

            MinTraversableHeight = minTraversableHeight;
            MaxTraversableStep = maxTraversableStep;
            TraversableAreaBorderSize = traversableAreaBorderSize;

            mIsLocal = true;

        }

        private PolyMesh(SerializationInfo info, StreamingContext context)
        {
            mIsLocal = true;

            if (info.MemberCount != 13)
            {
                root = PolyMeshEx.Empty;
                mIsDisposed = true;
                return;
            }

            float cellSize = info.GetSingle(CellSizeKey);
            float cellHeight = info.GetSingle(CellHeightKey);
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

            root = new PolyMeshEx(cellSize
                , cellHeight
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
            Dispose();
        }

        public int VertexCount { get { return root.VertexCount; } }
        public int PolygonCount { get { return root.PolygonCount; } }
        public int MaxVertsPerPoly { get { return root.MaxVertsPerPoly; } }

        public float[] BoundsMin { get { return root.BoundsMin; } }
        public float[] BoundsMax { get { return root.BoundsMax; } }

        public float CellSize { get { return root.CellSize; } }
        public float CellHeight { get { return root.CellHeight; } }

        public ushort[] GetVertices() { return root.GetVertices(); }
        public ushort[] GetPolygons() { return root.GetPolygons(false); }
        public ushort[] GetRegions() { return root.GetRegions(false); }
        public ushort[] GetFlags() { return root.GetFlags(); }
        public byte[] GetAreas() { return root.GetAreas(false); }

        public PolyMeshData GetData()
        {
            PolyMeshData result = new PolyMeshData();

            result.areas = GetAreas();
            result.boundsMax = BoundsMax;
            result.boundsMin = BoundsMin;
            result.cellHeight = CellHeight;
            result.cellSize = CellSize;
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

        public void  Dispose()
        {
            if (!mIsDisposed)
            {
                if (mIsLocal)
                    PolyMeshEx.Free(ref root);
                else
                    PolyMeshEx.FreeEx(ref root);
                mIsDisposed = true;
            }
        }

        public bool IsDisposed { get { return mIsDisposed; } }

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

            info.AddValue(CellSizeKey, root.CellSize);
            info.AddValue(CellHeightKey, root.CellHeight);
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

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
using org.critterai.nav.rcn;
using org.critterai.interop;

namespace org.critterai.nav
{
    /// <summary>
    /// A tile within a <see cref="Navmesh"/>
    /// </summary>
    /// <remarks>
    /// <p>Tiles always exist within the context of a <see cref="Navmesh"/>
    /// object.</p>
    /// <p>Tiles returned by a <see cref="Navmesh"/> are not guarenteed to
    /// be populated. (The tile at a location may have been removed.)
    /// To determine if a tile is active, check the polygon count in the 
    /// <see cref="NavmeshTileHeader"/>.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavmeshTile
    {
        private Navmesh mOwner;
        private IntPtr mTile;

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        /// <remarks>
        /// <p>Just because a tile has no polygons does not mean it is
        /// disposed.</p></remarks>
        public bool IsDisposed { get { return mOwner.IsDisposed; } }

        internal NavmeshTile(Navmesh owner, IntPtr tile) 
        {
            mOwner = owner;
            mTile = tile;
        }

        /// <summary>
        /// The reference id of the tile.
        /// </summary>
        /// <returns>The refernce id of the tile.</returns>
        public uint GetTileRef() 
        {
            if (mOwner.IsDisposed)
                return 0;
            return NavmeshTileEx.GetTileRef(mOwner.root, mTile);
        }

        /// <summary>
        /// Gets the reference id of the base polygon in the tile.
        /// </summary>
        /// <returns>The reference of the base polygon.</returns>
        public uint GetBasePolyRef()
        {
            if (mOwner.IsDisposed)
                return 0;
            return NavmeshTileEx.GetBasePolyRef(mOwner.root, mTile);
        }

        /// <summary>
        /// Gets the size of the buffer required by the
        /// <see cref="GetState"/> method.
        /// </summary>
        /// <returns>The size of the state data.</returns>
        public int GetStateSize()
        {
            if (mOwner.IsDisposed)
                return 0;
            return NavmeshTileEx.GetTileStateSize(mOwner.root, mTile);
        }

        /// <summary>
        /// Gets the non-structural state of the tile. (Flags, area ids, etc.)
        /// </summary>
        /// <remarks>
        /// <p>The state data is only valid until the tile reference changes.
        /// </p>
        /// </remarks>
        /// <param name="buffer">The buffer to load the state into.
        /// [Size: >= <see cref="GetStateSize"/>]</param>
        /// <returns></returns>
        public NavStatus GetState(byte[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshTileEx.GetTileState(mOwner.root
                , mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Sets the non-structural state defined by the state data.
        /// (Obtained from the <see cref="GetState"/> method.)
        /// </summary>
        /// <param name="stateData">The state data to apply.</param>
        /// <returns></returns>
        public NavStatus SetState(byte[] stateData)
        {
            if (mOwner.IsDisposed || stateData == null)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshTileEx.SetTileState(mOwner.root
                , mTile
                , stateData
                , stateData.Length);
        }

        /// <summary>
        /// Gets the tile header.
        /// </summary>
        /// <returns>The tile header.</returns>
        public NavmeshTileHeader GetHeader()
        {
            if (mOwner.IsDisposed)
            {
                return new NavmeshTileHeader();
            }

            IntPtr header = NavmeshTileEx.GetTileHeader(mTile);

            if (header == IntPtr.Zero)
            {
                return new NavmeshTileHeader();
            }

            return (NavmeshTileHeader)
                    Marshal.PtrToStructure(header, typeof(NavmeshTileHeader));
        }

        /// <summary>
        /// Gets a copy of the polygon buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: >= <see cref="NavmeshTileHeader.polyCount"/>]</param>
        /// <returns>The number of polygons returned.</returns>
        public int GetPolys(NavmeshPoly[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTilePolys(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets a copy of the vertex buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: >= <see cref="NavmeshTileHeader.vertCount"/>]</param>
        /// <returns>The number of vertices returned.</returns>
        public int GetVerts(float[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileVerts(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets a copy of the detailed vertex buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: >= <see cref="NavmeshTileHeader.detailVertCount"/>]</param>
        /// <returns>The number of vertices returned.</returns>
        public int GetDetailVerts(float[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileDetailVerts(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets a copy of the detail triangle buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: >= <see cref="NavmeshTileHeader.detailTriCount"/>]</param>
        /// <returns>The number of triangles returned.</returns>
        public int GetDetailTris(byte[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileDetailTris(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets a copy of the detail mesh buffer buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: >= <see cref="NavmeshTileHeader.detailMeshCount"/>]</param>
        /// <returns>The number of meshes returned.</returns>
        public int GetDetailMeshes(NavmeshDetailMesh[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileDetailMeshes(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets a copy of the link buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: >= <see cref="NavmeshTileHeader.maxLinkCount"/>]</param>
        /// <returns>The number of links returned.</returns>
        public int GetLinks(NavmeshLink[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileLinks(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets a copy of the <see cref="NavmeshBVNode"/> tree.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: >= <see cref="NavmeshTileHeader.bvNodeCount"/>]</param>
        /// <returns>The number of nodes returned.</returns>
        public int GetBVTree(NavmeshBVNode[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileBVTree(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets a copy of the off-mesh connection buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: >= <see cref="NavmeshTileHeader.connCount"/>]</param>
        /// <returns>The number of connections returned.</returns>
        public int GetConnections(NavmeshConnection[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileConnections(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets the reference id of the polygon based on its polygon 
        /// index within a tile.
        /// </summary>
        /// <param name="basePolyRef">The reference id of the tile's base 
        /// poygon. (<see cref="GetBasePolyRef"/>)</param>
        /// <param name="polyIndex">The polygon's index within the tile.</param>
        /// <returns>The reference id of the polygon.</returns>
        public static uint GetPolyRef(uint basePolyRef, int polyIndex)
        {
            return (basePolyRef | (uint)polyIndex);
        }
    }
}

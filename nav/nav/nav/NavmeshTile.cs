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
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavmeshTile
    {
        private Navmesh mOwner;
        private IntPtr mTile;

        internal NavmeshTile(Navmesh owner, IntPtr tile) 
        {
            mOwner = owner;
            mTile = tile;
        }

        public bool IsDisposed { get { return mOwner.IsDisposed; } }

        public uint GetTileRef() 
        {
            if (mOwner.IsDisposed)
                return 0;
            return NavmeshTileEx.GetTileRef(mOwner.root, mTile);
        }

        public uint GetBasePolyRef()
        {
            if (mOwner.IsDisposed)
                return 0;
            return NavmeshTileEx.GetBasePolyRef(mOwner.root, mTile);
        }

        public int GetStateSize()
        {
            if (mOwner.IsDisposed)
                return 0;
            return NavmeshTileEx.GetTileStateSize(mOwner.root, mTile);
        }

        public NavStatus GetState(byte[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshTileEx.GetTileState(mOwner.root
                , mTile
                , buffer
                , buffer.Length);
        }

        public NavStatus SetState(byte[] stateData)
        {
            if (mOwner.IsDisposed || stateData == null)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshTileEx.SetTileState(mOwner.root
                , mTile
                , stateData
                , stateData.Length);
        }

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

        public int GetPolys(NavmeshPoly[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTilePolys(mTile
                , buffer
                , buffer.Length);
        }

        public int GetVerts(float[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileVerts(mTile
                , buffer
                , buffer.Length);
        }

        public int GetDetailVerts(float[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileDetailVerts(mTile
                , buffer
                , buffer.Length);
        }

        public int GetDetailTris(byte[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileDetailTris(mTile
                , buffer
                , buffer.Length);
        }

        public int GetDetailMeshes(NavmeshDetailMesh[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileDetailMeshes(mTile
                , buffer
                , buffer.Length);
        }

        public int GetLinks(NavmeshLink[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileLinks(mTile
                , buffer
                , buffer.Length);
        }

        public int GetBVTree(NavmeshBVNode[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileBVTree(mTile
                , buffer
                , buffer.Length);
        }

        public int GetConnections(NavmeshConnection[] buffer)
        {
            if (mOwner.IsDisposed || buffer == null)
                return 0;

            return NavmeshTileEx.GetTileConnections(mTile
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Gets the polygon id based on its polygon index within a tile.
        /// </summary>
        /// <param name="basePolyRef">The base polygon id for the tile.
        /// (tile.basePolyRef)</param>
        /// <param name="polyIndex">The polygon's index within the tile.</param>
        /// <returns>The status flags for the request.</returns>
        public static uint GetPolyRef(uint basePolyRef, int polyIndex)
        {
            return (basePolyRef | (uint)polyIndex);
        }
    }
}

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
using org.critterai.nav.rcn;
using org.critterai.interop;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    /// <summary>
    /// A navigation mesh based on convex polygons.
    /// </summary>
    /// <remarks>
    /// <p>This class is usually used in conjunction with the
    /// <see cref="NavmeshQuery"/> class.</p>
    /// <p>Tile and Polygon Reference Ids: Reference ids are essentially 
    /// 'handles' to data in a certain structural state. If a tile's structure
    /// changes, all associated refrences become invalid.  Serialization
    /// does not impact references. Nor do changes to area and flag state.</p>
    /// <p>Technically, all navigation meshes are tiled.  A 'solo' mesh
    /// is simply a navigation mesh configured to have only a single tile.</p>
    /// <p>Most object references returned by this class cannot
    /// be compared for equality.  I.e. mesh.GetTile(0) != mesh.GetTile(0).
    /// The object state may be equal.  But the references are not.
    /// </p>
    /// <p>This class does not implement any asynchronous methods.  So the
    /// result status of all methods will contain a success or failure flag.
    /// </p>
    /// <p>This class is not compatible with Unity serialization. Manual
    /// serialization can be implemented using the 
    /// <see cref="GetSerializedMesh"/> mesh method.</p>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    [Serializable]
    public sealed class Navmesh
        : ManagedObject, ISerializable
    {
        // Local version is not needed.  Detour includes its own mesh 
        // versioning.
        // private const long ClassVersion = 1L;

        private const string DataKey = "d";

        /// <summary>
        /// A flag that indicates a link is external.  (Context dependant.)
        /// </summary>
        public const ushort ExternalLink = 0x8000;

        /// <summary>
        /// Indicates that a link is null. (Does not link to anything.)
        /// </summary>
        public const uint NullLink = 0xffffffff;

        /// <summary>
        /// The maximum supported vertices per polygon.
        /// </summary>
        public const int MaxAllowedVertsPerPoly = 6;

        /// <summary>
        /// The maximum supported number of areas.
        /// </summary>
        public const int MaxAreas = 64;

        /// <summary>
        /// The reference id for a null polygon. (Does not exist.)
        /// </summary>
        public const uint NullPoly = 0;

        /// <summary>
        /// The reference id for a null tile. (Does not exist.)
        /// </summary>
        public const uint NullTile = 0;

        /// <summary>
        /// Represents an polygon index that does not point to anything.
        /// </summary>
        public const ushort NullIndex = 0xffff;

        /// <summary>
        /// dtNavMesh object.
        /// </summary>
        internal IntPtr root;

        private Navmesh(IntPtr mesh)
            : base(AllocType.External)
        {
            root = mesh;
        }

        private Navmesh(SerializationInfo info, StreamingContext context)
            : base(AllocType.External)
        {
            root = IntPtr.Zero;

            if (info.MemberCount != 1)
                return;
            
            byte[] data = (byte[])info.GetValue(DataKey, typeof(byte[]));
            NavmeshEx.BuildMesh(data, data.Length, ref root);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Navmesh()
        {
            RequestDisposal();
        }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Request all resources controlled by the object be 
        /// immediately freed and the object marked as disposed.
        /// </summary>
        public override void RequestDisposal()
        {
            if (root != IntPtr.Zero)
            {
                NavmeshEx.FreeEx(ref root);
                root = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Returns the configuration parameters used to initialize the
        /// navigation mesh.
        /// </summary>
        /// <returns>The configuration parameters used to initialize the
        /// navigation mesh.</returns>
        public NavmeshParams GetConfig()
        {
            NavmeshParams result = new NavmeshParams();
            NavmeshEx.GetParams(root, result);
            return result;
        }

        /// <summary>
        /// Adds a tile to the navigation mesh.
        /// </summary>
        /// <param name="tileData">The tile data.</param>
        /// <param name="desiredTileRef">The desired reference for the tile.
        /// </param>
        /// <param name="resultTileRef">The actual reference assigned to the
        /// tile.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus AddTile(NavmeshTileData tileData
            , uint desiredTileRef
            , out uint resultTileRef)
        {
            if (tileData == null
                || tileData.IsOwned
                || tileData.Size == 0)
            {
                resultTileRef = 0;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            resultTileRef = 0;

            return NavmeshEx.AddTile(root
                , tileData
                , desiredTileRef
                , ref resultTileRef);
        }

        /// <summary>
        /// Removes the specified tile from the mesh.
        /// </summary>
        /// <param name="tileRef">The tile reference.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus RemoveTile(uint tileRef)
        {
            return NavmeshEx.RemoveTile(root, tileRef);
        }

        /// <summary>
        /// Derives the location based on the provided position in world space.
        /// </summary>
        /// <param name="position">Position [Form: (x, y, z)]</param>
        /// <param name="x">The tile's grid x-location.</param>
        /// <param name="z">The tiles's grid z-location.</param>
        public void DeriveTileLocation(float[] position
            , out int x
            , out int z)
        {
            x = 0;
            z = 0;
            NavmeshEx.DeriveTileLocation(root, position, ref x, ref z);
        }

        /// <summary>
        /// Gets the tile at the specified grid location.
        /// </summary>
        /// <param name="x">The tile's grid x-location.</param>
        /// <param name="z">The tiles's grid z-location.</param>
        /// <param name="layer">The tiles layer.</param>
        /// <returns></returns>
        public NavmeshTile GetTile(int x, int z, int layer)
        {
            IntPtr tile = NavmeshEx.GetTile(root, x, z, layer);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }

        /// <summary>
        /// Gets all tiles at the specified grid location.  (All layers.)
        /// </summary>
        /// <param name="x">The tile's grid x-location.</param>
        /// <param name="z">The tiles's grid z-location.</param>
        /// <param name="buffer">The result tiles.</param>
        /// <returns>The number of tiles returned in the buffer.</returns>
        public int GetTiles(int x, int z, NavmeshTile[] buffer)
        {
            IntPtr[] tiles = new IntPtr[buffer.Length];

            int tileCount = NavmeshEx.GetTiles(root, x, z, tiles, tiles.Length);

            for (int i = 0; i < tileCount; i++)
            {
                buffer[i] = new NavmeshTile(this, tiles[i]);
            }

            return tileCount;
        }

        /// <summary>
        /// Gets the reference id for the tile at the specified grid location.
        /// </summary>
        /// <param name="x">The tile's grid x-location.</param>
        /// <param name="z">The tiles's grid z-location.</param>
        /// <param name="layer">The tiles layer.</param>
        /// <returns>The reference id, or zero if there is no tile at the
        /// location.</returns>
        public uint GetTileRef(int x, int z, int layer)
        {
            return NavmeshEx.GetTileRef(root, x, z, layer);
        }

        /// <summary>
        /// Gets a tile using its reference id.
        /// </summary>
        /// <param name="tileRef">The reference id of the tile.</param>
        /// <returns>The tile, or null if none was found.</returns>
        public NavmeshTile GetTileByRef(uint tileRef)
        {
            IntPtr tile = NavmeshEx.GetTileByRef(root, tileRef);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }

        /// <summary>
        /// The maximum number of tiles supported by the navigation mesh.
        /// </summary>
        /// <returns>The maximum number of tiles supported by the navigation 
        /// mesh.</returns>
        public int GetMaxTiles()
        {
            return NavmeshEx.GetMaxTiles(root);
        }

        /// <summary>
        /// Gets a tile from the tile buffer.
        /// </summary>
        /// <param name="tileIndex">The index of the tile.
        /// [Limits: 0 &lt;= index &lt; <see cref="GetMaxTiles"/>]
        /// </param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavmeshTile GetTile(int tileIndex)
        {
            IntPtr tile = NavmeshEx.GetTile(root, tileIndex);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }

        /// <summary>
        /// Gets a polygon and its tile.
        /// </summary>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="tile">The tile the polygon belongs to.</param>
        /// <param name="poly">The polygon.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus GetTileAndPoly(uint polyRef
            , out NavmeshTile tile
            , out NavmeshPoly poly)
        {
            IntPtr pTile = IntPtr.Zero;
            IntPtr pPoly = IntPtr.Zero;

            NavStatus status = NavmeshEx.GetTileAndPoly(root
                , polyRef
                , ref pTile
                , ref pPoly);

            if (NavUtil.Succeeded(status))
            {
                tile = new NavmeshTile(this, pTile);
                poly = (NavmeshPoly)Marshal.PtrToStructure(pPoly
                    , typeof(NavmeshPoly));
            }
            else
            {
                tile = null;
                poly = new NavmeshPoly();
            }

            return status;

        }

        /// <summary>
        /// Indicates whether or not the specified polygon reference is valid 
        /// for the navigation mesh.
        /// </summary>
        /// <param name="polyRef">The reference id to check.</param>
        /// <returns>TRUE if the provided reference id valid.</returns>
        public bool IsValidPolyRef(uint polyRef)
        {
            return NavmeshEx.IsValidPolyRef(root, polyRef);
        }

        /// <summary>
        /// Gets the endpoints for an off-mesh connection, ordered by
        /// 'direction of travel'.
        /// </summary>
        /// <remarks>
        /// <p>Off-mesh connections are stored in the navigation mesh as
        /// special 2-vertex polygons with a single edge.  At least one of the
        /// vertices is expected to be inside a normal polygon. So an off-mesh
        /// connection is "entered" from a normal polygon at one of its 
        /// endpoints. This is the polygon identified by startPolyRef.</p>
        /// </remarks>
        /// <param name="startPolyRef">The polygon id in which the
        /// start endpoint lies.</param>
        /// <param name="connectionPolyRef">The off-mesh connection's reference 
        /// id.</param>
        /// <param name="startPoint">The start point buffer. (Out)
        /// [Form: (x, y, z)]</param>
        /// <param name="endPoint">The end point buffer. (Out)
        /// [Form: (x, y, z)]</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus GetConnectionEndpoints(uint startPolyRef
            , uint connectionPolyRef
            , float[] startPoint
            , float[] endPoint)
        {
            return NavmeshEx.GetConnEndpoints(root
                , startPolyRef
                , connectionPolyRef
                , startPoint
                , endPoint);
        }

        /// <summary>
        /// Gets an off-mesh connection.
        /// </summary>
        /// <param name="polyRef">The reference id of the off-mesh
        /// connection.</param>
        /// <returns>The off-mesh connection.</returns>
        public NavmeshConnection GetConnectionByRef(uint polyRef)
        {
            IntPtr conn = NavmeshEx.GetConnectionByRef(root, polyRef);

            NavmeshConnection result;
            if (conn == IntPtr.Zero)
                result = new NavmeshConnection();
            else
                result = (NavmeshConnection)Marshal.PtrToStructure(conn
                    , typeof(NavmeshConnection));

            return result;
        }

        /// <summary>
        /// Returns the flags for the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="flags">The polygon flags.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus GetPolyFlags(uint polyRef, out ushort flags)
        {
            flags = 0;
            return NavmeshEx.GetPolyFlags(root, polyRef, ref flags);
        }

        /// <summary>
        /// Sets the flags for the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="flags">The polygon flags.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus SetPolyFlags(uint polyRef, ushort flags)
        {
            return NavmeshEx.SetPolyFlags(root, polyRef, flags);
        }

        /// <summary>
        /// Returns the area id of the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="area">The area id of the polygon.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus GetPolyArea(uint polyRef, out byte area)
        {
            area = 0;
            return NavmeshEx.GetPolyArea(root, polyRef, ref area);
        }

        /// <summary>
        /// Sets the area id of the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference id of the polygon.</param>
        /// <param name="area">The area id of the polygon.
        /// [Limit: &lt; <see cref="Navmesh.MaxAreas"/>]</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus SetPolyArea(uint polyRef, byte area)
        {
            return NavmeshEx.SetPolyArea(root, polyRef, area);
        }

        /// <summary>
        /// Gets a serialized version of the mesh.
        /// </summary>
        /// <returns>The serialized mesh.</returns>
        public byte[] GetSerializedMesh()
        {
            if (IsDisposed)
                return null;

            IntPtr data = IntPtr.Zero;
            int dataSize = 0;

            NavmeshEx.GetSerializedNavmesh(root, ref data, ref dataSize);

            if (dataSize == 0)
                return null;

            byte[] resultData = UtilEx.ExtractArrayByte(data, dataSize);

            NavmeshEx.FreeSerializedNavmeshEx(ref data);

            return resultData;
        }

        /// <summary>
        /// Gets serialization data for the object.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        public void GetObjectData(SerializationInfo info
            , StreamingContext context)
        {
            if (IsDisposed)
                return;

            // info.AddValue(VersionKey, ClassVersion);
            info.AddValue(DataKey, GetSerializedMesh());
        }

        /// <summary>
        /// Builds a single tile navigation mesh from the provided
        /// data.
        /// </summary>
        /// <param name="buildData">The tile build data.</param>
        /// <param name="resultMesh">The result mesh.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public static NavStatus Build(NavmeshTileBuildData buildData
            , out Navmesh resultMesh)
        {
            IntPtr navMesh = IntPtr.Zero;

            NavStatus status = NavmeshEx.BuildMesh(buildData
                , ref navMesh);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(navMesh);
            else
                resultMesh = null;

            return status;
        }

        /// <summary>
        /// Builds a navigation mesh from data created by the 
        /// <see cref="GetSerializedMesh"/> method.
        /// </summary>
        /// <param name="serializedMesh">The serialized mesh.</param>
        /// <param name="resultMesh">The result mesh.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public static NavStatus Build(byte[] serializedMesh
            , out Navmesh resultMesh)
        {
            if (serializedMesh == null || serializedMesh.Length < 1)
            {
                resultMesh = null;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            IntPtr root = IntPtr.Zero;

            NavStatus status = NavmeshEx.BuildMesh(serializedMesh
                , serializedMesh.Length
                , ref root);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(root);
            else
                resultMesh = null;

            return status;
        }

        /// <summary>
        /// Builds an initialized navigation mesh ready for tiles to be added.
        /// </summary>
        /// <remarks>
        /// This is the build method to use when creating new multi-tile meshes.
        /// </remarks>
        /// <param name="config">The mesh configuration.</param>
        /// <param name="resultMesh">The result mesh.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public static NavStatus Build(NavmeshParams config
            , out Navmesh resultMesh)
        {
            if (config == null || config.maxTiles < 1)
            {
                resultMesh = null;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            IntPtr root = IntPtr.Zero;

            NavStatus status = NavmeshEx.BuildMesh(config, ref root);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(root);
            else
                resultMesh = null;

            return status;
        }
    }
}

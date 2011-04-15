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
#ifndef CAIRCN_DETOURNAVMESHEX_H
#define CAIRCN_DETOURNAVMESHEX_H

#include "DetourNavMesh.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

struct rcnTileInfo
{
    // Ease of maintenance takes precidence over storage efficiency 
    // for this structure. So Order as follows:
    //    1. dtMeshHeader fields.
    //    2. dtMeshTile fields with pointers removed.
    //    3. Any other custom fields.

    // dtMeshHeader fields.
	int magic;								// Magic number, used to identify the data.
	int version;							// Data version number.
	int x, y, layer;						// Location of the tile on the grid.
	unsigned int userId;					// User ID of the tile.
	int polyCount;							// Number of polygons in the tile.
	int vertCount;							// Number of vertices in the tile.
	int maxLinkCount;						// Number of allocated links.
	int detailMeshCount;					// Number of detail meshes.
	int detailVertCount;					// Number of detail vertices.
	int detailTriCount;						// Number of detail triangles.
	int bvNodeCount;						// Number of BVtree nodes.
	int offMeshConCount;					// Number of Off-Mesh links.
	int offMeshBase;						// Index to first polygon which is Off-Mesh link.
	float walkableHeight;					// Height of the agent.
	float walkableRadius;					// Radius of the agent
	float walkableClimb;					// Max climb height of the agent.
	float bmin[3], bmax[3];					// Bounding box of the tile.
	float bvQuantFactor;					// BVtree quantization factor (world to bvnode coords)

    // dtMeshTile fields.
	unsigned int salt;						// Counter describing modifications to the tile.
	unsigned int linksFreeList;				// Index to next free link.
	int dataSize;							// Size of the tile data.
	int flags;								// Tile flags, see dtTileFlags.

    // Extra fields.
    int index;
    unsigned int basePolyRef;

    bool load(const int tileIndex
        , const dtMeshTile* tile
        , const dtNavMesh* mesh);
};

struct rcnPolyInfo
{
	float verts[DT_VERTS_PER_POLYGON*3];	// 
	unsigned short flags;						// Flags (see dtPolyFlags).
	unsigned char vertCount;					// Number of vertices.
	unsigned char areaAndtype;					// Bit packed: Area ID of the polygon, and Polygon type.
};

extern "C"
{
    EXPORT_API void freeDTNavMesh(dtNavMesh** pNavMesh);

    EXPORT_API void dtnmGetParams(const dtNavMesh* pNavMesh
        , dtNavMeshParams* params);

    EXPORT_API int dtnmGetMaxTiles(const dtNavMesh* pNavMesh);

    EXPORT_API bool dtnmIsValidPolyRef(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef);

    EXPORT_API dtStatus dtnmGetTileInfo(const dtNavMesh* pNavMesh
            , const int tileIndex
            , rcnTileInfo* tileInfo);

    EXPORT_API dtStatus dtnmGetPolyFlags(const dtNavMesh* pNavMesh
            , const dtPolyRef polyRef
            , unsigned short* flags);

    EXPORT_API dtStatus dtnmSetPolyFlags(dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned short flags);

    EXPORT_API dtStatus dtnmGetPolyArea(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned char* area);

    EXPORT_API dtStatus dtnmSetPolyArea(dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned char area);

    EXPORT_API dtStatus dtnmGetTilePolys(const dtNavMesh* mesh
        , const int tileIndex
        , dtPoly* polys
        , int* polyCount
        , const int polySize);

    EXPORT_API dtStatus dtnmGetTileLinks(const dtNavMesh* mesh
        , const int tileIndex
        , dtLink* links
        , int* linkCount
        , const int linkSize);

    EXPORT_API dtStatus dtnmGetTileVerts(const dtNavMesh* mesh
        , const int tileIndex
        , float* verts
        , int* vertCount
        , const int vertsSize);

    EXPORT_API dtStatus dtnmGetTileDetailMeshes(const dtNavMesh* mesh
        , const int tileIndex
        , dtPolyDetail* meshes
        , int* meshCount
        , const int meshesSize);

    EXPORT_API dtStatus dtnmGetTileDetailVerts(const dtNavMesh* mesh
        , const int tileIndex
        , float* verts
        , int* vertCount
        , const int vertsSize);

    EXPORT_API dtStatus dtnmGetTileDetailTris(const dtNavMesh* mesh
        , const int tileIndex
        , unsigned char* tris
        , int* trisCount
        , const int trisSize);

    EXPORT_API dtStatus dtnmGetTileBVTree(const dtNavMesh* mesh
        , const int tileIndex
        , dtBVNode* nodes
        , int* nodeCount
        , const int nodesSize);

    EXPORT_API dtStatus dtnmGetTileConnections(const dtNavMesh* mesh
        , const int tileIndex
        , dtOffMeshConnection* conns
        , int* connCount
        , const int connsSize);

    EXPORT_API dtStatus dtnmGetConnectionEndPoints(
        const dtNavMesh* pNavMesh
        , const dtPolyRef prevRef
        , const dtPolyRef polyRef
        , float* startPos
        , float* endPos);
}


#endif
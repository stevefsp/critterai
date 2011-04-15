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
#include <string>
#include "DetourNavMeshEx.h"
#include "DetourCommon.h"

bool rcnTileInfo::load(const int tileIndex
    , const dtMeshTile* tile
    , const dtNavMesh* mesh)
{
    if (!tile || !mesh)
        return false;

    // TODO: Optimize with some memcpy blocks?

    dtMeshHeader* header = tile->header;

    magic = header->magic;
    version = header->version;
    x = header->x;
    y = header->y;
    layer = header->layer;
    userId = header->userId;
    polyCount = header->polyCount;
    vertCount = header->vertCount;
    maxLinkCount = header->maxLinkCount;
    detailMeshCount = header->detailMeshCount;
    detailVertCount = header->detailVertCount;
    detailTriCount = header->detailTriCount;
    bvNodeCount = header->bvNodeCount;
    offMeshConCount = header->offMeshConCount;
    offMeshBase = header->offMeshBase;
    walkableHeight = header->walkableHeight;
    walkableRadius = header->walkableRadius;
    walkableClimb = header->walkableClimb;

    dtVcopy(bmin, header->bmin);
    dtVcopy(bmax, header->bmax);
    bvQuantFactor = header->bvQuantFactor;

    salt = tile->salt;
    linksFreeList = tile->linksFreeList;
    dataSize = tile->dataSize;
    flags = tile->flags;

    index = tileIndex;
    basePolyRef = mesh->getPolyRefBase(tile);

    return true;
}

extern "C"
{
    EXPORT_API void freeDTNavMesh(dtNavMesh** pNavMesh)
    {
        dtFreeNavMesh(*pNavMesh);
    }

    EXPORT_API void dtnmGetParams(const dtNavMesh* pNavMesh
        , dtNavMeshParams* params)
    {
        if (!pNavMesh)
            return;

        const dtNavMeshParams* lparams = pNavMesh->getParams();

        params->maxPolys = lparams->maxPolys;
        params->maxTiles = lparams->maxTiles;
        params->tileHeight = lparams->tileHeight;
        params->tileWidth = lparams->tileWidth;

        dtVcopy(params->orig, lparams->orig);
    }

    EXPORT_API int dtnmGetMaxTiles(const dtNavMesh* pNavMesh)
    {
        if (!pNavMesh)
            return -1;

        return pNavMesh->getMaxTiles();
    }

    EXPORT_API bool dtnmIsValidPolyRef(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef)
    {
        if (!pNavMesh)
            return false;

        return pNavMesh->isValidPolyRef(polyRef);
    }

    EXPORT_API dtStatus dtnmGetConnectionEndPoints(
        const dtNavMesh* pNavMesh
        , const dtPolyRef prevRef
        , const dtPolyRef polyRef
        , float* startPos
        , float* endPos)
    {
        if (!pNavMesh)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->getOffMeshConnectionPolyEndPoints(
            prevRef
            , polyRef
            , startPos
            , endPos);
    }

    EXPORT_API dtStatus dtnmGetTileInfo(const dtNavMesh* pNavMesh
        , const int tileIndex
        , rcnTileInfo* tileInfo)
    {
        if (!pNavMesh || !tileInfo
                || tileIndex < 0
                || tileIndex >= pNavMesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = pNavMesh->getTile(tileIndex);
        tileInfo->load(tileIndex, tile, pNavMesh);

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetPolyFlags(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned short* flags)
    {
        if (!pNavMesh || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->getPolyFlags(polyRef, flags);
    }

    EXPORT_API dtStatus dtnmSetPolyFlags(dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned short flags)
    {
        if (!pNavMesh || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->setPolyFlags(polyRef, flags);
    }

    EXPORT_API dtStatus dtnmGetPolyArea(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned char* area)
    {
        if (!pNavMesh || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->getPolyArea(polyRef, area);
    }

    EXPORT_API dtStatus dtnmSetPolyArea(dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned char area)
    {
        if (!pNavMesh || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->setPolyArea(polyRef, area);
    }

    EXPORT_API dtStatus dtnmGetTilePolys(const dtNavMesh* mesh
        , const int tileIndex
        , dtPoly* polys
        , int* polyCount
        , const int polySize)
    {
        if (!mesh || !polys
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
        {
            *polyCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->polyCount;

        if (polySize < count)
        {
            *polyCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        memcpy(polys, tile->polys, sizeof(dtPoly) * count);
        *polyCount = count;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileLinks(const dtNavMesh* mesh
        , const int tileIndex
        , dtLink* links
        , int* linkCount
        , const int linkSize)
    {
        if (!mesh || !links
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
        {
            *linkCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->maxLinkCount;

        if (linkSize < count)
        {
            *linkCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        memcpy(links, tile->links, sizeof(dtLink) * count);
        *linkCount = count;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileVerts(const dtNavMesh* mesh
        , const int tileIndex
        , float* verts
        , int* vertCount
        , const int vertsSize)
    {
        if (!mesh || !verts
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
        {
            *vertCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->vertCount * 3;

        if (vertsSize < count)
        {
            *vertCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        memcpy(verts, tile->verts, sizeof(float) * count);
        *vertCount = tile->header->vertCount;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileDetailMeshes(const dtNavMesh* mesh
        , const int tileIndex
        , dtPolyDetail* meshes
        , int* meshCount
        , const int meshesSize)
    {
        if (!mesh || !meshes
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
        {
            *meshCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->detailMeshCount;

        if (meshesSize < count)
        {
            *meshCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        memcpy(meshes, tile->detailMeshes, sizeof(dtPolyDetail) * count);
        *meshCount = count;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileDetailVerts(const dtNavMesh* mesh
        , const int tileIndex
        , float* verts
        , int* vertCount
        , const int vertsSize)
    {
        if (!mesh || !verts
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
        {
            *vertCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->detailVertCount * 3;

        if (vertsSize < count)
        {
            *vertCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        memcpy(verts, tile->detailVerts, sizeof(float) * count);
        *vertCount = tile->header->detailVertCount;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileDetailTris(const dtNavMesh* mesh
        , const int tileIndex
        , unsigned char* tris
        , int* trisCount
        , const int trisSize)
    {
        if (!mesh || !tris
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
        {
            *trisCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->detailTriCount * 4;

        if (trisSize < count)
        {
            *trisCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        memcpy(tris, tile->detailTris, sizeof(unsigned char) * count);
        *trisCount = tile->header->detailTriCount;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileBVTree(const dtNavMesh* mesh
        , const int tileIndex
        , dtBVNode* nodes
        , int* nodeCount
        , const int nodesSize)
    {
        if (!mesh || !nodes
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
        {
            *nodeCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->bvNodeCount;

        if (nodesSize < count)
        {
            *nodeCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        memcpy(nodes, tile->bvTree, sizeof(dtBVNode) * count);
        *nodeCount = count;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileConnections(const dtNavMesh* mesh
        , const int tileIndex
        , dtOffMeshConnection* conns
        , int* connCount
        , const int connsSize)
    {
        if (!mesh || !conns
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
        {
            *connCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->offMeshConCount;

        if (connsSize < count)
        {
            *connCount = 0;
            return (DT_FAILURE | DT_INVALID_PARAM);
        }

        memcpy(conns, tile->offMeshCons, sizeof(dtOffMeshConnection) * count);
        *connCount = count;

        return DT_SUCCESS;
    }

}
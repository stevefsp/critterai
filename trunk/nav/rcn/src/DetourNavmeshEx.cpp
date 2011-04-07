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

void copyPoly(dtPoly* target, const dtPoly* source)
{
    target->areaAndtype = source->areaAndtype;
    target->flags = source->flags;
    target->vertCount = source->vertCount;
    target->firstLink = source->firstLink;

    for (int i = 0; i < DT_VERTS_PER_POLYGON; i++)
    {
        target->verts[i] = source->verts[i];
        target->neis[i] = source->neis[i];
    }
}

void copyLink(dtLink* target, const dtLink* source)
{
    target->bmax = source->bmax;
    target->bmin = source->bmin;
    target->edge = source->edge;
    target->next = source->next;
    target->ref = source->ref;
    target->side = source->side;
}

void copyPolyDetail(dtPolyDetail* target, const dtPolyDetail* source)
{
    target->triBase = source->triBase;
    target->triCount = source->triCount;
    target->vertCount = source->vertCount;
    target->vertBase = source->vertBase;
}

void copyBVNode(dtBVNode* target, const dtBVNode* source)
{
    for (int i = 0; i < 3; i++)
    {
        target->bmax[i] = source->bmax[i];
        target->bmin[i] = source->bmin[i];
    }
    target->i = source->i;
}

void copyOffMeshConnection(dtOffMeshConnection* target
                           , const dtOffMeshConnection* source)
{
    target->flags = source->flags;
    target->poly = source->poly;
    target->side = source->side;
    target->rad = source->rad;
    target->userId = source->userId;
    for (int i = 0; i < 6; i++)
    {
        target->pos[i] = source->pos[i];
    }
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

    //EXPORT_API dtStatus dtnmGetPoly(const dtNavMesh* pNavMesh
    //    , const dtPolyRef polyRef
    //    , dtPoly* resultPoly)
    //{
    //    if (!pNavMesh || !resultPoly || !polyRef)
    //        return (DT_FAILURE | DT_INVALID_PARAM);

    //    const dtMeshTile* tile = 0;
    //    const dtPoly* poly = 0;
    //    dtStatus status = pNavMesh->getTileAndPolyByRef(polyRef
    //        , &tile
    //        , &poly);

    //    if (dtStatusFailed(status))
    //        return status;

    //    copyPoly(resultPoly, poly);

    //    return DT_SUCCESS;
    //}

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
        , const int polySize)
    {
        if (!mesh || !polys
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->polyCount;

        if (polySize < count)
            return (DT_FAILURE | DT_INVALID_PARAM);

        for (int i = 0; i < count; i++)
        {
            copyPoly(&polys[i], &tile->polys[i]);
        }

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileLinks(const dtNavMesh* mesh
        , const int tileIndex
        , dtLink* links
        , const int linkSize)
    {
        if (!mesh || !links
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->maxLinkCount;

        if (linkSize < count)
            return (DT_FAILURE | DT_INVALID_PARAM);

        for (int i = 0; i < count; i++)
        {
            copyLink(&links[i], &tile->links[i]);
        }

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileVerts(const dtNavMesh* mesh
        , const int tileIndex
        , float* verts
        , const int vertsSize)
    {
        if (!mesh || !verts
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->vertCount * 3;

        if (vertsSize < count)
            return (DT_FAILURE | DT_INVALID_PARAM);

        for (int i = 0; i < count; i++)
        {
            verts[i] = tile->verts[i];
        }

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileDetailMeshes(const dtNavMesh* mesh
        , const int tileIndex
        , dtPolyDetail* meshes
        , const int meshesSize)
    {
        if (!mesh || !meshes
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->detailMeshCount;

        if (meshesSize < count)
            return (DT_FAILURE | DT_INVALID_PARAM);

        for (int i = 0; i < count; i++)
        {
            copyPolyDetail(&meshes[i], &tile->detailMeshes[i]);
        }

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileDetailVerts(const dtNavMesh* mesh
        , const int tileIndex
        , float* verts
        , const int vertsSize)
    {
        if (!mesh || !verts
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->detailVertCount * 3;

        if (vertsSize < count)
            return (DT_FAILURE | DT_INVALID_PARAM);

        for (int i = 0; i < count; i++)
        {
            verts[i] = tile->detailVerts[i];
        }

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileDetailTris(const dtNavMesh* mesh
        , const int tileIndex
        , unsigned char* tris
        , const int trisSize)
    {
        if (!mesh || !tris
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->detailTriCount * 4;

        if (trisSize < count)
            return (DT_FAILURE | DT_INVALID_PARAM);

        for (int i = 0; i < count; i++)
        {
            tris[i] = tile->detailTris[i];
        }

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileBVTree(const dtNavMesh* mesh
        , const int tileIndex
        , dtBVNode* nodes
        , const int nodeSize)
    {
        if (!mesh || !nodes
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->bvNodeCount;

        if (nodeSize < count)
            return (DT_FAILURE | DT_INVALID_PARAM);

        for (int i = 0; i < count; i++)
        {
            copyBVNode(&nodes[i], &tile->bvTree[i]);
        }

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetTileConnections(const dtNavMesh* mesh
        , const int tileIndex
        , dtOffMeshConnection* conns
        , const int connsSize)
    {
        if (!mesh || !conns
                || tileIndex < 0
                || tileIndex >= mesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = mesh->getTile(tileIndex);

        int count = tile->header->offMeshConCount;

        if (connsSize < count)
            return (DT_FAILURE | DT_INVALID_PARAM);

        for (int i = 0; i < count; i++)
        {
            copyOffMeshConnection(&conns[i], &tile->offMeshCons[i]);
        }

        return DT_SUCCESS;
    }

}
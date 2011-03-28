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

    //EXPORT_API dtStatus dtnmGetOffMeshConnectionPolyEndPoints(
    //    const dtNavMesh* pNavMesh
    //    , const dtPolyRef prevRef
    //    , const dtPolyRef polyRef
    //    , float* startPos
    //    , float* endPos)
    //{
    //    if (!pNavMesh)
    //        return (DT_FAILURE | DT_INVALID_PARAM);

    //    return pNavMesh->getOffMeshConnectionPolyEndPoints(
    //        prevRef
    //        , polyRef
    //        , startPos
    //        , endPos);
    //}

    EXPORT_API dtStatus dtnmGetTileInfo(const dtNavMesh* pNavMesh
        , const int tileIndex
        , rcnTileInfo* tileInfo)
    {
        if (!pNavMesh || !tileInfo
                || tileIndex < 0
                || tileIndex >= pNavMesh->getMaxTiles())
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = pNavMesh->getTile(tileIndex);
        dtMeshHeader* header = tile->header;

        tileInfo->polyCount = header->polyCount;
        tileInfo->vertCount = header->vertCount;
        tileInfo->walkableClimb = header->walkableClimb;
        tileInfo->walkableHeight = header->walkableHeight;
        tileInfo->walkableRadius = header->walkableRadius;
        tileInfo->x = header->x;
        tileInfo->y = header->y;
        dtVcopy(tileInfo->bmin, header->bmin);
        dtVcopy(tileInfo->bmax, header->bmax);
        tileInfo->basePolyRef = pNavMesh->getPolyRefBase(tile);

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmGetPolyInfo(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , rcnPolyInfo* polyInfo)
    {
        if (!pNavMesh || !polyInfo || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        const dtMeshTile* tile = 0;
        const dtPoly* poly = 0;
        dtStatus status = pNavMesh->getTileAndPolyByRef(polyRef
            , &tile
            , &poly);

        if (dtStatusFailed(status))
            return status;

        polyInfo->areaAndtype = poly->areaAndtype;
        polyInfo->flags = poly->flags;
        polyInfo->vertCount = poly->vertCount;

        for (int i = 0; i < poly->vertCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                float svi = tile->verts[poly->verts[i] * 3 + j];

                polyInfo->verts[i * 3 + j] = 
                    tile->verts[poly->verts[i] * 3 + j];
            }
        }

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

}
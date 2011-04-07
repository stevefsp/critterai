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
#include "Rcn.h"
#include "RecastAlloc.h"
#include "DetourNavMeshBuilder.h"
#include "DetourNavMeshQuery.h"

// Iterates an array of vertices and copies the unique vertices to
// another array.
// vertCount - The number of vertices in sourceVerts.
// sourceVerts - The source vertices in the form (x, y, z).  Length equals
// vertCount * 3.
// resultVerts - An initialized array to load unique vertices into.  
// Values will be in the form (x, y, z).  It must be the same length as 
// sourceVerts.
// indicesMap - An initialized array of length vertCount which will hold
// the map of indices from sourceVerts to resultVerts.  E.g. If the value
// at index 5 is 2, then sourceVerts[5*3] is located at resultVerts[2*3].
// Returns: The number of unique vertices found.
// Notes:
//    If there are no duplicate vertices, the content of the source and
//    result arrays will be identical and vertCount will equal 
//    resultVertCount.
int removeDuplicateVerts(int vertCount
    , const float* sourceVerts 
    , float* resultVerts
    , int* indicesMap)
{
    int resultCount = 0;

    for (int i = 0; i < vertCount; i++)
    {
        int index = resultCount;
        int pi = i*3;
        // Check to see if this vertex has already been seen.
        for (int j = 0; j < resultCount; j++)
        {
            int pj = j*3;
            if (rcnSloppyEquals(sourceVerts[pi+0], resultVerts[pj+0])
                && rcnSloppyEquals(sourceVerts[pi+1], resultVerts[pj+1])
                && rcnSloppyEquals(sourceVerts[pi+2], resultVerts[pj+2]))
            {
                // This vertex already exists.
                index = j;
                break;
            }
        }
        indicesMap[i] = index;
        if (index == resultCount)
        {
            // This is a new vertex.
            resultVerts[resultCount*3+0] = sourceVerts[pi+0];
            resultVerts[resultCount*3+1] = sourceVerts[pi+1];
            resultVerts[resultCount*3+2] = sourceVerts[pi+2];
            resultCount++;
        }
    }

    return resultCount;

};

void rcnTransferMessages(RCNBuildContext& context
    , RCNMessageBuffer& messages)
{
    if (context.getLogEnabled())
    {
        messages.size = context.getMessagePoolLength();
        messages.buffer = new char[messages.size];
        memcpy(messages.buffer, context.getMessagePool(), messages.size);
    }
}

extern "C"
{
    // Deletes memory allocated to all pointers within the mesh structure.
    EXPORT_API void rcnFreeMesh3(RCNMesh3* mesh)
    {
        if (mesh)
        {
            if (mesh->vertices)
                delete [] mesh->vertices;
            if (mesh->indices)
                delete [] mesh->indices;
            mesh->polyCount = 0;
            mesh->vertCount = 0;
        }
    }

    // Deletes memory allocated to all pointers within the buffer.
    EXPORT_API void rcnFreeMessageBuffer(RCNMessageBuffer* messages)
    {
        if (messages)
            delete [] messages->buffer;
    }

    EXPORT_API void rcnApplyNavMeshConfigLimits(RCNNavMeshConfig* config)
    {
        config->applyLimits();
    }

    EXPORT_API bool rcnBuildRCNavMesh(RCNNavMeshConfig config
        , RCNMesh3* pSourceMesh
        , unsigned char* pAreas
        , RCNMessageBuffer* pMessages
        , rcPolyMesh* pPolyMesh
        , rcPolyMeshDetail* pDetailMesh)
    {

         // Initialize the build context.

        int messageDetail = 
            (!pMessages || pMessages->messageDetail <= MDETAIL_NONE) 
                ? MDETAIL_NONE : pMessages->messageDetail;
        RCNBuildContext* pContext = new RCNBuildContext();
        pContext->messageDetail = messageDetail;
        pContext->enableLog(messageDetail != MDETAIL_NONE);

	    // Prepare timer and make initial log entry.
        // Design Note: This isn't really doing anything since timer is not 
        // implemented by the current pContext class.
	    pContext->resetTimers();
	    pContext->startTimer(RC_TIMER_TOTAL);
        if (messageDetail > MDETAIL_BRIEF)
            pContext->log(RC_LOG_PROGRESS, "Building mesh: Static Detour");

        // Build the mesh.

        if (!rcnBuildBaseRCNavMesh(config
            , pSourceMesh
            , pAreas
            , pContext
            , *pPolyMesh
            , *pDetailMesh))
        {
		    pContext->log(RC_LOG_ERROR
                , "Failed static mesh build.");
            rcFreePolyMesh(pPolyMesh);
            rcFreePolyMeshDetail(pDetailMesh);
            if (messageDetail > MDETAIL_NONE)
                rcnTransferMessages(*pContext, *pMessages);
            return false;
        }

        if (messageDetail > MDETAIL_NONE)
        {
            pContext->log(RC_LOG_PROGRESS, "Built Recast Meshes.");
            rcnTransferMessages(*pContext, *pMessages);
        }

        return true;

    }

    EXPORT_API dtStatus rcnBuildStaticDTNavMesh(rcPolyMesh* pPolyMesh
        , rcPolyMeshDetail* pDetailMesh
        , float walkableHeight
        , float walkableRadius
        , float walkableClimb
        , RCNOffMeshConnections* pOffMeshConnections
        , dtNavMesh** ppNavMesh)
    {

        if (!pPolyMesh || !pDetailMesh)
            return DT_FAILURE + DT_INVALID_PARAM;

		unsigned char* navData = 0;
		int navDataSize = 0;

		dtNavMeshCreateParams params;
		memset(&params, 0, sizeof(params));

        // Data from pPolyMesh.
		params.verts = pPolyMesh->verts;
		params.vertCount = pPolyMesh->nverts;
		params.polys = pPolyMesh->polys;
		params.polyAreas = pPolyMesh->areas;
		params.polyFlags = pPolyMesh->flags;
		params.polyCount = pPolyMesh->npolys;
		params.nvp = pPolyMesh->nvp;
		rcVcopy(params.bmin, pPolyMesh->bmin);
		rcVcopy(params.bmax, pPolyMesh->bmax);

        // Data from pDetailMesh.
		params.detailMeshes = pDetailMesh->meshes;
		params.detailVerts = pDetailMesh->verts;
		params.detailVertsCount = pDetailMesh->nverts;
		params.detailTris = pDetailMesh->tris;
		params.detailTriCount = pDetailMesh->ntris;

        // Configuration data.
		params.walkableHeight = walkableHeight;
        params.walkableRadius = walkableRadius;
        params.walkableClimb = walkableClimb;
        params.cs = pPolyMesh->cs;
        params.ch = pPolyMesh->ch;

        // Miscellany
        params.buildBvTree = true;  // One big tile.  So need the tree.

        if (pOffMeshConnections && pOffMeshConnections->count > 0)
        {
            // Design note: The final mesh will not contain any references
            // to the memory used by pOffMeshConnections.  So it is OK for
            // the data to be directly referenced by params during the build.
            params.offMeshConCount = pOffMeshConnections->count;
            params.offMeshConVerts = pOffMeshConnections->verts;
            params.offMeshConRad = pOffMeshConnections->radii;
            params.offMeshConDir = pOffMeshConnections->dirs;
            params.offMeshConAreas = pOffMeshConnections->areas;
            params.offMeshConFlags = pOffMeshConnections->flags;
            params.offMeshConUserID = pOffMeshConnections->ids;
        }
        else
            params.offMeshConCount = 0;

		if (!dtCreateNavMeshData(&params, &navData, &navDataSize))
            return DT_FAILURE + DT_INVALID_PARAM;
		
        dtNavMesh* pNavMesh = dtAllocNavMesh();
		if (!pNavMesh)
		{
            dtFree(navData);
            return DT_FAILURE + DT_OUT_OF_MEMORY;
		}

		dtStatus status = 
            pNavMesh->init(navData, navDataSize, DT_TILE_FREE_DATA);
		if (dtStatusFailed(status))
		{
            dtFreeNavMesh(pNavMesh);
            dtFree(navData);
            return status;
		}

        *ppNavMesh = pNavMesh;

        return DT_SUCCESS;

    }

    EXPORT_API dtStatus rcnBuildDTNavQuery(dtNavMesh* pNavMesh
        , const int maxNodes
        , dtNavMeshQuery** ppNavQuery)
    {
         // Initialize the build context.

        if (!pNavMesh)
            return DT_FAILURE + DT_INVALID_PARAM;
		
        dtNavMeshQuery* pNavQuery = dtAllocNavMeshQuery();
        if (!pNavQuery)
            return DT_FAILURE + DT_OUT_OF_MEMORY;

		dtStatus status = pNavQuery->init(pNavMesh, maxNodes);
		if (dtStatusFailed(status))
		{
            dtFreeNavMeshQuery(pNavQuery);
            return status;
		}
        
        *ppNavQuery = pNavQuery;

        return DT_SUCCESS;
    }

    EXPORT_API bool rcnFlattenDetailMesh(rcPolyMeshDetail* detailMesh
        , RCNMesh3* resultMesh)
    {
        /*
         * Remember: The detailMesh->tris array has a stride of 4
         * (3 indices + flags)
         *
         * The detail meshes are completely independant, which results
         * in duplicate verts.  The flattening process will remove
         * the duplicates.
         */

        if (!resultMesh || !detailMesh 
            || detailMesh->ntris == 0
            || detailMesh->nmeshes == 0
            || detailMesh->nverts == 0
            || !detailMesh->meshes
            || !detailMesh->tris
            || !detailMesh->verts)
            return false;

        resultMesh->indices = new int[(detailMesh->ntris)*3];
        resultMesh->polyCount = detailMesh->ntris;
        // Can't initialize the vertices yet.  Don't know the number of
        // unique vertices.
        
        float* uniqueVerts = new float[(detailMesh->nverts)*3];
        int* vertMap = new int[detailMesh->nverts];

        int resultVertCount = removeDuplicateVerts(
            detailMesh->nverts
            , detailMesh->verts
            , uniqueVerts
            , vertMap);

        int resultVertLength = resultVertCount*3;
        float* vertices = new float[resultVertLength];

        // Copy unique vertices to the output.
        // Using resultVertLength avoids copying the trash since uniqueVerts
        // was sized to assume no duplicate vertices. (An unlikely scenario.)
        for (int i = 0; i < resultVertLength; i++)
        {
            vertices[i] = uniqueVerts[i];
        }

        // Assign vert data to the result mesh.
        resultMesh->vertices = vertices;
        resultMesh->vertCount = resultVertCount;

        delete [] uniqueVerts;

        // Flatten and re-map the indices.
        int pCurrentTri = 0;
        for (int iMesh = 0; iMesh < detailMesh->nmeshes; iMesh++)
        {
            int vBase = detailMesh->meshes[iMesh*4+0];
            int tBase = detailMesh->meshes[iMesh*4+2];
            int tCount =  detailMesh->meshes[iMesh*4+3];
            for (int iTri = 0; iTri < tCount; iTri++)
            {
                const unsigned char* tri = &detailMesh->tris[(tBase+iTri)*4];
                for (int i = 0; i < 3; i++)
                {
                    resultMesh->indices[pCurrentTri] = vertMap[vBase+tri[i]];
                    pCurrentTri++;
                }
            }
        }

        delete [] vertMap;

        return true;
    }
}
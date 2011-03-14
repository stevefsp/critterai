/*
 * Copyright (c) 2010 Stephen A. Pratt
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
#include <math.h>
#include <string.h>
#include "RCN.h"
#include "RecastAlloc.h"
#include "DetourNavMeshBuilder.h"

namespace org 
{
namespace critterai
{
namespace nav
{
namespace rcn
{
    int getSolidSpanCount(const rcHeightfield& solid)
    {
	    const int w = solid.width;
	    const int h = solid.height;
        int result = 0;
    	
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    for (rcSpan* s = solid.spans[x + y*w]; s; s = s->next)
			    {
                    result++;
                }
            }
        }
        return result;
    }

    int getTraversableSpanCount(const rcHeightfield& solid)
    {
	    const int w = solid.width;
	    const int h = solid.height;
        int result = 0;
    	
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    for (rcSpan* s = solid.spans[x + y*w]; s; s = s->next)
			    {
                    result += (s->area == RC_NULL_AREA) ? 0 : 1;
                }
            }
        }
        return result;
    }

    int getTraversableSpanCount(const rcCompactHeightfield& chf)
    {
	    const int w = chf.width;
	    const int h = chf.height;
        int result = 0;
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    const rcCompactCell& c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count)
                    ; i < ni
                    ; i++)
			    {
				    if (chf.areas[i] != RC_NULL_AREA)
					    result++;
                }
            }
        }
        return result;
    }

    void flattenDetailMesh(const rcPolyMeshDetail& detailMesh, Mesh3& resultMesh)
    {
        /*
         * Remember: The tris array has a stride of 4: 3 indices + flags
         *
         * The detail meshes are completely independant, which results
         * in duplicate verts.  This process removes the duplicates.
         */ 

        resultMesh.vertsPerPolygon = 3;
        resultMesh.indices = new int[(detailMesh.ntris)*3];
        // Can't initialize the vertices yet.  Don't know the unique vert count.
        
        float* uniqueVerts = new float[(detailMesh.nverts)*3];
        int* vertMap = new int[detailMesh.nverts];

        int resultVertCount = removeDuplicateVerts(
            detailMesh.nverts
            , detailMesh.verts
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
        resultMesh.vertices = vertices;
        resultMesh.vertCount = resultVertCount;

        delete [] uniqueVerts;

        // Flatten and re-map the indices.
        int pCurrentTri = 0;
        for (int iMesh = 0; iMesh < detailMesh.nmeshes; iMesh++)
        {
            int vBase = detailMesh.meshes[iMesh*4+0];
            int tBase = detailMesh.meshes[iMesh*4+2];
            int tCount =  detailMesh.meshes[iMesh*4+3];
            for (int iTri = 0; iTri < tCount; iTri++)
            {
                const unsigned char* tri = &detailMesh.tris[(tBase+iTri)*4];
                for (int i = 0; i < 3; i++)
                {
                    resultMesh.indices[pCurrentTri] = vertMap[vBase+tri[i]];
                    pCurrentTri++;
                }
            }
        }

        delete [] vertMap;
    }

    bool buildStaticMesh(NMGenConfig config
        , Mesh3* sourceMesh
        , BuildContext* context
        , rcPolyMesh& polyMesh
        , rcPolyMeshDetail& detailMesh)
    {

        /**********************************************************************
         * Initialization
         *********************************************************************/

        if (!sourceMesh || sourceMesh->vertsPerPolygon != 3)
        {
            context->log(RC_LOG_ERROR, "Invalid source mesh.");
            return false;
        }

        // Some convenience variables.
        const int vertCount = sourceMesh->vertCount;
        const int triangleCount = sourceMesh->polyCount;
        const int messageDetail = context->messageDetail;

        config.applyLimits();

        // Derive various settings.

        float* bounds = new float[6];
        int height, width;

        deriveBounds3(sourceMesh->vertices
            , sourceMesh->vertCount * 3
            , bounds);
	    rcCalcGridSize(&bounds[0], &bounds[3], config.xzCellSize
            , &width, &height);

        const int vxMaxTraversableStep = 
            (int)floorf(config.maxTraversableStep / config.yCellSize);
        const int vxMinTraversableHeight =
            (int)ceilf(config.minTraversableHeight / config.yCellSize);
        const int vxTraversableAreaBorderSize = (int)ceilf(
            config.traversableAreaBorderSize / config.xzCellSize);
        const int vxHeightfieldBorderSize= (int)ceilf(
            config.heightfieldBorderSize / config.xzCellSize);
        const int vxMaxEdgeLength = (int)ceilf(
            config.maxEdgeLength / config.xzCellSize);

        // Log configuration related messages.

        if (messageDetail > 1)
        {
            context->log(RC_LOG_PROGRESS
                , "Source: %d vertices, %d triangles"
                , vertCount
                , triangleCount);
            context->log(RC_LOG_PROGRESS, "Source: %d x %d cells"
                , width, height);
        }
        if (messageDetail > 2)
        {
            context->log(RC_LOG_PROGRESS
                , "Source: Min:(%.3f, %.3f, %.3f) to Max:(%.3f, %.3f, %.3f)"
                            , bounds[0], bounds[1], bounds[2]
                            , bounds[3], bounds[4], bounds[5]);
            context->log(RC_LOG_PROGRESS
                , "Config: xzCellSize: %.3f wu"
                , config.xzCellSize);
            context->log(RC_LOG_PROGRESS
                , "Config: yCellSize: %.3f wu"
                , config.yCellSize);
             context->log(RC_LOG_PROGRESS
                , "Config: maxTraversableSlop: %.2f degrees"
                , config.maxTraversableSlope);             
            context->log(RC_LOG_PROGRESS
                , "Config: maxTraversableStep: %d vx"
                , vxMaxTraversableStep);
            context->log(RC_LOG_PROGRESS
                , "Config: minTraversableHeight: %d vx"
                , vxMinTraversableHeight);
            context->log(RC_LOG_PROGRESS
                , "Config: traversableAreaBorderSize: %d vx"
                , vxTraversableAreaBorderSize);
            context->log(RC_LOG_PROGRESS
                , "Config: heightfieldBorderSize: %d vx"
                , vxHeightfieldBorderSize);
             context->log(RC_LOG_PROGRESS
                , "Config: smoothingThreshold: %d"
                , config.smoothingThreshold);
             context->log(RC_LOG_PROGRESS
                , "Config: mergeRegionSize: %d"
                , config.mergeRegionSize);
             context->log(RC_LOG_PROGRESS
                , "Config: minIslandRegionSize: %d"
                , config.minIslandRegionSize);
            context->log(RC_LOG_PROGRESS
                , "Config: maxEdgeLength: %d vx"
                , vxMaxEdgeLength);
            context->log(RC_LOG_PROGRESS
                , "edgeMaxDeviation: %.3f wu"
                , config.edgeMaxDeviation);
            context->log(RC_LOG_PROGRESS
                , "Config: contourSampleDistance: %.3f wu"
                , config.contourSampleDistance);
            context->log(RC_LOG_PROGRESS
                , "Config: contourMaxDeviation: %.3f wu"
                , config.contourMaxDeviation);
             context->log(RC_LOG_PROGRESS
                , "Config: maxVertsPerPoly: %d"
                , config.maxVertsPerPoly);
            context->log(RC_LOG_PROGRESS
                , "Config: clipLedges: %d"
                , config.clipLedges);
        }
    	
        /**********************************************************************
         * Build solid heightfield.
         *********************************************************************/

        rcHeightfield* solidHeightfield = rcAllocHeightfield();
        bool failed = false;
	    if (!solidHeightfield)
	    {
            context->log(RC_LOG_ERROR
                , "Out of memory: Solid heightfield.");
		    failed = true;
	    }
	    else if (!rcCreateHeightfield(context
            , *solidHeightfield
            , width
            , height
            , &bounds[0]
            , &bounds[3]
            , config.xzCellSize
            , config.yCellSize))
	    {
            rcFreeHeightField(solidHeightfield);
		    context->log(RC_LOG_ERROR
                , "Could not create solid heightfield.");
		    failed = true;
	    }
    	
        delete[] bounds;
        bounds = 0;

        if (failed)
        {
            return false;
        }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Initialized solid heightfield.");

	    // Allocate array that can hold triangle area information.
	    unsigned char* triangleAreas = new unsigned char[triangleCount];
	    if (!triangleAreas)
	    {
		    context->log(RC_LOG_ERROR
                , "Out of memory: triangleAreas (%d).", triangleCount);
		    return false;
	    }

    	memset(triangleAreas, 0, sizeof(unsigned char) * triangleCount);

	    // Mark triangles which are walkable based on their slope
	    rcMarkWalkableTriangles(context
            , config.maxTraversableSlope
            , sourceMesh->vertices, sourceMesh->vertCount
            , sourceMesh->indices, sourceMesh->polyCount
            , triangleAreas);

	    rcRasterizeTriangles(context
            , sourceMesh->vertices, sourceMesh->vertCount
            , sourceMesh->indices, triangleAreas, sourceMesh->polyCount
            , *solidHeightfield
            , vxMaxTraversableStep);

        delete[] triangleAreas;
        triangleAreas = 0;
    	
        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built solid heightfield: %d spans."
                , getSolidSpanCount(*solidHeightfield));

	    // Apply various filters.
	    rcFilterLowHangingWalkableObstacles(context
            , vxMaxTraversableStep
            , *solidHeightfield);
	    rcFilterLedgeSpans(context
            , vxMinTraversableHeight
            , vxMaxTraversableStep
            , *solidHeightfield);
	    rcFilterWalkableLowHeightSpans(context
            , vxMinTraversableHeight
            , *solidHeightfield);

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Applied solid heightfield filters: %d remaining spans."
                , getTraversableSpanCount(*solidHeightfield));

        /**********************************************************************
         * Build compact (open) heightfield and regions.
         *********************************************************************/

        rcCompactHeightfield* compactHeightfield = rcAllocCompactHeightfield();
	    if (!compactHeightfield)
	    {
            context->log(RC_LOG_ERROR
                , "Out of memory: Compact heightfield.");
		    failed = true;
	    }
	    else if (!rcBuildCompactHeightfield(context
            , vxMinTraversableHeight
            , vxMaxTraversableStep
            , *solidHeightfield
            , *compactHeightfield))
	    {
            rcFreeCompactHeightfield(compactHeightfield);
		    context->log(RC_LOG_ERROR
                , "Could not build compact heightfield.");
            failed = true;
	    }
    	
		rcFreeHeightField(solidHeightfield);
		solidHeightfield = 0;

        if (failed)
        {
            return false;
        }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built compact heightfield: %d of %d traversable spans."
                , getTraversableSpanCount(*compactHeightfield)
                , compactHeightfield->spanCount);

	    // Create border around traversable surface.
	    if (!rcErodeWalkableArea(context
            , vxTraversableAreaBorderSize
            , *compactHeightfield))
	    {
		    context->log(RC_LOG_ERROR
                , "Could not generate open area border.");
            rcFreeCompactHeightfield(compactHeightfield);
            return false;
	    }
    	
        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Applied border: %d of %d tranversable spans."
                , getTraversableSpanCount(*compactHeightfield)
                , compactHeightfield->spanCount);

	    // Build distance field.
	    if (!rcBuildDistanceField(context, *compactHeightfield))
	    {
            rcFreeCompactHeightfield(compactHeightfield);
		    context->log(RC_LOG_ERROR
                , "Could not build distance field.");
		    return false;
	    }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built distance field: %d max distance."
                , compactHeightfield->maxDistance);

	    // Build regions.
	    if (!rcBuildRegions(context
            , *compactHeightfield
            , vxHeightfieldBorderSize
            , config.minIslandRegionSize
            , config.mergeRegionSize))
	    {
            rcFreeCompactHeightfield(compactHeightfield);
		    context->log(RC_LOG_ERROR
                , "Could not build regions.");
		    return false;
	    }
    	
        if (messageDetail > 2)
        {
            context->log(RC_LOG_PROGRESS
                , "Built regions: %d regions."
                , compactHeightfield->maxRegions);
            context->log(RC_LOG_PROGRESS
                , "Compact final: %d of %d traversable spans."
                , getTraversableSpanCount(*compactHeightfield)
                , compactHeightfield->spanCount);
        }

        /**********************************************************************
         * Build contours.
         *********************************************************************/

	    rcContourSet* contourSet = rcAllocContourSet();
	    if (!contourSet)
	    {
		    context->log(RC_LOG_ERROR
                , "Out of memory Contour set.");
		    failed = true;
	    }
	    else if (!rcBuildContours(context
            , *compactHeightfield
            , config.edgeMaxDeviation
            , vxMaxEdgeLength
            , *contourSet))
	    {
            rcFreeContourSet(contourSet);
		    context->log(RC_LOG_ERROR
                , "Could not create contours.");
		   failed = true;
	    }
    	
        if (failed)
        {
            rcFreeCompactHeightfield(compactHeightfield);
            return false;
        }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built contours: %d contours."
                , contourSet->nconts);

        /**********************************************************************
         * Build polygon mesh.
         *********************************************************************/

        if (!rcBuildPolyMesh(context
            , *contourSet
            , config.maxVertsPerPoly
            , polyMesh))
	    {
		    context->log(RC_LOG_ERROR
                , "Could not create polygon mesh.");
            failed = true;
	    }
    	
		rcFreeContourSet(contourSet);
		contourSet = 0;

        if (failed)
        {
            rcFreeCompactHeightfield(compactHeightfield);
            return false;
        }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built poly mesh: %d polygons."
                , polyMesh.maxpolys);

        /**********************************************************************
         * Build detail mesh.
         *********************************************************************/
    	
        if (!rcBuildPolyMeshDetail(context
            , polyMesh
            , *compactHeightfield
            , config.contourSampleDistance
            , config.contourMaxDeviation
            , detailMesh))
	    {
		    context->log(RC_LOG_ERROR
                , "Could not build detail mesh.");
            failed = true;
	    }

		rcFreeCompactHeightfield(compactHeightfield);
		compactHeightfield = 0;

        if (failed)
            return false;

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built detail mesh: %d submeshes, %d vertices, %d triangles"
                , detailMesh.nmeshes
                , detailMesh.nverts
                , detailMesh.ntris);

        if (detailMesh.nverts == 0)
        {
            context->log(RC_LOG_WARNING
                , "Build process did not result in a final mesh.");
            return false;
        }

        return true;
    }
}
}
}
}
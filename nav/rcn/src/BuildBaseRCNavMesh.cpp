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
#include <math.h>
#include <string.h>
#include "Rcn.h"
#include "RecastAlloc.h"

// Derives the minimum and maximum vertices for the AABB encompassing
// the vertices.  bounds = (minX, minY, minZ, maxX, maxY, maxZ)
void deriveBounds3(const float* vertices, int vertLength, float* bounds)
{
    if (vertLength <= 0)
        return;
    bounds[0] = vertices[0];
    bounds[1] = vertices[1];
    bounds[2] = vertices[2];
    bounds[3] = vertices[0];
    bounds[4] = vertices[1];
    bounds[5] = vertices[2];
    for (int p = 0; p < vertLength; p += 3)
    {
        bounds[0] = rcMin(bounds[0], vertices[p+0]);
        bounds[1] = rcMin(bounds[1], vertices[p+1]);
        bounds[2] = rcMin(bounds[2], vertices[p+2]);
        bounds[3] = rcMax(bounds[3], vertices[p+0]);
        bounds[4] = rcMax(bounds[4], vertices[p+1]);
        bounds[5] = rcMax(bounds[5], vertices[p+2]);
    }
}

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

bool rcnBuildBaseRCNavMesh(RCNNavMeshConfig config
    , RCNMesh3* pSourceMesh
    , RCNBuildContext* pContext
    , rcPolyMesh& polyMesh
    , rcPolyMeshDetail& detailMesh)
{

    /**********************************************************************
     * Initialization
     *********************************************************************/

    if (!pSourceMesh)
    {
        pContext->log(RC_LOG_ERROR, "Invalid source mesh.");
        return false;
    }

    // Some convenience variables.
    const int vertCount = pSourceMesh->vertCount;
    const int triangleCount = pSourceMesh->polyCount;
    const int messageDetail = pContext->messageDetail;

    config.applyLimits();

    // Derive various settings.

    float* bounds = new float[6];
    int height, width;

    deriveBounds3(pSourceMesh->vertices
        , pSourceMesh->vertCount * 3
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

    if (messageDetail > MDETAIL_BRIEF)
    {
        pContext->log(RC_LOG_PROGRESS
            , "Source: %d vertices, %d triangles"
            , vertCount
            , triangleCount);
        pContext->log(RC_LOG_PROGRESS, "Source: %d x %d cells"
            , width, height);
    }
    if (messageDetail > MDETAIL_SUMMARY)
    {
        pContext->log(RC_LOG_PROGRESS
            , "Source: Min:(%.3f, %.3f, %.3f) to Max:(%.3f, %.3f, %.3f)"
                        , bounds[0], bounds[1], bounds[2]
                        , bounds[3], bounds[4], bounds[5]);
        pContext->log(RC_LOG_PROGRESS
            , "Config: xzCellSize: %.3f wu"
            , config.xzCellSize);
        pContext->log(RC_LOG_PROGRESS
            , "Config: yCellSize: %.3f wu"
            , config.yCellSize);
         pContext->log(RC_LOG_PROGRESS
            , "Config: maxTraversableSlop: %.2f degrees"
            , config.maxTraversableSlope);             
        pContext->log(RC_LOG_PROGRESS
            , "Config: maxTraversableStep: %d vx"
            , vxMaxTraversableStep);
        pContext->log(RC_LOG_PROGRESS
            , "Config: minTraversableHeight: %d vx"
            , vxMinTraversableHeight);
        pContext->log(RC_LOG_PROGRESS
            , "Config: traversableAreaBorderSize: %d vx"
            , vxTraversableAreaBorderSize);
        pContext->log(RC_LOG_PROGRESS
            , "Config: heightfieldBorderSize: %d vx"
            , vxHeightfieldBorderSize);
         pContext->log(RC_LOG_PROGRESS
            , "Config: smoothingThreshold: %d"
            , config.smoothingThreshold);
         pContext->log(RC_LOG_PROGRESS
            , "Config: mergeRegionSize: %d"
            , config.mergeRegionSize);
         pContext->log(RC_LOG_PROGRESS
            , "Config: minIslandRegionSize: %d"
            , config.minIslandRegionSize);
        pContext->log(RC_LOG_PROGRESS
            , "Config: maxEdgeLength: %d vx"
            , vxMaxEdgeLength);
        pContext->log(RC_LOG_PROGRESS
            , "edgeMaxDeviation: %.3f wu"
            , config.edgeMaxDeviation);
        pContext->log(RC_LOG_PROGRESS
            , "Config: contourSampleDistance: %.3f wu"
            , config.contourSampleDistance);
        pContext->log(RC_LOG_PROGRESS
            , "Config: contourMaxDeviation: %.3f wu"
            , config.contourMaxDeviation);
         pContext->log(RC_LOG_PROGRESS
            , "Config: maxVertsPerPoly: %d"
            , config.maxVertsPerPoly);
        pContext->log(RC_LOG_PROGRESS
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
        pContext->log(RC_LOG_ERROR
            , "Out of memory: Solid heightfield.");
	    failed = true;
    }
    else if (!rcCreateHeightfield(pContext
        , *solidHeightfield
        , width
        , height
        , &bounds[0]
        , &bounds[3]
        , config.xzCellSize
        , config.yCellSize))
    {
        rcFreeHeightField(solidHeightfield);
	    pContext->log(RC_LOG_ERROR
            , "Could not create solid heightfield.");
	    failed = true;
    }
	
    delete[] bounds;
    bounds = 0;

    if (failed)
    {
        return false;
    }

    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Initialized solid heightfield.");

    // Allocate array that can hold triangle area information.
    unsigned char* triangleAreas = new unsigned char[triangleCount];
    if (!triangleAreas)
    {
	    pContext->log(RC_LOG_ERROR
            , "Out of memory: triangleAreas (%d).", triangleCount);
	    return false;
    }

	memset(triangleAreas, 0, sizeof(unsigned char) * triangleCount);

    // Mark triangles which are walkable based on their slope
    rcMarkWalkableTriangles(pContext
        , config.maxTraversableSlope
        , pSourceMesh->vertices, pSourceMesh->vertCount
        , pSourceMesh->indices, pSourceMesh->polyCount
        , triangleAreas);

    rcRasterizeTriangles(pContext
        , pSourceMesh->vertices, pSourceMesh->vertCount
        , pSourceMesh->indices, triangleAreas, pSourceMesh->polyCount
        , *solidHeightfield
        , vxMaxTraversableStep);

    delete[] triangleAreas;
    triangleAreas = 0;
	
    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Built solid heightfield: %d spans."
            , getSolidSpanCount(*solidHeightfield));

    // Apply various filters.
    rcFilterLowHangingWalkableObstacles(pContext
        , vxMaxTraversableStep
        , *solidHeightfield);
    rcFilterLedgeSpans(pContext
        , vxMinTraversableHeight
        , vxMaxTraversableStep
        , *solidHeightfield);
    rcFilterWalkableLowHeightSpans(pContext
        , vxMinTraversableHeight
        , *solidHeightfield);

    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Applied solid heightfield filters: %d remaining spans."
            , getTraversableSpanCount(*solidHeightfield));

    /**********************************************************************
     * Build compact (open) heightfield and regions.
     *********************************************************************/

    rcCompactHeightfield* compactHeightfield = rcAllocCompactHeightfield();
    if (!compactHeightfield)
    {
        pContext->log(RC_LOG_ERROR
            , "Out of memory: Compact heightfield.");
	    failed = true;
    }
    else if (!rcBuildCompactHeightfield(pContext
        , vxMinTraversableHeight
        , vxMaxTraversableStep
        , *solidHeightfield
        , *compactHeightfield))
    {
        rcFreeCompactHeightfield(compactHeightfield);
	    pContext->log(RC_LOG_ERROR
            , "Could not build compact heightfield.");
        failed = true;
    }
	
	rcFreeHeightField(solidHeightfield);
	solidHeightfield = 0;

    if (failed)
    {
        return false;
    }

    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Built compact heightfield: %d of %d traversable spans."
            , getTraversableSpanCount(*compactHeightfield)
            , compactHeightfield->spanCount);

    // Create border around traversable surface.
    if (!rcErodeWalkableArea(pContext
        , vxTraversableAreaBorderSize
        , *compactHeightfield))
    {
	    pContext->log(RC_LOG_ERROR
            , "Could not generate open area border.");
        rcFreeCompactHeightfield(compactHeightfield);
        return false;
    }
	
    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Applied border: %d of %d tranversable spans."
            , getTraversableSpanCount(*compactHeightfield)
            , compactHeightfield->spanCount);

    // Build distance field.
    if (!rcBuildDistanceField(pContext, *compactHeightfield))
    {
        rcFreeCompactHeightfield(compactHeightfield);
	    pContext->log(RC_LOG_ERROR
            , "Could not build distance field.");
	    return false;
    }

    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Built distance field: %d max distance."
            , compactHeightfield->maxDistance);

    // Build regions.
    if (!rcBuildRegions(pContext
        , *compactHeightfield
        , vxHeightfieldBorderSize
        , config.minIslandRegionSize
        , config.mergeRegionSize))
    {
        rcFreeCompactHeightfield(compactHeightfield);
	    pContext->log(RC_LOG_ERROR
            , "Could not build regions.");
	    return false;
    }
	
    if (messageDetail > MDETAIL_SUMMARY)
    {
        pContext->log(RC_LOG_PROGRESS
            , "Built regions: %d regions."
            , compactHeightfield->maxRegions);
        pContext->log(RC_LOG_PROGRESS
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
	    pContext->log(RC_LOG_ERROR
            , "Out of memory Contour set.");
	    failed = true;
    }
    else if (!rcBuildContours(pContext
        , *compactHeightfield
        , config.edgeMaxDeviation
        , vxMaxEdgeLength
        , *contourSet))
    {
        rcFreeContourSet(contourSet);
	    pContext->log(RC_LOG_ERROR
            , "Could not create contours.");
	   failed = true;
    }
	
    if (failed)
    {
        rcFreeCompactHeightfield(compactHeightfield);
        return false;
    }

    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Built contours: %d contours."
            , contourSet->nconts);

    /**********************************************************************
     * Build polygon mesh.
     *********************************************************************/

    if (!rcBuildPolyMesh(pContext
        , *contourSet
        , config.maxVertsPerPoly
        , polyMesh))
    {
	    pContext->log(RC_LOG_ERROR
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

    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Built poly mesh: %d polygons."
            , polyMesh.maxpolys);

    /**********************************************************************
     * Build detail mesh.
     *********************************************************************/
	
    if (!rcBuildPolyMeshDetail(pContext
        , polyMesh
        , *compactHeightfield
        , config.contourSampleDistance
        , config.contourMaxDeviation
        , detailMesh))
    {
	    pContext->log(RC_LOG_ERROR
            , "Could not build detail mesh.");
        failed = true;
    }

	rcFreeCompactHeightfield(compactHeightfield);
	compactHeightfield = 0;

    if (failed)
        return false;

    if (messageDetail > MDETAIL_SUMMARY)
        pContext->log(RC_LOG_PROGRESS
            , "Built detail mesh: %d submeshes, %d vertices, %d triangles"
            , detailMesh.nmeshes
            , detailMesh.nverts
            , detailMesh.ntris);

    if (detailMesh.nverts == 0)
    {
        pContext->log(RC_LOG_WARNING
            , "Build process did not result in a final mesh.");
        return false;
    }

    return true;
}
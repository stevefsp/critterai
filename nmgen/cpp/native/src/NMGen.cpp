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
#include "NMGen.h"

namespace org 
{
namespace critterai
{
namespace nmgen
{

    extern "C" EXPORT_API void applyStandardLimits(Configuration* config)
    {
        config->applyLimits();
    }

    extern "C" EXPORT_API void freeMesh(float** vertices, int** triangles)
    {
        delete [] *vertices;
        *vertices = 0;
        delete [] *triangles;
        *triangles = 0;
    }

    void finalizeContext(BuildContext* context
        , char* messages, int messagesLength)
    {
        if (context->getLogEnabled())
        {
            int copyLength = 
                rcMin(messagesLength, context->getMessagePoolLength());
            memcpy(messages, context->getMessagePool(), copyLength);
        }

        delete context;
        context = 0;
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

    void deriveBounds(const float* vertices
        , int vertLength
        , float* bounds)
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

    void removeDuplicateVerts(int vertCount
        , const float* sourceVerts 
        , float* resultVerts
        , int* indicesMap
        , int& resultVertCount)
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
                if (sloppyEquals(sourceVerts[pi+0], resultVerts[pj+0])
                    && sloppyEquals(sourceVerts[pi+1], resultVerts[pj+1])
                    && sloppyEquals(sourceVerts[pi+2], resultVerts[pj+2]))
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

        resultVertCount = resultCount;

    };

    void flattenDetailMesh(const rcPolyMeshDetail* detailMesh
        , float** resultVerts
        , int& resultVertCount
        , int* resultTriangles)
    {
        /*
         * Remember: The tris array has a stride of 4: 3 indices + flags
         *
         * The detail meshes are completely independant, which results
         * in duplicate verts.  This process removes the duplicates.
         */ 

        float* uniqueVerts = new float[(detailMesh->nverts)*3];
        int* vertMap = new int[detailMesh->nverts];
        
        removeDuplicateVerts(detailMesh->nverts
            , detailMesh->verts
            , uniqueVerts
            , vertMap
            , resultVertCount);

        int resultVertLength = resultVertCount*3;
        float* vertices = new float[resultVertLength];
        *resultVerts = vertices;

        // Copy unique vertices to the output.
        // Using resultVertLength avoids copying the trash since uniqueVerts
        // was sized to assume no duplicate vertices. (An unlikely scenario.)
        for (int i = 0; i < resultVertLength; i++)
        {
            vertices[i] = uniqueVerts[i];
        }

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
                    resultTriangles[pCurrentTri] = vertMap[vBase+tri[i]];
                    pCurrentTri++;
                }
            }
        }

        delete [] vertMap;
    }

    extern "C" EXPORT_API bool buildSimpleMesh(Configuration config
        , float* sourceVerts
        , int sourceVertsLength
        , int* sourceTriangles
        , int sourceTrianglesLength
        , float** resultVerts
        , int* resultVertLength
        , int** resultTriangles
        , int* resultTrianglesLength
        , char* messages
        , int messagesLength
        , int messageDetail)
    {

        /*
         * Message Detail:
         *    0 - None
         *    1 - Brief (Errors and warnings.)
         *    2 - Summary
         *    3 - Trace

        /**********************************************************************
         * Initialization
         *********************************************************************/

        BuildContext* context = new BuildContext();
        context->enableLog(messageDetail <= 0 || messagesLength <= 0 ?
            false : true);

        // Some convenience variables.
        const int vertCount = sourceVertsLength / 3;
        const int triangleCount = sourceTrianglesLength / 3;

	    // Prepare timer and make initial log entry.
        // Design Note: Since timer is not implemented by current context class
        // this really doesn't do anything.
	    context->resetTimers();
	    context->startTimer(RC_TIMER_TOTAL);
        if (messageDetail > 1)
            context->log(RC_LOG_PROGRESS, "Building mesh: Simple");

        applyStandardLimits(&config);

        // Derive various settings.

        float* bounds = new float[6];
        int height, width;

        deriveBounds(sourceVerts, sourceVertsLength, bounds);
	    rcCalcGridSize(&bounds[0], &bounds[3], config.xzResolution
            , &width, &height);

        int vxMaxTraversableStep = 
            (int)floorf(config.maxTraversableStep / config.yResolution);
        int vxMinTraversableHeight =
            (int)ceilf(config.minTraversableHeight / config.yResolution);
        int vxTraversableAreaBorderSize= (int)ceilf(
            config.traversableAreaBorderSize / config.xzResolution);
        int vxHeightfieldBorderSize= (int)ceilf(
            config.heightfieldBorderSize / config.xzResolution);
        int vxMaxEdgeLength = (int)ceilf(
            config.maxEdgeLength / config.xzResolution);

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
                , "Config: xzResolution: %.3f wu"
                , config.xzResolution);
            context->log(RC_LOG_PROGRESS
                , "Config: yResolution: %.3f wu"
                , config.yResolution);
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
                , "Config: minUnconnectedRegionSize: %d"
                , config.minUnconnectedRegionSize);
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
            , config.xzResolution
            , config.yResolution))
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
            finalizeContext(context, messages, messagesLength);
            return false;
        }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Initialized solid heightfield.");

	    // Allocate array that can hold triangle area information.
	    unsigned char* triangleAreas = new unsigned char[triangleCount];
	    if (!triangleAreas)
	    {
            finalizeContext(context, messages, messagesLength);
		    context->log(RC_LOG_ERROR
                , "Out of memory: triangleAreas (%d).", triangleCount);
		    return false;
	    }
    	
	    // Mark triangles which are walkable based on their slope
	    rcMarkWalkableTriangles(context
            , config.maxTraversableSlope
            , sourceVerts, vertCount
            , sourceTriangles, triangleCount
            , triangleAreas);

	    rcRasterizeTriangles(context
            , sourceVerts, vertCount
            , sourceTriangles, triangleAreas, triangleCount
            , *solidHeightfield);

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
            finalizeContext(context, messages, messagesLength);
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
            finalizeContext(context, messages, messagesLength);
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
            finalizeContext(context, messages, messagesLength);
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
            , config.minUnconnectedRegionSize
            , config.mergeRegionSize))
	    {
            rcFreeCompactHeightfield(compactHeightfield);
            finalizeContext(context, messages, messagesLength);
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
            finalizeContext(context, messages, messagesLength);
            return false;
        }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built contours: %d contours."
                , contourSet->nconts);

        /**********************************************************************
         * Build polygon mesh.
         *********************************************************************/

        rcPolyMesh* polyMesh = rcAllocPolyMesh();
	    if (!polyMesh)
	    {
            context->log(RC_LOG_ERROR
                , "Out of memory: Poly mesh.");
		    failed = true;
	    }
	    else if (!rcBuildPolyMesh(context
            , *contourSet
            , config.maxVertsPerPoly
            , *polyMesh))
	    {
            rcFreePolyMesh(polyMesh);
		    context->log(RC_LOG_ERROR
                , "Could not create polygon mesh.");
            failed = true;
	    }
    	
		rcFreeContourSet(contourSet);
		contourSet = 0;

        if (failed)
        {
            rcFreeCompactHeightfield(compactHeightfield);
            finalizeContext(context, messages, messagesLength);
            return false;
        }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built poly mesh: %d polygons."
                , polyMesh->maxpolys);

        /**********************************************************************
         * Build detail mesh.
         *********************************************************************/
    	
        rcPolyMeshDetail* detailMesh = rcAllocPolyMeshDetail();
	    if (!detailMesh)
	    {
		    context->log(RC_LOG_ERROR
                , "Out of memory: Detail mesh.");
		    failed = true;
	    }
	    else if (!rcBuildPolyMeshDetail(context
            , *polyMesh
            , *compactHeightfield
            , config.contourSampleDistance
            , config.contourMaxDeviation
            , *detailMesh))
	    {
		    context->log(RC_LOG_ERROR
                , "Could not build detail mesh.");
            rcFreePolyMeshDetail(detailMesh);
            failed = true;
	    }

		rcFreeCompactHeightfield(compactHeightfield);
		compactHeightfield = 0;
        rcFreePolyMesh(polyMesh);
        polyMesh = 0;

        if (failed)
        {
            finalizeContext(context, messages, messagesLength);
            return false;
        }

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Built detail mesh: %d submeshes, %d vertices, %d triangles"
                , detailMesh->nmeshes
                , detailMesh->nverts
                , detailMesh->ntris);

        if (detailMesh->nverts == 0)
        {
            context->log(RC_LOG_WARNING
                , "Build process did not result in a final mesh.");
            finalizeContext(context, messages, messagesLength);
            return false;
        }

        /**********************************************************************
         * Transfer detail mesh data to result arrays.
         *********************************************************************/

        *resultTriangles = new int[(detailMesh->ntris)*3];
        *resultTrianglesLength = (detailMesh->ntris)*3;

        flattenDetailMesh(detailMesh
            , resultVerts
            , *resultVertLength
            , *resultTriangles);
        // Remember: Result vert length currently contains a count, 
        // not a length.

        if (messageDetail > 2)
            context->log(RC_LOG_PROGRESS
                , "Vertices merged: %d."
                , detailMesh->nverts - (*resultVertLength));

        // Convert count to length.
        *resultVertLength  = (*resultVertLength) * 3;

        rcFreePolyMeshDetail(detailMesh);
        detailMesh = 0;

        if (messageDetail > 1)
            context->log(RC_LOG_PROGRESS
                , "Built simple mesh: %d vertices, %d triangles."
                , *resultVertLength / 3
                , *resultTrianglesLength / 3);

        finalizeContext(context, messages, messagesLength);

        return true;
    }
}
}
}
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
#ifndef CAIRCN_DETOURNAVMESHQUERY_EX_H
#define CAIRCN_DETOURNAVMESHQUERY_EX_H

#include "DetourNavMeshQuery.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

extern "C"
{

    // Externs for dtQueryFilter.

    EXPORT_API dtQueryFilter* dtqfAlloc();
    
    EXPORT_API void dtqfFree(dtQueryFilter* filter);
    
    EXPORT_API void dtqfSetAreaCost(dtQueryFilter* filter
        , const int index
        , const float cost);

    EXPORT_API float dtqfGetAreaCost(dtQueryFilter* filter
        , const int index);
    
    EXPORT_API void dtqfSetIncludeFlags(dtQueryFilter* filter
        , const unsigned short flags);
    
    EXPORT_API unsigned short dtqfGetIncludeFlags(dtQueryFilter* filter);

    EXPORT_API void dtqfSetExcludeFlags(dtQueryFilter* filter
        , const unsigned short flags);

    EXPORT_API unsigned short dtqfGetExcludeFlags(dtQueryFilter* filter);

    // Externs for dtNavMeshQuery.

    EXPORT_API void dtnqFree(dtNavMeshQuery** pNavQuery);

	EXPORT_API dtStatus dtqGetPolyWallSegments(dtNavMeshQuery* query
        , dtPolyRef ref
        , const dtQueryFilter* filter
        , float* segmentVerts
        , dtPolyRef* segmentRefs
        , int* segmentCount
        , const int maxSegments);

	EXPORT_API dtStatus dtqFindNearestPoly(dtNavMeshQuery* query
        , const float* center
        , const float* extents
		, const dtQueryFilter* filter
		, dtPolyRef* nearestRef
        , float* nearestPt);

    EXPORT_API dtStatus dtqQueryPolygons(dtNavMeshQuery* query 
            , const float* center
            , const float* extents
		    , const dtQueryFilter* filter
		    , dtPolyRef* polyIds
            , int* polyCount
            , const int maxPolys);

	EXPORT_API dtStatus dtqFindPolysAroundCircle(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , const float* centerPos
        , const float radius
	    , const dtQueryFilter* filter
	    , dtPolyRef* resultPolyRefs
        , dtPolyRef* resultParentRefs
        , float* resultCosts
	    , int* resultCount
        , const int maxResult);

	EXPORT_API dtStatus dtqFindPolysAroundShape(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , const float* verts
        , const int nverts
	    , const dtQueryFilter* filter
	    , dtPolyRef* resultRef
        , dtPolyRef* resultParent
        , float* resultCost
	    , int* resultCount
        , const int maxResult);

	EXPORT_API dtStatus dtqFindLocalNeighbourhood(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , const float* centerPos
        , const float radius
	    , const dtQueryFilter* filter
	    , dtPolyRef* resultRef
        , dtPolyRef* resultParent
	    , int* resultCount
        , const int maxResult);

	EXPORT_API dtStatus dtqClosestPointOnPoly(dtNavMeshQuery* query 
        , dtPolyRef ref
        , const float* pos
        , float* closest);

	EXPORT_API dtStatus dtqClosestPointOnPolyBoundary(dtNavMeshQuery* query 
        , dtPolyRef ref
        , const float* pos
        , float* closest);

	EXPORT_API dtStatus dtqGetPolyHeight(dtNavMeshQuery* query
        , dtPolyRef ref
        , const float* pos
        , float* height);

	EXPORT_API dtStatus dtqFindDistanceToWall(dtNavMeshQuery* query
        , dtPolyRef startRef
        , const float* centerPos
        , const float maxRadius
	    , const dtQueryFilter* filter
	    , float* hitDist
        , float* hitPos
        , float* hitNormal);
    
	EXPORT_API dtStatus dtqFindPath(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , dtPolyRef endRef
		, const float* startPos
        , const float* endPos
		, const dtQueryFilter* filter
		, dtPolyRef* path
        , int* pathCount
        , const int maxPath);

    EXPORT_API bool dtqIsInClosedList(dtNavMeshQuery* query
            , dtPolyRef ref);
	
	EXPORT_API dtStatus dtqRaycast(dtNavMeshQuery* query
        , dtPolyRef startRef
        , const float* startPos
        , const float* endPos
	    , const dtQueryFilter* filter
	    , float* t
        , float* hitNormal
        , dtPolyRef* path
        , int* pathCount
        , const int maxPath);


	EXPORT_API dtStatus dtqFindStraightPath(dtNavMeshQuery* query
        , const float* startPos
        , const float* endPos
		, const dtPolyRef* path
        , const int pathSize
	    , float* straightPath
        , unsigned char* straightPathFlags
        , dtPolyRef* straightPathRefs
	    , int* straightPathCount
        , const int maxStraightPath);
	
	EXPORT_API dtStatus dtqMoveAlongSurface(dtNavMeshQuery* query
        , dtPolyRef startRef
        , const float* startPos
        , const float* endPos
	    , const dtQueryFilter* filter
	    , float* resultPos
        , dtPolyRef* visited
        , int* visitedCount
        , const int maxVisitedSize);

    EXPORT_API dtStatus dtqInitSlicedFindPath(dtNavMeshQuery* query
        , dtPolyRef startRef
        , dtPolyRef endRef
        , const float* startPos
        , const float* endPos
        , const dtQueryFilter* filter);

    EXPORT_API dtStatus dtqUpdateSlicedFindPath(dtNavMeshQuery* query
        , const int maxIter
        , int* doneIters);

    EXPORT_API dtStatus dtqFinalizeSlicedFindPath(dtNavMeshQuery* query
        , dtPolyRef* path
        , int* pathCount
        , const int maxPath);
}

#endif
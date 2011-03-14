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
		, dtPolyRef* polys
        , int* polyCount
        , const int maxPolys);

	EXPORT_API dtStatus dtqFindPath(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , dtPolyRef endRef
		, const float* startPos
        , const float* endPos
		, const dtQueryFilter* filter
		, dtPolyRef* path
        , int* pathCount
        , const int maxPath);

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

	EXPORT_API dtStatus dtqFinalizeSlicedFindPathPartial(dtNavMeshQuery* query 
        , const dtPolyRef* existing
        , const int existingSize
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
	
	EXPORT_API dtStatus dtqFindDistanceToWall(dtNavMeshQuery* query
        , dtPolyRef startRef
        , const float* centerPos
        , const float maxRadius
	    , const dtQueryFilter* filter
	    , float* hitDist
        , float* hitPos
        , float* hitNormal);
	
	EXPORT_API dtStatus dtqFindPolysAroundCircle(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , const float* centerPos
        , const float radius
	    , const dtQueryFilter* filter
	    , dtPolyRef* resultRef
        , dtPolyRef* resultParent
        , float* resultCost
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
	
	EXPORT_API dtStatus dtqGetPolyWallSegments(dtNavMeshQuery* query 
        , dtPolyRef ref
        , const dtQueryFilter* filter
	    , float* segments
        , int* segmentCount
        , const int maxSegments);
	
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

	EXPORT_API bool dtqIsInClosedList(dtNavMeshQuery* query
        , dtPolyRef ref);
}

#endif
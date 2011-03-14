#include <string>
#include "DetourNavMeshQueryEx.h"

extern "C"
{
    EXPORT_API void dtnqFree(dtNavMeshQuery** pNavQuery)
    {
        dtFreeNavMeshQuery(*pNavQuery);
    }

	EXPORT_API dtStatus dtqFindNearestPoly(dtNavMeshQuery* query
        , const float* center
        , const float* extents
		, const dtQueryFilter* filter
		, dtPolyRef* nearestRef
        , float* nearestPt)
    {
        return query->findNearestPoly(center
            , extents
            , filter
            , nearestRef
            , nearestPt);
    }
}
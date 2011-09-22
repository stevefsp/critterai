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
#include <string.h>
#include "DetourCommon.h"
#include "DetourPathCorridor.h"
#include "DetourEx.h"

extern "C"
{
	EXPORT_API dtPathCorridor* dtpcAlloc(const int maxPath)
	{
		dtPathCorridor* corridor = new dtPathCorridor();
		if (corridor)
			corridor->init(maxPath);
		return corridor;
	}

	EXPORT_API void dtpcFree(dtPathCorridor* corridor)
	{
		if (corridor)
			corridor->~dtPathCorridor();
	}

	EXPORT_API void dtpcReset(dtPathCorridor* corridor
			, dtPolyRef ref, const float* pos)
	{
		if (corridor && pos)
			corridor->reset(ref, pos);
	}

	EXPORT_API int dtpcFindCorners(dtPathCorridor* corridor
		, float* cornerVerts
		, unsigned char* cornerFlags
		, dtPolyRef* cornerPolys
		, const int maxCorners
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	{
		if (corridor)
			return corridor->findCorners(cornerVerts
				, cornerFlags
				, cornerPolys
				, maxCorners
				, navquery
				, filter);

		return 0;
	}

	 EXPORT_API void dtpcOptimizePathVisibility(dtPathCorridor* corridor
		 , const float* next
		 , const float pathOptimizationRange
		 , dtNavMeshQuery* navquery
		 , const dtQueryFilter* filter)
	 {
		if (corridor)
			corridor->optimizePathVisibility(next
			, pathOptimizationRange
			, navquery
			, filter);
	 }

	 EXPORT_API bool dtpcOptimizePathTopology(dtPathCorridor* corridor
		 , dtNavMeshQuery* navquery
		 , const dtQueryFilter* filter)
	 {
		if (corridor)
			return corridor->optimizePathTopology(navquery, filter);
		return false;
	 }
	
	 EXPORT_API bool dtpcMoveOverOffmeshConnection(dtPathCorridor* corridor
		 , dtPolyRef offMeshConRef
		 , dtPolyRef* refs
		 , float* startPos
		 , float* endPos
		 , dtNavMeshQuery* navquery)
	 {
		if (corridor)
			return corridor->moveOverOffmeshConnection(offMeshConRef
			, refs
			, startPos
			, endPos
			, navquery);
		return false;
	 }
	
	 EXPORT_API dtPolyRef dtpcMovePosition(dtPathCorridor* corridor
		 , const float* npos
		 , dtNavMeshQuery* navquery
		 , const dtQueryFilter* filter
		 , float* pos)
	 {
		if (!corridor)
			return 0;

		corridor->movePosition(npos, navquery, filter);

		if (pos)
			dtVcopy(pos, corridor->getPos());

		return corridor->getFirstPoly();
	 }

	 EXPORT_API dtPolyRef dtpcMoveTargetPosition(dtPathCorridor* corridor
		 , const float* npos
		 , dtNavMeshQuery* navquery
		 , const dtQueryFilter* filter
		 , float* pos)
	 {
		if (!corridor)
			return 0;

		corridor->moveTargetPosition(npos, navquery, filter);

		if (pos)
			dtVcopy(pos, corridor->getTarget());

		return corridor->getLastPoly();

	 }
	
	 EXPORT_API void dtpcSetCorridor(dtPathCorridor* corridor
		 , const float* target
		 , const dtPolyRef* polys
		 , const int npolys)
	 {
		if (corridor)
			corridor->setCorridor(target, polys, npolys);
	 }
	
	 EXPORT_API dtPolyRef dtpcGetPos(dtPathCorridor* corridor
		 , float* pos)
	 {
		if (!corridor || !pos)
			return 0;

		dtVcopy(pos, corridor->getPos());
		return corridor->getFirstPoly();
	 }

	 EXPORT_API dtPolyRef dtpcGetTarget(dtPathCorridor* corridor
		 , float* target)
	 {
		if (!corridor || !target)
			return 0;

		dtVcopy(target, corridor->getTarget());
		return corridor->getLastPoly();
	 }
	 	
	 EXPORT_API dtPolyRef dtpcGetFirstPoly(dtPathCorridor* corridor)
	 {
		if (corridor)
			return corridor->getFirstPoly();
		return 0;
	 }

	 EXPORT_API dtPolyRef dtpcGetLastPoly(dtPathCorridor* corridor)
	 {
		if (corridor)
			return corridor->getLastPoly();
		return 0;
	 }
	
	 EXPORT_API int dtpcGetPath(dtPathCorridor* corridor
		 , dtPolyRef* path
		 , int maxPath)
	 {
		if (!corridor || !path || maxPath < 1)
			return 0;

		int count = dtMin(maxPath, corridor->getPathCount());

		memcpy(path, corridor->getPath(), sizeof(dtPolyRef) * count);

		return count;
	 }

	 EXPORT_API int dtpcGetPathCount(dtPathCorridor* corridor)
	 {
		if (corridor)
			return corridor->getPathCount();
		return 0;
	 }

	EXPORT_API bool dtpcGetData(dtPathCorridor* corridor
		, rcnPathCorridorData* result)
	{
		if (!corridor 
			|| !result 
			|| corridor->getPathCount() > MAX_RCN_PATH_CORRIDOR_SIZE)
		{
			return false;
		}

		int count = corridor->getPathCount();

		result->pathCount = count;
		dtVcopy(result->position, corridor->getPos());
		dtVcopy(result->target, corridor->getTarget());
		memcpy(&result->path[0]
			, corridor->getPath()
			, sizeof(dtPolyRef) * count);

		return true;
	}

	EXPORT_API bool dtpcIsValid(dtPathCorridor* corridor
		, const int maxLookAhead
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	{
		if (corridor)
			return corridor->isValid(maxLookAhead, navquery, filter);
		return false;
	}
}
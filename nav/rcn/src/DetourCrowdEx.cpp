#include <string.h>
#include "DetourCrowdEx.h"
#include "DetourCommon.h"

// This number represents the size of the dtCrowdAgent structure from
// the first field after the class fields to the end of the structure.
// (Inclusive)
// 
// Validation note: An easy way to validate the value is not too high
// is to temporarily decrease the int count by 1.  If interop unit tests 
// fail on validation of the last value of the rcnCrowdAgentData structure, 
// then the original upper bounds is good and no over-copying is occuring.
//
static const int RCNCAD_POST_SIZE = 
    (sizeof(float) * (19 + DT_CROWDAGENT_MAX_CORNERS * 3))
    + sizeof(dtCrowdNeighbour) * DT_CROWDAGENT_MAX_NEIGHBOURS
    + sizeof(int) * 2
    + sizeof(dtPolyRef) * DT_CROWDAGENT_MAX_CORNERS
    + sizeof(unsigned char) * DT_CROWDAGENT_MAX_CORNERS
    + sizeof(dtCrowdAgentParams);

extern "C"
{
    EXPORT_API dtCrowd* dtcDetourCrowdAlloc(const int maxAgents
        , const float maxAgentRadius
        , dtNavMesh* nav)
    {
        dtCrowd* result = new dtCrowd();
        if (result)
            result->init(maxAgents, maxAgentRadius, nav);
        return result;
    }

    EXPORT_API void dtcDetourCrowdFree(dtCrowd* crowd)
    {
        if (crowd)
            crowd->~dtCrowd();
    }

	EXPORT_API void dtcSetObstacleAvoidanceParams(dtCrowd* crowd
        , const int idx
        , dtObstacleAvoidanceParams params)
    {
        crowd->setObstacleAvoidanceParams(idx, &params);
    }

    EXPORT_API const dtObstacleAvoidanceParams* dtcGetObstacleAvoidanceParams(
        dtCrowd* crowd
        , const int idx)
    {
        return crowd->getObstacleAvoidanceParams(idx);
    }
	
	EXPORT_API const dtCrowdAgent* dtcGetAgent(dtCrowd* crowd
        , const int idx)
    {
        return crowd->getAgent(idx);
    }

	EXPORT_API const int dtcGetAgentCount(dtCrowd* crowd)
    {
        return crowd->getAgentCount();
    }
	
	EXPORT_API int dtcAddAgent(dtCrowd* crowd
        , const float* pos, const dtCrowdAgentParams params)
    {
        return crowd->addAgent(pos, &params);
    }

	EXPORT_API void dtcUpdateAgentParameters(dtCrowd* crowd
        , const int idx, const dtCrowdAgentParams params)
    {
        crowd->updateAgentParameters(idx, &params);
    }

	EXPORT_API void dtcRemoveAgent(dtCrowd* crowd
        , const int idx)
    {
        crowd->removeAgent(idx);
    }
	
	EXPORT_API bool dtcRequestMoveTarget(dtCrowd* crowd
        , const int idx
        , dtPolyRef ref
        , const float* pos)
    {
        return crowd->requestMoveTarget(idx, ref, pos);
    }

	EXPORT_API bool dtcAdjustMoveTarget(dtCrowd* crowd
        , const int idx
        , dtPolyRef ref
        , const float* pos)
    {
        return crowd->adjustMoveTarget(idx, ref, pos);
    }

	//EXPORT_API int dtcGetActiveAgents(dtCrowd* crowd
 //       , dtCrowdAgent** agents, const int maxAgents)
 //   {
 //       return crowd->getActiveAgents(agents, maxAgents);
 //   }

	EXPORT_API void dtcUpdate(dtCrowd* crowd, const float dt)
    {
        crowd->update(dt, nullptr);
    }
	
	EXPORT_API const dtQueryFilter* dtcGetFilter(dtCrowd* crowd)
    {
        return crowd->getFilter();
    }

	EXPORT_API void dtcGetQueryExtents(dtCrowd* crowd, float* extents)
    {
        const float* e = crowd->getQueryExtents();
        dtVcopy(extents, e);
    }
	
	EXPORT_API int dtcGetVelocitySampleCount(dtCrowd* crowd)
    {
        return crowd->getVelocitySampleCount();
    }
	
	EXPORT_API const dtProximityGrid* dtcGetGrid(dtCrowd* crowd)
    {
        return crowd->getGrid();
    }

	//EXPORT_API const dtPathQueue* dtcGetPathQueue(dtCrowd* crowd)
 //   {
 //       return crowd->getPathQueue();
 //   }

	EXPORT_API const dtNavMeshQuery* dtcGetNavMeshQuery(dtCrowd* crowd)
    {
        return crowd->getNavMeshQuery();
    }

    EXPORT_API int dtcaGetAgentCoreData(dtCrowd* crowd
        , rcnCrowdAgentCoreData* agentData
        , const int agentDataSize)
    {
        int agentCount = 0;
        for (int i = 0
            ; i < crowd->getAgentCount() && agentCount < agentDataSize
            ; i++)
        {
		    const dtCrowdAgent* ag = crowd->getAgent(i);
		    if (!ag->active) continue;

            rcnCrowdAgentCoreData* tag = &agentData[agentCount++];

            tag->desiredSpeed = ag->desiredSpeed;
            tag->state = ag->state;
            dtVcopy(&tag->dvel[0], &ag->dvel[0]);
            dtVcopy(&tag->vel[0], &ag->vel[0]);
            dtVcopy(&tag->npos[0], &ag->npos[0]);
        }
        return agentCount;
    }

    EXPORT_API void dtcaGetAgentDebugData(const dtCrowdAgent* a
        , rcnCrowdAgentData* ad)
    {

        // I know.  I'm new to C++.  Shoot me.

        // The primary reason for the custom structures is that
        // I can't get interop to handle the classes embeded
        // in the dtCrowdAgent structure.  The content of the fields
        // after the classes keeps getting corrupted during marshalling.

        // Before class references.
        ad->active = a->active;
        ad->state = a->state;

        // After class references.
        memcpy(&ad->t, &a->t, RCNCAD_POST_SIZE);

        // Copy the corridor data.

        dtVcopy(&ad->corridor.position[0], a->corridor.getPos());
        dtVcopy(&ad->corridor.target[0], a->corridor.getTarget());

        int count = dtMin(a->corridor.getPathCount()
            , MAX_RCN_PATH_CORRIDOR_SIZE);

        memcpy(&ad->corridor.path[0]
            , a->corridor.getPath()
            , sizeof(dtPolyRef) * count);

        ad->corridor.pathCount = count;

        // Copy the segment data.

        dtVcopy(&ad->boundary.center[0], a->boundary.getCenter());

        count = dtMin(a->boundary.getSegmentCount(), MAX_LOCAL_BOUNDARY_SEGS);

        for (int i = 0; i < count; i++)
        {
            memcpy(&ad->boundary.segs[i*6]
                , a->boundary.getSegment(i)
                , sizeof(float) * 6);
        }

        ad->boundary.segmentCount = count;
    }
}
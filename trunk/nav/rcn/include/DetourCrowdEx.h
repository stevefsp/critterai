#ifndef DETOURCROWDEX_H
#define DETOURCROWDEX_H

#include "DetourCrowd.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

static const int MAX_RCN_PATH_CORRIDOR_SIZE = 256;
static const int MAX_LOCAL_BOUNDARY_SEGS = 8;

struct rcnPathCorridorData
{

    float position[3];
    float target[3];

    dtPolyRef path[MAX_RCN_PATH_CORRIDOR_SIZE];
    int pathCount;
};

struct rcnLocalBoundary
{
    float center[3];
    float segs[6 * MAX_LOCAL_BOUNDARY_SEGS];
    int segmentCount;
};

struct rcnCrowdAgentData
{
	unsigned char active;
	unsigned char state;
	
    rcnPathCorridorData corridor;
    rcnLocalBoundary boundary;

    // IMPORTANT:  The size of everything after this point
    // in the structure must be tallied by RCNCAD_POST_SIZE.

	float t;
	float var;

	float topologyOptTime;
	
	dtCrowdNeighbour neis[DT_CROWDAGENT_MAX_NEIGHBOURS];
	int nneis;
	
	float desiredSpeed;

	float npos[3];
	float disp[3];
	float dvel[3];
	float nvel[3];
	float vel[3];

	dtCrowdAgentParams params;

	float cornerVerts[DT_CROWDAGENT_MAX_CORNERS*3];
	unsigned char cornerFlags[DT_CROWDAGENT_MAX_CORNERS];
	dtPolyRef cornerPolys[DT_CROWDAGENT_MAX_CORNERS];
	int ncorners;
};

struct rcnCrowdAgentCoreData
{
	unsigned char state;
	
	float desiredSpeed;

	float npos[3];
	float dvel[3];
	float vel[3];
};

extern "C"
{
    EXPORT_API dtCrowd* dtcDetourCrowdAlloc(const int maxAgents
        , const float maxAgentRadius
        , dtNavMesh* nav);

    EXPORT_API void dtcDetourCrowdFree(dtCrowd* crowd);

	EXPORT_API void dtcSetObstacleAvoidanceParams(dtCrowd* crowd
        , const int idx
        , const dtObstacleAvoidanceParams params);

    EXPORT_API const dtObstacleAvoidanceParams* dtcGetObstacleAvoidanceParams(
        dtCrowd* crowd
        , const int idx);
	
	EXPORT_API const dtCrowdAgent* dtcGetAgent(dtCrowd* crowd
        , const int idx);

	EXPORT_API const int dtcGetAgentCount(dtCrowd* crowd);
	
	EXPORT_API int dtcAddAgent(dtCrowd* crowd
        , const float* pos
        , const dtCrowdAgentParams params);

	EXPORT_API void dtcUpdateAgentParameters(dtCrowd* crowd
        , const int idx
        , const dtCrowdAgentParams params);

	EXPORT_API void dtcRemoveAgent(dtCrowd* crowd
        , const int idx);

    // This functionality is provided on the other side of the interop boundary.
	//EXPORT_API int dtcGetActiveAgents(dtCrowd* crowd
 //       , dtCrowdAgent** agents, const int maxAgents);

	EXPORT_API void dtcUpdate(dtCrowd* crowd, const float dt);
	
	EXPORT_API const dtQueryFilter* dtcGetFilter(dtCrowd* crowd);

	EXPORT_API void dtcGetQueryExtents(dtCrowd* crowd, float* extents);
	
	EXPORT_API int dtcGetVelocitySampleCount(dtCrowd* crowd);
	
	EXPORT_API const dtProximityGrid* dtcGetGrid(dtCrowd* crowd);

    // Don't see a purpose for providing this to the other side of 
    // the interop boundary.
	//EXPORT_API const dtPathQueue* dtcGetPathQueue(dtCrowd* crowd);
    
	EXPORT_API const dtNavMeshQuery* dtcGetNavMeshQuery(dtCrowd* crowd);

	EXPORT_API bool dtcRequestMoveTarget(dtCrowd* crowd
        , const int idx
        , dtPolyRef ref
        , const float* pos);

	EXPORT_API bool dtcAdjustMoveTarget(dtCrowd* crowd
        , const int idx
        , dtPolyRef ref
        , const float* pos);

    EXPORT_API int dtcaGetAgentCoreData(dtCrowd* crowd
            , rcnCrowdAgentCoreData* agentData
            , const int agentDataSize);

    EXPORT_API void dtcaGetAgentDebugData(const dtCrowdAgent* agent
            , rcnCrowdAgentData* agentData);
}

#endif

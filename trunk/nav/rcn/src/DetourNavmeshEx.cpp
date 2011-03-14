#include <string>
#include "DetourNavMeshEx.h"

extern "C"
{
    EXPORT_API void freeDTNavMesh(dtNavMesh** pNavMesh)
    {
        dtFreeNavMesh(*pNavMesh);
    }

}
#ifndef CAIRCN_DETOURNAVMESHEX_H
#define CAIRCN_DETOURNAVMESHEX_H

#include "DetourNavMesh.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

extern "C"
{
    EXPORT_API void freeDTNavMesh(dtNavMesh** pNavMesh);
}

#endif
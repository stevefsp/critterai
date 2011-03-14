#ifndef CAIRCN_RECASTEX_H
#define CAIRCN_RECASTEX_H

#include "Recast.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

extern "C"
{
    EXPORT_API void freeRCPolyMesh(rcPolyMesh* pPolyMesh);
    EXPORT_API void freeRCDetailMesh(rcPolyMeshDetail* pDetailMesh);
}

#endif
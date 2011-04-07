#ifndef DETOURPATHCORRIDOREX_H
#define DETOURPATHCORRIDOREX_H

#include "DetourPathCorridor.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

extern "C"
{

}

#endif

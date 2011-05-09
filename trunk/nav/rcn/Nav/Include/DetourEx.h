#ifndef CAI_DETOUREX_H
#define CAI_DETOUREX_H

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

#endif

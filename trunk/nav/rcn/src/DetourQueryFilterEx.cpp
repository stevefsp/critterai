#include "DetourNavMeshQueryEx.h"

extern "C"
{
    EXPORT_API dtQueryFilter* dtqfAlloc()
    {
        return new dtQueryFilter();
    }
    
    EXPORT_API void dtqfFree(dtQueryFilter* filter)
    {
        if (filter)
             filter->~dtQueryFilter();
    }
    
    EXPORT_API void dtqfSetAreaCost(dtQueryFilter* filter
        , const int index
        , const float cost)
    {
        filter->setAreaCost(index, cost);
    }

    EXPORT_API float dtqfGetAreaCost(dtQueryFilter* filter
        , const int index)
    {
        return filter->getAreaCost(index);
    }
    
    EXPORT_API void dtqfSetIncludeFlags(dtQueryFilter* filter
        , const unsigned short flags)
    {
        filter->setIncludeFlags(flags);
    }
    
    EXPORT_API unsigned short dtqfGetIncludeFlags(dtQueryFilter* filter)
    {
        return filter->getIncludeFlags();
    }

    EXPORT_API void dtqfSetExcludeFlags(dtQueryFilter* filter
        , const unsigned short flags)
    {
        filter->setExcludeFlags(flags);
    }

    EXPORT_API unsigned short dtqfGetExcludeFlags(dtQueryFilter* filter)
    {
        return filter->getExcludeFlags();
    }
}
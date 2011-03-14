#include "RecastEx.h"
#include "RecastAlloc.h"

extern "C"
{
    EXPORT_API void freeRCPolyMesh(rcPolyMesh* pPolyMesh)
    {
        // Dev Note: Expect that the structure was allocated externally.
        // So can't use rcFreePolyMesh.  Instead, only free the portions
        // expected to have been allocated internally.

        if (!pPolyMesh) 
            return;

        rcFree(pPolyMesh->verts);
        rcFree(pPolyMesh->polys);
        rcFree(pPolyMesh->regs);
	    rcFree(pPolyMesh->flags);
	    rcFree(pPolyMesh->areas);

        pPolyMesh->areas = 0;
        pPolyMesh->flags = 0;
        pPolyMesh->polys = 0;
        pPolyMesh->regs = 0;
        pPolyMesh->verts = 0;
        pPolyMesh->npolys = 0;
        pPolyMesh->nverts = 0;
    }

    EXPORT_API void freeRCDetailMesh(rcPolyMeshDetail* pDetailMesh)
    {
        // Dev Note: Expect that the structure was allocated externally.
        // So can't use rcFreePolyMeshDetail.  Instead, only free the portions
        // expected to have been allocated internally.

        if (!pDetailMesh) 
            return;

	    rcFree(pDetailMesh->meshes);
	    rcFree(pDetailMesh->verts);
	    rcFree(pDetailMesh->tris);

        pDetailMesh->meshes = 0;
        pDetailMesh->tris = 0;
        pDetailMesh->verts = 0;
        pDetailMesh->ntris = 0;
        pDetailMesh->nverts = 0;
        pDetailMesh->nmeshes = 0;
    }

}
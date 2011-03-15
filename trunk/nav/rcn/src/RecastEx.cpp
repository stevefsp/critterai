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
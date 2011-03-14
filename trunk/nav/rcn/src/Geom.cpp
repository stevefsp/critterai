#include "RCN.h"

// Derives the minimum and maximum vertices for the AABB encompassing
// the vertices.  bounds = (minX, minY, minZ, maxX, maxY, maxZ)
void rcnDeriveBounds3(const float* vertices, int vertLength, float* bounds)
{
    if (vertLength <= 0)
        return;
    bounds[0] = vertices[0];
    bounds[1] = vertices[1];
    bounds[2] = vertices[2];
    bounds[3] = vertices[0];
    bounds[4] = vertices[1];
    bounds[5] = vertices[2];
    for (int p = 0; p < vertLength; p += 3)
    {
        bounds[0] = rcMin(bounds[0], vertices[p+0]);
        bounds[1] = rcMin(bounds[1], vertices[p+1]);
        bounds[2] = rcMin(bounds[2], vertices[p+2]);
        bounds[3] = rcMax(bounds[3], vertices[p+0]);
        bounds[4] = rcMax(bounds[4], vertices[p+1]);
        bounds[5] = rcMax(bounds[5], vertices[p+2]);
    }
}

// Iterates an array of vertices and copies the unique vertices to
// another array.
// vertCount - The number of vertices in sourceVerts.
// sourceVerts - The source vertices in the form (x, y, z).  Length equals
// vertCount * 3.
// resultVerts - An initialized array to load unique vertices into.  
// Values will be in the form (x, y, z).  It must be the same length as 
// sourceVerts.
// indicesMap - An initialized array of length vertCount which will hold
// the map of indices from sourceVerts to resultVerts.  E.g. If the value
// at index 5 is 2, then sourceVerts[5*3] is located at resultVerts[2*3].
// Returns: The number of unique vertices found.
// Notes:
//    If there are no duplicate vertices, the content of the source and
//    result arrays will be identical and vertCount will equal 
//    resultVertCount.
int rcnRemoveDuplicateVerts(int vertCount
    , const float* sourceVerts 
    , float* resultVerts
    , int* indicesMap)
{
    int resultCount = 0;

    for (int i = 0; i < vertCount; i++)
    {
        int index = resultCount;
        int pi = i*3;
        // Check to see if this vertex has already been seen.
        for (int j = 0; j < resultCount; j++)
        {
            int pj = j*3;
            if (rcnSloppyEquals(sourceVerts[pi+0], resultVerts[pj+0])
                && rcnSloppyEquals(sourceVerts[pi+1], resultVerts[pj+1])
                && rcnSloppyEquals(sourceVerts[pi+2], resultVerts[pj+2]))
            {
                // This vertex already exists.
                index = j;
                break;
            }
        }
        indicesMap[i] = index;
        if (index == resultCount)
        {
            // This is a new vertex.
            resultVerts[resultCount*3+0] = sourceVerts[pi+0];
            resultVerts[resultCount*3+1] = sourceVerts[pi+1];
            resultVerts[resultCount*3+2] = sourceVerts[pi+2];
            resultCount++;
        }
    }

    return resultCount;

};

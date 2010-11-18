/*
 * Copyright (c) 2010 Stephen A. Pratt
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
#include "NMGenCLI.h"

namespace org 
{
namespace critterai
{
namespace nmgen
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="vertLength">
    /// The length of the vertices array. (Should be 3 * vertex count.)
    /// </param>
    /// <param name="triLength">
    /// The length of the triangles array. (Should be 3 * triangle count.)
    /// </param>
    TriangleMesh::TriangleMesh(int vertLength, int triLength)
    {
        vertices = gcnew array<float>(vertLength);
        triangles = gcnew array<int>(triLength);
    }

    /// <summary>
    /// Gets a flattened array of vertices for the specified triangle.
    /// </summary>
    /// <param name="index">The zero based index of the triangle to retrieve.
    /// </param>
    /// <returns>
    /// A flattened array of vertices for the specified triangle in the
    /// form (ax, ay, az, bx, by, bz, cx, cy, cz).
    /// </returns>
    array<float>^ TriangleMesh::GetTriangleVerts(int index)
    {
        int pTriangle = index*3;
        if (index < 0 || pTriangle >= triangles->Length)
            return nullptr;
        
        array<float>^ result = gcnew array<float>(9);
        
        for (int i = 0; i < 3; i++)
        {
            int pVert = triangles[pTriangle+i]*3;
            result[i*3] = vertices[pVert];
            result[i*3+1] = vertices[pVert+1];
            result[i*3+2] = vertices[pVert+2];
        }
        
        return result;
    }

    int TriangleMesh::TriangleCount::get()
    {
        return (triangles == nullptr ? 0 : triangles->Length / 3);
    }

    int TriangleMesh::VertCount::get()
    {
        return (vertices == nullptr ? 0 : vertices->Length / 3);
    }
}
}
}
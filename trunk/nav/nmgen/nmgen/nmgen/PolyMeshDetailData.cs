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
using System;

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents data for a <see cref="PolyMeshDetail"/> object.
    /// </summary>
    /// <remarks>
    /// <p>This class is not compatible with Unity serialization.</p>
    /// </remarks>
    /// <seealso cref="PolyMeshDetail"/>
    [Serializable]
    public sealed class PolyMeshDetailData
    {
        /// <summary>
        /// The sub-mesh data.
        /// [Form: (baseVertIndex, vertCount, baseTriIndex, triCount) 
        /// * meshCount] [Size: >= 4 * meshCount]
        /// </summary>
        /// <remarks>
        /// <p>Maximum number of vertices per sub-mesh: 127<br/>
        /// Maximum number of triangles per sub-mesh: 255</p>
        /// <p>An example of iterating the triangles in a sub-mesh. (Where
        /// iMesh is the index for the sub-mesh within detailMesh.)</p>
        /// <code>
        ///     int pMesh = iMesh * 4;
        ///     int pVertBase = meshes[pMesh + 0] * 3;
        ///     int pTriBase = meshes[pMesh + 2] * 4;
        ///     int tCount = meshes[pMesh + 3];
        ///     int vertX, vertY, vertZ;
        ///
        ///     for (int iTri = 0; iTri &lt; tCount; iTri++)
        ///     {
        ///        for (int iVert = 0; iVert &lt; 3; iVert++)
        ///        {
        ///            int pVert = pVertBase
        ///                + (tris[pTriBase + (iTri * 4 + iVert)] * 3);
        ///            vertX = verts[pVert + 0];
        ///            vertY = verts[pVert + 1];
        ///            vertZ = verts[pVert + 2];
        ///            // Do something with the vertex.
        ///        }
        ///    }
        /// </code>
        /// </remarks>
        public uint[] meshes;

        /// <summary>
        /// The mesh vertices. 
        /// [Form: (x, y, z) * vertCount] 
        /// [Size: >= 3 * vertCount]
        /// </summary>
        /// <remarks>
        /// <p>The vertices are grouped by sub-mesh and will contain duplicates
        /// since each sub-mesh is independantly defined.</p>
        /// <p>The first group of vertices for each sub-mesh are in the same
        /// order as the vertices for the sub-mesh's associated 
        /// <see cref="PolyMesh"/>polygon.  These vertices are followed by 
        /// any additional detail vertices.  So it the associated polygon has 
        /// 5 vertices, the sub-mesh will have a minimum of 5 vertices and the 
        /// first 5 vertices will be equivalent to the 5 polygon vertices.</p>
        /// </remarks>
        public float[] verts;

        /// <summary>
        /// The mesh triangles.
        /// [Form: vertIndexA, vertIndexB, vertIndexC, flag) * triCount]
        /// [Size: >= 4 * triCount]
        /// </summary>
        /// <remarks>
        /// <p>The triangles are grouped by sub-mesh.</p>
        /// <p><b>Vertices</b></p>
        /// <p> The vertex indices in the triangle array are local to the
        /// sub-mesh, not global.  To translate into an global index in the
        /// vertices array, the values must be offset by the sub-mesh's base
        /// vertex index.</p>
        /// <p>
        /// Example: If the baseVertexIndex for the sub-mesh is 5 and the
        /// triangle entry is (4, 8, 7, 0), then the actual indices for
        /// the vertices are (4 + 5, 8 + 5, 7 + 5).
        /// </p>
        /// <p><b>Flags</b></p>
        /// <p>The flags entry indicates which edges are internal and which
        /// are external to the sub-mesh.</p>
        ///
        /// <p>Internal edges connect to other triangles within the same
        /// sub-mesh.
        /// External edges represent portals to other sub-meshes or the null
        /// region.</p>
        ///
        /// <p>Each flag is stored in a 2-bit position.  Where position 0 is the
        /// lowest 2-bits and position 4 is the highest 2-bits:</p>
        ///
        /// <p>Position 0: Edge AB (>> 0)<br />
        /// Position 1: Edge BC (>> 2)<br />
        /// Position 2: Edge CA (>> 4)<br />
        /// Position 4: Unused</p>
        ///
        /// <p>Testing can be performed as follows:</p>
        /// <code>
        /// if (((flag >> 2) &amp; 0x3) == 0)
        /// {
        ///     // Edge BC is an external edge.
        /// }
        /// </code>
        /// </remarks>
        public byte[] tris;

        /// <summary>
        /// The sub-mesh count.
        /// </summary>
        public int meshCount;

        /// <summary>
        /// The vertex count.
        /// </summary>
        public int vertCount;

        /// <summary>
        /// The triangle count.
        /// </summary>
        public int triCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxVerts">The maximum vertices the object will
        /// hold.</param>
        /// <param name="maxTris">The maximum triangles the object will
        /// hold.</param>
        /// <param name="maxMeshes">The maximum sub-meshes the object
        /// will hold.</param>
        public PolyMeshDetailData(int maxVerts
            , int maxTris
            , int maxMeshes)
        {
            if (maxVerts < 3
                || maxTris < 1
                || maxMeshes < 1)
            {
                return;
            }

            meshes = new uint[maxMeshes * 4];
            tris = new byte[maxTris * 4];
            verts = new float[maxVerts * 3];
        }

        /// <summary>
        /// Clears all object data and resizes the buffers.
        /// </summary>
        /// <param name="maxVerts">The maximum vertices the object will
        /// hold.</param>
        /// <param name="maxTris">The maximum triangles the object will
        /// hold.</param>
        /// <param name="maxMeshes">The maximum sub-meshes the object
        /// will hold.</param>
        public void Reset(int maxVerts
            , int maxTris
            , int maxMeshes)
        {
            Reset();

            meshes = new uint[maxMeshes * 4];
            tris = new byte[maxTris * 4];
            verts = new float[maxVerts * 3];
        }

        private void Reset()
        {
            vertCount = 0;
            triCount = 0;
            meshCount = 0;
            meshes = null;
            tris = null;
            verts = null;
        }

        /// <summary>
        /// Checks the size of the buffers to see if they are large enough
        /// to hold the specified data.
        /// </summary>
        /// <param name="vertCount">The maximum vertices the object needs to
        /// hold.</param>
        /// <param name="triCount">The maximum triangles the object needs to
        /// hold.</param>
        /// <param name="meshCount">The maximum sub-meshes the object
        /// needs to hold.</param>
        /// <returns>TRUE if all buffers are large enough to fit the data.
        /// </returns>
        public bool CanFit(int vertCount
            , int triCount
            , int meshCount)
        {
            if (verts == null || verts.Length < vertCount * 3
                || tris == null || tris.Length < triCount * 4
                || meshes == null || meshes.Length < meshCount * 4)
            {
                return false;
            }
            return true;
        }
    }
}

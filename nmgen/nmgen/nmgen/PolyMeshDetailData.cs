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
        /// </summary>
        public uint[] meshes;

        /// <summary>
        /// The mesh vertices.
        /// </summary>
        public float[] verts;

        /// <summary>
        /// The mesh triangles.
        /// </summary>
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

        public void Reset(int maxVerts
            , int maxTris
            , int maxMeshes)
        {
            Reset();

            meshes = new uint[maxMeshes * 4];
            tris = new byte[maxTris * 4];
            verts = new float[maxVerts * 3];
        }

        public void Reset()
        {
            vertCount = 0;
            triCount = 0;
            meshCount = 0;
            meshes = null;
            tris = null;
            verts = null;
        }

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

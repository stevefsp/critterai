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
using UnityEngine;
using System.Collections.Generic;

namespace org.critterai.geom
{
    /// <summary>
    /// Provides utility methods related to the UnityEngine.Mesh class.
    /// </summary>
    /// <remarks>
    /// <para>WARNING: This class is not meant for general public use,
    /// so the methods often have abnormal parameter restrictions.  Review the
    /// method documentation thoroughly before use!</para>
    /// </remarks>
    public static class MeshUtil
    {
        /*
         * Design notes:
         * 
         * Make sure argument validation is added before making any of the 
         * private methods public.
         * 
         */

        /// <summary>
        /// Estimates the aggregate maximum vertex and triangle count for a mesh.
        /// (Used for sizing buffers.)
        /// </summary>
        /// <remarks>It is costly to determine unique vertex count for meshes
        /// that contain sub-meshes.  No attempt in made, so the 
        /// vertex count may be larger than actually required.</remarks>
        /// <param name="mesh">The mesh to evaluate.</param>
        /// <param name="vertCount">The maximum vertices in the mesh.</param>
        /// <param name="triCount">The number of triangles in the mesh.</param>
        public static void EstimateSize(Mesh mesh
            , out int vertCount
            , out int triCount)
        {
            vertCount = 0;
            triCount = 0;

            if (mesh == null || mesh.triangles.Length == 0)
                return;

            vertCount += mesh.vertexCount;

            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                triCount += mesh.GetTriangles(i).Length;
            }
        }

        public static void EstimateSize(MeshFilter[] filters
            , out int vertCount
            , out int triCount)
        {
            vertCount = 0;
            triCount = 0;

            if (filters == null || filters.Length == 0)
                return;

            foreach (MeshFilter filter in filters)
            {
                if (filter == null || filter.sharedMesh == null)
                    continue;

                Mesh mesh = filter.sharedMesh;

                vertCount += mesh.vertexCount;

                for (int i = 0; i < mesh.subMeshCount; ++i)
                {
                    triCount += mesh.GetTriangles(i).Length;
                }
            }
        }

        /// <summary>
        /// Combines the vertices and triangles from all MeshFilters attached to
        /// the provided GameObjects (including children).  
        /// </summary>
        /// <remarks>
        /// <para>The output parameters will be null if the method return value 
        /// is FALSE.</para>
        /// <para>Vertices will be in world space coordinates.</para>
        /// </remarks>
        /// <param name="sources">An array of GameObjects cointaining the 
        /// meshes to be combined.</param>
        /// <param name="vertices">The combined vertices in the form (x, y, z).
        /// </param>
        /// <param name="triangles">The combined triangles in the form
        /// (vertAIndex, vertBIndex, vertCIndex).</param>
        /// <returns>TRUE if any triangles were found in the GameObjects.
        /// Otherwise FALSE.
        /// </returns>
        [System.Obsolete("Will be removed in v0.5")]
        public static bool CombineMeshFilters(GameObject[] sources
            , out Vector3[] vertices
            , out int[] triangles)
        {
            if (sources == null || sources.Length == 0)
            {
                vertices = null;
                triangles = null;
                return false;
            }

            int vertCount = 0;
            int triCount = 0;
            MeshFilter[] filters = GetMeshFilters(sources
                , out vertCount
                , out triCount);

            if (filters.Length == 0)
            {
                vertices = null;
                triangles = null;
                return false;
            }

            // Combine the mesh's.

            vertices = new Vector3[vertCount];
            triangles = new int[triCount * 3];

            CombineMeshes(filters, vertices, triangles);

            return true;
        }

        /// <summary>
        /// Returns an array of MeshFilters contained by the provided 
        /// GameObjects. (Recursive search.)
        /// </summary>
        /// <remarks><para>All returned MeshFilters are guarenteed to contain meshes
        /// with a triangle count > 0.</para></remarks>
        /// <param name="sources">An array of game objects to be searched.
        /// </param>
        /// <param name="vertexCount">The total number of vertices found in the
        /// MeshFilters.</param>
        /// <param name="triangleCount">The total number of triangles found in 
        /// the MeshFilters.</param>
        /// <returns>TRUE if any valid MeshFilters were found.</returns>
        [System.Obsolete("No longer in use.  Will be removed in v0.5")]
        private static MeshFilter[] GetMeshFilters(GameObject[] sources
            , out int vertexCount
            , out int triangleCount)
        {

            List<MeshFilter> filterList = new List<MeshFilter>();
            vertexCount = 0;
            triangleCount = 0;

            // Design note: Don't need to do a length check.
            if (sources == null)
                return null;

            // Build a list of mesh filters from all sources.
            // Must filter out null meshes and meshes without any triangles.
            foreach (GameObject source in sources)
            {
                if (source != null)
                {
                    MeshFilter[] filterArray =
                        source.GetComponentsInChildren<MeshFilter>();

                    if (filterArray != null)
                    {
                        foreach (MeshFilter filter in filterArray)
                        {
                            Mesh mesh = filter.sharedMesh;
                            if (mesh != null
                                && mesh.triangles.Length > 0)
                            {
                                filterList.Add(filter);

                                for (int iSubMesh = 0
                                    ; iSubMesh < mesh.subMeshCount
                                    ; ++iSubMesh )
                                {
                                    /*
                                     * Note: The vertex sizing method is not
                                     * particularly good.  There is no
                                     * easy method of detecting sub-mesh
                                     * vertex count. So the vertext count will
                                     * be overestimated for meshes with 
                                     * sub-meshes.
                                     */
                                    vertexCount += mesh.vertexCount;
                                    triangleCount += 
                                        mesh.GetTriangles(iSubMesh).Length;
                                }
                            }
                        }
                    }
                }
            }

            triangleCount /= 3;

            return filterList.ToArray();
        }

        /// <summary>
        /// Combines the provided meshes into a single triangle mesh.
        /// </summary>
        /// <remarks>
        /// <para>WARNING: The <paramref name="meshes"/> array must not contain
        /// any null values below <paramref name="meshCount"/>.</para>
        /// </remarks>
        /// <param name="meshes">The meshes to aggregate. 
        /// [Size: >= <paramref name="meshCount"/>]</param>
        /// <param name="transforms">The PRS transforms to apply to each mesh
        /// when aggregated. [Size: >= <paramref name="meshCount"/>]</param>
        /// <param name="meshCount">The number of meshes.</param>
        /// <param name="buffer">A buffer pre-sized to hold the new 
        /// vertices and triangles.</param>
        /// <param name="resetBuffer">If true, the buffer will be filled from
        /// the zero indices.  If false, the new data will be apppened to
        /// the existing mesh.</param>
        public static void CombineMeshes(Mesh[] meshes
            , Matrix4x4[] transforms
            , int meshCount
            , TriangleMesh buffer
            , bool resetBuffer)
        {

            if (resetBuffer)
            {
                buffer.triCount = 0;
                buffer.vertCount = 0;
            }

            if (meshes == null
                || meshes.Length == 0
                || buffer == null
                || !TriangleMesh.Validate(buffer, false))
            {
                return;
            }

            int iStart = 0;
            Mesh mesh = new Mesh();
            int iVertOffset = buffer.vertCount;
            while (iStart < meshCount)
            {
                CombineInstance[] combine;

                iStart = GetFilterBatch(meshes, transforms
                    , iStart, meshCount
                    , out combine);

                mesh.CombineMeshes(combine, true, true);

                Vector3[] vectorVerts = mesh.vertices;
                int vertCount = mesh.vertexCount;
                for (int i = 0; i < vertCount; i++)
                {
                    buffer.verts[buffer.vertCount] = vectorVerts[i];
                    buffer.vertCount++;
                }

                int[] tris = mesh.triangles;
                int pTriOffset = buffer.triCount * 3;
                for (int i = 0; i < tris.Length; i++)
                {
                    buffer.tris[pTriOffset + i] = iVertOffset + tris[i];
                }

                buffer.triCount += tris.Length / 3;
                iVertOffset = buffer.vertCount;
                mesh.Clear();
            }
            GameObject.DestroyImmediate(mesh);
        }

        public static void CombineMeshes(MeshFilter[] filters
            , int filterCount
            , TriangleMesh buffer
            , bool resetBuffer)
        {
            if (resetBuffer)
            {
                buffer.triCount = 0;
                buffer.vertCount = 0;
            }

            if (filters == null
                || filters.Length == 0
                || buffer == null
                || !TriangleMesh.Validate(buffer, false))
            {
                return;
            }

            int iStart = 0;
            Mesh mesh = new Mesh();
            int iVertOffset = buffer.vertCount;
            while (iStart < filterCount)
            {
                CombineInstance[] combine;

                iStart = GetFilterBatch(filters
                    , iStart, filterCount
                    , out combine);

                mesh.CombineMeshes(combine, true, true);

                Vector3[] vectorVerts = mesh.vertices;
                int vertCount = mesh.vertexCount;
                for (int i = 0; i < vertCount; i++)
                {
                    buffer.verts[buffer.vertCount] = vectorVerts[i];
                    buffer.vertCount++;
                }

                int[] tris = mesh.triangles;
                int pTriOffset = buffer.triCount * 3;
                for (int i = 0; i < tris.Length; i++)
                {
                    buffer.tris[pTriOffset + i] = iVertOffset + tris[i];
                }

                buffer.triCount += tris.Length / 3;
                iVertOffset = buffer.vertCount;
                mesh.Clear();
            }

            GameObject.DestroyImmediate(mesh);
        }

        /// <summary>
        /// Combines all meshes in the provided MeshFilters into a single mesh.
        /// </summary>
        /// <param name="filters">The filters to combine. (All filters must
        /// have meshes attached.)</param>
        /// <param name="vertices">The combined vertices in the form (x, y, z). 
        /// Array must be sized to hold the vertices from all meshes. [Out]</param>
        /// <param name="triangles">The combined triangles in the form
        /// (vertAIndex, vertBIndex, vertCIndex).  Must be sized to hold
        /// all triangles from all meshes. [Out]</param>
        [System.Obsolete("No longer in use.  Will be removed in v0.5")]
        private static void CombineMeshes(MeshFilter[] filters
            , Vector3[] vertices, int[] triangles)
        {
            if (filters.Length == 0)
                return;

            int iStart = 0;
            Mesh mesh = new Mesh();
            int iVertOffset = 0;
            int pTriOffset = 0;
            while (iStart < filters.Length)
            {
                CombineInstance[] combine;

                iStart = GetFilterBatch(filters
                    , iStart, filters.Length
                    , out combine);

                mesh.CombineMeshes(combine, true, true);

                Vector3[] vectorVerts = mesh.vertices;
                int vertCount = mesh.vertexCount;
                int pVertOffset = iVertOffset * 3;
                for (int i = 0; i < vertCount; i++)
                {
                    vertices[pVertOffset + (i)] = vectorVerts[i];
                }

                int[] tris = mesh.triangles;
                for (int i = 0; i < tris.Length; i++)
                {
                    triangles[pTriOffset + i] = iVertOffset + tris[i];
                }

                iVertOffset += vertCount;
                pTriOffset += tris.Length;
                mesh.Clear();
            }
            GameObject.DestroyImmediate(mesh);
        }

        /// <summary>
        /// Returns an array of CombineInstance structures suitable for 
        /// combining. (Vertex count will not exceed the maximum allowed for a 
        /// Mesh object.)
        /// </summary>
        /// <param name="meshes">The list of meshes to use.</param>
        /// <param name="transforms">The transforms to apply to the meshes
        /// during the combine process.
        /// </param>
        /// <param name="startIndex">The index of the mesh to start the
        /// batching process at. (The mesh will be included in the 
        /// result.)</param>
        /// <param name="combinedInstances">An array of CombineInstance 
        /// structures batched from the meshes.</param>
        /// <returns>The next index value higher than the last mesh included
        /// in the batch.</returns>
        private static int GetFilterBatch(Mesh[] meshes
            , Matrix4x4[] transforms
            , int startIndex
            , int meshCount
            , out CombineInstance[] combinedInstances)
        {
            // Debug.Log(startIndex);

            int includeCount = 1;
            int subMeshCount = meshes[startIndex].subMeshCount;
            int vertCount = meshes[startIndex].vertexCount;

            // Note: This value should be 64K.  But anything above 32K
            // results in problems.
            // TODO: EVAL: Why the 32K limit?
            const int VertLimit = 32000;

            for (int i = startIndex + 1; i < meshCount; i++)
            {
                int count = meshes[i].vertexCount;
                if (vertCount + count > VertLimit)
                    break;
                vertCount += count;
                includeCount++;
                subMeshCount += meshes[i].subMeshCount;
            }

            // Debug.Log(includeCount + " : " + vertCount);

            combinedInstances = new CombineInstance[subMeshCount];

            for (int filterOffset = 0, iCombinedInstance = 0
                ; filterOffset < includeCount
                ; filterOffset++)
            {
                Mesh sharedMesh = meshes[startIndex + filterOffset];

                for (int iSubMesh = 0
                    ; iSubMesh < sharedMesh.subMeshCount
                    ; ++iSubMesh)
                {
                    combinedInstances[iCombinedInstance].mesh = sharedMesh;
                    combinedInstances[iCombinedInstance].subMeshIndex =
                        iSubMesh;
                    combinedInstances[iCombinedInstance].transform =
                        transforms[startIndex + filterOffset];

                    iCombinedInstance++;
                }
            }

            return startIndex + includeCount;
        }

        /// <summary>
        /// Returns an array of CombineInstance structures suitable for 
        /// combining. (Vertex count will not exceed the maximum allowed for a 
        /// Mesh object.)
        /// </summary>
        /// <param name="filters">The list of MeshFilters to use. (All filters
        /// must have meshes attached.)</param>
        /// <param name="startIndex">The index of the filter to start the
        /// batching process at. (The filter's mesh will be included in the 
        /// result.)</param>
        /// <param name="combinedInstances">An array of CombineInstance 
        /// structures batched from the filters.</param>
        /// <returns>The next index value higher than the last filter included
        /// in the batch.</returns>
        private static int GetFilterBatch(MeshFilter[] filters
            , int startIndex
            , int filterCount
            , out CombineInstance[] combinedInstances)
        {
            int meshCount = 1;
            int subMeshCount = filters[startIndex].sharedMesh.subMeshCount; 
            int vertCount = filters[startIndex].sharedMesh.vertexCount;
            for (int i = startIndex + 1; i < filterCount; i++)
            {
                int count = filters[i].sharedMesh.vertexCount;
                if (vertCount + count > 65500)  // Rounding down.
                    break;
                vertCount += count;
                meshCount++;
                subMeshCount += filters[i].sharedMesh.subMeshCount;
            }

            combinedInstances = new CombineInstance[subMeshCount];

            for (int filterOffset = 0, iCombinedInstance = 0
                ; filterOffset < meshCount
                ; filterOffset++)
            {
                MeshFilter filter = filters[startIndex + filterOffset];
                Mesh sharedMesh = filter.sharedMesh;

                for (int iSubMesh = 0
                    ; iSubMesh < sharedMesh.subMeshCount
                    ; ++iSubMesh)
                {
                    combinedInstances[iCombinedInstance].mesh = sharedMesh;
                    combinedInstances[iCombinedInstance].subMeshIndex = 
                        iSubMesh;
                    combinedInstances[iCombinedInstance].transform = 
                        filter.transform.localToWorldMatrix;

                    iCombinedInstance++;
                }
            }

            return startIndex + meshCount;
        }
    }
}

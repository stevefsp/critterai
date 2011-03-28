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
    public static class MeshUtil
    {
        /*
         * Design notes:
         * 
         * This class contains stub code for Terrains.  This is for
         * future use.  Terrains are not currently supported.
         * 
         * Make sure argument validation is added before making any of the 
         * private methods public.
         */

        /// <summary>
        /// Combines the vertices and triangles from all MeshFilters attached to
        /// the provided GameObjects (including children).  
        /// </summary>
        /// <remarks>
        /// <p>The output parameters will be null if the method return value is 
        /// FALSE.</p>
        /// <p>Vertices will be in world space coordinates.</p>
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
        public static bool CombineMeshFilters(GameObject[] sources
            , out float[] vertices, out int[] triangles)
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

            vertices = new float[vertCount * 3];
            triangles = new int[triCount * 3];

            CombineMeshes(filters, vertices, triangles);

            return true;
        }

        /// <summary>
        /// Returns an array of MeshFilters contained by the provided GameObjects.
        /// </summary>
        /// <remarks>All returned MeshFilters are guarenteed to contain meshes
        /// with a triangle count > 0.</remarks>
        /// <param name="sources">An array of game objects to be searched.</param>
        /// <param name="vertexCount">The total number of vertices found in the
        /// MeshFilters.</param>
        /// <param name="triangleCount">The total number of triangles found in the
        /// MeshFilters.</param>
        /// <returns>TRUE if any valid MeshFilters were found.</returns>
        public static MeshFilter[] GetMeshFilters(GameObject[] sources
            , out int vertexCount, out int triangleCount)
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
                                vertexCount += mesh.vertexCount;
                                triangleCount += mesh.triangles.Length;
                            }
                        }
                    }
                }
            }

            triangleCount /= 3;

            return filterList.ToArray();
        }

        /// <summary>
        /// Combines all meshes in the provided MeshFilters into a single mesh.
        /// </summary>
        /// <param name="filters">The filters to combine. (All filters must
        /// have meshes attached.)</param>
        /// <param name="vertices">The combined vertices in the form (x, y, z). 
        /// Array must be sized to hold the virtices from all meshes.</param>
        /// <param name="triangles">The combined triangles in the form
        /// (vertAIndex, vertBIndex, vertCIndex).  Must be sized to hold
        /// all triangles from all meshes.</param>
        private static void CombineMeshes(MeshFilter[] filters
            , float[] vertices, int[] triangles)
        {
            if (filters.Length == 0)
                return;

            CombineInstance[] combine;
            int iStart = 0;
            Mesh mesh = new Mesh();
            int iVertOffset = 0;
            int pTriOffset = 0;
            while (iStart < filters.Length)
            {
                iStart = GetFilterBatch(filters, iStart, out combine);

                mesh.CombineMeshes(combine, true, true);

                Vector3[] vectorVerts = mesh.vertices;
                int vertCount = mesh.vertexCount;
                int pVertOffset = iVertOffset * 3;
                for (int i = 0; i < vertCount; i++)
                {
                    vertices[pVertOffset + (i * 3) + 0] = vectorVerts[i].x;
                    vertices[pVertOffset + (i * 3) + 1] = vectorVerts[i].y;
                    vertices[pVertOffset + (i * 3) + 2] = vectorVerts[i].z;
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
        /// Returns an array of CombineInstance structures suitable for combining.
        /// (Vertex count will not exceed the maximum allowed for a Mesh object.)
        /// </summary>
        /// <param name="filters">The list of MeshFilters to use. (All filters
        /// must have meshes attached.)</param>
        /// <param name="startIndex">The index of the filter to start the
        /// batching process at. (The filter's mesh will be included in the 
        /// result.)</param>
        /// <param name="combinedInstances">An array of CombineInstance structures
        /// batched from the filters.</param>
        /// <returns>The next index value higher than the last filter included
        /// in the batch.</returns>
        private static int GetFilterBatch(MeshFilter[] filters
            , int startIndex
            , out CombineInstance[] combinedInstances)
        {
            int meshCount = 1;
            int vertCount = filters[startIndex].sharedMesh.vertexCount;
            for (int i = startIndex + 1; i < filters.Length; i++)
            {
                int count = filters[i].sharedMesh.vertexCount;
                if (vertCount + count > 65500)  // Rounding down.
                    break;
                vertCount += count;
                meshCount++;
            }

            combinedInstances = new CombineInstance[meshCount];
            for (int i = 0; i < meshCount; i++)
            {
                combinedInstances[i].mesh = filters[startIndex + i].sharedMesh;
                combinedInstances[i].transform
                    = filters[startIndex + i].transform.localToWorldMatrix;
            }

            return startIndex + meshCount;
        }

        //// Code staged for further use.  (When Terrain data is supported.)
        //// Initial coding of this method is complete.  But it has not been tested.
        //private static bool CombineGeometry(GameObject[] sources
        //    , out float[] vertices, out int[] triangles)
        //{
        //    if (sources == null)
        //    {
        //        vertices = null;
        //        triangles = null;
        //        return false;
        //    }

        //    float[] filterVerts = null;
        //    int[] filterTris = null;
        //    CAIMeshUtil.CombineMeshFilters(sources
        //        , out filterVerts
        //        , out filterTris);

        //    float[] terrainVerts = null;
        //    int[] terrainTris = null;
        //    // This method is a stub.  It doesn't do anything.
        //    CAIMeshUtil.CombineTerrains(sources
        //        , out terrainVerts
        //        , out terrainTris);

        //    if (filterTris == null && terrainTris == null)
        //    {
        //        vertices = null;
        //        triangles = null;
        //        return false;
        //    }
        //    else if (filterTris == null)
        //    {
        //        vertices = terrainVerts;
        //        triangles = terrainTris;
        //    }
        //    else if (terrainTris == null)
        //    {
        //        vertices = filterVerts;
        //        triangles = filterTris;
        //    }
        //    else
        //    {
        //        Mesh3.Combine(filterVerts, filterTris
        //            , terrainVerts, terrainTris
        //            , out vertices, out triangles);
        //    }

        //    return true;
        //}

        //// This is a stub.
        //private static bool CombineTerrains(GameObject[] sources
        //    , out float[] vertices, out int[] triangles)
        //{
        //    vertices = null;
        //    triangles = null;
        //    return false;
        //}

        //// Initial coding of this method is complete.  But it has not been tested.
        //private static bool GetTerrainData(GameObject[] sources
        //    , out TerrainData[] terrainData
        //    , out Vector3[] terrainPositions)
        //{
        //    // This code has not been tested.
        //    List<TerrainData> terrainDataList = new List<TerrainData>();
        //    List<Vector3> positionList = new List<Vector3>();

        //    foreach (GameObject source in sources)
        //    {
        //        if (source != null)
        //        {
        //            Terrain[] terrainArray =
        //                source.GetComponentsInChildren<Terrain>();

        //            if (terrainArray != null)
        //            {
        //                foreach (Terrain terrain in terrainArray)
        //                {
        //                    TerrainData td = terrain.terrainData;
        //                    if (td != null)
        //                    {
        //                        terrainDataList.Add(td);
        //                        positionList.Add(terrain.transform.position);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (terrainDataList.Count == 0)
        //    {
        //        terrainData = null;
        //        terrainPositions = null;
        //    }
        //    else
        //    {
        //        terrainData = terrainDataList.ToArray();
        //        terrainPositions = positionList.ToArray();
        //    }

        //    return (terrainDataList.Count > 0);
        //}
    }
}

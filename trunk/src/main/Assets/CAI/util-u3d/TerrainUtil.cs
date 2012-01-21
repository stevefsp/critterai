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
using UnityEngine;
using org.critterai.geom;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai
{
    /// <summary>
    /// Provides utility methods related to the UnityEngine.Terrain class.
    /// </summary>
    public static class TerrainUtil
    {
        /// <summary>
        /// Triangluates a terrain object.
        /// </summary>
        /// <remarks>
        /// <para>The heightmap is triangluated based on the terrain's
        /// resolution settings.  Trees are triangluated based on the
        /// first mesh in the associated prototypes. (Only one mesh per
        /// mesh filter is supported.)</para>
        /// <para>Detail objects such as grass, rocks, shrubs, etc., are
        /// not triangulated.</para>
        /// <para>The buffers in the triangle mesh will contain unsused space
        /// if a tree's prototype has no mesh. (Not an expected condition.)
        /// </para>
        /// </remarks>
        /// <param name="terrain">The terrain to triangulate.</param>
        /// <param name="includeTrees">If true, the trees will be included
        /// included in the final mesh.</param>
        /// <returns>A triangle mesh.</returns>
        public static TriangleMesh Triangulate(Terrain terrain
            , bool includeTrees)
        {
            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            Vector3 scale = terrain.terrainData.heightmapScale;

            int xCount = terrain.terrainData.heightmapWidth;
            int zCount = terrain.terrainData.heightmapHeight;

            TriangleMesh m = GetMeshBuffer(terrain);

            // Generate suface sample points.
            for (float xPos = 0; xPos <= size.x; xPos += scale.x)
            {
                for (float zPos = 0; zPos <= size.z; zPos += scale.z)
                {
                    Vector3 pos = new Vector3(origin.x + xPos, 0, origin.z + zPos);
                    pos.y = terrain.SampleHeight(pos);
                    m.verts[m.vertCount] = pos;
                    m.vertCount++;
                }
            }

            // Triangulate surface sample points.
            for (int x = 0; x < xCount - 1; x++)
            {
                for (int z = 0; z < zCount - 1; z++)
                {
                    int i = z + (x * zCount);
                    int irow = i + zCount;

                    m.tris[m.triCount * 3 + 0] = i;
                    m.tris[m.triCount * 3 + 1] = irow + 1;
                    m.tris[m.triCount * 3 + 2] = irow;
                    m.triCount++;

                    m.tris[m.triCount * 3 + 0] = i;
                    m.tris[m.triCount * 3 + 1] = i + 1;
                    m.tris[m.triCount * 3 + 2] = irow + 1;
                    m.triCount++;
                }
            }

            // Add all tree meshes.

            if (includeTrees
                && terrain.terrainData.treePrototypes != null
                && terrain.terrainData.treePrototypes.Length > 0)
            {
                AddTreesToBuffer(terrain, m);
            }

            return m;
        }

        private static void AddTreesToBuffer(Terrain terrain, TriangleMesh buffer)
        {
            TerrainData data = terrain.terrainData;

            Mesh[] protoMeshes = new Mesh[data.treePrototypes.Length];

            for (int i = 0; i < protoMeshes.Length; i++)
            {
                MeshFilter filter =
                    data.treePrototypes[i].prefab.GetComponent<MeshFilter>();

                if (filter == null || filter.sharedMesh == null)
                {
                    protoMeshes[i] = null;
                    Debug.LogWarning(string.Format(
                        "{0} : There is no mesh attached the {1} tree prototype."
                            + "Trees based on this prototype will be ignored."
                        , terrain.name
                        , data.treePrototypes[i].prefab.name));
                }
                else
                    protoMeshes[i] = filter.sharedMesh;
            }

            Mesh[] treeMeshes = new Mesh[data.treeInstances.Length];
            Matrix4x4[] treeTransforms = new Matrix4x4[data.treeInstances.Length];

            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            int usableTrees = 0;
            foreach (TreeInstance tree in data.treeInstances)
            {
                if (protoMeshes[tree.prototypeIndex] == null)
                    continue;

                treeMeshes[usableTrees] = protoMeshes[tree.prototypeIndex];

                Vector3 pos = tree.position;
                pos.x *= terrainSize.x;
                pos.y *= terrainSize.y;
                pos.z *= terrainSize.z;
                pos += terrainPos;

                Vector3 scale =
                    new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);

                treeTransforms[usableTrees] =
                    Matrix4x4.TRS(pos, Quaternion.identity, scale);

                usableTrees++;
            }

            MeshUtil.CombineMeshes(treeMeshes, treeTransforms, usableTrees, buffer, false);
        }

        private static TriangleMesh GetMeshBuffer(Terrain terrain)
        {
            TerrainData data = terrain.terrainData;

            int vertCount = data.heightmapWidth * data.heightmapHeight;
            int triCount = (data.heightmapWidth - 1) * (data.heightmapHeight - 1) * 2;

            // Debug.Log("Before: " + vertCount + " : " + triCount);

            int[] protoVertCount = new int[data.treePrototypes.Length];
            int[] protoTriCount = new int[data.treePrototypes.Length];

            for (int i = 0; i < protoVertCount.Length; i++)
            {
                MeshFilter filter =
                    data.treePrototypes[i].prefab.GetComponent<MeshFilter>();

                if (filter == null || filter.sharedMesh == null)
                {
                    protoVertCount[i] = 0;
                    protoTriCount[i] = 0;
                }
                else
                {
                    MeshUtil.EstimateSize(filter.sharedMesh
                        , out protoVertCount[i]
                        , out protoTriCount[i]);
                }
            }

            foreach (TreeInstance tree in data.treeInstances)
            {
                vertCount += protoVertCount[tree.prototypeIndex];
                triCount += protoTriCount[tree.prototypeIndex];
            }

            // Debug.Log("Final: " + vertCount + " : " + triCount);

            return new TriangleMesh(vertCount, triCount);
        }
    }
}

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
using org.critterai;

/// <summary>
/// Recursively searches an array of game objects for MeshFilters and combines
/// the meshes into a single triangle mesh.
/// </summary>
/// <remarks>
/// <para>The standard triangle limit for the Unity Mesh type does not apply.</para>
/// </remarks>
[System.Serializable]
[AddComponentMenu("CAI/Mesh Filter Source (Array)")]
public class MeshFilterSource
    : DSGeometry 
{
    /// <summary>
    /// The GameObjects to search for meshes.  (Search is recursive.)
    /// </summary>
    public GameObject[] sources = new GameObject[1];

    /// <summary>
    /// TRUE if any of the GameObjects contain at least one Unity Mesh.
    /// </summary>
    public override bool HasGeometry
    {
        get
        {
            MeshFilter[] filters = U3DUtil.GetComponents<MeshFilter>(sources);
            foreach (MeshFilter filter in filters)
            {
                if (filter.sharedMesh != null)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Derives the bounds for the aggregate meshes.
    /// [Form: (minX, minY, minZ, maxX, maxY, maxZ)]
    /// </summary>
    /// <remarks>
    /// This method performs a full build of the source geometry.  So
    /// if the <see cref="TriangleMesh"/> data is going to be needed, it is best
    /// to use <see cref="GetGeometry"/>, then derive the bounds from the
    /// result.
    /// </remarks>
    /// <returns>The bounds of the aggregate meshes.</returns>
    public override float[] GetGeometryBounds()
    {
        float[] verts;
        int[] tris;
        float[] bounds = new float[6];
        if (MeshUtil.CombineMeshFilters(sources, out verts, out tris))
        {
            Vector3Util.GetBounds(verts, bounds);
        }
        return bounds;
    }

    /// <summary>
    /// Derives an aggregate <see cref="TriangleMesh"/> from all Unity Meshes
    /// attached to the GameObjects. (Recursive search.)
    /// </summary>
    /// <returns>An aggregate <see cref="TriangleMesh"/>.
    /// Or NULL if the aggregation failed.</returns>
    public override TriangleMesh GetGeometry()
    {
        TriangleMesh mesh = new TriangleMesh();

        if (MeshUtil.CombineMeshFilters(sources, out mesh.verts, out mesh.tris))
        {
            mesh.vertCount = mesh.verts.Length / 3;
            mesh.triCount = mesh.tris.Length / 3;
            return mesh;
        }

        return null;
    }
}

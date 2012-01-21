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
using System.Collections.Generic;
using org.critterai.geom;
using org.critterai;

/// <summary>
/// Recursively searches an all game objects with the specified tags 
/// for MeshFilters and combines the meshes into a single triangle mesh.
/// </summary>
/// <remarks>
/// <para>The standard triangle limit for the Unity Mesh type does not apply.</para>
/// </remarks>
[System.Serializable]
[System.Obsolete("Not used in new build process. Will be removed in v0.5")]
public class TaggedMeshFilterSource 
    : DSGeometry 
{
    /// <summary>
    /// One or more tags to base the search on.
    /// </summary>
    public string[] sourceTags = new string[1];

    private GameObject[] GetSources()
    {
        if (sourceTags == null)
            return null;
        else if (sourceTags.Length == 1)
            // Shortcut.
            return GameObject.FindGameObjectsWithTag(sourceTags[0]);
        else
        {
            // Need to aggregate.
            List<GameObject> result = new List<GameObject>();
            foreach (string tag in sourceTags)
            {
                if (tag != null && tag.Length > 0)
                {
                    GameObject[] g = GameObject.FindGameObjectsWithTag(tag);
                    if (g != null)
                    {
                        result.AddRange(g);
                    }
                }
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// TRUE if any of the GameObjects contain at least one Unity Mesh.
    /// </summary>
    public override bool HasGeometry
    {
        get
        {
            GameObject[] sources = GetSources();
            if (sources == null)
                return false;

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
    /// Derives an aggregate <see cref="TriangleMesh"/> from all
    /// Unity Meshes attached to the tagged GameObject's. (Recursive search.)
    /// </summary>
    /// <returns>An aggregate <see cref="TriangleMesh"/>.</returns>
    [System.Obsolete("Not used in new build process. Will be removed in v0.5")]
    public override TriangleMesh GetGeometry()
    {
        GameObject[] sources = GetSources();
        if (sources == null)
            return null;

        TriangleMesh mesh = new TriangleMesh();

        if (MeshUtil.CombineMeshFilters(sources, out mesh.verts, out mesh.tris))
        {
            mesh.vertCount = mesh.verts.Length / 3;
            mesh.triCount = mesh.tris.Length / 3;
            return mesh;
        }

        return null;
    }

    public override TriangleMesh GetGeometry(out byte[] areas)
    {
        TriangleMesh result = GetGeometry();

        if (result == null)
            areas = null;
        else
        {
            areas = new byte[result.triCount];
            for (int i = 0; i < areas.Length; i++)
            {
                areas[i] = 63;
            }
        }

        return result;
    }
}

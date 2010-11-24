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
using org.critterai.math;

/// <summary>
/// Provides a simple way to view navigation meshes, both in the Unity
/// Editor and at runtime.
/// </summary>
/// <remarks>
/// <p>Used primarily for debug purposes.</p>
/// <p>See <a href="http://www.critterai.org/nmgen_unitypro" target="_parent">
/// Getting Started with Unity Pro</a> for information on how to use this
/// class.</p>
/// <p>Once a navigation mesh is assigned to an instance of this class it will 
/// be automatically monitored for changes, even in the Unity Editor.
/// </p>
/// </remarks>
/// <seealso cref="NavmeshViewEditor"/>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public sealed class NavmeshView 
    : MonoBehaviour 
{
    /// <summary>
    /// If TRUE, the mesh will be displayed.  If FALSE, the mesh will be
    /// hidden.  (Enables/Disables the mesh filter.)
    /// </summary>
    public bool meshIsVisible = true;

    /// <summary>
    /// The navigation mesh to monitor and display.
    /// </summary>
    public Navmesh navmesh = null;

    private int lastRevision = int.MinValue;
    private bool lastVisible = false;

    /// <summary>
    /// Indicates the navigation mesh revision the current view mesh
    /// represents.
    /// </summary>
    /// <remarks>
    /// <p>If this value does not match the navigation mesh, then a rebuild
    /// is needed.</p>
    /// <p>Will equal <see cref="System.Int32.MinValue"/> if a navigation mesh
    /// has not been assigned.</p></remarks>
    public int Revision { get { return lastRevision; } }

    /// <summary>
    /// Runs at startup of the object.
    /// </summary>
	void Start()
    {
        if (navmesh != null)
            BuildMesh();
	}

    /// <summary>
    /// Runs every update.
    /// </summary>
    void Update()
    {
        if (lastVisible != meshIsVisible)
        {
            lastVisible = meshIsVisible;
            GetComponent<MeshRenderer>().enabled = meshIsVisible;
        }
        if (meshIsVisible
            && navmesh != null
            && navmesh.revision != lastRevision)
            BuildMesh();
    }

    /// <summary>
    /// Builds the display mesh based on the content of the associated
    /// Navigation mesh object.  (The mesh is assigned to the object's mesh 
    /// filter.)
    /// </summary>
    public void BuildMesh()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null)
        {
            Debug.LogError("Mesh filter is missing on " + name);
            return;
        }
        if (navmesh == null 
            || navmesh.vertices == null 
            || navmesh.vertices.Length == 0)
        {
            if (filter.sharedMesh != null)
                // Design note: Using immediate since normal destroy
                // is not supported in the editor.
                DestroyImmediate(filter.sharedMesh);
            filter.sharedMesh = null;
        }
        else
        {
            Mesh m = new Mesh();

            m.vertices = Vector3Util.GetVectors(navmesh.vertices);
            m.triangles = (int[])navmesh.triangles.Clone();

            // Not getting fancy with the UV.
            // Only reason I'm including them is to shut the shader up.

            Vector3 origin = navmesh.minBounds;
            float deltaX = navmesh.maxBounds.x - origin.x;
            float deltaZ = navmesh.maxBounds.z - origin.z;

            Vector2[] uv = new Vector2[m.vertices.Length];
            for (int i = 0; i < uv.Length; i++)
            {
                Vector3 vert = m.vertices[i];
                uv[i] = new Vector2((vert.x - origin.x) / deltaX
                    , (vert.z - origin.z) / deltaZ);
            }
            m.uv = uv;

            m.RecalculateNormals();

            if (filter.sharedMesh != null)
                // Design note: Using immediate since normal destroy
                // is not supported in the editor.
                DestroyImmediate(filter.sharedMesh);
            filter.sharedMesh = m;
			m.RecalculateBounds();
            lastRevision = navmesh.revision;
        }
    }

}

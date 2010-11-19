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
/// Represents core navigation mesh data.
/// </summary>
[System.Serializable]
public class Navmesh 
	: MonoBehaviour 
{
    /*
     * Design notes:
     * 
     * This class is attached to Unity objects.  So it should never 
     * reference the NavmeshGenerator class.
     */

    /// <summary>
    /// The change revision of the mesh.
    /// </summary>
    /// <remarks>
    /// Other components monitor this value to detect and respond to changes
    /// in the navigation mesh.  
    /// <p>Anything that changes the mesh should increment this value.
    /// Calling the <see cref="Reset()"/> or <see cref="RebuildBounds()"/> 
    /// methods will automatically increment this value.</p>
    /// </remarks>
    [HideInInspector()]
    public int revision = 0;

    /// <summary>
    /// The mesh vertices in the form (x, y, z).
    /// </summary>
    [HideInInspector()]
    public float[] vertices = null;

    /// <summary>
    /// The mesh triangles in the form (vertIndexA, vertIndexB, vertIndexC).
    /// </summary>
    [HideInInspector()]
    public int[] triangles = null;

    /// <summary>
    /// The minimum bounds of the mesh's AABB.
    /// </summary>
    /// <seealso cref="RebuildBounds()"/>
    [HideInInspector()]
    public Vector3 minBounds = Vector3.zero;

    /// <summary>
    /// The maximum bounds of the mesh's AABB.
    /// </summary>
    /// <seealso cref="RebuildBounds()"/>
    [HideInInspector()]
    public Vector3 maxBounds = Vector3.zero;

    /// <summary>
    /// The plane tolerance to use when detecting whether a point
    /// in on the navigation mesh. (Useful to navigation planners.)
    /// </summary>
    /// <remarks>
    /// This value should be set above the maximum distance the navigation mesh 
    /// deviates from the source geometry and below 0.5 times the minimum 
    /// distance between overlapping navigation mesh polygons.
    /// </remarks>
    public float planeTolerance = 0.5f;

    /// <summary>
    /// Clears all mesh information, setting the vertices and triangles to null.
    /// </summary>
    /// <remarks>
    /// Calling this method will increment the revision field.
    /// </remarks>
    public void Reset()
    {
        vertices = null;
        triangles = null;
        minBounds = Vector3.zero;
        maxBounds = Vector3.zero;
        revision++;
    }

    /// <summary>
    /// Derives the AABB bounds based on the mesh'es vertices.
    /// </summary>
    /// <remarks>
    /// Calling this method will increment the revision field.
    /// </remarks>
    public void RebuildBounds()
    {
        if (vertices == null || vertices.Length == 0)
        {
            minBounds = Vector3.zero;
            maxBounds = Vector3.zero;
        }
        else
        {
            float[] bounds = Vector3Util.GetBounds(vertices, null);
            minBounds = new Vector3(bounds[0], bounds[1], bounds[2]);
            maxBounds = new Vector3(bounds[3], bounds[4], bounds[5]);
        }
        revision++;
    }
}

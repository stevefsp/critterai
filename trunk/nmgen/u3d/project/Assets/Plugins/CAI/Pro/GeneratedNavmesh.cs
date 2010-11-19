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

/// <summary>
/// Represents a triangle based navigation mesh generated from all MeshFilter
/// data in the attached source geometry. (Unity Pro Only)
/// </summary>
/// <remarks>
/// <p>Notes on use within Unity</p>
/// <ul>
/// <li>The functionality provided by this class can only be used within 
/// Unity Pro.  It will not function within the free version of Unity.</li>
/// <li>It is safe to attach instances of this class to scene objects that
/// will be used in Unity Web Player deployments.  It does not contain
/// any references to <see cref="NavmeshGenerator"/></li>
/// </ul>
/// <p>The mesh build is performed by the <see cref="GeneratedNavmeshEditor"/>
/// class via the inspector.</p>
/// <p>The planeTolerance field is auto-set based on the configuration of the 
/// generator.</p>
/// </remarks>
[System.Serializable]
[AddComponentMenu("CAI/Navmesh (Generated)")]
public sealed class GeneratedNavmesh
    : Navmesh
{
    /*
     * Design notes:
     * 
     * This class is attached to Unity objects.  So it should never 
     * reference the NavmeshGenerator class.
     * 
     */

    /// <summary>
    /// The configuration to use when building the mesh.
    /// </summary>
    public NavmeshConfig config = 
        NavmeshConfig.GetDefault();

    /// <summary>
    /// The source which will provide the geometry used to build the
    /// navigation mesh.
    /// </summary>
    public CAISource sourceGeometry = null;

    /// <summary>
    /// A string suitable for use when storing and retrieving information from
    /// disk.
    /// </summary>
    /// <remarks>
    /// Use of this value ensures there are no name clashes within a project.
    /// E.g. Two scenes with navigation meshes, both  called "Navmesh".
    /// </remarks>
    public string GetCacheID()
    {
        return name + "-" + (GetInstanceID() < 0 ? "n" : "p") + GetInstanceID();
    }

}

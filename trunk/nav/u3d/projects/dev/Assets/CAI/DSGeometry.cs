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

/// <summary>
/// The base class for components that provide triangle mesh data for use
/// by other components.
/// </summary>
public abstract class DSGeometry 
    : MonoBehaviour 
{
    /// <summary>
    /// TRUE if geometry data is available.
    /// </summary>
    /// <returns>TRUE if geometry data is available.</returns>
    public abstract bool HasGeometry { get; }

    /// <summary>
    /// Derives and returns the bounds of the geometry. 
    /// [Form: (minX, minY, minZ, maxX, maxY, maxZ)]
    /// </summary>
    /// <returns>The bounds of the geometry.</returns>
    public abstract float[] GetGeometryBounds();

    /// <summary>
    /// Gets the geometry.
    /// </summary>
    /// <returns>The geometry, or NULL if geometry is not available.</returns>
    public abstract TriangleMesh GetGeometry();
}

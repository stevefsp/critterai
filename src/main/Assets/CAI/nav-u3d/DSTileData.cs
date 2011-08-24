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
using org.critterai.nav;

/// <summary>
/// The base class for components that provide navigation mesh
/// tile data for use by components.
/// </summary>
public abstract class DSTileData
    : MonoBehaviour 
{
    /// <summary>
    /// Tile build data.
    /// </summary>
    /// <remarks>
    /// <para>Set <paramref name="includeTilePos"/> to FALSE if the tile
    /// will be used to build a single-tile mesh.</para>
    /// </remarks>
    /// <param name="includeTilePos">If TRUE, tile position data will
    /// be included in the build data.  Otherwise the values will be
    /// set to zero.</param>
    /// <returns>Tile build data.  Or NULL if none is available.</returns>
    public abstract NavmeshTileBuildData GetTileBuildData(bool includeTilePos);

    /// <summary>
    /// TRUE if tile data is available.
    /// </summary>
    public abstract bool HasTileBuildData { get; }
}

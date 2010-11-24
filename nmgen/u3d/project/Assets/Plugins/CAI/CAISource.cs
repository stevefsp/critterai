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
/// Provides a standard API for generating a list of GameObjects.
/// </summary>
/// <remarks>
/// <p>A 'source' is a generic term referring to GameObjects that are
/// used as a source of information.  The concrete class may use any
/// number of methods to derive the list of sources it provides.</p>
/// <p>This class provides a way of getting a list of GameObjects without the
/// client class having any idea how the list is derived.</p>
/// </remarks>
[System.Serializable]
public abstract class CAISource
    : MonoBehaviour
{
    /*
     * Design notes:
     * 
     * Concrete implementations of this class are attached to Unity objects.
     * So this class should never reference the NavmeshGenerator class.
     * 
     */

    /// <summary>
    /// Returns a list of GameObjects to be used as sources.
    /// </summary>
    /// <remarks>
    /// The return value may be null, and the returned array may contain
    /// null values.
    /// </remarks>
    /// <returns>Returns an array of GameObjects used as sources. Or null
    /// if no sources are available.</returns>
    public abstract GameObject[] GetSources();
}

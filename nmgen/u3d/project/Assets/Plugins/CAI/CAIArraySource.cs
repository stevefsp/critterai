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
/// Provides a list of GameObjects based on an array.
/// </summary>
/// <remarks>
/// <p>One or more GameObject's are assigned directly to the <see cref="sources"/>
/// array.  Then a reference to the array is returned by
/// <see cref="GetSources"/>.</p>
/// <p>See <a href="http://www.critterai.org/nmgen_unitypro" target="_parent">
/// Getting Started with Unity Pro</a> for information on how to use this
/// class.</p>
/// </remarks>
/// <seealso cref="CAIArraySourceEditor"/>
[System.Serializable]
[AddComponentMenu("CAI/Array Source")]
public sealed class CAIArraySource
    : CAISource
{
    /*
     * Design notes:
     * 
     * Instances of this class are attached to Unity objects.
     * So it should never reference the NavmeshGenerator class.
     * 
     */

    /// <summary>
    /// The GameObjects to be returned by the <see cref="GetSources"/>
    /// method.
    /// </summary>
    public GameObject[] sources = new GameObject[1];

    /// <summary>
    /// Returns a reference to the array of GameObject sources attached 
    /// to the instance.
    /// </summary>
    /// <remarks>
    /// The return value may be null, and the returned array may contain
    /// null values.
    /// </remarks>
    /// <returns>A reference to the array of GameObject sources attached 
    /// to the instance.</returns>
    public override GameObject[] GetSources()
    {
        return sources;
    }
}

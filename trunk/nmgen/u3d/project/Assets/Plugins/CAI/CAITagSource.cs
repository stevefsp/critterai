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
using System.Collections.Generic;

/// <summary>
/// Provides a list of GameObject sources based on a tag search.
/// </summary>
/// <remarks>
/// One or more tags are assiged to the <see cref="sourceTags"/>
/// array.  Whenever <see cref="GetSources"/> is called a search is
/// performed and all GameObjects assigned the tag(s) will be returned.
/// </remarks>
[System.Serializable]
[AddComponentMenu("CAI/Tag Source")]
public sealed class CAITagSource
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
    /// The tags to search for when gathering sources.
    /// </summary>
    public string[] sourceTags = new string[1];

    /// <summary>
    /// Rerturns an array of GameObjects searched for by tag.
    /// </summary>
    /// <remarks>
    /// The return value may be null, but the array will never contain null 
    /// values.
    /// </remarks>
    /// <returns>An array of GameObjects searched for by tag.</returns>
    public override GameObject[] GetSources()
    {
        if (sourceTags == null)
        {
            return null;
        }
        else if (sourceTags.Length == 1)
        {
            // Shortcut.
            return GameObject.FindGameObjectsWithTag(sourceTags[0]);
        }
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
}

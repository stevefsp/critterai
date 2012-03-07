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
using Buffer = System.Buffer;
using System.Collections.Generic;

namespace org.critterai.u3d
{
    /// <summary>
    /// Provides general purpose utility functions for Unity.
    /// </summary>
    public static class UnityUtil
    {
        /// <summary>
        /// Searches the provided game objects for a type of
        /// component.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="sources">An array of game objects to search.</param>
        /// <returns>The components found during the search.</returns>
        public static T[] GetComponents<T>(GameObject[] sources, bool includeChildren)
            where T : Component
        {
            List<T> result = new List<T>();
            foreach (GameObject go in sources)
            {
                if (go == null || !go.active)
                    continue;

                if (includeChildren)
                {
                    T[] cs = go.GetComponentsInChildren<T>(false);
                    result.AddRange(cs);
                }
                else
                {
                    T cs = go.GetComponent<T>();

                    if (cs != null)
                        result.Add(cs);
                }
            }
            return result.ToArray();
        }

        //public static Texture2D CreateTexture(int width, int height, Color color)
        //{
        //    Color[] pixels = new Color[width * height];

        //    for (int i = 0; i < pixels.Length; i++)
        //        pixels[i] = color;

        //    Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
        //    result.SetPixels(pixels);
        //    result.Apply();

        //    return result;
        //}
    }
}

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

namespace org.critterai
{
    /// <summary>
    /// Provides general purpose utility functions for Unity.
    /// </summary>
    public static class UnityUtil
    {
        /// <summary>
        /// A string helpful in creating uniquely named project assets.
        /// </summary>
        /// <remarks>
        /// <para>The value is only unique with the scope of the Unity 
        /// project.</para>
        /// <para>Format: objectName + n|p + objectId<br/>
        /// Example: Agent-p3142</para>
        /// </remarks>
        /// <param name="unityObject">A Unity Object.</param>
        /// <returns>A unique string for the object.</returns>
        public static string GetCacheID(Object unityObject)
        {
            return unityObject.name 
                + "-" + (unityObject.GetInstanceID() < 0 ? "n" : "p") 
                + unityObject.GetInstanceID();
        }

        public static Texture2D CreateTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            result.SetPixels(pixels);
            result.Apply();

            return result;
        }

        // Not commutative
        public static bool Matches(Object target, Object source, MatchType option)
        {
            if (target == null || source == null)
                return false;

            switch (option)
            {
                case MatchType.Strict:

                    return (target == source);

                case MatchType.BeginsWith:

                    return (target.name.StartsWith(source.name));

                case MatchType.Instance:

                    return (target == source
                        || (target.name.StartsWith(source.name)
                            && target.name.Contains("Instance")));
            }
            return false;
        }

        /// <summary>
        /// Recursively searches the provided game objects for a type of
        /// component.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="sources">An array of game objects to search.</param>
        /// <returns>The components found during the search.</returns>
        public static T[] GetComponents<T>(GameObject[] sources, bool searchRecursive)
            where T : Component
        {
            return GetComponents<T>(sources, searchRecursive, false);
        }

        /// <summary>
        /// Recursively searches the provided game objects for a type of
        /// component.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="sources">An array of game objects to search.</param>
        /// <returns>The components found during the search.</returns>
        private static T[] GetComponents<T>(GameObject[] sources
            , bool searchRecursive
            , bool includeInactive)
            where T : Component
        {
            List<T> result = new List<T>();
            foreach (GameObject go in sources)
            {
                if (go == null || (!go.active && !includeInactive))
                    continue;

                if (searchRecursive)
                {
                    T[] cs = go.GetComponentsInChildren<T>(includeInactive);
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
    }
}

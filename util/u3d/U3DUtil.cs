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
    public static class U3DUtil
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

        /// <summary>
        /// Recursively searches the provided game objects for a type of
        /// component.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="sources">An array of game objects to search.</param>
        /// <returns>The components found during the search.</returns>
        public static T[] GetComponents<T>(GameObject[] sources)
            where T : Component
        {
            List<T> result = new List<T>();
            foreach (GameObject go in sources)
            {
                if (go == null)
                    continue;
                T[] cs = go.GetComponentsInChildren<T>();
                if (cs == null)
                    continue;
                foreach (T c in cs)
                    result.Add(c);
            }
            return result.ToArray();
        }
    }
}

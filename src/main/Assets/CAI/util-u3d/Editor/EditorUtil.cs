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

namespace org.critterai.util.u3d.editor
{
    /// <summary>
    /// Provides general purpose utility functions for Unity.
    /// </summary>
    public static class EditorUtil
    {

        // TODO: Review functions.  Should some of these be moved to an 
        // editor only class?

        private static GUIStyle mHelpStyle;
        private static GUIStyle mWarningStyle;
        private static GUIStyle mErrorStyle;

        public static GUIStyle ErrorStyle
        {
            get
            {
                if (mErrorStyle == null)
                {
                    mErrorStyle = new GUIStyle(GUI.skin.box);
                    mErrorStyle.wordWrap = true;
                    mErrorStyle.alignment = TextAnchor.UpperLeft;

                    Color c = Color.red;
                    c.a = 0.75f;
                    mErrorStyle.normal.textColor = c;
                }
                return mErrorStyle;
            }
        }

        public static GUIStyle WarningStyle
        {
            get
            {
                if (mWarningStyle == null)
                {
                    mWarningStyle = new GUIStyle(GUI.skin.box);
                    mWarningStyle.wordWrap = true;
                    mWarningStyle.alignment = TextAnchor.UpperLeft;

                    Color c = Color.yellow;
                    c.a = 0.75f;
                    mWarningStyle.normal.textColor = c;
                }
                return mWarningStyle;
            }
        }

        public static GUIStyle HelpStyle
        {
            get
            {
                if (mHelpStyle == null)
                {
                    mHelpStyle = new GUIStyle(GUI.skin.box);
                    mHelpStyle.wordWrap = true;
                    mHelpStyle.alignment = TextAnchor.UpperLeft;

                    Color c = Color.white;
                    c.a = 0.75f;
                    mHelpStyle.normal.textColor = c;

                }
                return mHelpStyle;
            }
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
            return GetComponents<T>(sources, false);
        }

        /// <summary>
        /// Recursively searches the provided game objects for a type of
        /// component.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="sources">An array of game objects to search.</param>
        /// <returns>The components found during the search.</returns>
        private static T[] GetComponents<T>(GameObject[] sources
            , bool includeInactive)
            where T : Component
        {
            List<T> result = new List<T>();
            foreach (GameObject go in sources)
            {
                if (go == null)
                    continue;

                T[] cs = go.GetComponentsInChildren<T>(includeInactive);

                if (cs == null)
                    continue;

                result.AddRange(cs);
            }
            return result.ToArray();
        }
    }
}

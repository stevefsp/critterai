/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using UnityEditor;
using System.IO;

namespace org.critterai.u3d.editor
{
    /// <summary>
    /// Provides general purpose editor utility functions for Unity.
    /// </summary>
    internal static class EditorUtil
    {
        public const int AssetGroup = 100;
        public const int ViewGroup = 1000;
        public const int ManagerGroup = 2000;

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

        public static bool OnGUIManageObjectList<T>(string label
            , List<T> items
            , bool allowScene) 
            where T : UnityEngine.Object
        {
            if (items == null)
                return false;

            // Never allow nulls.  So get rid of them first.
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] == null)
                    items.RemoveAt(i);
            }

            GUILayout.Label(label);

            if (items.Count > 0)
            {
                int delChoice = -1;

                for (int i = 0; i < items.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

#if UNITY_3_0_0	|| UNITY_3_1 || UNITY_3_2 || UNITY_3_3
                    T item = (T)EditorGUILayout.ObjectField(items[i], typeof(T));
#else
                    T item = (T)EditorGUILayout.ObjectField(items[i], typeof(T), allowScene);
#endif

                    if (item == items[i] || !items.Contains(item))
                        items[i] = item;

                    if (GUILayout.Button("Remove"))
                        delChoice = i;

                    EditorGUILayout.EndHorizontal();
                }

                if (delChoice >= 0)
                    items.RemoveAt(delChoice);
            }

            EditorGUILayout.Separator();

            T nitem = (T)EditorGUILayout.ObjectField("Add", null, typeof(T), allowScene);

            if (nitem != null)
            {
                if (!items.Contains(nitem))
                    items.Add(nitem);
            }

            return GUI.changed;
        }

        public static bool OnGUIManageStringList(List<string> items, bool isTags)
        {
            if (items == null)
                return false;

            if (items.Count > 0)
            {
                GUILayout.Label((isTags ? "Tags" : "Items"));

                int delChoice = -1;

                for (int i = 0; i < items.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    string item;

                    if (isTags)
                        item = EditorGUILayout.TagField(items[i]);
                    else
                        item = EditorGUILayout.TextField(items[i]);

                    if (item == items[i] || !items.Contains(item))
                        items[i] = item;

                    if (GUILayout.Button("Remove"))
                        delChoice = i;

                    EditorGUILayout.EndHorizontal();
                }

                if (delChoice >= 0)
                    items.RemoveAt(delChoice);
            }

            EditorGUILayout.Separator();

            string ntag = EditorGUILayout.TagField("Add", "");

            if (ntag.Length > 0)
            {
                if (!items.Contains(ntag))
                    items.Add(ntag);
            }

            return GUI.changed;
        }

        public static T CreateAsset<T>(ScriptableObject atAsset, string label) where T : ScriptableObject
        {
            string name = typeof(T).ToString();
            string path = GenerateStandardPath(atAsset, name);

            T result = ScriptableObject.CreateInstance<T>();
            result.name = name;

            AssetDatabase.CreateAsset(result, path);

            if (label.Length > 0)
                AssetDatabase.SetLabels(result, new string[1] { label });

            AssetDatabase.SaveAssets();

            return result;
        }

        public static T CreateAsset<T>(string label) where T : ScriptableObject
        {
            string name = typeof(T).Name;
            string path = GenerateStandardPath(name);

            T result = ScriptableObject.CreateInstance<T>();
            result.name = name;

            AssetDatabase.CreateAsset(result, path);

            if (label.Length > 0)
                AssetDatabase.SetLabels(result, new string[1] { label });

            AssetDatabase.SaveAssets();

            return result;
        }

        private static string GenerateStandardPath(string name)
        {
            return GenerateStandardPath(Selection.activeObject, name);
        }

        private static string GenerateStandardPath(Object atAsset, string name)
        {
            string dir = AssetDatabase.GetAssetPath(atAsset);

            if (dir.Length == 0)
                // Selection must not be an asset.
                dir = "Assets";
            else if (!Directory.Exists(dir))
                // Selection must be a file asset.
                dir = Path.GetDirectoryName(dir);

            return AssetDatabase.GenerateUniqueAssetPath(dir + "/" + name + ".asset");
        }
    }
}

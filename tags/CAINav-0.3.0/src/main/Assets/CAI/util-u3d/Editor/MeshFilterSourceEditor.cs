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
using UnityEditor;

/// <summary>
/// Custom inspector for <see cref="MeshFilterSourceEditor"/>.
/// </summary>
[CustomEditor(typeof(MeshFilterSource))]
public class MeshFilterSourceEditor
    : Editor
{
    private bool mForceDirty = false;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        MeshFilterSource targ = (MeshFilterSource)target;
        GameObject[] sources = targ.sources;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        int count = sources.Length;
        if (GUILayout.Button("Add Source"))
            count++;
        if (GUILayout.Button("Remove Source"))
            count--;
        count = Mathf.Max(1, count);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        if (count != sources.Length)
        {
            // The number of sources needs to be changed.
            targ.sources = new GameObject[count];
            count = Mathf.Min(sources.Length, count);
            for (int i = 0; i < count; i++)
            {
                targ.sources[i] = sources[i];
            }
            sources = targ.sources;
            mForceDirty = true;
        }

        for (int i = 0; i < sources.Length; i++)
        {
            GameObject orig = sources[i];
#if UNITY_3_0_0	|| UNITY_3_1 || UNITY_3_2 || UNITY_3_3
            sources[i] = (GameObject)EditorGUILayout.ObjectField(
                sources[i]
                , typeof(GameObject));
#else
            sources[i] = (GameObject)EditorGUILayout.ObjectField(
                sources[i]
                , typeof(GameObject)
                , true);
#endif
            // Note: This next check is needed because the object field
            // allows project assets to be assigned.  But we only want
            // scene objects.  Project assets will never show as having 
            // geometry.  Going the extra mile and warning about empty
            // scene objects as well.
            if (sources[i] != null && sources[i] != orig)
            {
                bool hasGeometry = false;
                MeshFilter[] filters = 
                    sources[i].GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter filter in filters)
                {
                    if (filter.sharedMesh != null)
                    {
                        hasGeometry = true;
                        break;
                    }
                }
                if (!hasGeometry)
                {
                    Debug.LogWarning(string.Format(
                        "{0}: {1} does not contain any source geometry."
                            + " This is OK if geometry is going to be added"
                            + " later. This warning can also be triggered if"
                            + " {1} is a project asset (rather than a scene"
                            + "  object) which is not supported."
                        , targ.name
                        , sources[i].name)
                        , sources[i]);
                }
            }
        }

        EditorGUILayout.Separator();
        EditorGUILayout.EndVertical();

        if (GUI.changed || mForceDirty)
        {
            EditorUtility.SetDirty(target);
            mForceDirty = false;
        }
    }
}

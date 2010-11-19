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
/// Provides a custom inspector for <see cref="NavmeshView"/>.
/// </summary>
[CustomEditor(typeof(CAIArraySource))]
public class CAIArraySourceEditor 
    : Editor
{
    private bool mForceDirty = false;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        CAIArraySource sa = (CAIArraySource)target;
        GameObject[] sources = sa.sources;

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
            sa.sources = new GameObject[count];
            count = Mathf.Min(sources.Length, count);
            for (int i = 0; i < count; i++)
            {
                sa.sources[i] = sources[i];
            }
            sources = sa.sources;
            mForceDirty = true;
        }

        for (int i = 0; i < sources.Length; i++)
        {
            sources[i] = (GameObject)EditorGUILayout.ObjectField(
                sources[i]
                , typeof(GameObject));
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

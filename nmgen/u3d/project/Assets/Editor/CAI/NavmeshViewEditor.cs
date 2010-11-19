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
[CustomEditor(typeof(NavmeshView))]
public class NavmeshViewEditor 
    : Editor
{
    #region GUIContent Definitions

    private static GUIContent mNavmeshText = 
        new GUIContent("Source Navmesh");

    private static GUIContent mDisplayMeshText =  
        new GUIContent("Display Mesh");

    private static GUIContent mForceRebuildText = 
        new GUIContent("Force Rebuild");

    #endregion

    private bool mForceDirty = false;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        NavmeshView view = (NavmeshView)target;

        EditorGUIUtility.LookLikeControls(CAISet.DefaultLabelWidth);

        view.meshIsVisible = EditorGUILayout.Toggle(mDisplayMeshText
            , view.meshIsVisible);

        view.navmesh = (Navmesh)EditorGUILayout.ObjectField(mNavmeshText
            , view.navmesh
            , typeof(Navmesh));

        EditorGUILayout.Separator();

        if (GUILayout.Button(mForceRebuildText))
        {
            view.BuildMesh();
            mForceDirty = true;
        }

        EditorGUILayout.Separator();

        if (view.meshIsVisible 
            && view.navmesh != null)
        {
            int rev = view.navmesh.revision;
            if (rev != view.Revision)
            {
                EditorGUILayout.LabelField("Revision Mismatch"
                    , "Needs a forced rebuild.");
            }
            else
            {
                EditorGUILayout.LabelField("Navmesh Revision", rev.ToString());
            }
        }

        EditorGUILayout.Separator();

        if (GUI.changed || mForceDirty)
        {
            EditorUtility.SetDirty(target);
            mForceDirty = false;
        }
    }
}

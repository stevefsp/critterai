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

[CustomEditor(typeof(NavmeshTileBridge))]
public class NavmeshTileBridgeEditor
    : Editor
{
    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        NavmeshTileBridge targ = (NavmeshTileBridge)target;

        EditorGUIUtility.LookLikeControls(125);

        EditorGUILayout.Separator();

        const string label = "State";
        string state = "Ready";

        if (targ.sourcePolyMesh == null)
            state = "No PolyMesh source.";
        else if (!targ.sourcePolyMesh.HasMesh)
            state = "No PolyMesh data.";
            
        EditorGUILayout.LabelField(label, state);
        

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("PolyMesh Source");
        targ.sourcePolyMesh = (BakedPolyMesh)EditorGUILayout.ObjectField(
            targ.sourcePolyMesh, typeof(BakedPolyMesh));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        targ.userId = Mathf.Max(0
            , EditorGUILayout.IntField("UserId", targ.userId));

        targ.tileX = EditorGUILayout.IntField("Tile X", targ.tileX);
        targ.tileZ = EditorGUILayout.IntField("Tile Z", targ.tileZ);

        targ.tileLayer = EditorGUILayout.IntField("Layer", targ.tileLayer);

        targ.bvTreeEnabled = EditorGUILayout.Toggle("Enable BVTree"
            , targ.bvTreeEnabled);

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }
}

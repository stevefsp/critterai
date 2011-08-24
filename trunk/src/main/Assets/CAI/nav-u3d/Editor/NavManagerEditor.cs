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
/// Custom inspector for <see cref="NavManager"/>.
/// </summary>
[CustomEditor(typeof(NavManager))]
public class NavManagerEditor
    : Editor
{
    private bool mForceDirty = false;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        NavManager targ = (NavManager)target;

        EditorGUIUtility.LookLikeControls(125);

        EditorGUILayout.Separator();

        const string label = "State";
        string state = "Ready";

        if (targ.navmeshSource == null)
            state = "No navmesh source.";
        else if (!targ.navmeshSource.HasNavmesh())
            state = "No navmesh data.";
        else if (targ.enableCrowdManager && targ.avoidanceSource == null)
            state = "No avoidance source.";

        EditorGUILayout.LabelField(label, state);

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Navmesh Source");
        targ.navmeshSource = (BakedNavmesh)EditorGUILayout.ObjectField(
            targ.navmeshSource
            , typeof(BakedNavmesh));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Avoidance Source");
        targ.avoidanceSource = (AvoidanceConfigSet)EditorGUILayout.ObjectField(
            targ.avoidanceSource
            , typeof(AvoidanceConfigSet));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        targ.maxQueryNodes = Mathf.Max(0, EditorGUILayout.IntField(
            "Max Query Nodes"
            , targ.maxQueryNodes));

        targ.enableCrowdManager = EditorGUILayout.Toggle("Enable Crowd"
            , targ.enableCrowdManager);

        GUI.enabled = !targ.enableCrowdManager;

        // Don't like the way the vector 3 control looked.

        EditorGUILayout.PrefixLabel("Default Extents");
        EditorGUIUtility.LookLikeControls(50);

        targ.initialExtents.x = 
            EditorGUILayout.FloatField("X", targ.initialExtents.x);
        targ.initialExtents.y =
            EditorGUILayout.FloatField("Y", targ.initialExtents.y);
        targ.initialExtents.z =
            EditorGUILayout.FloatField("Z", targ.initialExtents.z);

        if (GUI.changed)
        {
            targ.initialExtents.x = Mathf.Max(0, targ.initialExtents.x);
            targ.initialExtents.y = Mathf.Max(0, targ.initialExtents.y);
            targ.initialExtents.z = Mathf.Max(0, targ.initialExtents.z);
        }

        GUI.enabled = true;

        EditorGUIUtility.LookLikeControls(125);

        GUI.enabled = targ.enableCrowdManager;

        targ.maxCrowdAgents = Mathf.Max(1, EditorGUILayout.IntField(
            "Max Agents"
            , targ.maxCrowdAgents));

        targ.maxAgentRadius = Mathf.Max(0, EditorGUILayout.FloatField(
            "Max Agent Radius"
            , targ.maxAgentRadius));

        GUI.enabled = true;

        EditorGUILayout.Separator();

        if (GUI.changed || mForceDirty)
        {
            EditorUtility.SetDirty(target);
            mForceDirty = false;
        }
    }
}

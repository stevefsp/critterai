//*
// * Copyright (c) 2012 Stephen A. Pratt
// * 
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the "Software"), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// * 
// * The above copyright notice and this permission notice shall be included in
// * all copies or substantial portions of the Software.
// * 
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// * THE SOFTWARE.
// */
//using UnityEngine;
//using UnityEditor;
//using org.critterai.u3d.editor;
//using org.critterai.nav.u3d;
//using org.critterai.nav.u3d.editor;

//// TODO: EVAL: Staged for v0.5

///// <summary>
///// <see cref="CAINavGroup"/> editor.
///// </summary>
//[CustomEditor(typeof(CAINavGroup))]
//public class CAINavGroupEditor
//    : Editor
//{
//    private bool mForceDirty = false;

//    /// <summary>
//    /// Controls behavior of the inspector.
//    /// </summary>
//    public override void OnInspectorGUI()
//    {
//        CAINavGroup targ = (CAINavGroup)target;

//        EditorGUIUtility.LookLikeControls(125);

//        EditorGUILayout.Separator();

//        string msg = "";

//        INavmeshData navmeshData = targ.NavmeshData;

//        if (navmeshData == null || !navmeshData.HasNavmesh)
//            msg = "No navigation mesh.\n";

//        if (targ.enableCrowdManager && targ.avoidanceData == null)
//            msg += "No avoidance source.";

//        if (msg.Length > 0)
//            GUILayout.Box(msg.Trim(), EditorUtil.ErrorStyle, GUILayout.ExpandWidth(true));

//        EditorGUILayout.Separator();
//        EditorGUILayout.BeginHorizontal();

//        targ.NavmeshData = NavEditorUtil.OnGUINavmeshDataField("Navmesh Data", targ.NavmeshData);

//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.BeginHorizontal();

//        EditorGUILayout.PrefixLabel("Avoidance Data");

//        targ.avoidanceData = (CAICrowdAvoidanceSet)EditorGUILayout.ObjectField(
//            targ.avoidanceData
//            , typeof(CAICrowdAvoidanceSet)
//            , false);

//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.Separator();

//        targ.maxQueryNodes = Mathf.Max(0, EditorGUILayout.IntField(
//            "Max Query Nodes"
//            , targ.maxQueryNodes));

//        targ.enableCrowdManager = EditorGUILayout.Toggle("Enable Crowd"
//            , targ.enableCrowdManager);

//        if (targ.enableCrowdManager)
//            EditorGUILayout.LabelField("Default extents", "Using crowd's");
//        else
//        {
//            // Don't like the way the vector 3 control looked.

//            GUILayout.Label("Default Extents");

//            EditorGUIUtility.LookLikeControls(50);

//            targ.initialExtents.x =
//                EditorGUILayout.FloatField("X", targ.initialExtents.x);
//            targ.initialExtents.y =
//                EditorGUILayout.FloatField("Y", targ.initialExtents.y);
//            targ.initialExtents.z =
//                EditorGUILayout.FloatField("Z", targ.initialExtents.z);

//            if (GUI.changed)
//            {
//                targ.initialExtents.x = Mathf.Max(0, targ.initialExtents.x);
//                targ.initialExtents.y = Mathf.Max(0, targ.initialExtents.y);
//                targ.initialExtents.z = Mathf.Max(0, targ.initialExtents.z);
//            }
//        }

//        EditorGUIUtility.LookLikeControls(125);

//        GUI.enabled = targ.enableCrowdManager;

//        targ.maxCrowdAgents = Mathf.Max(1, EditorGUILayout.IntField(
//            "Max Agents"
//            , targ.maxCrowdAgents));

//        targ.maxAgentRadius = Mathf.Max(0, EditorGUILayout.FloatField(
//            "Max Agent Radius"
//            , targ.maxAgentRadius));

//        GUI.enabled = true;

//        EditorGUILayout.Separator();

//        bool origChanged = GUI.changed;
//        bool orig = targ.DebugEnabled;

//        targ.DebugEnabled = EditorGUILayout.Toggle("Show Navmesh", targ.DebugEnabled);

//        if (orig != targ.DebugEnabled)
//            SceneView.RepaintAll();
//        GUI.changed = origChanged;

//        EditorGUILayout.Separator();

//        if (GUI.changed || mForceDirty)
//        {
//            EditorUtility.SetDirty(target);
//            mForceDirty = false;
//        }
//    }

//    [MenuItem("GameObject/Create CAI/Navigation Group", false, 0)]
//    static void CreateGameObject()
//    {
//        GameObject go = new GameObject("NavGroup");
//        go.AddComponent<CAINavGroup>();
//    }
//}

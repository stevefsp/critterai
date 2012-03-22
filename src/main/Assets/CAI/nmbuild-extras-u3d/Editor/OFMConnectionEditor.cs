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
using org.critterai.u3d;
using org.critterai.u3d.editor;
using UnityEngine;
using UnityEditor;

/// <summary>
/// <see cref="OFMConnection"/> compiler.
/// </summary>
[CustomEditor(typeof(OFMConnection))]
public class OFMConnectionEditor
    : Editor
{
    private static Vector3 markerSize = new Vector3(0.3f, 0.05f, 0.3f);

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        OFMConnection targ = (OFMConnection)target;

        EditorGUILayout.Separator();

        bool changed = GUI.changed;

        NMGenComponent.debugEnabled = 
            EditorGUILayout.Toggle("Show All", NMGenComponent.debugEnabled);

        if (GUI.changed)
            SceneView.RepaintAll();

        GUI.changed = changed;

        // EditorGUILayout.Separator();
        // targ.End = (Transform)EditorGUILayout.ObjectField("Endpoint", targ.End, typeof(Transform), true);

        EditorGUILayout.Separator();

        targ.Radius = EditorGUILayout.FloatField("Radius", targ.Radius);
        targ.UserId = EditorGUILayout.IntField("User Id", targ.UserId);
        targ.IsBidirectional = EditorGUILayout.Toggle("Bidirectional", targ.IsBidirectional);

        EditorGUILayout.Separator();

        targ.OverrideArea = EditorGUILayout.Toggle("Override Area", targ.OverrideArea);

        GUI.enabled = targ.OverrideArea;

        if (GUI.enabled)
            targ.AreaInt = EditorGUILayout.IntField("Area", targ.Area);
        else
            EditorGUILayout.LabelField("Area", "Default");

        GUI.enabled = true;

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    void OnSceneGUI()
    {
        OFMConnection targ = (OFMConnection)target;

        Undo.SetSnapshotTarget(target, "Endpoint move.");

        targ.EndPoint = Handles.PositionHandle(targ.EndPoint, Quaternion.identity);

        if (Input.GetMouseButtonDown(0))
        {
            Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
        }

        if (GUI.changed)
            EditorUtility.SetDirty(targ);
    }

    [DrawGizmo(GizmoType.NotSelected | GizmoType.SelectedOrChild | GizmoType.Pickable)]
    static void DrawGizmo(OFMConnection marker, GizmoType type)
    {
        if (!NMGenComponent.debugEnabled && (type & GizmoType.SelectedOrChild) == 0)
            return;

        Gizmos.color = ColorUtil.IntToColor(marker.Area, 0.6f);

        Gizmos.DrawCube(marker.transform.position, markerSize);
        Gizmos.DrawCube(marker.EndPoint, markerSize);
    }

    [MenuItem(EditorUtil.NMGenGameObjectMenu + "Off-Mesh Connection"
        , false
        , EditorUtil.GameObjectGroup + 1)]
    static void CreateGameObject()
    {
        GameObject go = new GameObject("OffMeshConn");
        OFMConnection comp = go.AddComponent<OFMConnection>();
        comp.EndPoint = go.transform.position + Vector3.up + Vector3.right + Vector3.forward;
    }
}

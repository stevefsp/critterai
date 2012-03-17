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
using UnityEditor;
using org.critterai.u3d;

public class AreaMarkerEditor
    : NMGenComponentEditor
{
    private static Vector3 markerSize = new Vector3(0.3f, 0.05f, 0.3f);

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public static void OnGUIStandard(NMGenAreaMarker targ)
    {
        EditorGUILayout.Separator();

        bool changed = GUI.changed;

        BoxAreaMarker.debugEnabled = EditorGUILayout.Toggle("Show All", BoxAreaMarker.debugEnabled);

        if (GUI.changed)
            SceneView.RepaintAll();

        GUI.changed = changed;

        EditorGUILayout.Separator();

        // Note: Clamp before sending to property.
        targ.Priority = EditorGUILayout.IntField("Priority", targ.Priority);
        targ.AreaInt = EditorGUILayout.IntField("Area", targ.Area);

        EditorGUILayout.Separator();
    }

    protected static void DrawStandardGizmo(NMGenAreaMarker marker, GizmoType type)
    {
        if (!NMGenAreaMarker.debugEnabled && (type & GizmoType.SelectedOrChild) == 0)
            return;

        Gizmos.color = ColorUtil.IntToColor(marker.Area, 0.6f);

        Vector3 pos = marker.transform.position;

        Gizmos.DrawCube(pos, markerSize);
    }
}

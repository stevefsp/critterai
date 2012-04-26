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
using System.Collections.Generic;
using org.critterai.nav;
using org.critterai.u3d.editor;

/// <summary>
/// <see cref="CAINavSettings"/> editor.
/// </summary>
[CustomEditor(typeof(CAINavSettings))]
public class CAINavSettingsEditor
    : Editor
{
    private static bool mShowAreas = true;
    private static bool mShowFlags = true;

    private static string mNewName = "";
    private static byte mNewArea = Navmesh.NullArea;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Separator();

        mShowAreas = EditorGUILayout.Foldout(mShowAreas, "Area Names");

        if (mShowAreas)
            OnGUIAreas();

        EditorGUILayout.Separator();

        mShowFlags = EditorGUILayout.Foldout(mShowFlags, "Flag Names");

        if (mShowFlags)
            OnGUIFlags();

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private void OnGUIAreas()
    {
        CAINavSettings targ = (CAINavSettings)target;

        List<byte> areas = targ.areas;
        List<string> areaNames = targ.areaNames;

        EditorGUILayout.Separator();

        GUILayout.Label("Name / Area");

        EditorGUILayout.Separator();

        if (areas.Count > 0)
        {
            EditorGUILayout.BeginVertical();

            int delChoice = -1;

            for (int i = 0; i < areas.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                GUI.enabled = (areas[i] != Navmesh.NullArea);

                string areaName = EditorGUILayout.TextField(areaNames[i]);

                if (areaName.Length > 0 
                    && areaName != areaNames[i] 
                    && !areaNames.Contains(areaName))
                {
                    areaNames[i] = areaName;
                }

                GUI.enabled = !(areas[i] == Navmesh.NullArea || areas[i] == Navmesh.MaxArea);

                byte area =
                    NavUtil.ClampArea(EditorGUILayout.IntField(areas[i], GUILayout.Width(40)));

                if (area == areas[i] || !areas.Contains(area))
                    areas[i] = area;

                if (GUILayout.Button("X"))
                    delChoice = i;

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            if (delChoice >= 0)
            {
                areaNames.RemoveAt(delChoice);
                areas.RemoveAt(delChoice);
                GUI.changed = true;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        mNewName = EditorGUILayout.TextField(mNewName).Trim();
        mNewArea =
            NavUtil.ClampArea(EditorGUILayout.IntField(mNewArea, GUILayout.Width(40)));

        GUI.enabled = (mNewName.Length > 0);

        if (GUILayout.Button("Add"))
        {
            if (areaNames.Contains(mNewName) || areas.Contains(mNewArea))
            {
                Debug.LogError(string.Format("{0}: Name or area already defined: Name: {1}, Area: {2}"
                    , targ.name, mNewName, mNewArea));
            }
            else
            {
                areaNames.Add(mNewName);
                areas.Add(mNewArea);
                mNewName = "";
                mNewArea = Navmesh.NullArea;
                GUI.changed = true;
            }
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void OnGUIFlags()
    {
        CAINavSettings targ = (CAINavSettings)target;

        string[] names = targ.flagNames;

        EditorGUILayout.Separator();

        for (int i = 0; i < names.Length; i++)
        {
            string val = EditorGUILayout.TextField(string.Format("0x{0:X}", 1 << i), names[i]);
            names[i] = (val.Length == 0 ? names[i] : val);
        }
    }

    [MenuItem(EditorUtil.MainMenu + "Nav Settings", false, EditorUtil.GlobalGroup)]
    static void EditSettings()
    {
        CAINavSettings item = EditorUtil.GetGlobalAsset<CAINavSettings>();
        Selection.activeObject = item;
    }
}

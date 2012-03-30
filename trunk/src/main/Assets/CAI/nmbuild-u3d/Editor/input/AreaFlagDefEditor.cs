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
using org.critterai.nmgen;
using System.Collections.Generic;
using org.critterai.u3d.editor;
using org.critterai.nmbuild.u3d.editor;

/// <summary>
/// <see cref="AreaFlagDef"/> editor.
/// </summary>
[CustomEditor(typeof(AreaFlagDef))]
public class AreaFlagDefEditor
    : Editor
{
    private CAINavSettings mSettings;
    private string[] mAreaNames;
    private string[] mFlagNames;

    void OnEnable()
    {
        mSettings = EditorUtil.GetGlobalAsset<CAINavSettings>();
        mAreaNames = mSettings.GetAreaNames();
        mFlagNames = mSettings.GetFlagNames();
    }

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        AreaFlagDef targ = (AreaFlagDef)target;

        // Has someone done something naughty?

        if (targ.areas == null || targ.areas == null || targ.areas.Count != targ.flags.Count)
        {
            Debug.LogError(targ.name 
                + ": Data null reference or size mismatch. Resetting component.");
            targ.areas = new List<byte>();
            targ.flags = new List<int>();
        }

        List<byte> areas = targ.areas;
        List<int> flags = targ.flags;

        EditorGUILayout.Separator();

        targ.SetPriority(EditorGUILayout.IntField("Priority", targ.Priority));

        EditorGUILayout.Separator();

        GUILayout.Label("Area / Flags");

        EditorGUILayout.Separator();

        if (areas.Count > 0)
        {
            EditorGUILayout.BeginVertical();

            int delChoice = -1;

            for (int i = 0; i < areas.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Note: Duplicates are a waste, but technically ok.

                int orig = mSettings.GetAreaNameIndex(areas[i]);
                int curr = EditorGUILayout.Popup(orig, mAreaNames);

                if (curr != orig)
                    areas[i] = mSettings.GetArea(mAreaNames[curr]);

                flags[i] = EditorGUILayout.MaskField(flags[i], mFlagNames);

                if (GUILayout.Button("X"))
                    delChoice = i;

                EditorGUILayout.EndHorizontal();
            }

            if (delChoice >= 0)
            {
                flags.RemoveAt(delChoice);
                areas.RemoveAt(delChoice);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        int selected = EditorGUILayout.Popup("Add", -1, mAreaNames);

        if (selected >= 0)
        {
            areas.Add(mSettings.GetArea(mAreaNames[selected]));
            flags.Add(org.critterai.nmbuild.NMBuild.DefaultFlag);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        GUILayout.Box(
            "Input Build Processor\nAdds an NMGen processor that adds flags to polygons based"
                + " on area id. E.g. Add the 'swim' flag to all 'water' polygons."
            , EditorUtil.HelpStyle
            , GUILayout.ExpandWidth(true));

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Compiler : Area Flag Def"
        , false, NMBEditorUtil.CompilerGroup)]
    static void CreateAsset()
    {
        AreaFlagDef item = EditorUtil.CreateAsset<AreaFlagDef>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}

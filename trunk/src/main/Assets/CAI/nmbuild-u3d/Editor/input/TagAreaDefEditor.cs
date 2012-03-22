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
using org.critterai.nav;
using org.critterai.u3d.editor;
using org.critterai.nmbuild.u3d.editor;

/// <summary>
/// <see cref="TagAreaDef"/> editor.
/// </summary>
[CustomEditor(typeof(TagAreaDef))]
public class TagAreaDefEditor
    : Editor
{
    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        TagAreaDef targ = (TagAreaDef)target;

        // Has someone done something naughty?

        if (targ.areas == null || targ.tags == null)
        {
            Debug.LogError(targ.name + "Data null reference. Resetting component.");
            targ.areas = new List<byte>();
            targ.tags = new List<string>();
        }

        List<byte> areas = targ.areas;
        List<string> tags = targ.tags;

        if (areas.Count > tags.Count)
        {
            areas.RemoveRange(tags.Count, areas.Count - tags.Count);
            Debug.LogError(targ.name + "Data size mismatch. Area list truncated.");
        }
        else if (tags.Count > areas.Count)
        {
            tags.RemoveRange(areas.Count, tags.Count - areas.Count);
            Debug.LogError(targ.name + "Data size mismatch. Mesh list truncated.");
        }

        EditorGUILayout.Separator();

        targ.SetPriority(EditorGUILayout.IntField("Priority", targ.Priority));

        EditorGUILayout.Separator();

        targ.recursive = EditorGUILayout.Toggle("Recursive", targ.recursive);

        EditorGUILayout.Separator();

        GUILayout.Label("Tag / Area");

        EditorGUILayout.Separator();

        if (areas.Count > 0)
        {
            EditorGUILayout.BeginVertical();

            int delChoice = -1;

            for (int i = 0; i < areas.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                string tag = EditorGUILayout.TagField(tags[i]);

                if (tag == tags[i] || !tags.Contains(tag))
                    tags[i] = tag;
                else
                {
                    Debug.LogWarning(targ.name + ": " 
                        + tag + " is already defined.  Change ignored.");
                }

                areas[i] = NavUtil.ClampArea(
                    EditorGUILayout.IntField(areas[i], GUILayout.Width(40)));

                if (GUILayout.Button("Remove"))
                    delChoice = i;

                EditorGUILayout.EndHorizontal();
            }

            if (delChoice >= 0)
            {
                tags.RemoveAt(delChoice);
                areas.RemoveAt(delChoice);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        string ntag = EditorGUILayout.TagField("Add", "");

        if (ntag.Length > 0)
        {
            if (tags.Contains(ntag))
            {
                Debug.LogWarning(targ.name + ": "
                        + ntag + " is already defined.  New entry igored.");
            }
            else
            {
                tags.Add(ntag);
                areas.Add(NMGen.MaxArea);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        GUILayout.Box(
            "Input Build Processor\nApplies area ids to components based on the specified tags."
                + " If recursive, then the area will be applied if any of a component's parents"
                + " have the tag."
            , EditorUtil.HelpStyle
            , GUILayout.ExpandWidth(true));

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Area Definition : Tag", false, NMBEditorUtil.AreaGroup)]
    static void CreateAsset()
    {
        TagAreaDef item = EditorUtil.CreateAsset<TagAreaDef>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}

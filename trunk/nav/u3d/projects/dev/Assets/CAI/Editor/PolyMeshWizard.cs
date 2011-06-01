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
using org.critterai.nmgen.u3d.editor;

/// <summary>
/// Provides a way of selecting and creating polygon mesh component structures.
/// </summary>
public sealed class PolyMeshWizard
    : EditorWindow
{

    private PolyMeshEditorFlags mNMGenFlags = PolyMeshEditorFlags.MeshFilterArray
        | PolyMeshEditorFlags.BuildConfig
        | PolyMeshEditorFlags.BakedPolyMesh;

    /// <summary>
    /// Presents the GUI.
    /// </summary>
    public void OnGUI()
    {
        mNMGenFlags = HandleSelection(mNMGenFlags, false);

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        bool closeIt = false;

        if (GUILayout.Button("Create"))
        {
            mNMGenFlags |= PolyMeshEditorFlags.BakedPolyMesh;
            BakedPolyMesh polyMesh;
            Selection.activeGameObject = Build(mNMGenFlags, out polyMesh);
            closeIt = true;
        }

        if (GUILayout.Button("Cancel"))
            closeIt = true;

        EditorGUILayout.EndHorizontal();

        if (closeIt)
            this.Close();
    }

    /// <summary>
    /// Builds the polygon mesh component structure.
    /// </summary>
    /// <param name="flags">The components to include.</param>
    /// <param name="mesh">A convenience reference to the polygon mesh 
    /// component</param>
    /// <returns>The game object that was created. Or null on error.</returns>
    public static GameObject Build(PolyMeshEditorFlags flags
        , out BakedPolyMesh mesh)
    {
        if ((flags & PolyMeshEditorFlags.BakedPolyMesh) == 0)
        {
            mesh = null;
            return null;
        }

        GameObject result = null;

        result = new GameObject("BakedPolyMesh");

        DSGeometry geomSource = null;
        if ((flags & PolyMeshEditorFlags.MeshFilterArray) != 0)
            geomSource =
                result.AddComponent<MeshFilterSource>();
        else if ((flags & PolyMeshEditorFlags.TaggedMeshFilter) != 0)
            geomSource =
                result.AddComponent<TaggedMeshFilterSource>();

        NMGenBuildConfig nmgenConfig = null;
        if ((flags & PolyMeshEditorFlags.BuildConfig) != 0)
            nmgenConfig =
                result.AddComponent<NMGenBuildConfig>();

        mesh = result.AddComponent<BakedPolyMesh>();

        mesh.buildConfig = nmgenConfig;
        mesh.geomSource = geomSource;

        return result;
    }

    /// <summary>
    /// Presents the option selection portion of the GUI.
    /// </summary>
    /// <param name="flags">The current flags.</param>
    /// <param name="includePolyMesh">TRUE if the polygon mesh option should
    /// be included in the GUI.</param>
    /// <returns>The new flags.</returns>
    public static PolyMeshEditorFlags HandleSelection(PolyMeshEditorFlags flags
        , bool includePolyMesh)
    {
        EditorGUIUtility.LookLikeControls(250);
        EditorGUILayout.PrefixLabel("Geometry Source Options");

        bool allowed = (!includePolyMesh 
            || ((flags & PolyMeshEditorFlags.BakedPolyMesh) != 0));

        GUI.enabled = allowed;

        EditorGUIUtility.LookLikeControls(150);

        bool bo;
        bool bn;

        bo = (flags & PolyMeshEditorFlags.MeshFilterArray) != 0;
        bn = EditorGUILayout.Toggle("Mesh Filter (Array)", bo);

        if (bn)
        {
            flags |= PolyMeshEditorFlags.MeshFilterArray;
            flags &= ~PolyMeshEditorFlags.TaggedMeshFilter;
        }
        else
            flags &= ~PolyMeshEditorFlags.MeshFilterArray;

        bo = (flags & PolyMeshEditorFlags.TaggedMeshFilter) != 0;
        bn = EditorGUILayout.Toggle("Mesh Filter (Tagged)", bo);

        if (bn)
        {
            flags |= PolyMeshEditorFlags.TaggedMeshFilter;
            flags &= ~PolyMeshEditorFlags.MeshFilterArray;
        }
        else
            flags &= ~PolyMeshEditorFlags.TaggedMeshFilter;

        GUI.enabled = true;

        EditorGUILayout.Separator();
        EditorGUIUtility.LookLikeControls(250);
        EditorGUILayout.PrefixLabel("PolyMesh Options");

        GUI.enabled = allowed;
        EditorGUIUtility.LookLikeControls(150);

        bo = (flags & PolyMeshEditorFlags.BuildConfig) != 0;
        bn = EditorGUILayout.Toggle("NMGen Build Config", bo);

        if (bn)
            flags |= PolyMeshEditorFlags.BuildConfig;
        else
            flags &= ~PolyMeshEditorFlags.BuildConfig;

        GUI.enabled = true;

        if (includePolyMesh)
        {
            bo = (flags & PolyMeshEditorFlags.BakedPolyMesh) != 0;
            bn = EditorGUILayout.Toggle("Baked PolyMesh", bo);

            if (bn)
                flags |= PolyMeshEditorFlags.BakedPolyMesh;
            else
                flags &= ~PolyMeshEditorFlags.BakedPolyMesh;
        }

        return flags;
    }

    /// <summary>
    /// Defines the menu item for the editor.
    /// </summary>
    [MenuItem("GameObject/Create CAI/PolyMesh", false, 0)]
    public static void CreatePolyMeshMenu()
    {
        PolyMeshWizard window = EditorWindow.GetWindow<PolyMeshWizard>(true
            , "PolyMesh Options"
            , true);

        window.position = new Rect(100, 100, 250, 180);
        window.Focus();
    }
}

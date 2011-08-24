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

public sealed class NavmeshWizard
    : EditorWindow
{

    private PolyMeshEditorFlags mNMGenFlags = PolyMeshEditorFlags.MeshFilterArray
        | PolyMeshEditorFlags.BuildConfig
        | PolyMeshEditorFlags.BakedPolyMesh;

    public void OnGUI()
    {
        mNMGenFlags = PolyMeshWizard.HandleSelection(mNMGenFlags, true);

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        bool closeIt = false;

        if (GUILayout.Button("Create"))
        {
            BakedNavmesh mesh;
            Selection.activeGameObject = Build(mNMGenFlags, out mesh);
            closeIt = true;
        }

        if (GUILayout.Button("Cancel"))
            closeIt = true;

        EditorGUILayout.EndHorizontal();

        if (closeIt)
            this.Close();
    }

    public static GameObject Build(PolyMeshEditorFlags flags, out BakedNavmesh mesh)
    {
        BakedPolyMesh polyMesh;
        GameObject goPolyMesh = PolyMeshWizard.Build(flags, out polyMesh);

        GameObject goNavmesh = new GameObject("BakedNavmesh");

        NavmeshTileBridge tileSource =
            goNavmesh.AddComponent<NavmeshTileBridge>();
        tileSource.sourcePolyMesh = polyMesh;

        mesh = goNavmesh.AddComponent<BakedNavmesh>();

        mesh.sourceData = tileSource;

        if (goPolyMesh != null)
            goPolyMesh.transform.parent = goNavmesh.transform;

        return goNavmesh;
    }

    [MenuItem("GameObject/Create CAI/Navmesh", false, 0)]
    public static void AddNavmesh()
    {
        NavmeshWizard window = EditorWindow.GetWindow<NavmeshWizard>(true
            , "Navmesh Options"
            , true);

        window.position = new Rect(100, 100, 250, 180);
        window.Focus();
    }
}

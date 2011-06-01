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
using org.critterai.nav;
using org.critterai.nav.u3d;

[CustomEditor(typeof(BakedNavmesh))]
public class BakedNavmeshEditor
    : Editor
{
    private bool mForceDirty = false;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        BakedNavmesh targ = (BakedNavmesh)target;

        EditorGUIUtility.LookLikeControls(125);

        EditorGUILayout.Separator();

        const string label = "State";
        string state;

        if (targ.HasNavmesh())
            state = "Baked";
        else if (targ.sourceData == null)
            state = "No tile source.";
        else if (targ.sourceData.HasTileBuildData)
            state = "Ready to bake.";
        else
            state = "No tile data.";

        EditorGUILayout.LabelField(label, state);

        EditorGUILayout.Separator();

        bool bo = targ.DisplayMesh;
        bool bn = EditorGUILayout.Toggle("Display Mesh", bo);
        if (bn != bo)
            targ.DisplayMesh = bn;

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Tile Data Source");
        targ.sourceData = (DSTileData)EditorGUILayout.ObjectField(
            targ.sourceData, typeof(DSTileData));
         
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        if (!IsReadyToBuild(targ))
            GUI.enabled = false;
        if (GUILayout.Button("Bake") && BuildMesh(targ))
            mForceDirty = true;
        GUI.enabled = true;

        if (!targ.HasNavmesh())
            GUI.enabled = false;
        if (GUILayout.Button("Clear Mesh") && targ.HasNavmesh())
        {
            targ.ClearMesh();
            mForceDirty = true;
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        if (GUI.changed || mForceDirty)
        {
            EditorUtility.SetDirty(target);
            mForceDirty = false;
        }
    }

    private static bool BuildMesh(BakedNavmesh targ)
    {
        string pre = targ.name + ": NavmeshBuilder: Build aborted: ";

        NavmeshTileBuildData buildData = 
            targ.sourceData.GetTileBuildData(false);

        if (buildData == null || buildData.IsDisposed)
        {
            Debug.LogError(pre 
                + " Data source provided null or disposed tile data.");
            return false;
        }

        Navmesh mesh;
        NavStatus status = Navmesh.Build(buildData, out mesh);

        // Debug.Log(NavmeshDebug.ToString(mesh.GetTile(0).GetHeader()));

        if (NavUtil.Failed(status))
        {
            Debug.LogError(pre + "Status flags: " + status.ToString());
            return false;
        }

        if (!targ.Bake(mesh))
        {
            Debug.LogError(pre + "Could not load mesh into builder."
                + " Internal error?");
            return false;
        }

        return true;
    }

    private static bool IsReadyToBuild(BakedNavmesh targ)
    {
        return (targ.sourceData != null && targ.sourceData.HasTileBuildData);
    }
}

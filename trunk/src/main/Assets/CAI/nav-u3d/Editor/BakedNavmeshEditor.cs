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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Custom inspector for <see cref="BakedNavmesh"/>.
/// </summary>
[CustomEditor(typeof(BakedNavmesh))]
public class BakedNavmeshEditor
    : Editor
{
    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        bool mForceDirty = false;
        BakedNavmesh targ = (BakedNavmesh)target;

        EditorGUIUtility.LookLikeControls(125);

        EditorGUILayout.Separator();

        const string label = "State";
        string state;

        if (targ.HasNavmesh)
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
#if UNITY_3_0_0	|| UNITY_3_1 || UNITY_3_2 || UNITY_3_3
        targ.sourceData = (DSTileData)EditorGUILayout.ObjectField(
            targ.sourceData, typeof(DSTileData));
#else
        targ.sourceData = (DSTileData)EditorGUILayout.ObjectField(
            targ.sourceData, typeof(DSTileData), true);
#endif
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        if (!IsReadyToBuild(targ))
            GUI.enabled = false;
        if (GUILayout.Button("Bake") && BuildMesh(targ))
            mForceDirty = true;
        GUI.enabled = true;

        GUI.enabled = targ.HasNavmesh;
        if (GUILayout.Button("Clear Mesh"))
        {
            targ.ClearMesh();
            mForceDirty = true;
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = targ.HasNavmesh;
        if (GUILayout.Button("Save"))
        {
            string filePath = EditorUtility.SaveFilePanel(
                "Save Navigation Mesh"
                , ""
                , targ.name
                , "navmesh");
            SaveMesh(targ, filePath);
        }
        GUI.enabled = true;

        if (GUILayout.Button("Load"))
        {
            string filePath = EditorUtility.OpenFilePanel(
                "Select Serialized Navmesh"
                , ""
                , "navmesh");
            if (LoadMesh(targ, filePath))
                mForceDirty = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        if (GUI.changed || mForceDirty)
        {
            EditorUtility.SetDirty(target);
            mForceDirty = false;
        }
    }

    //private static bool LoadMeshBytes(BakedNavmesh targ, string filePath)
    //{
    //    string msg = null;

    //    if (filePath.Length == 0)
    //        return false;

    //    FileStream fs = null;
    //    BinaryFormatter formatter = new BinaryFormatter();

    //    try
    //    {
    //        fs = new FileStream(filePath, FileMode.Open);
    //        System.Object obj = formatter.Deserialize(fs);

    //        // Roundabout load method is used for error detection.

    //        Navmesh nm;
    //        NavStatus status = Navmesh.Build((byte[])obj, out nm);
    //        if (NavUtil.Failed(status))
    //            msg = status.ToString();
    //        else if (!targ.Bake(nm))
    //            msg = "Could not load mesh. Internal error?";
    //    }
    //    catch (System.Exception ex)
    //    {
    //        msg = ex.Message;
    //    }
    //    finally
    //    {
    //        if (fs != null)
    //            fs.Close();
    //    }

    //    if (msg != null)
    //    {
    //        Debug.LogError(targ.name + ": BakedNavmesh: Load bytes failed: " 
    //            + msg);
    //        return false;
    //    }

    //    return true;
    //}

    private static bool LoadMesh(BakedNavmesh targ, string filePath)
    {
        string msg = null;

        if (filePath.Length == 0)
            return false;

        FileStream fs = null;
        BinaryFormatter formatter = new BinaryFormatter();

        try
        {
            fs = new FileStream(filePath, FileMode.Open);

            if (!targ.Bake((Navmesh)formatter.Deserialize(fs)))
                msg = "Could not load mesh. Internal error?";
        }
        catch (System.Exception ex)
        {
            msg = ex.Message;
        }
        finally
        {
            if (fs != null)
                fs.Close();
        }

        if (msg != null)
        {
            Debug.LogError(targ.name + ": BakedNavmesh: Load failed: " + msg);
            return false;
        }

        return true;
    }

    private static void SaveMesh(BakedNavmesh targ, string filePath)
    {
        if (filePath.Length == 0 || !targ.HasNavmesh)
            return;

        FileStream fs = null;
        BinaryFormatter formatter = new BinaryFormatter();

        try
        {
            fs = new FileStream(filePath, FileMode.Create);
            formatter.Serialize(fs, targ.GetNavmesh());
        }
        catch (System.Exception ex)
        {
            Debug.LogError(targ.name + ": BakedNavmesh: Save failed: "
                + ex.Message);
        }
        finally
        {
            if (fs != null)
                fs.Close();
        }
    }

    //private static void SaveMeshBytes(BakedNavmesh targ, string filePath)
    //{
    //    if (filePath.Length == 0 || !targ.HasNavmesh)
    //        return;

    //    FileStream fs = null;
    //    BinaryFormatter formatter = new BinaryFormatter();

    //    try
    //    {
    //        fs = new FileStream(filePath, FileMode.Create);
    //        formatter.Serialize(fs, targ.GetNavmesh().GetSerializedMesh());
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError(targ.name + ": BakedNavmesh: Save bytes failed: "
    //            + ex.Message);
    //    }
    //    finally
    //    {
    //        if (fs != null)
    //            fs.Close();
    //    }
    //}

    private static bool BuildMesh(BakedNavmesh targ)
    {
        string pre = targ.name + ": BakedNavmesh: Bake aborted: ";

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

        if (NavUtil.Failed(status))
        {
            Debug.LogError(pre + "Status flags: " + status.ToString());
            return false;
        }

        if (!targ.Bake(mesh))
        {
            Debug.LogError(pre + "Could not load mesh."
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

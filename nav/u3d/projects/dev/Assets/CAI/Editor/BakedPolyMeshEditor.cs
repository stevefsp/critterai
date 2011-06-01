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
using org.critterai.nmgen;
using org.critterai.geom;
using org.critterai.nmgen.u3d;
using org.critterai;
using System.IO;

/// <summary>
/// Provides a custom inspector for <see cref="CAIArraySource"/>.
/// </summary>
[CustomEditor(typeof(BakedPolyMesh))]
public class BakedPolyMeshEditor
    : Editor
{
    public static bool displayDetails = false;

    private const int DefaultLabelWidth = 150;

    private bool mForceDirty = false;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        BakedPolyMesh targ = (BakedPolyMesh)target;

        EditorGUIUtility.LookLikeControls(DefaultLabelWidth);

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();

        if (targ.HasMesh)
        {
            displayDetails = EditorGUILayout.Foldout(displayDetails
                , "Mesh Baked");
            if (displayDetails)
            {
                EditorGUILayout.LabelField("Poly Count"
                    , targ.PolyCount.ToString());
                EditorGUILayout.LabelField("Detail Tri Count"
                    , targ.DetailTriCount.ToString());

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Source Tri Count"
                    , targ.SourceTriCount.ToString());

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Poly Bounds Min"
                    , targ.PolyBoundsMin.ToString());
                EditorGUILayout.LabelField("Poly Bounds Max"
                    , targ.PolyBoundsMax.ToString());

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Source Bounds Min"
                    , targ.SourceBoundsMin.ToString());
                EditorGUILayout.LabelField("Source Bounds Max"
                    , targ.SourceBoundsMax.ToString());
            }
        }
        else
        {
            const string label = "State";
            string state = "Ready to bake.";

            if (targ.geomSource == null)
                state = "No geometry source.";
            else if (!targ.geomSource.HasGeometry)
                state = "No geometry data.";
            else if (targ.buildConfig == null)
                state = "No build config.";
            
            EditorGUILayout.LabelField(label, state);
        }

        EditorGUILayout.Separator();

        bool bo;
        bool bn;

        // Note: It is no use trying to block GUI.changed for fields
        // we don't care about tracking. See comment at end of method
        // for details.

        bo = targ.DisplayPolyMesh;
        bn = EditorGUILayout.Toggle("Display Poly Mesh", bo);
        if (bo != bn)
            targ.DisplayPolyMesh = bn;

        bo = targ.DisplayPolyMeshDetail;
        bn = EditorGUILayout.Toggle("Display Detail Mesh", bo);
        if (bo != bn)
            targ.DisplayPolyMeshDetail = bn;

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Configuration Source");
        targ.buildConfig = (NMGenBuildConfig)EditorGUILayout.ObjectField(
            targ.buildConfig
            , typeof(NMGenBuildConfig));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Geometry Source");
        targ.geomSource = (DSGeometry)EditorGUILayout.ObjectField(
            targ.geomSource
            , typeof(DSGeometry));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        if (!IsReadyToBuild(targ))
            GUI.enabled = false;
        if (GUILayout.Button("Bake Mesh") && BuildMesh(targ))
            mForceDirty = true;
        GUI.enabled = true;

        if (!targ.HasMesh)
            GUI.enabled = false;
        if (GUILayout.Button("Clear Mesh") && targ.HasMesh)
        {
            targ.ClearMesh();
            mForceDirty = true;
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        if (targ.buildConfig == null)
        {
            if (GUILayout.Button("Add New Configuration"))
            {
                targ.buildConfig
                    = targ.gameObject.AddComponent<NMGenBuildConfig>();
            }
        }
        else if (targ.buildConfig.GetType() == typeof(NMGenBuildConfig))
        {
            if (targ.geomSource == null)
                GUI.enabled = false;
            if (GUILayout.Button("Derive Configuration"))
            {
                EditorUtility.DisplayProgressBar(targ.name
                    , "Getting source geometry..."
                    , 0);

                TriangleMesh source = targ.geomSource.GetGeometry();

                EditorUtility.ClearProgressBar();

                if (source == null || source.vertCount < 1)
                {
                    Debug.LogWarning(targ.name +
                         ": Can't derive configuration.  No source geometry."
                         , targ);
                }
                else
                {
                    float[] bounds = Vector3Util.GetBounds(source.verts
                        , new float[6]);
                    ((NMGenBuildConfig)targ.buildConfig).config.Derive(
                        bounds[0], bounds[1], bounds[2]
                        , bounds[3], bounds[4], bounds[5]);
                }
            }
            GUI.enabled = true;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.EndVertical();

        mForceDirty = (GUI.changed || mForceDirty);

        if (GUI.changed || mForceDirty)
        {
            // Setting the target to dirty triggers a scene update.
            // So can't avoid tracking unimportant non-serialized fields.
            EditorUtility.SetDirty(target);
            mForceDirty = false;
        }
    }

    private bool IsReadyToBuild(BakedPolyMesh targ)
    {
        return !(targ.buildConfig == null || targ.geomSource == null);
    }

    private bool BuildMesh(BakedPolyMesh targ)
    {
        const string ProgressTitle = "Building poly mesh...";

        TriangleMesh source = targ.geomSource.GetGeometry();

        if (source == null || source.triCount < 1)
        {
            Debug.LogError(targ.name 
                + ": Build failed: The geometry source did not"
                + " provide any geometry."
                , targ);
            return false;
        }

        EditorUtility.DisplayProgressBar(ProgressTitle
            , "Getting source geometry..."
            , 0);

        NMGenBuildParams config = targ.buildConfig.GetBuildConfig();

        EditorUtility.ClearProgressBar();

        if (config == null)
        {
            Debug.LogError(targ.name
                + ": Build failed: The configuration source did not"
                + " provide a build configuration."
                , targ);
            return false;
        }

        float[] bounds = Vector3Util.GetBounds(source.verts, new float[6]);
        config.SetBoundsMin(bounds[0], bounds[1], bounds[2]);
        config.SetBoundsMax(bounds[3], bounds[4], bounds[5]);

        const int threshold = 5000000;
        int cellCount = (int)(((bounds[3] - bounds[0]) / config.XZCellSize)
            * ((bounds[5] - bounds[2]) / config.XZCellSize));

        if (cellCount > threshold
            && !EditorUtility.DisplayDialog("Are you sure?"
                , "Your source geometry and configuration will likely result"
                    + " in a long build time.  Are you sure you are ready?"
                , "Yes, build the mesh now."
                , "No, I am not ready."))
        {
            return false;
        }

        IncrementalBuilder builder = new IncrementalBuilder(true
            , config.GetConfig()
            , config.BuildFlags
            , source);

        bool progDisplayed = !builder.IsFinished;

        try
        {
            while (!builder.IsFinished)
            {
                EditorUtility.DisplayProgressBar(
                    ProgressTitle
                    , NMGenUtil.GetBuildStateText(builder.State)
                    , NMGenUtil.GetBuildProgress(builder.State));
                builder.Build();
            }
        }
        catch (System.Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError(targ.name + ": " + ex.Message, targ);
            return false;
        }

        if (progDisplayed)
            EditorUtility.ClearProgressBar();

        if (builder.State == BuildState.Aborted)
        {
            string message = targ.name + ": Build failed.";
            string[] msgs = builder.GetMessages();
            foreach (string msg in msgs)
            {
                message += "\n" + msg;
            }
            Debug.LogError(message, targ);
            return false;
        }

        if (!targ.Bake(builder.PolyMesh
            , builder.DetailMesh
            , source.triCount
            , Vector3Util.GetVector(config.GetBoundsMin())
            , Vector3Util.GetVector(config.GetBoundsMax())))
        {
            Debug.LogError(targ.name
                + ": Build failed: Could not perform final load."
                + " Internal error?"
                , targ);
            return false;
        }

        return true;
    }

    private string GetVectorString(float[] v)
    {
        return string.Format("[{0,0:F3}, {1,0:F3}, {2,0:F3}]"
            , v[0], v[1], v[2]);
    }
}

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
using System.Collections.Generic;
using org.critterai.mesh;
using org.critterai.math;
using System.IO;

/// <summary>
/// Provides custom inspector and navigation mesh build functionality for
/// <see cref="GeneratedNavmesh"/>.  (Unity Pro Only)
/// </summary>
/// <remarks>
/// <p>Customize this inspector to meet your needs by changing
/// the various variables located in <see cref="GNMSet"/>.</p>
/// <p>Customize the behavior of the configuration settings by
/// updating the <see cref="NavmeshConfig"/> code.</p>
/// </remarks>
[CustomEditor(typeof(GeneratedNavmesh))]
public class GeneratedNavmeshEditor 
    : Editor
{
    /*
     * Design notes:
     * 
     * The reason for the current messaging design is that we don't always know
     * whether or not a message will be sent to the console until the build 
     * process is complete.  So all messages are queued and the what to
     * display is decided later.
     * 
     */

    private const string ErrorMarker = " (ERROR)";
    private const string WarnMarker = " (WARNING)";
    private const string PrefixRoot = "NMGen: ";
    private const string PrefixPerf = "NMGen: Perf: ";
    private const string PrefixExt = "Lib: ";
    private const string PrefixGen = "Gen: ";
    private const string PrefixAgg = "Agg: ";
    private const string PrefixBuild = "Build: ";

    private string mProgressTitle = "";

    // Use of the progress displayed variable ensures that progress
    // are cleaned up even if there is a GUI exception.
    private bool mProgressDisplayed = false;

    private bool mForceDirty = false;

    // Design note: Only set this to TRUE if there is a warning or error.
    private bool mForceMessages = false;

    private List<string> mPerformanceMessages = new List<string>();
    private List<string> mMessages = new List<string>();

    // The vert count value is dual use.  A value of zero indicates
    // "data unavailable" for all source geometry data.
    private int mSourceVertCount = 0;
    private int mSourceTriangleCount = 0;
    private float[] mSourceBounds = null;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        ProcessMessages();

        GeneratedNavmesh nm = (GeneratedNavmesh)target;

        EditorGUIUtility.LookLikeControls(CAISet.DefaultLabelWidth);

        EditorGUILayout.Separator();
        ProcessGUIStats(nm);        // Statistics
        ProcessGUIMainSec(nm);      // Main button section (No foldout)
        ProcessGUIPriConfig(nm);    // Primary configuration
        ProcessGUISecConfig(nm);    // Other configuration
        ProcessGUIAdvanced(nm);     // Advanced functionality
        EditorGUILayout.Separator();

        //  Cleanup and finalize.

        if (mProgressDisplayed)
            EditorUtility.ClearProgressBar();

        if (GUI.changed || mForceDirty)
        {
            nm.planeTolerance = Mathf.Min(nm.config.minTraversableHeight / 2
                , nm.config.yResolution * 2);
            EditorUtility.SetDirty(target);
            mForceDirty = false;
        }
    }

    /// <summary>
    /// Display the inspector's statistics foldout.
    /// </summary>
    /// <param name="nm">The active navigation mesh.</param>
    private void ProcessGUIStats(GeneratedNavmesh nm)
    {
        const string NavmeshHeaderText = "Navmesh";
        const string SourceGeomText = "Source Geometry";
        const string NotGeneratedText = "Not generated.";
        const string VertText = "Vertices";
        const string TriText = "Triangles";
        const string MinBoundsText = "Min Bounds";
        const string MaxBoundsText = "Max Bounds";

        GNMSet.DisplayStats = EditorGUILayout.Foldout(GNMSet.DisplayStats
            , "Statistics");
        if (GNMSet.DisplayStats)
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField(NavmeshHeaderText, "");

            if (nm.vertices == null || nm.vertices.Length == 0)
            {
                EditorGUILayout.LabelField("No mesh data.", "");
            }
            else
            {
                EditorGUILayout.LabelField(VertText
                    , (nm.vertices.Length / 3).ToString());
                EditorGUILayout.LabelField(TriText
                    , (nm.triangles.Length / 3).ToString());
                EditorGUILayout.LabelField(MinBoundsText
                    , nm.minBounds.ToString());
                EditorGUILayout.LabelField(MaxBoundsText
                    , nm.maxBounds.ToString());
                EditorGUILayout.LabelField("Revision"
                    , nm.revision.ToString());
            }

            EditorGUILayout.Separator();
            if (mSourceVertCount > 0)
            {
                EditorGUILayout.LabelField(SourceGeomText, "(Last Known)");

                EditorGUILayout.LabelField(VertText
                    , mSourceVertCount.ToString());
                EditorGUILayout.LabelField(TriText
                    , mSourceTriangleCount.ToString());
                EditorGUILayout.LabelField(MinBoundsText
                    , Vector3Util.GetVector(mSourceBounds, 0).ToString());
                EditorGUILayout.LabelField(MaxBoundsText
                    , Vector3Util.GetVector(mSourceBounds, 1).ToString());
            }
            else
            {
                EditorGUILayout.LabelField(SourceGeomText, NotGeneratedText);
            }

            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Display and process the inspector's primary configuration foldout.
    /// </summary>
    /// <param name="nm">The active navigation mesh.</param>
    private void ProcessGUIPriConfig(GeneratedNavmesh nm)
    {
        EditorGUILayout.Separator();
        GNMSet.DisplayPriConfig = EditorGUILayout.Foldout(
            GNMSet.DisplayPriConfig
            , "Primary Configuration");
        if (GNMSet.DisplayPriConfig)
        {
            EditorGUILayout.BeginVertical();

            nm.config.minTraversableHeight = EditorGUILayout.FloatField(
                GNMSet.MinHeightText
                , nm.config.minTraversableHeight);

            nm.config.maxTraversableStep = EditorGUILayout.FloatField(
                GNMSet.MaxStepText
                , nm.config.maxTraversableStep);

            nm.config.traversableAreaBorderSize = EditorGUILayout.FloatField(
                GNMSet.BorderSizeText
                , nm.config.traversableAreaBorderSize);

            nm.config.maxTraversableSlope = EditorGUILayout.Slider(
                GNMSet.MaxSlopeText
                , nm.config.maxTraversableSlope
                , 0
                , 85.5f);

            nm.config.xzResolution = EditorGUILayout.FloatField(
                GNMSet.XZResolutionText
                , nm.config.xzResolution);

            nm.config.yResolution = EditorGUILayout.FloatField(
                GNMSet.YResolutionText
                , nm.config.yResolution);

            nm.config.maxEdgeLength = EditorGUILayout.FloatField(
                GNMSet.MaxEdgeText
                , nm.config.maxEdgeLength);

            nm.config.edgeMaxDeviation = EditorGUILayout.FloatField(
                GNMSet.EdgeDeviationText
                , nm.config.edgeMaxDeviation);

            nm.config.contourSampleDistance = EditorGUILayout.FloatField(
                GNMSet.ContourSampleText
                , nm.config.contourSampleDistance);

            nm.config.contourMaxDeviation = EditorGUILayout.FloatField(
                GNMSet.ContourDevText
                , nm.config.contourMaxDeviation);

            nm.config.minIslandRegionSize = EditorGUILayout.IntField(
                GNMSet.IslandRegionText
                , nm.config.minIslandRegionSize);

            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Display and process the inspector's secondary configuration foldout.
    /// </summary>
    /// <param name="nm">The active navigation mesh.</param>
    private void ProcessGUISecConfig(GeneratedNavmesh nm)
    {
        EditorGUILayout.Separator();
        GNMSet.DisplayOtherConfig = EditorGUILayout.Foldout(
            GNMSet.DisplayOtherConfig
            , "Other Configuration");
        if (GNMSet.DisplayOtherConfig)
        {
            EditorGUILayout.BeginVertical();

            nm.config.maxVertsPerPoly = EditorGUILayout.IntSlider(
                GNMSet.MaxPolyVertText
                , nm.config.maxVertsPerPoly
                , 3
                , 8);

            nm.config.smoothingThreshold = EditorGUILayout.IntSlider(
                GNMSet.SmoothingText
                , nm.config.smoothingThreshold
                , 0
                , 4);

            nm.config.heightfieldBorderSize = EditorGUILayout.FloatField(
                GNMSet.HFBorderText
                , nm.config.heightfieldBorderSize);

            nm.config.mergeRegionSize = EditorGUILayout.IntField(
                GNMSet.MergeSizeText
                , nm.config.mergeRegionSize);

            nm.config.clipLedges = EditorGUILayout.Toggle(
                GNMSet.ClipLedgesText
                , nm.config.clipLedges);

            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Display and process the inspector's main button section.
    /// </summary>
    /// <param name="nm">The active navigation mesh.</param>
    private void ProcessGUIMainSec(GeneratedNavmesh nm)
    {
        EditorGUILayout.Separator();
        EditorGUILayout.BeginVertical();

        nm.sourceGeometry = (CAISource)EditorGUILayout.ObjectField(
            nm.sourceGeometry
            , typeof(CAISource));

        EditorGUILayout.Separator();

        bool doBuild = false;
        if (GUILayout.Button(GNMSet.BuildMeshText))
        {
            doBuild = true;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(GNMSet.CleanConfigText))
        {
            NavmeshGenerator.ApplyMandatoryLimits(nm.config);
            nm.config.Clean();
            nm.config.ApplyDecimalLimits();
            mForceDirty = true;
        }

        if (GUILayout.Button(GNMSet.DeriveConfigText))
        {
            mProgressTitle = "Deriving Configuration";
            DeriveConfig(nm);
            mForceDirty = true;
            BugWorkaround();
        }

        if (GUILayout.Button(GNMSet.ResetConfigText))
        {
            nm.config = NavmeshConfig.GetDefault();
            mForceDirty = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        if (doBuild)
        {
            mProgressTitle = "Building Navigation Mesh";
            BuildMesh(nm);
            mForceDirty = true;
            BugWorkaround();
        }
    }

    /// <summary>
    /// Display and process the inspector's advanced feature foldout.
    /// </summary>
    /// <param name="nm">The active navigation mesh.</param>
    private void ProcessGUIAdvanced(GeneratedNavmesh nm)
    {
        EditorGUILayout.Separator();
        GNMSet.DisplayAdvancedOptions = EditorGUILayout.Foldout(
            GNMSet.DisplayAdvancedOptions
            , "Advanced");
        if (GNMSet.DisplayAdvancedOptions)
        {
            EditorGUILayout.BeginVertical();
            GNMSet.AlwaysShowMessages = 
                EditorGUILayout.Toggle(GNMSet.AlwaysShowMsgsText
                , GNMSet.AlwaysShowMessages);
            GNMSet.PerformanceMsgsEnabled = 
                EditorGUILayout.Toggle(GNMSet.EnablePerfMsgsText
                , GNMSet.PerformanceMsgsEnabled); 
            EditorGUILayout.Separator();
            if (GUILayout.Button(GNMSet.SaveNavmeshText))
                SaveNavmesh(nm);
            if (GUILayout.Button(GNMSet.ClearNavmeshText))
                nm.Reset();
            EditorGUILayout.Separator();
            if (GUILayout.Button(GNMSet.SaveCombinedMeshText))
            {
                SaveCombinedSource(nm);
                BugWorkaround();
            }
            EditorGUILayout.Separator();
            EditorGUIUtility.LookLikeControls(75);
            EditorGUILayout.LabelField("Help"
                , @"www.critterai.org");
            EditorGUIUtility.LookLikeControls(CAISet.DefaultLabelWidth);
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Attempts to derive a good configuration for the source geometry.
    /// </summary>
    /// <remarks>
    /// If no source geometry is found, peforms a standard cleaning.
    /// </remarks>
    /// <param name="nm">The active navigation mesh.</param>
    private void DeriveConfig(GeneratedNavmesh nm)
    {
        NavmeshGenerator.ApplyMandatoryLimits(nm.config);
        BoundedMesh3 sourceMesh = null;
        if (BuildCombinedSource(nm, out sourceMesh))
        {
            nm.config.Derive(sourceMesh.bounds);
        }
        NavmeshGenerator.ApplyMandatoryLimits(nm.config);
        nm.config.Clean();
        nm.config.ApplyDecimalLimits();
    }

    /// <summary>
    /// Builds the navigation mesh from the source geometry.
    /// </summary>
    /// <param name="nm">The active navigation mesh.</param>
    private void BuildMesh(GeneratedNavmesh nm)
    {
        /*
         * Design note:
         * 
         * The build code is located here rather than in the navigation 
         * mesh class because it references the native assembly.  Putting this 
         * code in the navigation mesh class would make that class unusable for 
         * web client builds.
         * 
         */

        long startTickAll = System.DateTime.Now.Ticks;

        if (nm.config.yResolution * 2.49f > nm.config.maxTraversableStep)
        {
            PostMessage(PrefixGen, WarnMarker, true
                , "The y-resolution is close to or greater than"
                + " the maximum traversable step.  Some valid steps may be"
                + " lost. For best results, the y-resolution should be <="
                + " (max traversable step / 2.5).");
        }
        if (nm.config.yResolution * 1.99f >= nm.config.minTraversableHeight)
        {
            PostMessage(PrefixGen, WarnMarker, true 
                , "The y-resolution and minimum traversable"
                + " height combination may result in a bad mesh. The"
                + " y-resolution should be <="
                + " (min traversable height / 2).");
        }

        BoundedMesh3 sourceMesh = null;

        if (!BuildCombinedSource(nm, out sourceMesh))
        {
            // There is no source geometry.
            nm.Reset();
            return;
        }

        int width = (int)((sourceMesh.bounds[3] - sourceMesh.bounds[0])
            / nm.config.xzResolution * 0.5f);
        int depth = (int)((sourceMesh.bounds[5] - sourceMesh.bounds[2])
            / nm.config.xzResolution * 0.5f);

        if (width * depth > GNMSet.ResolutionWarningThreshold)
        {
            if (!EditorUtility.DisplayDialog("Are you sure?"
                    , "Your xzResolution will result in quite a"
                        + " high resolution build. (" + width + " x "
                        + depth + " cells.) This may take a while."
                        + " Are you sure you are ready?"
                    , "Yes, build the mesh now."
                    , "No, I am not ready."))
            {
                return;
            }
        }
        else
        {
            width = (int)((sourceMesh.bounds[3] - sourceMesh.bounds[0])
                / nm.config.contourSampleDistance);
            depth = (int)((sourceMesh.bounds[5] - sourceMesh.bounds[2])
                / nm.config.contourSampleDistance);
            if (width * depth > GNMSet.SamplingWarningThreshold
                && !EditorUtility.DisplayDialog("Are you sure?"
                        , "Your contour sample distance is quite low and will"
                            + " result in " + (width * depth) 
                            + " sample points.  This may take a while."
                            + " Are you sure you are ready?"
                        , "Yes, build the mesh now."
                        , "No, I am not ready."))
            {
                return;
            }
        }

        mProgressDisplayed = true;
        EditorUtility.DisplayProgressBar(mProgressTitle
            , "Building final mesh. External call. Please wait..."
            , 1);

        float[] resultVerts;
        int[] resultIndices;
        string[] messages;

        long startTickExternal = System.DateTime.Now.Ticks;

        // Design Note: Always requesting verbose trace detail just in case
        // there is a problem.
        bool success = NavmeshGenerator.BuildMesh(nm.config
            , sourceMesh.vertices
            , sourceMesh.indices
            , out resultVerts
            , out resultIndices
            , out messages
            , CAIMessageStyle.Trace);

        PostPerf(PrefixBuild, "Navmesh Build Time: " 
            + CAIUtil.GetNowDeltaMS(startTickExternal));

        EditorUtility.ClearProgressBar();
        mProgressDisplayed = false;

        if (!success || GNMSet.AlwaysShowMessages)
        {
            foreach (string message in messages)
            {
                PostMessage(PrefixExt, message);
            }
            if (!success)
            {
                PostMessage(PrefixBuild, ErrorMarker, true
                    , "Navmesh build failed.  See previous messages"
                        + " for details.");
                return;
            }
        }
        nm.vertices = resultVerts;
        nm.triangles = resultIndices;
        nm.RebuildBounds();

        PostPerf(PrefixBuild, "Total Build Time: "
            + CAIUtil.GetNowDeltaMS(startTickAll));
    }

    /// <summary>
    /// Builds a combined mesh of vertices and triangles from all 
    /// geometry sources.
    /// </summary>
    /// <remarks>
    /// The only time the combine process should fail is if there are no
    /// meshes with triangles in the sources.
    /// </remarks>
    /// <param name="nm">The active navigation mesh.</param>
    /// <param name="combinedMesh">The combined mesh.</param>
    /// <returns>TRUE if a combined mesh was sucessfully generated.</returns>
    private bool BuildCombinedSource(GeneratedNavmesh nm
        , out BoundedMesh3 combinedMesh)
    {
        long startTotal = System.DateTime.Now.Ticks;

        // Next value is used for GUI control purposes.  Default value to
        // indicate failure.
        mSourceVertCount = 0;

        mProgressDisplayed = true;
        EditorUtility.DisplayProgressBar(mProgressTitle
            , "Combining meshes filters..."
            , 0.5f);

        float[] vertices = null;
        int[] triangles = null;
        if (!CAIMeshUtil.CombineMeshFilters(nm.sourceGeometry.GetSources()
            , out vertices, out triangles))
        {
            PostMessage(PrefixAgg, WarnMarker, true
                , "No geometry (mesh or terrain) found in sources.");
            combinedMesh = null;
            return false;
        }

        combinedMesh = new BoundedMesh3(3, vertices, triangles);

        // Update the detail data used by the GUI.
        mSourceVertCount = combinedMesh.VertexCount;
        mSourceTriangleCount = combinedMesh.PolyCount;
        mSourceBounds = combinedMesh.bounds;
       
        PostPerf(PrefixAgg, "Combine source geometry: "
            + CAIUtil.GetNowDeltaMS(startTotal));

        EditorUtility.ClearProgressBar();

        return true;
    }

    /// <summary>
    /// Saves the combined source geomertry mesh to disk. (Any location.)
    /// </summary>
    /// <remarks>
    /// <p>Will be saved in simplified Wavefront format. (Vertices and 
    /// triangles only.)</p>
    /// </remarks>
    /// <param name="nm">The active navigation mesh.</param>
    private void SaveCombinedSource(GeneratedNavmesh nm)
    {
        BoundedMesh3 mesh = null;

        if (BuildCombinedSource(nm, out mesh))
        {
            string path = EditorUtility.SaveFilePanel("Save Location"
                , "Assets"
                , nm.GetCacheID() + "-source"
                , "obj");
            if (path.Length != 0)
                // Design note: Including the machine string allows
                // the option for generating the intended navigation mesh from
                // the source externally.
                SaveWavefront(path
                    , "Combined source geometry for " + nm.GetCacheID()
                        + "\n# " + nm.config.GetMachineString()
                    , mesh.vertices
                    , mesh.indices);
        }
    }

    /// <summary>
    /// Saves the navigation mesh data to disk. (Any location.)
    /// </summary>
    /// <remarks>
    /// <p>Will be saved in simplified Wavefront format. (Vertices and 
    /// triangles only.)</p>
    /// </remarks>
    /// <param name="nm">The active navigation mesh.</param>
    private static void SaveNavmesh(GeneratedNavmesh nm)
    {
        if (nm.vertices != null && nm.triangles != null)
        {
            string path = EditorUtility.SaveFilePanel("Save Location"
                , "Assets"
                , nm.GetCacheID()
                , "obj");
            if (path.Length != 0)
                // Design note: Including the machine string permits
                // the mesh to be loaded back into a generated navigation 
                // mesh object.
                SaveWavefront(path
                    , "Navmesh for: " + nm.GetCacheID()
                        + "\n# " + nm.config.GetMachineString()
                    , nm.vertices
                    , nm.triangles);
        }
    }

    /// <summary>
    /// Saves mesh data to disk in simplified wavefront format.
    /// (Vertices and triangles only.)
    /// </summary>
    /// <param name="path">The path to save the data to.
    /// </param>
    /// <param name="vertices">The mesh vertices in the form (x, y, z).</param>
    /// <param name="triangles">The mesh triangles in the form
    /// (vertAIndex, vertBIndex, vertCIndex).</param>
    /// <param name="comment">A comment to append to the beginning of the
    /// saved file.
    /// <remarks>If the comment is multi-line, each new line must begin
    /// with a '#'.</remarks></param>
    private static void SaveWavefront(string path
        , string comment
        , float[] vertices
        , int[] triangles)
    {
        StreamWriter sw = new StreamWriter(path);
        sw.Write("# " + comment + "\n"
            + Wavefront.TranslateTo(vertices, triangles, true, true));
        sw.Close();
    }

    /// <summary>
    /// Adds a performance message to queue.
    /// </summary>
    /// <param name="prefix">
    /// The standard prefix to append to the message.
    /// </param>
    /// <param name="content">The performance related message.</param>
    private void PostPerf(string prefix, string content)
    {
        if (GNMSet.PerformanceMsgsEnabled)
            mPerformanceMessages.Add(PrefixPerf + prefix + content);
    }

    /// <summary>
    /// Adds a messages to the queue. (Used for non-critical information 
    /// messages.)
    /// </summary>
    /// <param name="prefix">
    /// The standard prefix to append to the message.
    /// </param>
    /// <param name="content">The message.</param>
    private void PostMessage(string prefix, string content)
    {
        PostMessage(prefix, "", false, content);
    }

    /// <summary>
    /// Adds a message to the queue.  (Normally used for critical messages.)
    /// </summary>
    /// <param name="prefix">
    /// The standard prefix to append to the message.
    /// </param>
    /// <param name="marker">
    /// The standard suffix marker to append to the message.
    /// </param>
    /// <param name="force">If TRUE, all messages in the queue will be forced
    /// to the console during the next inspector update.</param>
    /// <param name="content">The message.</param>
    private void PostMessage(string prefix
        , string marker
        , bool force
        , string content)
    {
        mMessages.Add(PrefixRoot + prefix + content + marker);
        mForceMessages = (mForceMessages || force);
    }

    /// <summary>
    /// Process all messages. (Including performance messages.)
    /// </summary>
    /// <remarks>
    /// Standard message behavior is followed.  (E.g. Performance messages
    /// will only be sent to the console if peformance messaging is enabled.)
    /// <p>Any messages not sent to the console will be cleared.</p>
    /// </remarks>
    private void ProcessMessages()
    {
        if (mMessages.Count > 0)
        {
            foreach (string message in mMessages)
            {
                if (message.Contains(ErrorMarker))
                {
                    Debug.LogError(message);
                }
                else if (message.Contains(WarnMarker))
                {
                    Debug.LogWarning(message);
                }
                else if (mForceMessages || GNMSet.AlwaysShowMessages)
                {
                    Debug.Log(message);
                }
            }
            mMessages.Clear();
        }
        mForceMessages = false;

        if (mPerformanceMessages.Count > 0)
        {
            if (GNMSet.PerformanceMsgsEnabled)
            {
                foreach (string message in mPerformanceMessages)
                {
                    Debug.Log(message);
                }
            }
            mPerformanceMessages.Clear();
        }
    }

    /// <summary>
    /// Run the ExitGUI bug workaround.
    /// </summary>
    /// <remarks>
    /// Place at the end of operations which are causing GUI exceptions.
    /// <p>This is known to occur when the progress bar is used within
    /// a button block.</p>
    /// </remarks>
    private static void BugWorkaround()
    {
        // This next line is a workaround to a bug. (I think.)
        // GUI exceptions will occur if it is removed.
        // http://forum.unity3d.com/threads/43565-Problem-creating-EditorWindow-from-Inspector
        GUIUtility.ExitGUI();
    }

}

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
using org.critterai.nmgen.u3d;
using Math = System.Math;

/// <summary>
/// The custom inspector for <see cref="NMGenBuildConfig"/>.
/// </summary>
[CustomEditor(typeof(NMGenBuildConfig))]
public class NMGenBuildConfigEditor
    : Editor
{
    #region Editor Control Values

    /// <summary>
    /// The default label with to use for custom editors.
    /// </summary>
    private const int DefaultLabelWidth = 175;

    /// <summary>
    /// Controls the open/closed state of the primary configuration foldout.
    /// </summary>
    public static bool displayPrimaryConfig = true;

    /// <summary>
    /// Controls the open/closed state of the secondary configuration foldout.
    /// </summary>
    public static bool displayOtherConfig = false;

    #endregion

    #region GUIContent Definitions

    /***************************************************************************
     * GUI Content Definitions
     * 
     * Change the names of things.  Change tooltips.
     **************************************************************************/

    private static GUIContent XZSizeText = new GUIContent(
        "XZ Cell Size"
        , "The xz-plane voxel size to use when sampling the source geometry.");

    private static GUIContent YSizeText = new GUIContent(
        "Y Cell Size"
        , "The y-axis voxel size to use when sampling the source geometry.");

    private const string HeightLabel = "Walkable Height";

    private static GUIContent HeightText = new GUIContent(
        HeightLabel
        , "Minimum floor to ceiling height that will still allow the"
            + " floor area to be considered traversable.");

    private const string StepLabel = "Walkable Step";

    private static GUIContent StepText = new GUIContent(
        StepLabel
        , "Maximum ledge height that is considered to still be traversable."
            + " Allows the mesh to flow over curbs and up/down stairways.");

    private static GUIContent SlopeText = new GUIContent(
        "Walkable Slope"
        , "The maximum slope that is considered traversable. (In degrees.)");

    private const string RadiusLabel = "Walkable Radius";

    private static GUIContent RadiusText = new GUIContent(
        RadiusLabel
        , "Represents the closest any part of a mesh can get to an"
            + " obstruction in the source geometry.");

    private const string EdgeLenLabel = "Max Edge Length";

    private static GUIContent EdgeLenText = new GUIContent(
        EdgeLenLabel
        , "The maximum length of polygon edges along the border of the"
            + " mesh. (Extra vertices will be inserted if needed.)");

    private static GUIContent EdgeDevText = new GUIContent(
        "Edge Max Deviation"
        , "The maximum distance the edges of the mesh may deviate from"
            + " the source geometry.");

    private static GUIContent DetailSampleText = new GUIContent(
        "Detail Sample Dist"
        , "Sets the sampling distance to use when matching the"
            + " mesh surface to the source geometry.");

    private static GUIContent DetailDevText = new GUIContent(
        "Detail Max Deviation"
        , "The maximum distance the mesh surface can deviate from the"
            + " surface of the source geometry. (Beware of setting"
            + " too low.)");

    private const string IslandRegionLabel = "Min Island Area";

    private static GUIContent IslandRegionText = new GUIContent(
        IslandRegionLabel
        , "The minimum area size allowed for isolated island meshes."
            + " (Prevents the formation of meshes that are too small to be"
            + " of use.)");

    private static GUIContent MaxPolyVertText = new GUIContent(
        "Max Vertices Per Polygon"
        , "The maximum number of vertices per polygon for polygons"
            + " generated during the polygon mesh build process.");

    private static GUIContent HFBorderText = new GUIContent(
        "AABB Border Size (vx)"
        , "The closest the mesh may come to the xz-plane AABB of the"
            + " source geometry.");

    private const string MergeSizeLabel = "Merge Region Area";

    private static GUIContent MergeSizeText = new GUIContent(
        MergeSizeLabel
        , "Any regions smaller than this area size will, if possible, be"
            + " merged with larger regions.");

    private static GUIContent LedgeSpansText = new GUIContent(
        "Ledges Not Walkable"
        , "Increases mesh distance from ledges.");

    private static GUIContent LowHeightText = new GUIContent(
        "Low Height Not Walkable"
        , "Areas with low clearance are not walkable");

    private static GUIContent LowObstacleText = new GUIContent(
        "Low Obstacles Walkable"
        , "Low lying obstacles are walkable. (Curbs, steps, etc.)");

    private static GUIContent TessAreasText = new GUIContent(
        "Tessellate Area Edges");

    private static GUIContent TessWallsText = new GUIContent(
        "Tessellate Wall Edges");

    private static GUIContent UseMonoText = new GUIContent(
        "Use Monotone Partitioning");

    private static GUIContent FlagPolysText = new GUIContent(
        "Apply Poly Flag"
        , "Applys the 0x01 flag to the final polygons.");

    #endregion

    private bool mForceDirty = false;

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        NMGenBuildConfig ctarget = (NMGenBuildConfig)target;

        EditorGUIUtility.LookLikeControls(DefaultLabelWidth);

        EditorGUILayout.Separator();
        ProcessGUIMainSec(ctarget);      // Main button section (No foldout)
        ProcessGUIPriConfig(ctarget.config);    // Primary configuration
        ProcessGUISecConfig(ctarget.config);    // Other configuration
        EditorGUILayout.Separator();

        if (GUI.changed || mForceDirty)
        {
            EditorUtility.SetDirty(target);
            mForceDirty = false;
        }
    }

    private void ApplyDecimalLimits(NMGenBuildParams config)
    {
        config.XZCellSize = (float)Math.Round(config.XZCellSize, 2);
        config.YCellSize = (float)Math.Round(config.YCellSize, 2);

        config.DetailMaxDeviation = 
            (float)Math.Round(config.DetailMaxDeviation, 2);
        config.DetailSampleDistance = 
            (float)Math.Round(config.DetailSampleDistance, 2);
        config.EdgeMaxDeviation = 
            (float)Math.Round(config.EdgeMaxDeviation, 2);

        config.MaxEdgeLength = (float)Math.Round(config.MaxEdgeLength, 2);
        config.MergeRegionArea = (float)Math.Round(config.MergeRegionArea, 2);
        config.MinRegionArea = (float)Math.Round(config.MinRegionArea, 2);
        config.WalkableHeight = (float)Math.Round(config.WalkableHeight, 2);
        config.WalkableRadius = (float)Math.Round(config.WalkableRadius, 2);
        config.WalkableSlope = (float)Math.Round(config.WalkableSlope, 2);
        config.WalkableStep = (float)Math.Round(config.WalkableStep, 2);
    }

    private void ProcessGUIMainSec(NMGenBuildConfig ctarget)
    {
        EditorGUILayout.Separator();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clean"))
        {
            ApplyDecimalLimits(ctarget.config);
            mForceDirty = true;
        }

        if (GUILayout.Button("Reset"))
        {
            ctarget.config = new NMGenBuildParams();
            mForceDirty = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private string Effective(float value)
    {
        return "  (" + Math.Round(value, 2) + ")";
    }

    private void ProcessGUIPriConfig(NMGenBuildParams config)
    {
        EditorGUILayout.Separator();
        displayPrimaryConfig = EditorGUILayout.Foldout(
            displayPrimaryConfig
            , "Primary Configuration");
        if (displayPrimaryConfig)
        {
            EditorGUILayout.BeginVertical();

            float xz = config.XZCellSize;
            float y = config.YCellSize;
            float a = xz * xz;
            float val;
            
            val = (float)Math.Ceiling(config.WalkableHeight / y) * y;
            HeightText.text = HeightLabel + Effective(val);
            config.WalkableHeight = 
                EditorGUILayout.FloatField(HeightText, config.WalkableHeight);

            val = (float)Math.Floor(config.WalkableStep / y) * y;
            StepText.text = StepLabel + Effective(val);
            config.WalkableStep = 
                EditorGUILayout.FloatField(StepText, config.WalkableStep);

            val = (float)Math.Ceiling(config.WalkableRadius / xz) * xz;
            RadiusText.text = RadiusLabel + Effective(val);
            config.WalkableRadius = 
                EditorGUILayout.FloatField(RadiusText, config.WalkableRadius);

            config.WalkableSlope = EditorGUILayout.Slider(
                SlopeText
                , config.WalkableSlope
                , 0
                , NMGen.MaxAllowedSlope);

            config.XZCellSize = 
                EditorGUILayout.FloatField(XZSizeText, config.XZCellSize);

            config.YCellSize = 
                EditorGUILayout.FloatField(YSizeText, config.YCellSize);

            val = (float)Math.Ceiling(config.MaxEdgeLength / xz) * xz;
            EdgeLenText.text = EdgeLenLabel + Effective(val);
            config.MaxEdgeLength = 
                EditorGUILayout.FloatField(EdgeLenText, config.MaxEdgeLength);

            config.EdgeMaxDeviation = EditorGUILayout.FloatField(
                EdgeDevText
                , config.EdgeMaxDeviation);

            config.DetailSampleDistance = EditorGUILayout.FloatField(
                DetailSampleText
                , config.DetailSampleDistance);

            config.DetailMaxDeviation = EditorGUILayout.FloatField(
                DetailDevText
                , config.DetailMaxDeviation);

            val = (float)Math.Ceiling(config.MinRegionArea / a) * a;
            IslandRegionText.text = IslandRegionLabel + Effective(val);
            config.MinRegionArea = 
                EditorGUILayout.FloatField(IslandRegionText, config.MinRegionArea);

            EditorGUILayout.EndVertical();
        }
    }

    private void ProcessGUISecConfig(NMGenBuildParams config)
    {
        EditorGUILayout.Separator();

        displayOtherConfig = EditorGUILayout.Foldout(
            displayOtherConfig
            , "Other Configuration");

        if (displayOtherConfig)
        {
            EditorGUILayout.BeginVertical();

            config.MaxVertsPerPoly = EditorGUILayout.IntSlider(
                MaxPolyVertText
                , config.MaxVertsPerPoly
                , 3
                , NMGen.MaxAllowedVertsPerPoly);

            config.BorderSize = EditorGUILayout.IntField(
                HFBorderText
                , config.BorderSize);

            float a = config.XZCellSize * config.XZCellSize;
            float val = (float)Math.Ceiling(config.MergeRegionArea / a) * a;
            MergeSizeText.text = MergeSizeLabel + Effective(val);
            config.MergeRegionArea = EditorGUILayout.FloatField(MergeSizeText
                , config.MergeRegionArea);

            EditorGUILayout.Separator();

            bool ledgeSpans  = (config.BuildFlags 
                & BuildFlags.LedgeSpansNotWalkable) != 0;
            bool lowHeight = (config.BuildFlags
                & BuildFlags.LowHeightSpansNotWalkable) != 0;
            bool lowObstacle = (config.BuildFlags
                & BuildFlags.LowObstaclesWalkable) != 0;
            bool tessWalls = (config.BuildFlags
                & BuildFlags.TessellateWallEdges) != 0;
            bool tessAreas = (config.BuildFlags
                & BuildFlags.TessellateAreaEdges) != 0;
            bool useMono = (config.BuildFlags
                & BuildFlags.UseMonotonePartitioning) != 0;
            bool flagPolys = (config.BuildFlags
                & BuildFlags.ApplyPolyFlags) != 0;
            
            ledgeSpans = EditorGUILayout.Toggle(LedgeSpansText, ledgeSpans);
            lowHeight = EditorGUILayout.Toggle(LowHeightText, lowHeight);
            lowObstacle = EditorGUILayout.Toggle(LowObstacleText, lowObstacle);
            tessWalls = EditorGUILayout.Toggle(TessWallsText, tessWalls);
            tessAreas = EditorGUILayout.Toggle(TessAreasText, tessAreas);
            useMono = EditorGUILayout.Toggle(UseMonoText, useMono);
            flagPolys = EditorGUILayout.Toggle(FlagPolysText, flagPolys);

            config.BuildFlags =
                (ledgeSpans ? BuildFlags.LedgeSpansNotWalkable : 0)
                | (lowHeight ? BuildFlags.LowHeightSpansNotWalkable : 0)
                | (lowObstacle ? BuildFlags.LowObstaclesWalkable : 0)
                | (tessWalls ? BuildFlags.TessellateWallEdges : 0)
                | (tessAreas ? BuildFlags.TessellateAreaEdges : 0)
                | (useMono ? BuildFlags.UseMonotonePartitioning : 0)
                | (flagPolys ? BuildFlags.ApplyPolyFlags : 0);

            EditorGUILayout.EndVertical();
        }
    }


}

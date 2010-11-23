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

/// <summary>
/// Holds configuration settings and state that are shared between all 
/// instances of <see cref="GeneratedNavmeshEditor"/>.  (Unity Pro Only)
/// </summary>
/// <remarks>
/// <p>This class can be edited to alter the text/tooltips and behavior of the
/// <see cref="GeneratedNavmeshEditor"/> custom inspector.</p>
/// <p>To change the inspector defaults, except for the configuration settings,
/// search the code for "Editor control values".</p>
/// <p>If you want to change label text or tool tips, search the code for
/// "GUI Content Definitions".</p>
/// </remarks>
public static class GNMSet
{
    /*
     * Design notes:
     * 
     * These settings are not included in the main class so that non-coders can
     * more easily edit them without worrying about messing up the main code.
     * 
     * Basically, ease of editing the values was chosen over cleanliness in the
     * main code.
     * 
     * In general, don't add help text to buttons.  It tends to get in the way.
     * 
     * At some point some of the functionality of this class may be moved to
     * a configuration file
     * 
     */

    /***************************************************************************
     * Editor control values
     * 
     * Change the initialization values to change the default layout at
     * session startup.
     **************************************************************************/

    // Changing these values will change the default messaging behavior.

    /// <summary>
    /// If TRUE, all messages will be sent to the console, whether there
    /// was an error or not.
    /// </summary>
    public static bool AlwaysShowMessages = false;

    /// <summary>
    /// If TRUE, performance related message will be sent to the console.
    /// </summary>
    public static bool PerformanceMsgsEnabled = false;


    /// <summary>
    /// Controls the open/closed state of the statistics foldout.
    /// </summary>
    public static bool DisplayStats = false;

    /// <summary>
    /// Controls the open/closed state of the primary configuration foldout.
    /// </summary>
    public static bool DisplayPriConfig = true;

    /// <summary>
    /// Controls the open/closed state of the secondary configuration foldout.
    /// </summary>
    public static bool DisplayOtherConfig = false;

    /// <summary>
    /// Controls the open/closed state of the advanced options foldout.
    /// </summary>
    public static bool DisplayAdvancedOptions = false;

    /***************************************************************************
     * Miscellaneous Constants
     **************************************************************************/

    // If you are getting unnecessary warning dialogs about potentailly long
    // mesh build times, then you can raise these values.  If you aren't
    // getting enough warnings, then you can lower them.

    /// <summary>
    /// Constrols the point at which the cell size warning dialog is
    /// displayed.
    /// </summary>
    /// <remarks>
    /// <p>Raise this value if getting too many warnings.  Lower if getting
    /// too few</p>
    /// <p>The value is the number of cells which will be needed
    /// during the generation.</p>
    /// </remarks>
    public static int CellSizeWarningThreshold = 1500000;

    /// <summary>
    /// Constrols the point at which the contour sampling warning dialog is
    /// displayed.
    /// </summary>
    /// <remarks>
    /// <p>Raise this value if getting too many warnings.  Lower if getting
    /// too few</p>
    /// <p>The value is the number of contour sample points which will be
    /// needed during the generation.</p>
    /// </remarks>
    public static int SamplingWarningThreshold = 750000;

    #region GUIContent Definitions

    /***************************************************************************
     * GUI Content Definitions
     * 
     * Change the names of things.  Change tooltips.
     **************************************************************************/

    // Definitions for primary configuration foldout.

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.xzCellSize"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent XZCellSizeText = new GUIContent(
        "XZ Cell Size"
        , "The xz-plane voxel size to use when sampling the source geometry.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.yCellSize"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent YCellSizeText = new GUIContent(
        "Y Cell Size"
        , "The y-axis voxel size to use when sampling the source geometry.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.minTraversableHeight"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent MinHeightText = new GUIContent(
        "Maximum Client Height"
        , "Minimum floor to ceiling height that will still allow the"
            + " floor area to be considered traversable.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.maxTraversableStep"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent MaxStepText = new GUIContent(
        "Maximum Client Step"
        , "Maximum ledge height that is considered to still be traversable."
            + " Allows the mesh to flow over curbs and up/down stairways.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.maxTraversableSlope"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent MaxSlopeText = new GUIContent(
        "Max Traversable Slope"
        , "The maximum slope that is considered traversable. (In degrees.)");

    /// <summary>
    /// The GUIContent for the 
    /// <see cref="NavmeshConfig.traversableAreaBorderSize"/> configuration 
    /// setting.
    /// </summary>
    public static GUIContent BorderSizeText = new GUIContent(
        "Client Radius"
        , "Represents the closest any part of a mesh can get to an"
            + " obstruction in the source geometry.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.maxEdgeLength"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent MaxEdgeText = new GUIContent(
        "Max Edge Length"
            , "The maximum length of polygon edges along the border of the"
                + " mesh. (Extra vertices will be inserted if needed.)");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.edgeMaxDeviation"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent EdgeDeviationText = new GUIContent(
        "Edge Max Deviation"
        , "The maximum distance the edges of the mesh may deviate from"
            + " the source geometry.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.contourSampleDistance"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent ContourSampleText = new GUIContent(
        "Contour Sample Dist"
        , "Sets the sampling distance to use when matching the"
            + " mesh surface to the source geometry.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.contourMaxDeviation"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent ContourDevText = new GUIContent(
        "Contour Max Deviation"
        , "The maximum distance the mesh surface can deviate from the"
            + " surface of the source geometry. (Beware of setting"
            + " too low.)");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.minIslandRegionSize"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent IslandRegionText = new GUIContent(
        "Min Island Mesh Size"
        , "The minimum area size allowed for isolated island meshes."
            + " (Prevents the formation of meshes that are too small to be"
            + " of use.)");

    // Definitions for the secondary configuration foldout.

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.maxVertsPerPoly"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent MaxPolyVertText = new GUIContent(
        "Max Vertices Per Polygon"
        , "The maximum number of vertices per polygon for polygons"
            + " generated during the contour to polygon conversion process.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.smoothingThreshold"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent SmoothingText = new GUIContent(
        "Smoothing Threshold"
        , "The amount of smoothing to be performed when generating the"
            + " distance field used for deriving regions.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.heightfieldBorderSize"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent HFBorderText = new GUIContent(
        "AABB Border Size"
        , "The closest the mesh may come to the xz-plane AABB of the"
            + " source geometry.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.mergeRegionSize"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent MergeSizeText = new GUIContent(
        "Merge Region Size"
        , "Any regions smaller than this area size will, if possible, be"
            + " merged with larger regions.");

    /// <summary>
    /// The GUIContent for the <see cref="NavmeshConfig.clipLedges"/>
    /// configuration setting.
    /// </summary>
    public static GUIContent ClipLedgesText = new GUIContent(
        "Clip Ledges"
        , "Indicates whether ledges should be considered un-walkable.");

    // Definitions for main button area.

    /// <summary>
    /// The GUIContent for the clean configuration button.
    /// </summary>
    public static GUIContent CleanConfigText = new GUIContent(
    "Clean Config"
    , "");

    /// <summary>
    /// The GUIContent for the reset configuration button.
    /// </summary>
    public static GUIContent ResetConfigText = new GUIContent(
        "Reset Config"
        , "");

    /// <summary>
    /// The GUIContent for the derive configuration button.
    /// </summary>
    public static GUIContent DeriveConfigText = new GUIContent(
        "Derive Config"
        , "");

    /// <summary>
    /// The GUIContent for the build navigation mesh button.
    /// </summary>
    public static GUIContent BuildMeshText = new GUIContent(
        "Build Mesh");

    // Definitions for advanced options foldout.

    /// <summary>
    /// The GUIContent for the always show message toggle.
    /// </summary>
    public static GUIContent AlwaysShowMsgsText = new GUIContent(
        "Show Trace Messages"
        , "");

    /// <summary>
    /// The GUIContent for the enabled performance messsages toggle.
    /// </summary>
    public static GUIContent EnablePerfMsgsText = new GUIContent(
        "Performance Messages"
        , "");

    /// <summary>
    /// The GUIContent for the save navigation mesh button.
    /// </summary>
    public static GUIContent SaveNavmeshText = new GUIContent(
        "Save Navmesh"
        , "");

    /// <summary>
    /// The GUIContent for the clear navigation mesh button.
    /// </summary>
    public static GUIContent ClearNavmeshText = new GUIContent(
        "Clear Navmesh");

    /// <summary>
    /// The GUIContent for the save combined source geometry button.
    /// </summary>
    public static GUIContent SaveCombinedMeshText = new GUIContent(
        "Save Combined Geometry"
        , "");

    #endregion
}

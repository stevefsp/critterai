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
using System.Collections.Generic;
using org.critterai.nav;
using org.critterai.nav.u3d;
using org.critterai.nmbuild;
using org.critterai.nmbuild.u3d.editor;
using org.critterai.nmgen;
using UnityEngine;

/// <summary>
/// A navigation mesh build. (Editor Only)
/// </summary>
/// <remarks>
/// <para>
/// This class is only meant for use with Unity editors.  Its public interface is very limited.
/// </para>
/// </remarks>
[System.Serializable]
public sealed class NavmeshBuild
    : ScriptableObject
{
    /*
     * Design notes: 
     * 
     * Unity REEEALLY hates null serialized fields for non-unity objects.  
     * I've tried all sorts of workarounds for the fields that are supposed to 
     * be null when not in use. But Unity keeps un-nulling the fields at unexpected 
     * times.  In most cases I just don't allow fields to be unset.  For the build
     * data, it has a 'valid' check build into it.
     * 
     * It is important to the design that the input data only be cleared by
     * an uncontroled editor reset or a manual reset of the build.
     * The recovery state is detected by the input data going missing while
     * there is build data.
     */

    /// <summary>
    /// The minimum supported tile size for tiled navigation meshes.
    /// </summary>
    public const int MinAllowedTileSize = 12;

    #region Serialized Fields

    [SerializeField]
    internal List<ScriptableObject> inputProcessors = new List<ScriptableObject>();

    [SerializeField]
    private NavmeshBuildType mBuildType = NavmeshBuildType.Standard;

    [SerializeField]
    private ScriptableObject mSceneQuery;

    [SerializeField]
    private ScriptableObject mTarget;

    [SerializeField]
    private NMGenConfig mConfig = new NMGenConfig();

    [SerializeField]
    private Vector3 mBoundsMin;

    [SerializeField]
    private Vector3 mBoundsMax;

    [SerializeField]
    private TileBuildData mBuildData;

    [SerializeField]
    private bool mCleanInputGeometry = true;

    [SerializeField]
    private bool mIsDirty = false;

    #endregion

    #region Non-Serialized Fields

    // This information is lost during a Unity editor reset.

    private InputGeometry mInputGeom;
    private InputBuildInfo mInputInfo;
    private ProcessorSet mNMGenProcessors;
    private ConnectionSet mConnections;  // Off-mesh.
    private TileSetDefinition mTileSet;

    #endregion

    /// <summary>
    /// The state of the build.
    /// </summary>
    public NavmeshBuildState BuildState
    {
        get
        {
            if (mTarget == null || inputProcessors.Count == 0)
                return NavmeshBuildState.Invalid;

            if (mNMGenProcessors == null)
            {
                if (mBuildData.IsValid)
                    return NavmeshBuildState.NeedsRecovery;
                else
                    return NavmeshBuildState.Inactive;
            }

            if (mBuildData.IsValid)
                return NavmeshBuildState.Buildable;
            else
                return NavmeshBuildState.InputCompiled;
        }
    }

    internal bool AutoCleanGeometry
    {
        get { return mCleanInputGeometry; }
        set { mCleanInputGeometry = value; }
    }

    internal NavmeshBuildType BuildType
    {
        get { return mBuildType; }
        set
        {
            if (value != mBuildType)
            {
                ResetBuild();

                mBuildType = value;

                if (mBuildType == NavmeshBuildType.Advanced)
                    BuildSelector.Instance.Add(this);
                else
                    BuildSelector.Instance.Remove(this);

                mIsDirty = true;
            }
        }
    }

    internal bool IsDirty 
    { 
        get { return mIsDirty || mBuildData.IsDirty; }
        set
        {
            mIsDirty = value;
            mBuildData.IsDirty = value;
        }
    }

    internal bool HasInputData
    {
        get { return (mNMGenProcessors != null); }
    }

    internal IInputBuildProcessor[] GetInputProcessors()
    {
        IInputBuildProcessor[] result = new IInputBuildProcessor[inputProcessors.Count];

        // Assumption: The editor is properly controlling additions to the list.
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = (IInputBuildProcessor)inputProcessors[i];
        }

        return result;
    }

    internal InputGeometry InputGeom { get { return mInputGeom; } }

    internal ProcessorSet NMGenProcessors { get { return mNMGenProcessors; } }

    internal ConnectionSet Connections { get { return mConnections; } }

    internal InputBuildInfo InputInfo { get { return mInputInfo; } }

    internal bool TargetHasNavmesh
    {
        get { return (mTarget == null) ? false : BuildTarget.HasNavmesh; }
    }

    internal ISceneQuery SceneQuery
    {
        get { return (ISceneQuery)mSceneQuery; }
        set
        {
            if (value == null || value is ScriptableObject)
            {
                ScriptableObject so = (ScriptableObject)value;
                if (mSceneQuery != so)
                {
                    mSceneQuery = so;
                    mIsDirty = true;
                }
            }
        }
    }

    internal INavmeshData BuildTarget
    {
        get { return (INavmeshData)mTarget; }
        set
        {
            if (value is ScriptableObject || value == null)
            {
                ScriptableObject so = (ScriptableObject)value;
                if (mTarget != so)
                {
                    mTarget = so;
                    mIsDirty = true;
                }
            }
        }
    }

    internal NMGenConfig Config { get { return mConfig; } }

    internal bool HasBuildData { get { return mBuildData.IsValid; } }

    internal TileBuildData BuildData 
    { 
        get { return (mBuildData.IsValid ? mBuildData : null); } 
    }

    internal TileSetDefinition TileSetDefinition { get { return mTileSet; } }

    internal bool ContainsProcessor<T>() where T : IInputBuildProcessor
    {
        foreach (IInputBuildProcessor processor in inputProcessors)
        {
            if (processor is T)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Resets the build to the <see cref="NavmeshBuildState.Inactive"/> state, clearing all 
    /// build data.
    /// </summary>
    public void ResetBuild()
    {
        mBuildData = new TileBuildData();
        mTileSet = null;

        mInputGeom = null;
        mInputInfo = new InputBuildInfo();
        mNMGenProcessors = null;
        mConnections = null;

        mBoundsMin = Vector3.zero;
        mBoundsMax = Vector3.zero;

        mIsDirty = true;
    }

    internal bool SetConfigFromTarget(BuildContext logger)
    {
        if (mBuildData.IsValid)
            // Can't do it while in buildable state.
            return false;

        if (!CanLoadFromTarget(false, logger))
            return false;

        Navmesh navmesh = BuildTarget.GetNavmesh();

        if (navmesh == null)
        {
            logger.LogError("Build target does not have an existing navigation mesh. (It lied.)"
                    , this);
            return false;
        }

        SetConfigFromTargetIntern(navmesh);

        return true;
    }

    private void SetConfigFromTargetIntern(Navmesh navmesh)
    {
        NavmeshBuildInfo targetConfig = BuildTarget.BuildInfo;
        NMGenParams currConfig = mConfig.GetConfig();

        // Note: Must ensure exact match with original configuration.
        // So this process is using the fields and trusting the 
        // original configuration to have valid values.

        currConfig.tileSize = targetConfig.tileSize;
        currConfig.walkableHeight = targetConfig.walkableHeight;
        currConfig.walkableRadius = targetConfig.walkableRadius;
        currConfig.walkableStep = targetConfig.walkableStep;
        currConfig.xzCellSize = targetConfig.xzCellSize;
        currConfig.yCellSize = targetConfig.yCellSize;
        currConfig.borderSize = targetConfig.borderSize;

        mBoundsMin = navmesh.GetConfig().origin;

        int maxTiles = navmesh.GetMaxTiles();

        // Make sure the maximum bounds fits the target mesh.
        // Note: Will not shrink the existing max bounds.
        for (int i = 0; i < maxTiles; i++)
        {
            NavmeshTile tile = navmesh.GetTile(i);

            if (tile == null)
                continue;

            NavmeshTileHeader tileHeader = tile.GetHeader();

            if (tileHeader.polyCount == 0)
                continue;

            mBoundsMax = Vector3.Max(mBoundsMax, tileHeader.boundsMax);
        }

        mConfig.SetConfig(currConfig);

        mIsDirty = true;
    }

    internal bool SetInputData(InputGeometry geometry
        , InputBuildInfo info
        , INMGenProcessor[] processors
        , ConnectionSet connections
        , bool threadSafeOnly
        , BuildContext mLogger)
    {
        // Remember: Never allow this method to clear the input geometry.
        // Note: Don't set class fields until it is safe.

        if (geometry == null)
        {
            mLogger.LogError("Set input data: Invalid parameters.", this);
            return false;
        }

        // Generate the processor set.

        processors = org.critterai.ArrayUtil.Compress(processors);

        List<INMGenProcessor> lprocessors = new List<INMGenProcessor>();
        if (processors != null)
            lprocessors.AddRange(processors);

        NMGenBuildFlag bflags = mConfig.BuildFlags;
        bool threadCheckFail = false;

        // This section makes sure we don't get duplicate processors.
        // It also checks for thread-safety.
        foreach (INMGenProcessor p in lprocessors)
        {
            if (p is FilterLedgeSpans)
                bflags &= ~NMGenBuildFlag.LedgeSpansNotWalkable;

            if (p is FilterLowHeightSpans)
                bflags &= ~NMGenBuildFlag.LowHeightSpansNotWalkable;

            if (p is LowObstaclesWalkable)
                bflags &= ~NMGenBuildFlag.LowObstaclesWalkable;

            if (threadSafeOnly && !p.IsThreadSafe)
            {
                mLogger.LogError(p.Name + " processor is not thread-safe.", this);
                threadCheckFail = true;
            }
        }

        if (threadCheckFail)
        {
            mLogger.LogError("One or more processors is not thread-safe.", this);
            return false;
        }

        lprocessors.AddRange(ProcessorSet.GetStandard(bflags));

        ProcessorSet pset = ProcessorSet.Create(lprocessors.ToArray());

        if (pset == null)
        {
            mLogger.LogError("Set input data: No NMGen processors available.", this);
            return false;
        }

        // If necessary, re-create the tile set definition.

        Vector3 bmin;
        Vector3 bmax;

        DeriveBounds(geometry.BoundsMin, geometry.BoundsMax, out bmin, out bmax);
         
        if (mBuildData.IsTiled)
        {

            mTileSet = TileSetDefinition.Create(bmin, bmax, mConfig.GetConfig(), geometry);

            if (mTileSet == null)
            {
                mLogger.LogError("Set input data: Create tile build definition: Unexpected error."
                    + " Invalid input data or configuration."
                    , this);
                return false;
            }
        }

        // Everything is OK.  Set the rest of the fields.

        mNMGenProcessors = pset;
        mBoundsMin = bmin;
        mBoundsMax = bmax;
        mInputGeom = geometry;
        mInputInfo = info;
        mConnections = (connections == null ? ConnectionSet.CreateEmpty() : connections);

        mIsDirty = true;

        return true;
    }

    private void DeriveBounds(Vector3 bminIn, Vector3 bmaxIn, out Vector3 bmin, out Vector3 bmax)
    {
        if (mBuildData.IsValid && mBuildData.IsTiled)
        {
            // Note: Can't change the origin of existing tile data.
            bmin = mBoundsMin;

            // Extend the maximum bounds if needed.
            bmax = Vector3.Max(mBoundsMax, bmaxIn);
        }
        else
        {
            bmin = bminIn;
            bmax = bmaxIn;
        }
    }

    internal bool CanLoadFromTarget(bool fullCheck, BuildContext logger)
    {
        INavmeshData target = BuildTarget;

        if (target == null || !target.HasNavmesh)
        {
            if (logger != null)
                logger.LogError("Build target does not have an existing navigation mesh.", this);
            return false;
        }

        NavmeshBuildInfo targetConfig = target.BuildInfo;

        // Note: The tile size is checked since the original builder 
        // may have supported a tile size not supported by the the standard build.
        if (targetConfig == null
            || targetConfig.tileSize >= 0 && targetConfig.tileSize < MinAllowedTileSize)
        {
            if (logger != null)
                logger.LogError("Unavailable or unsupported build target configuration.", this);
            return false;
        }

        if (!fullCheck)
            return true;

        Navmesh nm = target.GetNavmesh();

        if (nm == null)
        {
            if (logger != null)
            {
                logger.LogError(
                    "Build target does not have an existing navigation mesh. (It lied.)", this);
            }
            return false;
        }

        NavmeshParams nmConfig = nm.GetConfig();

        if (nmConfig.maxTiles < 2)
        {
            if (logger != null)
                logger.LogError("Target navigation mesh is not tiled.", this);
            return false;
        }

        int tileCount = 0;
        for (int i = 0; i < nmConfig.maxTiles; i++)
        {
            NavmeshTile tile = nm.GetTile(i);

            if (tile == null)
                continue;

            NavmeshTileHeader header = tile.GetHeader();

            if (header.polyCount == 0)
                continue;

            tileCount++;

            if (header.layer > 0)
            {
                if (logger != null)
                {
                    logger.LogError(
                        "Target navigation mesh contains layered tiles. (Not supported.)", this);
                }
                return false;
            }
        }

        if (tileCount < 2)
        {
            if (logger != null)
            {
                logger.LogError(
                    "Target navigation mesh is either not tiled or has no tiles loaded.", this);
            }
            return false;
        }

        return true;
    }

    internal void DiscardBuildData()
    {
        mBuildData.Resize(0, 0);
        mTileSet = null;
    }

    internal bool InitializeBuild(bool fromTarget, BuildContext mLogger)
    {
        Navmesh navmesh = null;

        if (fromTarget)
        {
            if (!CanLoadFromTarget(true, mLogger))
                return false;

            navmesh = BuildTarget.GetNavmesh();

            SetConfigFromTargetIntern(navmesh);
        }

        mIsDirty = true;

        // Note: If loading from the target, the tile size was already validated.
        // So it won't trigger this adjustment.
        if (mConfig.TileSize != 0 
            && mConfig.TileSize < MinAllowedTileSize)
        {
            string msg = string.Format("Tile size too small. Reverting tile size from"
                + " {0} to 0 (non-tiled). Minimum tile size is {1}"
                , mConfig.TileSize, MinAllowedTileSize);

            mLogger.LogWarning(msg, this);

            mConfig.TileSize = 0;
        }

        if (mConfig.TileSize == 0)
            mBuildData.Resize(1, 1);
        else
        {
            // Need to check to see if the the build is truly tiled.

            int w;
            int d = 0;

            if (navmesh == null)
            {
                NMGen.DeriveSizeOfTileGrid(mBoundsMin, mBoundsMax
                    , mConfig.XZCellSize, mConfig.TileSize
                    , out w, out d);
            }
            else
                // Existing navmesh will always be tiled.
                w = 2;

            if (w > 1 || d > 1)
            {
                mTileSet = TileSetDefinition.Create(mBoundsMin, mBoundsMax
                    , mConfig.GetConfig()
                    , mInputGeom);

                if (mTileSet == null)
                {
                    mLogger.LogError("Create tile build definition: Unexpected error."
                        + " Invalid input data or configuration."
                        , this);

                    return false;
                }

                mBuildData.Resize(mTileSet.Width, mTileSet.Depth);
            }
            else
                // Not really tiled.
                mBuildData.Resize(1, 1);
        }

        if (navmesh != null)
        {
            // Need to load the tiles from existing navmesh.

            NavmeshTileExtract[] tiles;
            NavmeshParams meshConfig;

            NavStatus status = Navmesh.ExtractTileData(navmesh.GetSerializedMesh()
                , out tiles
                , out meshConfig);

            if ((status & NavStatus.Failure) != 0)
            {
                mLogger.LogError("Could not extract the tile data from the target's"
                    + " navigation mesh. (Can't initialize from build target.)"
                    , this);
                return false;
            }

            foreach (NavmeshTileExtract tile in tiles)
            {
                int polyCount = tile.header.polyCount;

                if (polyCount == 0)
                    continue;

                int tx = tile.header.tileX;
                int tz = tile.header.tileZ;

                if (tx >= mBuildData.Width || tz >= mBuildData.Depth)
                {
                    // Shouldn't happen.  Probably indicates in internal error
                    // of some type.

                    string msg = string.Format("The existing navigation mesh"
                        + " contains a tile outside the expected range. Ignoring"
                        + " the tile. (Source: {0}) (Tile: [{1},{2}])"
                        , mTarget.name, tx, tz);

                    mLogger.LogWarning(msg, this);

                    continue;
                }

                mBuildData.SetAsBaked(tx, tz, tile.data, polyCount);
            }
        }

        // Note: Dirty state was set earlier in the code.

        return true;
    }

    void OnEnable()
    {
        if (mBuildData == null || !mBuildData.IsValid)
            // Don't trust Unity de-serialization of null objects.
            mBuildData = new TileBuildData();

        CleanBuild();

        if (mBuildType == NavmeshBuildType.Advanced)
            BuildSelector.Instance.Add(this);
    }

    void OnDisable()
    {
        BuildSelector.Instance.Remove(this);
    }

    internal void CleanBuild()
    {
        // OnDebugRender = null;

        if (BuildState == NavmeshBuildState.Invalid)
        {
            // Possible naughty user.  Silent reset.
            ResetBuild();

        }
        else if (mBuildData.IsValid)
        {
            if (mBuildData.BakeableCount() == 0)
                // Nothing to bake.  Just do a silent reset.
                ResetBuild();
        }
    }
}

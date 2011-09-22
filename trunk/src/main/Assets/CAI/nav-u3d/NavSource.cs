/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using org.critterai;
using org.critterai.nav;
using org.critterai.nav.u3d;

/// <summary>
/// A centralized component used to configure and share common navigation
/// resources.
/// </summary>
/// <remarks>
/// <para> Any request for the navigation objects will result in an
/// immediate initialization.  Otherwise the navigation objects are 
/// automaticaly initialized during the <c>Awake()</c> operation.</para>
/// <para>
/// The configuration is essentially immutable after initialization.
/// E.g. Changing the navigation mesh source, the maximum query nodes, etc.
/// has no effect.  Only misuse, such as manually disposing a resource,
/// will trigger a reinitialization.
/// </para>
/// </remarks>
[System.Serializable]
[AddComponentMenu("CAI/Navigation Source")]
public class NavSource 
    : MonoBehaviour 
{
    /// <summary>
    /// The navigation mesh to be used by the query objects and
    /// <see cref="CrowdManager"/>.
    /// </summary>
    public BakedNavmesh navmeshSource = null;

    /// <summary>
    /// The avoidance configuration to be used by the 
    /// <see cref="CrowdManager"/>.
    /// </summary>
    /// <remarks>
    /// <para>Required if <see cref="enableCrowdManager"/> is TRUE.</para>
    /// </remarks>
    public AvoidanceConfigSet avoidanceSource = null;

    /// <summary>
    /// The maximum search nodes to use for the query object.
    /// </summary>
    /// <remarks><para>Does not apply to the <see cref="CrowdManager"/>.
    /// </para></remarks>
    public int maxQueryNodes = 2048;

    /// <summary>
    /// TRUE if the <see cref="CrowdManager"/> should be created.
    /// </summary>
    public bool enableCrowdManager = true;

    /// <summary>
    /// The maximum number agents the <see cref="CrowdManager"/> will support.
    /// </summary>
    public int maxCrowdAgents = 10;

    /// <summary>
    /// The maximum agent radius the <see cref="CrowdManager"/> will support.
    /// </summary>
    public float maxAgentRadius = 0.5f;

    /// <summary>
    /// The initial value of the search extents.
    /// </summary>
    /// <remarks>
    /// <para>This field is ignored if <see cref="enableCrowdManager"/> is TRUE. 
    /// In that case the <see cref="CrowdManager"/> extents will be used.</para>
    /// </remarks>
    public Vector3 initialExtents = new Vector3(1, 1, 1);

    [System.NonSerialized]
    private NavGroup mNavGroup = new NavGroup();

    /// <summary>
    /// TRUE if the the manager's assets have been created and are ready for
    /// use.
    /// </summary>
    /// <remarks>
    /// <para>This property doen't only indicate if the resources have been
    /// initialized.  It also indicates if any have been manually disposed.
    /// </para>
    /// </remarks>
    public bool IsActive
    {
        get
        {
            return (mNavGroup.query != null
                && !mNavGroup.query.IsDisposed
                && !mNavGroup.mesh.IsDisposed
                && (mNavGroup.crowd == null 
                    || (!mNavGroup.crowd.IsDisposed)));
        }
    }

    /// <summary>
    /// The navigation resources provided by the source.
    /// </summary>
    /// <remarks>
    /// <para>The resources will be initialized if they are not already
    /// available.</para>
    /// </remarks>
    public NavGroup NavGroup
    {
        get
        {
            if (!IsActive)
                InitializeOnce();
            return mNavGroup;
        }
    }

    /// <summary>
    /// Creates all navigation objects. (Don't call if already active!)
    /// </summary>
    private bool InitializeOnce()
    {
        if (navmeshSource == null || !navmeshSource.HasNavmesh)
        {
            Debug.LogError(name 
                + ": Aborted initialization. Souce has no navmesh.");
            return false;
        }

        if (enableCrowdManager && avoidanceSource == null)
        {
            Debug.LogError(name + ": Aborted initialization."
                + " Avoidance configuration is not available.");
            return false;
        }

        Navmesh mNavmeshRoot = navmeshSource.GetNavmesh();
        NavmeshQuery mQueryRoot;
        NavStatus status
            = NavmeshQuery.Build(mNavmeshRoot, maxQueryNodes, out mQueryRoot);
        if (NavUtil.Failed(status))
        {
            mNavmeshRoot = null;
            Debug.LogError(name 
                + ": Aborted initialization. Failed query creation: "
                + status.ToString());
            return false;
        }

        U3DNavmeshQuery mQuery = new U3DNavmeshQuery(mQueryRoot);
        CrowdManager mCrowd = null;

        float[] mDefaultExtents;
        NavmeshQueryFilter mDefaultFilter;

        if (enableCrowdManager)
        {
            mCrowd =
                new CrowdManager(maxCrowdAgents, maxAgentRadius, mNavmeshRoot);
            mDefaultExtents = mCrowd.GetQueryExtents();
            mDefaultFilter = mCrowd.QueryFilter;
            int count = Mathf.Min(CrowdManager.MaxAvoidanceParams
                , AvoidanceConfigSet.MaxCount);
            for (int i = 0; i < count; i++)
            {
                mCrowd.SetAvoidanceConfig(i, avoidanceSource[i]);
            }
        }
        else
        {
            mCrowd = null;
            mDefaultExtents =
                Vector3Util.GetVector(initialExtents, new float[3]);
            mDefaultFilter = new NavmeshQueryFilter();
        }

        mNavGroup = new NavGroup(mNavmeshRoot
            , mQuery
            , mCrowd
            , mDefaultFilter
            , mDefaultExtents, false);

        return true;
    }

	void Awake() 
    {
        if (IsActive)
            return;
        InitializeOnce();
	}
}

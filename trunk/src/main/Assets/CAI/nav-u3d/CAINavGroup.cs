//*
// * Copyright (c) 2012 Stephen A. Pratt
// * 
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the "Software"), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// * 
// * The above copyright notice and this permission notice shall be included in
// * all copies or substantial portions of the Software.
// * 
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// * THE SOFTWARE.
// */
//using UnityEngine;
//using org.critterai.nav;
//using org.critterai.nav.u3d;

//// TODO: EVAL: Staged for v0.5

///// <summary>
///// A component used to provide a <see cref="NavGroup"/> object on initialialization.
///// </summary>
///// <remarks>
///// <para>
///// The expected use case is that this object is used to provide  a <see cref="NavGroup"/> 
///// to a central navigation component, then it gets disposed.
///// </para>
///// <para
///// >Changing the object fields will have no effect after the object has been initialized. 
///// The first call to <see cref="GetNavGroup"/> will result in initialization.
///// </para>
///// </remarks>
//[System.Serializable]
//[ExecuteInEditMode]
//public class CAINavGroup 
//    : MonoBehaviour 
//{
//    /// <summary>
//    /// The navigation mesh to be used by the query objects and <see cref="CrowdManager"/>.
//    /// </summary>
//    [SerializeField]
//    private ScriptableObject mNavmeshData = null;

//    /// <summary>
//    /// The avoidance configuration to be used by the <see cref="CrowdManager"/>.
//    /// </summary>
//    /// <remarks>
//    /// <para>
//    /// Required if <see cref="enableCrowdManager"/> is true.
//    /// </para>
//    /// </remarks>
//    public CAICrowdAvoidanceSet avoidanceData = null;

//    /// <summary>
//    /// The maximum search nodes to use for the query object.
//    /// </summary>
//    /// <remarks>
//    /// <para>
//    /// Does not apply to the <see cref="CrowdManager"/>.
//    /// </para>
//    /// </remarks>
//    public int maxQueryNodes = 2048;

//    /// <summary>
//    /// True if the <see cref="CrowdManager"/> should be created.
//    /// </summary>
//    public bool enableCrowdManager = true;

//    /// <summary>
//    /// The maximum number agents the <see cref="CrowdManager"/> will support.
//    /// </summary>
//    public int maxCrowdAgents = 10;

//    /// <summary>
//    /// The maximum agent radius the <see cref="CrowdManager"/> will support.
//    /// </summary>
//    public float maxAgentRadius = 0.5f;

//    /// <summary>
//    /// The initial value of the search extents.
//    /// </summary>
//    /// <remarks>
//    /// <para>
//    /// This field is ignored if <see cref="enableCrowdManager"/> is true. In that case the 
//    /// <see cref="CrowdManager"/> extents will be used.
//    /// </para>
//    /// </remarks>
//    public Vector3 initialExtents = new Vector3(1, 1, 1);

//    private NavGroup mNavGroup = new NavGroup();

//    private bool mDebugEnabled;
//    private Navmesh mDebugMesh;
//    private int mDebugVersion;

//    /// <summary>
//    /// The navigation data used to build the <see cref="Navmesh"/>.
//    /// </summary>
//    public INavmeshData NavmeshData
//    {
//        get { return (INavmeshData)mNavmeshData; }
//        set 
//        { 
//            if (mNavmeshData is ScriptableObject)
//                mNavmeshData = (ScriptableObject)value; 
//        }
//    }

//    /// <summary>
//    /// True if debug visualizations are enabled.
//    /// </summary>
//    public bool DebugEnabled
//    {
//        get { return mDebugEnabled; }
//        set 
//        { 
//            mDebugEnabled = value;
//            if (!mDebugEnabled)
//                mDebugMesh = null;
//        }
//    }

//    private bool IsValid
//    {
//        get
//        {
//            return (mNavmeshData && NavmeshData.HasNavmesh 
//                && (!enableCrowdManager || avoidanceData));
//        }
//    }

//    private bool IsActive
//    {
//        get
//        {
//            return (mNavGroup.query != null
//                && !mNavGroup.query.IsDisposed
//                && !mNavGroup.mesh.IsDisposed
//                && (mNavGroup.crowd == null || (!mNavGroup.crowd.IsDisposed)));
//        }
//    }

//    /// <summary>
//    /// The navigation resources provided by the source.
//    /// </summary>
//    /// <remarks>
//    /// <para>
//    /// The resources will be initialized if they are not already available.
//    /// </para>
//    /// </remarks>
//    /// <returns>The navigation resources provided by the source.</returns>
//    public NavGroup GetNavGroup()
//    {
//        if (!IsActive)
//            InitializeOnce();
//        return mNavGroup;
//    }

//    private bool InitializeOnce()
//    {
//        if (mNavmeshData == null || !NavmeshData.HasNavmesh)
//        {
//            Debug.LogError("Aborted initialization. Source has no navmesh.", this);
//            return false;
//        }

//        if (enableCrowdManager && avoidanceData == null)
//        {
//            Debug.LogError(name + ": Aborted initialization."
//                + " Avoidance configuration is not available.");
//            return false;
//        }

//        Navmesh navmesh = NavmeshData.GetNavmesh();
//        NavmeshQuery query;
//        NavStatus status = NavmeshQuery.Create(navmesh, maxQueryNodes, out query);
//        if (NavUtil.Failed(status))
//        {
//            navmesh = null;
//            Debug.LogError(name 
//                + ": Aborted initialization. Failed query creation: "
//                + status.ToString());
//            return false;
//        }

//        CrowdManager crowd = null;

//        Vector3 defaultExtents;
//        NavmeshQueryFilter mDefaultFilter;

//        if (enableCrowdManager)
//        {
//            crowd = CrowdManager.Create(maxCrowdAgents, maxAgentRadius, navmesh);
//            defaultExtents = crowd.GetQueryExtents();
//            mDefaultFilter = crowd.QueryFilter;
//            int count = Mathf.Min(CrowdManager.MaxAvoidanceParams, CAICrowdAvoidanceSet.MaxCount);
//            for (int i = 0; i < count; i++)
//            {
//                crowd.SetAvoidanceConfig(i, avoidanceData[i]);
//            }
//        }
//        else
//        {
//            crowd = null;
//            defaultExtents = initialExtents;
//            mDefaultFilter = new NavmeshQueryFilter();
//        }

//        mNavGroup = new NavGroup(navmesh
//            , query
//            , crowd
//            , mDefaultFilter
//            , defaultExtents, false);

//        return true;
//    }

//    void OnRenderObject()
//    {
//        if (!mDebugEnabled)
//            return;

//        INavmeshData data = NavmeshData;

//        if (!mNavmeshData || !data.HasNavmesh)
//        {
//            mDebugMesh = null;
//            return;
//        }

//        if (mDebugMesh == null || data.Version != mDebugVersion)
//        {
//            mDebugMesh = data.GetNavmesh();
//            mDebugVersion = data.Version;

//            if (mDebugMesh == null)
//                return;
//        }

//        NavDebug.Draw(mDebugMesh, false);
//    }

//}

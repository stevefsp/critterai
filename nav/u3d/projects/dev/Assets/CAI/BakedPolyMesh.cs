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
using org.critterai.nmgen;
using org.critterai.nmgen.u3d;
using org.critterai;

/// <summary>
/// A polygon mesh that is built at design time and creates
/// <see cref="PolyMesh"/> and <see cref="PolyMeshDetail"/> objects at run-time.
/// </summary>
/// <remarks>
/// <p>This component provides means of serializing mesh data
/// in the Unity. (Since the detault <see cref="PolyMesh"/> and 
/// <see cref="PolyMeshDetail"/> serialization methods are not compatible with 
/// Unity serialization.)</p>
/// </remarks>
[System.Serializable]
[ExecuteInEditMode]
[AddComponentMenu("CAI/Baked PolyMesh")]
public class BakedPolyMesh
    : MonoBehaviour
{
    /*
     * Design notes:
     *
     * Odd Unity behavior:  The byte arrays sometimes change from
     * null to byte[0].  Unity doesn't like serializing a null array?
     * So have to account for that.
     */

    /// <summary>
    /// The configuation to use when baking the polygon mesh.
    /// </summary>
    /// <remarks>
    /// <p>This field can be set to null after baking.</p>
    /// </remarks>
    public NMGenBuildConfig buildConfig = null;

    /// <summary>
    /// The source geometry used to bake the polygon mesh.
    /// </summary>
    /// <remarks>
    /// <p>This field can be set to null after baking.</p>
    /// </remarks>
    public DSGeometry geomSource = null;

    private bool mDisplayPolyMesh = false;
    private bool mDisplayDetailMesh = false;

    // Stores the data object used for debug display.
    private System.Object mDebugObject = null;

    [SerializeField]
    private byte[] mPolyPack = null;

    [SerializeField]
    private byte[] mDetailPack = null;

    [SerializeField]
    private Vector3 mSourceBoundsMin;

    [SerializeField]
    private Vector3 mSourceBoundsMax;

    [SerializeField]
    private Vector3 mPolyBoundsMax;

    [SerializeField]
    private Vector3 mPolyBoundsMin;

    [SerializeField]
    private int mPolyCount = 0;

    [SerializeField]
    private int mDetailTriCount = 0;

    [SerializeField]
    private int mSourceTriCount = 0;

    /// <summary>
    /// The number of polygons in the baked polybon mesh.
    /// </summary>
    public int PolyCount { get { return mPolyCount; } }

    /// <summary>
    /// The number of triangles in the geometry used to build the
    /// polygon mesh.
    /// </summary>
    /// <remarks>
    /// <p>Not critical. Used for reporting.</p>
    /// </remarks>
    public int SourceTriCount { get { return mSourceTriCount; } }

    /// <summary>
    /// The number of triangles in the baked detail mesh.
    /// </summary>
    public int DetailTriCount { get { return mDetailTriCount; } }

    /// <summary>
    /// The minimum AABB bounds of the geometry used to build the mesh.
    /// </summary>
    /// <remarks>
    /// <p>Not critical. Used for reporting.</p>
    /// </remarks>
    public Vector3 SourceBoundsMin
    {
        get { return mSourceBoundsMin; }
    }

    /// <summary>
    /// The maximum AABB bounds of the geometry used to build the mesh.
    /// </summary>
    /// <remarks>
    /// <p>Not critical. Used for reporting.</p>
    /// </remarks>
    public Vector3 SourceBoundsMax
    {
        get { return mSourceBoundsMax; }
    }

    /// <summary>
    /// The minimum bounds of the polygon mesh's AABB.
    /// </summary>
    public Vector3 PolyBoundsMin
    {
        get { return mPolyBoundsMin; }
    }

    /// <summary>
    /// The maximum bounds of the polygon mesh's AABB.
    /// </summary>
    public Vector3 PolyBoundsMax
    {
        get { return mPolyBoundsMax; }
    }

    /// <summary>
    /// TRUE if the polygon mesh debug visualization should be displayed.
    /// </summary>
    public bool DisplayPolyMesh
    {
        get { return mDisplayPolyMesh; }
        set
        {
            if (value != mDisplayPolyMesh)
                mDebugObject = null;

            mDisplayPolyMesh = value;

            mDisplayDetailMesh = 
                (mDisplayPolyMesh ? false : mDisplayDetailMesh);
        }
    }

    /// <summary>
    /// TRUE if the detail mesh debug visualization should be displayed.
    /// </summary>
    public bool DisplayPolyMeshDetail
    {
        get { return mDisplayDetailMesh; }
        set
        {
            if (value != mDisplayDetailMesh)
                mDebugObject = null;

            mDisplayDetailMesh = value;

            mDisplayPolyMesh =
                (mDisplayDetailMesh ? false : mDisplayPolyMesh);
        }
    }

    /// <summary>
    /// TRUE if the object contains baked data.
    /// </summary>
    /// <returns></returns>
    public bool HasMesh
    {
        get { return !(mPolyPack == null || mPolyPack.Length == 0); }
    }

    /// <summary>
    /// Bakes the mesh data.
    /// </summary>
    /// <remarks>Only the mesh parameters are critical.  The other parameters
    /// provide data used for reporting.</remarks>
    /// <param name="polyMesh">The source polygon mesh.</param>
    /// <param name="detailMesh">The source detail mesh.</param>
    /// <param name="sourceTriCount">The number of triangles in the
    /// source geometry used to build the meshes.</param>
    /// <param name="sourceBoundsMin">The minimum AABB bounds of the
    /// source geometry used to build the mesh.</param>
    /// <param name="sourceBoundsMax">The maximum AABB bounds of the
    /// source geometry used to build the mesh.</param>
    /// <returns></returns>
    public bool Bake(PolyMesh polyMesh
        , PolyMeshDetail detailMesh
        , int sourceTriCount
        , Vector3 sourceBoundsMin
        , Vector3 sourceBoundsMax)
    {
        ClearMesh();

        if (polyMesh == null 
            || polyMesh.IsDisposed 
            || polyMesh.PolyCount < 1
            || detailMesh == null 
            || detailMesh.IsDisposed 
            || detailMesh.MeshCount < 1
            || sourceTriCount < 0)
        {
            return false;
        }

        mPolyPack = polyMesh.GetSerializedData(false);
        mDetailPack = detailMesh.GetSerializedData(false);

        if (mPolyPack == null || mDetailPack == null)
        {
            ClearMesh();
            return false;
        }

        mSourceTriCount = sourceTriCount;
        mSourceBoundsMax = sourceBoundsMax;
        mSourceBoundsMin = sourceBoundsMin;

        mPolyCount = polyMesh.PolyCount;
        mPolyBoundsMax = Vector3Util.GetVector(polyMesh.GetBoundsMax(), 0);
        mPolyBoundsMin = Vector3Util.GetVector(polyMesh.GetBoundsMin(), 0);

        mDetailTriCount = detailMesh.TriCount;

        return true;
    }

    /// <summary>
    /// Creates a polygon mesh from the baked data.
    /// </summary>
    /// <returns>A polygon mesh created from the baked data.</returns>
    public PolyMesh GetPolyMesh()
    {
        if (HasMesh)
            return new PolyMesh(mPolyPack);
        return null;
    }

    /// <summary>
    /// Creates a detail mesh from the baked data.
    /// </summary>
    /// <returns>A detail mesh created from the baked data.</returns>
    public PolyMeshDetail GetDetailMesh()
    {
        if (HasMesh)
            return new PolyMeshDetail(mDetailPack);
        return null;
    }

    /// <summary>
    /// Creates polygon mesh data from the baked data.
    /// </summary>
    /// <returns>Polygon mesh data created from the baked data.</returns>
    public PolyMeshData GetPolyData()
    {
        if (!HasMesh)
            return null;

        PolyMesh mesh = new PolyMesh(mPolyPack);

        return mesh.GetData(false);
    }

    /// <summary>
    /// Creates detail mesh data from the baked data.
    /// </summary>
    /// <returns>Detail mesh data created from the baked data.</returns>
    public PolyMeshDetailData GetDetailData()
    {
        if (!HasMesh)
            return null;

        PolyMeshDetail mesh = new PolyMeshDetail(mDetailPack);

        return mesh.GetData(false);
    }

    /// <summary>
    /// Clears all mesh data.
    /// </summary>
    public void ClearMesh()
    {
        mPolyPack = null;
        mDetailPack = null;
        mDebugObject = null;
        mDetailTriCount = 0;
        mPolyCount = 0;
        mSourceTriCount = 0;
        mSourceBoundsMax = Vector3.zero;
        mSourceBoundsMin = Vector3.zero;
        mPolyBoundsMin = Vector3.zero;
        mPolyBoundsMax = Vector3.zero;
    }

    void OnRenderObject()
    {
        // Design note: The draw methods handle detection
        // of whether drawing can actually occur.
        if (mDisplayPolyMesh)
            DrawPolyMesh();
        else if (mDisplayDetailMesh)
            DrawDetailMesh();
    }

    private void DrawPolyMesh()
    {
        if (!HasMesh)
            return;

        if (mDebugObject == null)
            mDebugObject = new PolyMesh(mPolyPack).GetData(false);

        NMGenDebug.Draw((PolyMeshData)mDebugObject);
    }

    private void DrawDetailMesh()
    {
        if (!HasMesh)
            return;

        if (mDebugObject == null)
            mDebugObject = new PolyMeshDetail(mDetailPack).GetData(false);

        NMGenDebug.Draw((PolyMeshDetailData)mDebugObject);

    }
}

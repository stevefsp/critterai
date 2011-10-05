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
using org.critterai.nav;
using org.critterai.nav.u3d;

/// <summary>
/// A navigation mesh that is baked at design time and creates a
/// <see cref="Navmesh"/> object at run-time.
/// </summary>
[System.Serializable]
[ExecuteInEditMode]
[AddComponentMenu("CAI/Baked Navmesh")]
public sealed class BakedNavmesh
    : MonoBehaviour 
{
    /// <summary>
    /// The source data to use to bake the mesh.
    /// </summary>
    /// <remarks>
    /// <para>Only applicable until the mesh is baked.  
    /// Can be set to null after baking.</para>
    /// </remarks>
    public DSTileData sourceData = null;

    [System.NonSerialized]
    private bool mDisplayMesh = false;

    [System.NonSerialized]
    private Navmesh mDebugObject = null;

    [SerializeField]
    private byte[] mDataPack = null;

    /// <summary>
    /// True to display the debug visualization.
    /// </summary>
    public bool DisplayMesh
    {
        get { return mDisplayMesh; }
        set 
        {
            mDisplayMesh = value;
            if (!mDisplayMesh)
                mDebugObject = null;
        }
    }

    /// <summary>
    /// TRUE if the navigation mesh is available. (Has been baked.)
    /// </summary>
    /// <returns>TRUE if the navigation mesh is available.</returns>
    public bool HasNavmesh
    {
        get { return (mDataPack != null && mDataPack.Length > 0); }
    }

    /// <summary>
    /// Bakes the navigation mesh data for later use.
    /// </summary>
    /// <param name="navmesh">The navigation mesh to bake.</param>
    /// <returns>TRUE if the bake was successful.</returns>
    public bool Bake(Navmesh navmesh)
    {
        if (navmesh == null || navmesh.IsDisposed)
            return false;

        mDataPack = navmesh.GetSerializedMesh();

        if (mDataPack == null)
            return false;

        return true;
    }

    /// <summary>
    /// Clears the baked data.
    /// </summary>
    public void ClearMesh()
    {
        mDataPack = null;
        mDebugObject = null;
    }

    /// <summary>
    /// Creates a new navigation mesh from the baked data.
    /// </summary>
    /// <returns>A new navigation mesh. Or null if there is no baked
    /// data.</returns>
    public Navmesh GetNavmesh()
    {
        if (!HasNavmesh)
            return null;

        Navmesh result;
        if (NavUtil.Failed(Navmesh.Build(mDataPack, out result)))
            return null;

        return result;
    }

    void OnRenderObject()
    {
        if (!mDisplayMesh || !HasNavmesh)
            return;

        // The disposal check is necessary because of Unity behavior.  
        // A recompile can cause a non-null invalid mesh to come into
        // being.  (Is it attempting some sort of serialization on
        // the private field?)
        // TODO: Receck this since non-serializable attr was attached.
        if (mDebugObject == null || mDebugObject.IsDisposed)
            mDebugObject = GetNavmesh();

        NavDebug.Draw(mDebugObject);

    }
}

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
using org.critterai.interop;
using UnityEngine;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Provides a Unity friendly interface to the <see cref="PathCorridor"/> 
    /// class.
    /// </summary>
    /// <remarks>
    /// <para>See the <see cref="PathCorridor"/> class for 
    /// detailed documentation.</para>
    /// <para>While this is technically a convenience class, it is implemented
    /// in such a way that its features will have the minimum possible negative
    /// impact performance and memory.</para>
    /// </remarks>
    public class U3DPathCorridor
    {
        // Note: Purposefully not sealed.

        private float[] vbuffA = new float[3];
        private float[] vbuffB = new float[3];

        private PathCorridor mRoot;
        private U3DNavmeshQuery mQuery;

        /// <summary>
        /// The root corridor.
        /// </summary>
        public PathCorridor RootCorridor { get { return mRoot; } }

        /// <summary>
        /// The maximum path size that can be handled by the corridor.
        /// </summary>
        public int MaxPathSize { get { return mRoot.MaxPathSize; } }

        /// <summary>
        /// The type of unmanaged resource used by the object.
        /// </summary>
        public AllocType ResourceType { get { return mRoot.ResourceType; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return mRoot.IsDisposed; } }

        /// <summary>
        /// The query object used by the corridor.
        /// </summary>
        public U3DNavmeshQuery Query
        {
            get { return mQuery; }
            set 
            {
                mQuery = value;
                if (mQuery == null)
                    mRoot.Query = null;
                else
                    mRoot.Query = value.RootQuery;
            }
        }

        /// <summary>
        /// The query filter used by the corridor.
        /// </summary>
        public NavmeshQueryFilter Fitler
        {
            get { return mRoot.Fitler; }
            set { mRoot.Fitler = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxPathSize">The maximum path size that can
        /// be handled by the object. [Limit: >= 1]</param>
        /// <param name="query">The query to be used by the corridor.
        /// </param>
        /// <param name="filter">The query filter to be used by the corridor.
        /// </param>
        public U3DPathCorridor(int maxPathSize
            , U3DNavmeshQuery query
            , NavmeshQueryFilter filter)
        {
            mQuery = query;
            mRoot = new PathCorridor(maxPathSize, mQuery.RootQuery, filter);
        }

        /// <summary>
        /// Immediately frees all unmanaged resources allocated by the
        /// object.
        /// </summary>
        public void RequestDisposal()
        {
            mRoot.RequestDisposal();
        }

        /// <summary>
        /// Resets the corridor to the specified position.
        /// </summary>
        /// <remarks>
        /// <para>The position's polygon reference must be valid.</para>
        /// </remarks>
        /// <param name="position">The position of the client.
        /// </param>
        public void Reset(NavmeshPoint position)
        {
            mRoot.Reset(position.polyRef
                , Vector3Util.GetVector(position.point, vbuffA));
        }

        /// <summary>
        /// Finds the corners in the corridor from the position toward the 
        /// target. (The straightened path.)
        /// </summary>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: Maximum Corners > 1]</param>
        /// <returns>The number of corners returned in the buffers.</returns>
        public int FindCorners(CornerData buffer)
        {
            return mRoot.FindCorners(buffer);
        }

        /// <summary>
        /// Attempts to optimize the path if the specified point is visible 
        /// from the current position.
        /// </summary>
        /// <param name="next">The point to search toward.</param>
        /// <param name="optimizationRange">The maximum range to search.
        /// [Limit: > 0]</param>
        public void OptimizePathVisibility(Vector3 next
            , float optimizationRange)
        {
            mRoot.OptimizePathVisibility(Vector3Util.GetVector(next, vbuffA)
                , optimizationRange);
        }

        /// <summary>
        /// Attempts to optimize the path using a local area search.
        /// (Partial replanning.)
        /// </summary>
        public void OptimizePathTopology()
        {
            mRoot.OptimizePathTopology();
        }

        /// <summary>
        /// Moves over an off-mesh connection.
        /// </summary>
        /// <param name="connectionRef">The connection polygon reference.
        /// </param>
        /// <param name="endpointRefs">Polygon endpoint references.
        /// [Length: 2]</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="endPosition">The end position.</param>
        /// <returns>True if the operation succeeded.</returns>
        public bool MoveOverConnection(uint connectionRef
            , uint[] endpointRefs
            , out Vector3 startPosition
            , out Vector3 endPosition)
        {
            ResetVBuffA();
            ResetVBuffB();

            bool result = mRoot.MoveOverConnection(connectionRef
                , endpointRefs
                , vbuffA
                , vbuffB);

            startPosition = Vector3Util.GetVector(vbuffA);
            endPosition = Vector3Util.GetVector(vbuffB);

            return result;
        }

        /// <summary>
        /// Moves the position from the current location to the desired
        /// location, adjusting the corridor as needed to reflect the
        /// new position.
        /// </summary>
        /// <param name="desiredPosition">The desired position.</param>
        /// <returns>The result of the move. (The actual new position.)
        /// </returns>
        public NavmeshPoint MovePosition(Vector3 desiredPosition)
        {
            NavmeshPoint position = new NavmeshPoint();
            ResetVBuffB();
            position.polyRef = mRoot.MovePosition(
                Vector3Util.GetVector(desiredPosition, vbuffA)
                , vbuffB);
            position.point = Vector3Util.GetVector(vbuffB);
            return position;
        }

        /// <summary>
        /// Moves the target from the curent location to the desired
        /// location, adjusting the corridor as needed to reflect the
        /// change.
        /// </summary>
        /// <param name="desiredTarget">The desired target.</param>
        /// <returns>The result of the move. (The actual new target.)
        /// </returns>
        public NavmeshPoint MoveTarget(Vector3 desiredTarget)
        {
            NavmeshPoint position = new NavmeshPoint();
            ResetVBuffB();
            position.polyRef = mRoot.MoveTarget(
                Vector3Util.GetVector(desiredTarget, vbuffA)
                , vbuffB);
            position.point = Vector3Util.GetVector(vbuffB);
            return position;
        }

        /// <summary>
        /// Loads a new path and target into the corridor.
        /// </summary>
        /// <param name="target">The target location within the last
        /// polygon of the path.</param>
        /// <param name="path">The path corridor. 
        /// [(polyRef) * <paramref name="pathCount"/>]</param>
        /// <param name="pathCount">The number of polygons in the path.
        /// [Limits: 0 &lt;= value &lt;= <see cref="MaxPathSize"/>.</param>
        public void SetCorridor(Vector3 target
            , uint[] path
            , int pathCount)
        {
            mRoot.SetCorridor(Vector3Util.GetVector(target, vbuffA)
                , path
                , pathCount);
        }

        /// <summary>
        /// Gets the current position within the corridor.
        /// </summary>
        /// <returns>The current position within the corridor.
        /// </returns>
        public NavmeshPoint GetPosition()
        {
            NavmeshPoint p = new NavmeshPoint();
            p.polyRef = mRoot.GetPosition(vbuffA);
            p.point = Vector3Util.GetVector(vbuffA);
            return p;
        }

        /// <summary>
        /// Gets the current target within the corridor.
        /// </summary>
        /// <returns>The current target within the corridor.
        /// </returns>
        public NavmeshPoint GetTarget()
        {
            NavmeshPoint p = new NavmeshPoint();
            p.polyRef = mRoot.GetTarget(vbuffA);
            p.point = Vector3Util.GetVector(vbuffA);
            return p;
        }

        /// <summary>
        /// The polygon reference id of the first polygon in the corridor.
        /// (The polygon containing the position.)
        /// </summary>
        /// <returns>The polygon reference id of the first polygon in the 
        /// corridor.</returns>
        public uint GetFirstPoly()
        {
            return mRoot.GetFirstPoly();
        }

        /// <summary>
        /// The polygon reference id of the last polygon in the corridor.
        /// (The polygon containing the target.)
        /// </summary>
        /// <returns>The polygon reference id of the last polygon in the 
        /// corridor.</returns>
        public uint GetLastPoly()
        {
            return mRoot.GetLastPoly();
        }

        /// <summary>
        /// Obtains a copy of the corridor path.
        /// </summary>
        /// <remarks><para>
        /// The buffer should be sized to hold the entire path.
        /// (See: <see cref="GetPathCount"/> and <see cref="MaxPathSize"/>.)
        /// </para></remarks>
        /// <param name="buffer">The buffer to load with the result.
        /// [(polyRef) * pathCount]</param>
        /// <returns>The number of polygons in the path.</returns>
        public int GetPath(uint[] buffer)
        {
            return mRoot.GetPath(buffer);
        }

        /// <summary>
        /// The number of polygons in the current corridor path.
        /// </summary>
        /// <returns>The number of polygons in the current corridor path.
        /// </returns>
        public int GetPathCount()
        {
            return mRoot.GetPathCount();
        }

        /// <summary>
        /// Checks the current corridor path to see if the polygon references
        /// remain valid.
        /// </summary>
        /// <remarks>
        /// <para>The path can be invalidated if there are structural
        /// changes to the underlying navigation mesh, or the state of
        /// a polygon within the path changes resulting in it being filtered 
        /// out. (E.g. An exclusion or inclusion flag changes.)</para>
        /// </remarks>
        /// <param name="maxLookAhead">The number of polygons from the
        /// beginning of the corridor to search.</param>
        /// <returns>True if the seached portion of the path is still
        /// valid.</returns>
        public bool IsValid(int maxLookAhead)
        {
            return mRoot.IsValid(maxLookAhead);
        }

        private void ResetVBuffA()
        {
            vbuffA[0] = 0;
            vbuffA[1] = 0;
            vbuffA[2] = 0;
        }

        private void ResetVBuffB()
        {
            vbuffB[0] = 0;
            vbuffB[1] = 0;
            vbuffB[2] = 0;
        }
    }
}

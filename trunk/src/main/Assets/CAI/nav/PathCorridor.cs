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
using System;
using org.critterai.nav.rcn;
using org.critterai.interop;

namespace org.critterai.nav
{
    /// <summary>
    /// Represents a dynamic polygon corridor used to plan agent movement.
    /// </summary>
    /// <remarks>
    /// <para>Instances of this class are loaded with a path, usually obtained
    /// from a <see cref="NavmeshQuery.FindPath"/> call.  The object
    /// is then used to plan local movement, which the corridor automatically
    /// updating as needed to deal with inaccurate agent locomotion.</para>
    /// <para>Example of a common use case:</para>
    /// <ol>
    /// <li>Construct the corridor object using the <see cref="NavmeshQuery"/> 
    /// and <see cref="NavmeshQueryFilter"/> objects in use by the agent.</li>
    /// <li>Optain a path from the query object.</li>
    /// <li>Use <see cref="Reset"/> to load the agent's current position. (At 
    /// the beginning of the path.)</li>
    /// <li>Use <see cref="SetCorridor"/> to load the path and target.</li>
    /// <li>Use <see cref="FindCorners"/> to plan movement. (This handles
    /// path straightening.)</li>
    /// <li>Use <see cref="MovePosition"/> to feed agent movement back into
    /// the corridor.</li>
    /// <li>If the target is moving, use <see cref="MoveTarget"/> to
    /// update the end of the corridor.</li>
    /// <li>Repeat the last 3 steps to continue to move the agent.</li>
    /// </ol>
    /// <para>
    /// One of the difficulties in maintaining a path is that floating point
    /// errors, locomotion inaccuracies, and/or local steering can result in 
    /// the agent crossing the boundary of the path corridor, temporarily 
    /// invalidating the path.  This class uses local mesh queries to detect 
    /// and update the corridor as needed to handle these types of issues.
    /// </para>
    /// <para>The Path Corridor Explorer in the
    /// <a href="http://code.google.com/p/critterai/downloads/list">
    /// CAINav Sample Pack</a> can be used to explore the features of this 
    /// class.</para>
    /// </remarks>
    public sealed class PathCorridor
        : IManagedObject
    {
        private IntPtr mRoot;
        private int mMaxPathSize;
        private NavmeshQueryFilter mFilter;
        private NavmeshQuery mQuery;

        /// <summary>
        /// The maximum path length that can be handled by the object.
        /// </summary>
        public int MaxPathSize { get { return mMaxPathSize; } }

        /// <summary>
        /// The type of unmanaged resource in the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mRoot == IntPtr.Zero); } }

        /// <summary>
        /// The query used by the object.
        /// </summary>
        public NavmeshQuery Query
        {
            get { return mQuery; }
            set { mQuery = value; }
        }

        /// <summary>
        /// The query filter used by the object.
        /// </summary>
        public NavmeshQueryFilter Fitler
        {
            get { return mFilter; }
            set { mFilter = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxPathSize">The maximum path length that can
        /// be handled by the object. [Limit: >= 1]</param>
        /// <param name="query">The query to be used by the object.
        /// </param>
        /// <param name="filter">The query filter to be used by the object.
        /// </param>
        public PathCorridor(int maxPathSize
            , NavmeshQuery query
            , NavmeshQueryFilter filter)
        {
            maxPathSize = Math.Max(1, maxPathSize);

            mRoot = PathCorridorEx.dtpcAlloc(maxPathSize);

            if (mRoot == IntPtr.Zero)
                mMaxPathSize = 0;
            else
                mMaxPathSize = maxPathSize;

            mQuery = query;
            mFilter = filter;
        }

        ~PathCorridor()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Immediately frees all unmanaged resources allocated by the
        /// object.
        /// </summary>
        public void RequestDisposal()
        {
            if (!IsDisposed)
            {
                PathCorridorEx.dtpcFree(mRoot);
                mRoot = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Resets the corridor to the specified position.
        /// </summary>
        /// <remarks>Essentially, this method is used to halt the agent. It
        /// sets the position and goal to the specified location, and reduces
        /// the corridor to the location's polygon. (Path size = 1.)
        /// </remarks>
        /// <param name="polyRef">The reference of the polygon containing
        /// the position.</param>
        /// <param name="position">The position of the agent. [(x, y, z)]</param>
        public void Reset(uint polyRef, float[] position)
        {
            PathCorridorEx.dtpcReset(mRoot, polyRef, position);
        }

        /// <summary>
        /// Finds the corners in the corridor toward the target. 
        /// (The straightened path.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the method used to plan local movement within the corridor.
        /// One or more corners can be detected in order to plan movement. It
        /// is performs essentially the same function as 
        /// <see cref="NavmeshQuery.GetStraightPath"/>.
        /// </para>
        /// <para>Due to internal optimizations, the maximum 
        /// number of corners returned will be @p cornerPolys.Length - 1.
        /// For example: If the buffers are sized to hold 10 corners, the method 
        /// will never return more than 9 corners.  So if 10 corners are needed,
        /// the buffers should be sized for 11 corners.</para>
        /// <para>Behavior is undefined if the buffer sizes are not based
        /// on the same maximum corner count. E.g. The flag and polygon buffers 
        /// are different sizes.</para>
        /// </remarks>
        /// <param name="cornerVerts">The resulting corner vertices.
        /// [(x, y, z) * cornerCount]</param>
        /// <param name="cornerFlags">Flags describing each corner.
        /// [(flat) * cornerCount]
        /// </param>
        /// <param name="cornerPolys">The reference id of the polygon that is 
        /// being entered at the corner. [(polyRef) * cornerCount] </param>
        /// <returns>The number of corners returned in the buffers.</returns>
        public int FindCorners(float[] cornerVerts
            , WaypointFlag[] cornerFlags
            , uint[] cornerPolys)
        {
            return PathCorridorEx.dtpcFindCorners(mRoot
                , cornerVerts
                , cornerFlags
                , cornerPolys
                , cornerPolys.Length
                , mQuery.root
                , mFilter.root);
        }

        /// <summary>
        /// Attempts to optimize the path if the specified point is visible 
        /// from the current position.
        /// </summary>
        /// <remarks>
        /// <para>Inaccurate locomotion or dynamic obstacle avoidance can force
        /// the agent position significantly outside the original corridor.
        /// Over time this can result in the formation of a non-optimal 
        /// corridor.  This method uses an efficient local visibility search 
        /// to try to re-optimize the corridor between the current position 
        /// and <paramref name="next"/>.
        /// </para>
        /// <para>The corridor will change only if <paramref name="next"/> is 
        /// visible from the current position and moving directly toward the
        /// point is better than following the existing path.</para>
        /// <para>The more inaccurate the agent movement, the more 
        /// beneficial this method becomes.  Simply adjust the frequency of
        /// the call to match the needs to the agent.</para>
        /// <para>This method is not suitable for long distance searches.</para>
        /// </remarks>
        /// <param name="next">The point to search toward. [(x, y, z)]</param>
        /// <param name="optimizationRange">The maximum range to search.</param>
        public void OptimizePathVisibility(float[] next
            , float optimizationRange)
        {
            PathCorridorEx.dtpcOptimizePathVisibility(mRoot
                , next
                , optimizationRange
                , mQuery.root
                , mFilter.root);
        }

        /// <summary>
        /// Attempts to optimize the path using a local area search.
        /// (Partial replanning.)
        /// </summary>
        /// <remarks>
        /// <para>Inaccurate locomotion or dynamic obstacle avoidance can force
        /// the agent position significantly outside the original corridor.
        /// Over time this can result in the formation of a non-optimal 
        /// corridor.  This method will use a local area path search 
        /// to try to re-optimize the corridor.</para>
        /// <para>The more inaccurate the agent movement, the more 
        /// beneficial this method becomes.  Simply adjust the frequency of
        /// the call to match the needs to the agent.</para>
        /// </remarks>
        public void OptimizePathTopology()
        {
            PathCorridorEx.dtpcOptimizePathTopology(mRoot
                , mQuery.root
                , mFilter.root);
        }

        /// <summary>
        /// Moves over an off-mesh connection.
        /// </summary>
        /// <remarks>This method is minimally tested and documented.</remarks>
        /// <param name="connectionRef">The connection polygon reference.
        /// </param>
        /// <param name="endpointRefs">Polygon endpoint references.
        /// [Length: 2]</param>
        /// <param name="startPosition">The start position. [(x, y, z)]</param>
        /// <param name="endPosition">The end position. [(x, y, z)]</param>
        /// <returns>True if the operation succeeded.</returns>
        public bool MoveOverConnection(uint connectionRef
            , uint[] endpointRefs
            , float[] startPosition
            , float[] endPosition)
        {
            return PathCorridorEx.dtpcMoveOverOffmeshConnection(mRoot
                , connectionRef
                , endpointRefs
                , startPosition
                , endPosition
                , mQuery.root);
        }

        /// <summary>
        /// Moves the position from the current position to the desired
        /// position, adjusting the corridor as needed to reflect the
        /// new position.
        /// </summary>
        /// <remarks>
        /// <para>The movement is constrained to the surface of the navigation
        /// mesh.  The corridor is automatically adjusted (shorted or 
        /// lengthened) in order to remain valid.  The new position will
        /// be located in the adjusted corridor's first polygon.</para>
        /// <para>The expected use case is that the desired position will
        /// be 'near' the current corridor.  What is considered 'near' depends
        /// on local polygon density, query search extents, etc.</para>
        /// <para>The resulting position will differ from the desired position if
        /// the desired position is not on the navigation mesh, or it can't
        /// be reached using a local search.</para>
        /// </remarks>
        /// <param name="desiredPosition">The desired position. [(x, y, z)]
        /// </param>
        /// <param name="position">The result of the move. [(x, y, z)] [Out]
        /// </param>
        /// <returns>A reference to the <paramref name="position"/> parameter.
        /// </returns>
        public float[] MovePosition(float[] desiredPosition, float[] position)
        {
            PathCorridorEx.dtpcMovePosition(mRoot
                , desiredPosition
                , mQuery.root
                , mFilter.root
                , position);
            return position;
        }

        /// <summary>
        /// Moves the target from the curent target to the desired
        /// location, adjusting the corridor as needed to reflect the
        /// change.
        /// </summary>
        /// <remarks>
        /// <para>The movement is constrained to the surface of the navigation
        /// mesh.  The corridor is automatically adjusted (shorted or 
        /// lengthened) in order to remain valid.  The new target will
        /// be located in the adjusted corridor's last polygon.</para>
        /// <para>The expected use case is that the desired target will
        /// be 'near' the current corridor.  What is considered 'near' depends
        /// on local polygon density, query search extents, etc.</para>
        /// <para>The resulting target will differ from the desired target if
        /// the desired target is not on the navigation mesh, or it can't
        /// be reached using a local search.</para>
        /// </remarks>
        /// <param name="desiredTarget">The desired target. [(x, y, z)]
        /// </param>
        /// <param name="target">The result of the move. [(x, y, z)] [Out]
        /// </param>
        /// <returns>A reference to the <paramref name="target"/> parameter.
        /// </returns>
        public float[] MoveTarget(float[] desiredTarget, float[] target)
        {
            PathCorridorEx.dtpcMoveTargetPosition(mRoot
                , desiredTarget
                , mQuery.root
                , mFilter.root
                , target);
            return target;
        }

        /// <summary>
        /// Loads a new corridor and target into the object.
        /// </summary>
        /// <para>
        /// The current corridor position is expected to be within the first
        /// polygon in the path.  The target is expected to be in the last 
        /// polygon.
        /// </para>
        /// <param name="target">The target location within the last
        /// polygon of the path. [(x, y, z)]</param>
        /// <param name="path">The path corridor. 
        /// [(polyRef) * <paramref name="pathCount"/>]</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        public void SetCorridor(float[] target
            , uint[] path
            , int pathCount)
        {
            PathCorridorEx.dtpcSetCorridor(mRoot, target, path, pathCount);
        }

        /// <summary>
        /// Gets the current position within the corridor.
        /// </summary>
        /// <param name="position">The position within the corridor. [(x, y, z)] [Out]</param>
        /// <returns>A reference to the <paramref name="position"/> parameter.
        /// </returns>
        public float[] GetPosition(float[] position)
        {
            PathCorridorEx.dtpcGetPos(mRoot, position);
            return position;
        }

        /// <summary>
        /// Gets the current target within the corridor.
        /// </summary>
        /// <param name="target">The position within the corridor. [(x, y, z)] [Out]</param>
        /// <returns>A reference to the <paramref name="target"/> parameter.
        /// </returns>
        public float[] GetTarget(float[] target)
        {
            PathCorridorEx.dtpcGetTarget(mRoot, target);
            return target;
        }

        /// <summary>
        /// The polygon reference id of the first polygon in the corridor.
        /// (The polygon containing the position.)
        /// </summary>
        /// <returns>The polygon reference id of the first polygon in the 
        /// corridor.</returns>
        public uint GetFirstPoly()
        {
            return PathCorridorEx.dtpcGetFirstPoly(mRoot);
        }

        /// <summary>
        /// Obtains a copy of the path corridor.
        /// </summary>
        /// <remarks><para>
        /// The buffer should be sized to hold the entire path.
        /// (See: <see cref="GetPathCount"/>)</para></remarks>
        /// <param name="buffer">The buffer to load with the result.
        /// [(polyRef) * pathCount]</param>
        /// <returns>The number of polygons in the path.</returns>
        public int GetPath(uint[] buffer)
        {
            return PathCorridorEx.dtpcGetPath(mRoot, buffer, buffer.Length);
        }

        /// <summary>
        /// The number of polygons in the current path corridor.
        /// </summary>
        /// <returns>The number of polygons in the current path corridor.
        /// </returns>
        public int GetPathCount()
        {
            return PathCorridorEx.dtpcGetPathCount(mRoot);
        }

        /// <summary>
        /// Checks the current path corridor to see if the polygon references
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
            return PathCorridorEx.dtpcIsValid(mRoot
                , maxLookAhead
                , mQuery.root
                , mFilter.root);
        }

        /// <summary>
        /// Loads the corridor data into the provided 
        /// <see cref="PathCorridorData"/> buffer.
        /// </summary>
        /// <remarks>
        /// <para>Will fail if the corridor's path size 
        /// exceeds <see cref="PathCorridorData.MaxPathSize"/>.  In this case,
        /// use the individual accessors. (E.g. <see cref="GetPath"/>)</para>
        /// </remarks>
        /// <param name="buffer">The buffer to load the data into.</param>
        /// <returns>False if the operation failed.</returns>
        public bool GetData(PathCorridorData buffer)
        {
            return PathCorridorEx.dtpcGetData(mRoot, buffer);
        }
    }
}

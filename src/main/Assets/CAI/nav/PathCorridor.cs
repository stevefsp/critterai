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
    /// Represents a dynamic polygon corridor used to plan client movement.
    /// </summary>
    /// <remarks>
    /// <para>The corridor is loaded with a path, usually obtained
    /// from a <see cref="NavmeshQuery"/> <c>FindPath</c> call.  The corridor
    /// is then used to plan local movement, with the corridor automatically
    /// updating as needed to deal with inaccurate client locomotion.</para>
    /// <para>Example of a common use case:</para>
    /// <ol>
    /// <li>Construct the corridor object using the <see cref="NavmeshQuery"/> 
    /// and <see cref="NavmeshQueryFilter"/> objects in use by the
    /// navigation client.</li>
    /// <li>Obtain a path from a <see cref="NavmeshQuery"/> object.</li>
    /// <li>Use <see cref="Reset"/> to load the client's current position. (At 
    /// the beginning of the path.)</li>
    /// <li>Use <see cref="SetCorridor"/> to load the path and target.</li>
    /// <li>Use <see cref="FindCorners"/> to plan movement. (This handles
    /// path straightening.)</li>
    /// <li>Use <see cref="MovePosition"/> to feed client movement back into
    /// the corridor. (The corridor will automatically adjust as needed.)</li>
    /// <li>If the target is moving, use <see cref="MoveTarget"/> to
    /// update the end of the corridor.  (The corridor will automatically
    /// adjust as needed.)</li>
    /// <li>Repeat the previous 3 steps to continue to move the client.</li>
    /// </ol>
    /// <para>The corridor position and target are always constrained to 
    /// the navigation mesh.</para>
    /// <para>
    /// One of the difficulties in maintaining a path is that floating point
    /// errors, locomotion inaccuracies, and/or local steering can result in 
    /// the client crossing the boundary of the path corridor, temporarily 
    /// invalidating the path.  This class uses local mesh queries to detect 
    /// and update the corridor as needed to handle these types of issues.
    /// </para>
    /// <para>
    /// The fact that local mesh queries are used to move the position and
    /// target locations results in two beahviors that need to be considered:
    /// </para>
    /// <para>
    /// Every time a move method is used there is a chance that the path will 
    /// become non-optimial. Basically, the further 
    /// the target is moved from its original location, and the further the 
    /// position is moved outside the original corridor, the more likely the 
    /// path will become non-optimal. This issue can be addressed by 
    /// periodically running the <see cref="OptimizePathTopology"/> and 
    /// <see cref="OptimizePathVisibility"/> methods.
    /// </para>
    /// <para>
    /// All local mesh queries have distance limitations.  (Review the
    /// <see cref="NavmeshQuery"/> methods for details.)
    /// So the most accurate use case is to move the position and target in 
    /// small increments.  If a large increment is used, then the corridor may 
    /// not be able to accurately find the new location.  Because of this 
    /// limiation, if a position is moved in a large increment, then compare 
    /// the desired and resulting polygon references. If the two do not match, 
    /// then path replanning may be needed.  E.g. If you move the target, 
    /// check <see cref="PathCorridor.GetLastPoly()"/> to see if it is the 
    /// expected polygon.</para>
    /// </remarks>
    public sealed class PathCorridor
        : IManagedObject
    {
        private IntPtr mRoot;
        private int mMaxPathSize;
        private NavmeshQueryFilter mFilter;
        private NavmeshQuery mQuery;

        /// <summary>
        /// The maximum path size that can be handled by the corridor.
        /// </summary>
        public int MaxPathSize { get { return mMaxPathSize; } }

        /// <summary>
        /// The type of unmanaged resource used by the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mRoot == IntPtr.Zero); } }

        /// <summary>
        /// The query object used by the corridor.
        /// </summary>
        /// <remarks>
        /// <para>This property can be set to null.  This supports the ability
        /// to create pools of re-usable path corridor objects.  But it
        /// means that care needs to be taken not to use the object until
        /// a new query object has been assigned.</para>
        /// </remarks>
        public NavmeshQuery Query
        {
            get { return mQuery; }
            set { mQuery = value; }
        }

        /// <summary>
        /// The query filter used by the corridor.
        /// </summary>
        /// <remarks>
        /// <para>This property can be set to null.  This supports the ability
        /// to create pools of re-usable path corridor objects.  But it
        /// means that care needs to be taken not to use the object until
        /// a new filter object has been assigned.</para>
        /// </remarks>
        public NavmeshQueryFilter Filter
        {
            get { return mFilter; }
            set { mFilter = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// <para>The query and filter parameters can be set to null.  This 
        /// supports the ability to create pools of re-usable path corridor 
        /// objects.  But it means that care needs to be taken not to use the 
        /// corridor until query and filter objects have been set.</para>
        /// </remarks>
        /// <param name="maxPathSize">The maximum path size that can
        /// be handled by the object. [Limit: >= 1]</param>
        /// <param name="query">The query to be used by the corridor.
        /// </param>
        /// <param name="filter">The query filter to be used by the corridor.
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

        /// <summary>
        /// Destructor.
        /// </summary>
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
        /// <remarks>This method sets the position and target to the specified 
        /// location, and reduces the corridor to the location's polygon. 
        /// (Path size = 1)
        /// </remarks>
        /// <param name="polyRef">The reference of the polygon containing
        /// the position.</param>
        /// <param name="position">The position of the client. [(x, y, z)]
        /// </param>
        public void Reset(uint polyRef, float[] position)
        {
            PathCorridorEx.dtpcReset(mRoot, polyRef, position);
        }

        /// <summary>
        /// Finds the corners in the corridor from the position toward the 
        /// target. (The straightened path.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the method used to plan local movement within the corridor.
        /// One or more corners can be detected in order to plan movement. It
        /// performs essentially the same function as 
        /// <see cref="NavmeshQuery.GetStraightPath"/>.
        /// </para>
        /// <para>Due to internal optimizations, the maximum 
        /// number of corners returned will be <c>cornerPolys.Length - 1</c>
        /// For example: If the buffers are sized to hold 10 corners, the method 
        /// will never return more than 9 corners.  So if 10 corners are needed,
        /// the buffers should be sized for 11 corners.</para>
        /// <para>If the target is within range, it will be the last corner
        /// and have a polygon reference id of zero.</para>
        /// <para>Behavior is undefined if the buffer sizes are not based
        /// on the same maximum corner count. E.g. The flag and polygon buffers 
        /// are different sizes.</para>
        /// </remarks>
        /// <param name="buffer">The buffer to load the results into.
        /// [Size: Maximum Corners > 1]</param>
        /// <returns>The number of corners returned in the buffers.</returns>
        public int FindCorners(CornerData buffer)
        {
            buffer.cornerCount = PathCorridorEx.dtpcFindCorners(mRoot
                , buffer.verts
                , buffer.flags
                , buffer.polyRefs
                , buffer.polyRefs.Length
                , mQuery.root
                , mFilter.root);
            return buffer.cornerCount;
        }

        /// <summary>
        /// Attempts to optimize the path if the specified point is visible 
        /// from the current position.
        /// </summary>
        /// <remarks>
        /// <para>Inaccurate locomotion or dynamic obstacle avoidance can force 
        /// the argent position significantly outside the original corridor. 
        /// Over time this can result in the formation of a non-optimal 
        /// corridor. Non-optimal paths can also form near the corners of 
        /// tiles.</para>
        /// <para>
        /// This function uses an efficient local visibility search to try 
        /// to optimize the corridor between the current position and 
        /// <paramref name="next"/> next.
        /// </para>
        /// <para>The corridor will change only if <paramref name="next"/> is 
        /// visible from the current position and moving directly toward the
        /// point is better than following the existing path.</para>
        /// <para>The more inaccurate the client movement, the more 
        /// beneficial this method becomes.  Simply adjust the frequency of
        /// the call to match the needs to the client.</para>
        /// <para>This method is not suitable for long distance searches.</para>
        /// </remarks>
        /// <param name="next">The point to search toward. [(x, y, z)]</param>
        /// <param name="optimizationRange">The maximum range to search.
        /// [Limit: > 0]</param>
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
        /// the client position significantly outside the original corridor.
        /// Over time this can result in the formation of a non-optimal 
        /// corridor.  This method will use a local area path search 
        /// to try to re-optimize the corridor.</para>
        /// <para>The more inaccurate the client movement, the more 
        /// beneficial this method becomes.  Simply adjust the frequency of
        /// the call to match the needs to the client.</para>
        /// <para>This is a local optimization and will not necessarily effect 
        /// the entire corridor through to the goal.  So it should normally be 
        /// called based on a time increment rather than movement events.
        /// For example, optimize once a second rather than only one a  
        /// goal change or larger than normal position change.</para>
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
        /// <remarks>
        /// <para>This method is minimally tested and documented.</para>
        /// </remarks>
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
        /// Moves the position from the current location to the desired
        /// location, adjusting the corridor as needed to reflect the
        /// new position.
        /// </summary>
        /// <remarks>
        /// <para>Behavior:</para>
        /// <ul>
        /// <li>The movement is constrained to the surface of the navigation 
        /// mesh.</li>
        /// <li>The corridor is automatically adjusted (shorted or lengthened) 
        /// in order to remain valid. </li>
        /// <li>The new position will be located in the adjusted corridor's 
        /// first polygon.</li>
        /// </ul>
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
        /// <returns>The polygon reference of the new position.
        /// (The first polygon of the corridor.)</returns>
        public uint MovePosition(float[] desiredPosition
            , float[] position)
        {
            return PathCorridorEx.dtpcMovePosition(mRoot
                , desiredPosition
                , mQuery.root
                , mFilter.root
                , position);
        }

        /// <summary>
        /// Moves the target from the curent location to the desired
        /// location, adjusting the corridor as needed to reflect the
        /// change.
        /// </summary>
        /// <remarks>
        /// <para>Behavior:</para>
        /// <ul>
        /// <li>The movement is constrained to the surface of the navigation 
        /// mesh.</li>
        /// <li>The corridor is automatically adjusted (shorted or lengthened) 
        /// in order to remain valid. </li>
        /// <li>The new target will be located in the adjusted corridor's 
        /// last polygon.</li>
        /// </ul>
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
        /// <returns>The polygon reference of the resulting target.
        /// (The last polygon in the corridor.)
        /// </returns>
        public uint MoveTarget(float[] desiredTarget
            , float[] target)
        {
            return PathCorridorEx.dtpcMoveTargetPosition(mRoot
                , desiredTarget
                , mQuery.root
                , mFilter.root
                , target);
        }

        /// <summary>
        /// Loads a new path and target into the corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The current corridor position is expected to be within the first
        /// polygon in the path.  The target is expected to be in the last 
        /// polygon.
        /// </para>
        /// </remarks>
        /// <param name="target">The target location within the last
        /// polygon of the path. [(x, y, z)]</param>
        /// <param name="path">The path corridor. 
        /// [(polyRef) * <paramref name="pathCount"/>]</param>
        /// <param name="pathCount">The number of polygons in the path.
        /// [Limits: 0 &lt;= value &lt;= <see cref="MaxPathSize"/>.</param>
        public void SetCorridor(float[] target
            , uint[] path
            , int pathCount)
        {
            PathCorridorEx.dtpcSetCorridor(mRoot, target, path, pathCount);
        }

        /// <summary>
        /// Gets the current position within the corridor.
        /// </summary>
        /// <param name="position">The position within the corridor. 
        /// [(x, y, z)] [Out]</param>
        /// <returns>The polygon reference of the position. (The first polygon
        /// in the corridor.)
        /// </returns>
        public uint GetPosition(float[] position)
        {
            return PathCorridorEx.dtpcGetPos(mRoot, position);
        }

        /// <summary>
        /// Gets the current target within the corridor.
        /// </summary>
        /// <param name="target">The position within the corridor. [(x, y, z)] 
        /// [Out]</param>
        /// <returns>The polygon reference of the target. (The last polygon
        /// in the corridor.)
        /// </returns>
        public uint GetTarget(float[] target)
        {
            return PathCorridorEx.dtpcGetTarget(mRoot, target);
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
        /// The polygon reference id of the last polygon in the corridor.
        /// (The polygon containing the target.)
        /// </summary>
        /// <returns>The polygon reference id of the last polygon in the 
        /// corridor.</returns>
        public uint GetLastPoly()
        {
            return PathCorridorEx.dtpcGetLastPoly(mRoot);
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
            return PathCorridorEx.dtpcGetPath(mRoot, buffer, buffer.Length);
        }

        /// <summary>
        /// The number of polygons in the current corridor path.
        /// </summary>
        /// <returns>The number of polygons in the current corridor path.
        /// </returns>
        public int GetPathCount()
        {
            return PathCorridorEx.dtpcGetPathCount(mRoot);
        }

        /// <summary>
        /// Checks the current corridor path to see if its polygon references
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
        /// <para>Make sure the buffer is sized to hold the entire result.
        /// (See: <see cref="GetPathCount"/> and <see cref="MaxPathSize"/>.)
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer to load the data into.
        /// [Size: Maximum Path Size >= <see cref="GetPathCount"/>]</param>
        /// <returns>False if the operation failed.</returns>
        public bool GetData(PathCorridorData buffer)
        {
            // Only performs a partial parameter validation.
            if (buffer == null
                || buffer.path == null
                || buffer.path.Length < 1)
            {
                return false;
            }

            if (buffer.path.Length == PathCorridorData.MarshalBufferSize)
                return PathCorridorEx.dtpcGetData(mRoot, buffer);

            buffer.pathCount = GetPath(buffer.path);
            GetPosition(buffer.position);
            GetTarget(buffer.target);

            return true;
        }
    }
}

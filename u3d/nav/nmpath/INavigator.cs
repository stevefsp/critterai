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
using System;
using UnityEngine;
using Path = org.critterai.nav.nmpath.MasterPath.Path;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Provides a means of making navigation requests to a navigator.
    /// </summary>
    public interface INavigator
    {

        /*
         * Design note:
         * 
         * The reason for the existence of this interface is to permit
         * adapters to be created for the Nav object.  For example
         * a debug navigator that tracks performance.
         * 
         */

        /// <summary>
        /// Requests construction of a path from the start to the goal point.
        /// </summary>
        /// <remarks>
        /// The Request will fail if the start or goal point is outside the 
        /// the xz-column of the mesh.
        /// <para>This is a potentially costly operation.  Consider using 
        /// <see cref="RepairPath(float, float, float, MasterPath.Path)">ReparePath</see> if the client
        /// already has a path to the goal.</para>
        /// </remarks>
        /// <param name="startX">startX The x-value of the start point of the path. (startX, startY, startZ)</param>
        /// <param name="startY">The y-value of the start point of the path. (startX, startY, startZ)</param>
        /// <param name="startZ">The z-value of the start point of the path. (startX, startY, startZ)</param>
        /// <param name="goalX">The x-value of the goal point on the path. (goalX, goalY, goalZ)</param>
        /// <param name="goalY">The y-value of the goal point on the path. (goalX, goalY, goalZ)</param>
        /// <param name="goalZ">The z-value of the goal point on the path. (goalX, goalY, goalZ)</param>
        /// <returns>The resulting path request.</returns>
        MasterNavRequest<Path>.NavRequest GetPath(float startX, float startY,
                float startZ, float goalX, float goalY, float goalZ);

        /// <summary>
        /// Requests that a in-progress path request be discarded.
        /// This will result in the request being marked as <see cref="NavRequestState.Failed">Failed</see>.
        /// </summary>
        /// <param name="request">The path request to cancel.</param>
        void DiscardPathRequest(MasterNavRequest<Path>.NavRequest request);

        /// <summary>
        /// Request that an active path be maintained within the cache.  The path's age
        /// will be Reset.
        /// <remarks>
        /// This Request-type only applies if the Nav supports path caching.
        /// <para>To keep a path alive, this operation must be called more frequently
        /// the maximum amount of time the Nav is configured to cache paths.</para>
        /// <para>This Request-type is guaranteed to complete after one Process cycle
        /// of the Nav.  (Usually one frame.)</para>
        /// </remarks>
        /// </summary>
        /// <param name="path">The path to keep alive.</param>
        void KeepPathAlive(Path path);

        /// <summary>
        /// Attempt to repair a path using the new start location.
        /// </summary>
        /// <remarks>
        /// A limited search will be performed to find a path from the new start location
        /// back to the known path.  If the path cannot be repaired, a new full path
        /// search will be required.
        /// <para>If the repair is successful, a new path will be generated.</para>
        /// <para>Under normal use cases, this request may take multiple frames
        /// to complete.</para>
        /// </remarks>
        /// <param name="startX">The x-value of the start point of the repaired path. 
        /// (startX, startY, startZ)</param>
        /// <param name="startY">y-value of the start point of the repaired path. 
        /// (startX, startY, startZ)</param>
        /// <param name="startZ">The z-value of the start point of the repaired path. 
        /// (startX, startY, startZ)</param>
        /// <param name="path">The path to attempt to repair.</param>
        /// <returns>The path repair request.</returns>
        MasterNavRequest<Path>.NavRequest RepairPath(float startX, float startY,
                float startZ, Path path);

        /// <summary>
        /// Gets the position on the navigation mesh that is closest to the
        /// provided point.
        /// </summary>
        /// <remarks>
        /// This Request-type is guaranteed to complete after one Process cycle
        /// of the Nav. (Usually one frame.)
        /// <para>This Request-type is guaranteed to complete successfully as long as the
        /// <see cref="MasterNavigator">navigator</see> has not been disposed.</para>
        /// </remarks>
        /// <param name="x">The x-value of the test point.  (x, y, z)</param>
        /// <param name="y">The y-value of the test point.  (x, y, z)</param>
        /// <param name="z">The z-value of the test point.  (x, y, z)</param>
        /// <returns>The pending request.</returns>
        MasterNavRequest<Vector3>.NavRequest GetNearestValidLocation(float x,
                float y, float z);

        /// <summary>
        /// Indicates whether or not the supplied point is within the column of the
        /// navigation mesh and within the y-axis distance of the surface of the mesh.
        /// </summary>
        /// <remarks>
        /// This Request-type is guaranteed to complete after one Process cycle
        /// of the Nav. (Usually one frame.)
        /// <para>This Request-type is guaranteed to complete successfully as long as the
        /// <see cref="MasterNavigator">navigator</see> has not been disposed.</para>
        /// </remarks>
        /// <param name="x">The x-value of the test point.  (x, y, z)</param>
        /// <param name="y">The y-value of the test point.  (x, y, z)</param>
        /// <param name="z">The z-value of the test point.  (x, y, z)</param>
        /// <param name="yTolerance">The allowed y-axis distance the position may be from the surface
        /// of the navigation mesh and still be considered valid.</param>
        /// <returns>The pending request.</returns>
        MasterNavRequest<Boolean>.NavRequest IsValidLocation(float x, float y,
                float z, float yTolerance);

        /// <summary>
        /// Indicates whether or not the navigator has been disposed.  The navigator
        /// will not accept new requests while disposed.
        /// </summary>
        Boolean IsDisposed { get; }

    }
}

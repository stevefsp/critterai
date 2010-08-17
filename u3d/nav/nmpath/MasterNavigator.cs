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
using System.Collections.Generic;
using System.Text;
using Path = org.critterai.nav.nmpath.MasterPath.Path;
using UnityEngine;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Provides a means of performing path finding queries and managing paths related to
    /// a triangle-based navigation mesh.
    /// </summary>
    /// <remarks>
    /// Primary use case:
    /// <ol>
    /// <li>The MasterNavigator is constructed using the <see cref="NavUtil">NavUtil</see> class
    /// and assigned to an owner thread to manage processing.</li>
    /// <li>Clients are provided with <see cref="Navigator">Navigator</see> references via 
    /// <see cref="Nav">Nav</see> as needed.</li>
    /// <li>Clients make navigation requests via their Navigator reference.</li>
    /// <li>The owner thread uses one of the processing operations to process the requests.
    /// E.G. <see cref="Process(Boolean)">Process(Boolean)</see>.</li>
    /// </ol>
    /// <para>Primary Features:</para>
    /// <ul>
    /// <li>Can be run in a multi-threaded environment by obtaining a 
    /// <see cref="ThreadedNavigator">ThreadedNavigator</see> from  <see cref="NavUtil">NavUtil</see>.</li>
    /// <li>Processing load can be throttled to spread the cost of navigation requests
    /// across multiple frames to prevent spikes.</li>
    /// <li>Provides caching and re-use of paths.</li>
    /// </ul>
    /// <para>Instances of this class are not thread-safe but can be used by multiple threads if:</para>
    /// <ul>
    /// <li>The instance of this class is created using  <see cref="NavUtil">NavUtil</see>.</li>
    /// <li>Only a single (owner) thread holds a reference to the instance of the class.</li>
    /// <li>All other threads interact with the instance through a Navigator reference
    /// obtained via <see cref="Nav">Nav</see>.</li> 
    /// </ul>
    /// </remarks>
    public sealed class MasterNavigator
    {
        /*
         * Design Notes:
         * 
         * Potential Optimizations:
         * 
         * For Path Repair
         * 
         * Before starting the the path repair, find the Cell
         * for the new location and check if it is directly
         * connected to any cells in the existing path.  
         * If so, consider search complete.
         * (Select Cell with highest index if there is more than one.) 
         * 
         * This optimization is based on the expectation that
         * most repairs are requested when only straying slightly off
         * the path.
         */
        
        /// <summary>
        /// Provides a means of making navigation requests to a <see cref="MasterNavigator">MasterNavigator</see>.
        /// </summary>
        /// <remarks>
        /// Primary use case:
        /// <ol>
        /// <li>Client obtains a reference from a MasterNavigator via 
        /// <see cref="MasterNavigator.Nav">MasterNavigator.Nav</see>.</li>
        /// <li>Client monitors validity of its Navigator via  
        /// <see cref="IsDisposed">IsDisposed</see>.</li>
        /// <li>Client makes navigation requests.</li>
        /// <li>Client monitors pending requests for completion.</li>
        /// <li>When Request is complete, client checks the request state.  
        /// If the request completed satisfactorily, client retrieves the data from the request.</li>
        /// </ol>
        /// <para>All operations are thread-safe as long as the MasterNavigator
        /// is handling in a thread-safe manner.</para>
        /// </remarks>
        public class Navigator : INavigator
        {

            private readonly MasterNavigator mRoot;
            
            internal Navigator(MasterNavigator root) 
            {
                mRoot = root;
            }

            /// <summary>
            /// Requests that a in-progress path request be discarded.
            /// This will result in the request being marked as <see cref="NavRequestState.Failed">Failed</see>.
            /// </summary>
            /// <param name="request">The path request to cancel.</param>
            public void DiscardPathRequest(MasterNavRequest<Path>.NavRequest request)
            {
                if (mRoot.mIsDisposed || request == null)
                    return;
                lock (mRoot.mSearchCancellationRequests) 
                {
                    mRoot.mSearchCancellationRequests.Enqueue(request);
                }
            }

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
            public MasterNavRequest<Vector3>.NavRequest GetNearestValidLocation(float x, float y ,float z)
            {
                if (mRoot.mIsDisposed)
                    return new MasterNavRequest<Vector3>(NavRequestState.Failed).Request;
                VectorJob job = new VectorJob(new Vector3(x, y, z));
                lock (mRoot.mAStarRequests) 
                {
                    mRoot.mNearestLocationRequests.Enqueue(job);
                }
                return job.request.Request;
            }

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
            public MasterNavRequest<Path>.NavRequest GetPath(float startX, float startY, float startZ,
                    float goalX, float goalY, float goalZ) 
            {   
                if (mRoot.mIsDisposed)
                    return mRoot.mFailedPath.Request;
                PathJob job = null;
                if (mRoot.mAStarPool.Count == 0)
                    job = new PathJob(startX, startY, startZ
                            , goalX, goalY, goalZ, mRoot.Heuristic);
                lock (mRoot.mAStarRequests) 
                {
                    if (job == null)
                    {
                        job = mRoot.mAStarPool.Dequeue();
                        job.Initialize(startX, startY, startZ
                                , goalX, goalY, goalZ);
                    }
                    mRoot.mAStarRequests.Enqueue(job);
                }
                return job.request.Request;
            }

            /// <summary>
            /// Indicates whether or not the navigator has been disposed.  The navigator
            /// will not accept new requests while disposed.
            /// </summary>
            public Boolean IsDisposed { get { return mRoot.mIsDisposed; } }

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
            public MasterNavRequest<Boolean>.NavRequest IsValidLocation(float x, float y, float z
                    , float yTolerance) 
            {
                if (mRoot.mIsDisposed)
                    return new MasterNavRequest<Boolean>(NavRequestState.Failed, false).Request;
                yTolerance = Math.Max(0, yTolerance);
                ValidLocationJob job = new ValidLocationJob(x, y, z, yTolerance);
                lock (mRoot.mAStarRequests) 
                {
                    mRoot.mValidLocationRequests.Enqueue(job);
                }
                return job.request.Request;
            }

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
            public void KeepPathAlive(Path path)
            {
                if (mRoot.mIsDisposed || path == null || path.IsDisposed)
                    return;
                lock (mRoot.mKeepAliveRequests) 
                {
                    mRoot.mKeepAliveRequests.Enqueue(path);
                }
            }

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
            public MasterNavRequest<Path>.NavRequest RepairPath(float startX, float startY, float startZ,
                    Path path)
            {
                if (mRoot.mIsDisposed || path == null || path.IsDisposed)
                    return mRoot.mFailedPath.Request;
                RepairJob job = null;
                if (mRoot.mRepairPool.Count == 0)
                    job = new RepairJob(startX, startY, startZ, path);
                lock (mRoot.mAStarRequests) 
                {
                    if (job == null)
                    {
                        job = mRoot.mRepairPool.Dequeue();
                        job.Initialize(startX, startY, startZ, path);
                    }
                    mRoot.mPathRepairRequests.Enqueue(job);
                }
                return job.request.Request;
            }
            
        }
        
        /// <summary>
        /// A re-usable A* search job.
        /// </summary>
        private class PathJob
        {
            public MasterNavRequest<Path> request = null;
            public AStarSearch search;
            public Boolean searchIsReady = false;        
            public float startX;
            public float startY;
            public float startZ;    
            public float goalX;
            public float goalY;
            public float goalZ;
            public PathJob(float startX, float startY, float startZ
                    , float goalX, float goalY, float goalZ, DistanceHeuristicType heuristic)
            {
                search =  new AStarSearch(heuristic);
                Initialize(startX, startY, startZ, goalX, goalY, goalZ);
            }
            public void Initialize(float startX, float startY, float startZ
                    , float goalX, float goalY, float goalZ)
            {
                // Dev Note: The Request is re-initialized during the Reset.
                Reset();
                this.startX = startX;
                this.startY = startY;
                this.startZ = startZ;
                this.goalX = goalX;
                this.goalY = goalY;
                this.goalZ = goalZ;
            }
            public void InitializeSearch(TriCell startCell, TriCell goalCell)
            {
                search.Initialize(startX, startY, startZ
                        , goalX, goalY, goalZ
                        , startCell
                        , goalCell);
                searchIsReady = true;
            }
            public void Reset()
            {
                request = new MasterNavRequest<Path>();
                startX = 0;
                startY = 0;
                startZ = 0;
                goalX = 0;
                goalY = 0;
                goalZ = 0;
                search.Reset();
                searchIsReady = false;
            }

        }
        
        /// <summary>
        /// A re-usable path repair job.
        /// </summary>
        private class RepairJob
        {
            public Path path;
            public MasterNavRequest<Path> request = null;
            public readonly DijkstraSearch search = new DijkstraSearch();
            public MasterPath source = null;
            public float startX;
            public float startY;
            public float startZ;        
            public RepairJob(float startX, float startY, float startZ, Path path)
            {
                Initialize(startX, startY, startZ, path);
            }
            public void Initialize(float startX, float startY, float startZ, Path path)
            {
                Reset();
                this.path = path;
                this.startX = startX;
                this.startY = startY;
                this.startZ = startZ;
                request = new MasterNavRequest<Path>();
            }
            public void InitializeSearch(MasterPath source, TriCell startCell)
            {
                this.source = source;
                TriCell[] goalCells = source.GetRawCopy(new TriCell[source.Size]);
                search.Initialize(startX, startY, startZ, startCell, goalCells, 2, false);
            }
            public void Reset()
            {
                request = null;
                path = null;
                startX = 0;
                startY = 0;
                startZ = 0;
                source = null;
                search.Reset();
            }
        }
        
        /// <summary>
        /// A job used for <see cref="Navigator.IsValidLocation(float, float, float, float)">
        /// Navigator.IsValidLocation</see>
        /// requests.
        /// </summary>
        private class ValidLocationJob
        {
            public Vector3 point;
            public readonly MasterNavRequest<Boolean> request = new MasterNavRequest<Boolean>();
            public readonly float tolerance;
            public ValidLocationJob(float x, float y, float z, float tolerance)
            {
                point = new Vector3(x, y, z);
                this.tolerance = tolerance;
            }
        }
        
        /// <summary>
        /// A job used for requests involving Vector3 objects.
        /// </summary>
        private class VectorJob
        {
            public Vector3 data;
            public readonly MasterNavRequest<Vector3> request = new MasterNavRequest<Vector3>();
            public VectorJob(Vector3 data)
            {
                this.data = data;
            }
        }
        
        private readonly DistanceHeuristicType mHeuristic;
        private readonly int mMaxPathAge;
        private readonly long mMaxProcessingTimeslice;
        private readonly int mRepairSearchDepth;

        /// <summary>
        /// The cached pathes.
        /// </summary>
        private readonly List<MasterPath> mActivePaths;
        
        /// <summary>
        ///  The in-progress A* searches.
        /// </summary>
        private readonly List<PathJob> mAStarJobs  = new List<PathJob>();
        
        
        /// <summary>
        /// The A* search pool. (Searches available for new requests.)
        /// <para>IMPORTANT: Access to this object must be lock via a lock
        /// on the mAStarRequests object.</para>
        /// </summary>
        private readonly Queue<PathJob> mAStarPool;
        
        /// <summary>
        /// The maximum pool Size for A* searches.
        /// </summary>
        private readonly int mAStarPoolMax;

        /// <summary>
        /// The Request queue for new A* searches.
        /// <para>IMPORTANT: All access to this object must be locked.</para>
        /// </summary>
        private readonly Queue<PathJob> mAStarRequests = new Queue<PathJob>();

        private readonly MasterNavRequest<Path> mFailedPath 
                = new MasterNavRequest<Path>(NavRequestState.Failed);
        
        private Boolean mIsDisposed = false;
        
        /// <summary>
        /// A queue for requests to keep a path in the cache beyond its normal
        /// expiration.
        /// <para>IMPORTANT: Access to this object must be lock via a lock
        /// on the mAStarRequests object.</para>
        /// </summary>
        private readonly Queue<Path> mKeepAliveRequests = new Queue<Path>();
        
        /// <summary>
        /// The navigation mesh.
        /// </summary>
        private readonly TriNavMesh mMesh;

        private readonly Navigator mNavigator;
        
        /// <summary>
        /// All active "nearest location" jobs.
        /// </summary>
        private readonly Queue<VectorJob> mNearestLocationJobs  = new Queue<VectorJob>();
        
        /// <summary>
        /// Pending "nearest location" requests.
        /// <para>IMPORTANT: Access to this object must be lock via a lock
        /// on the mAStarRequests object.</para>
        /// </summary>
        private readonly Queue<VectorJob> mNearestLocationRequests  = new Queue<VectorJob>();

        /// <summary>
        /// The path ID to assign to the next path.
        /// </summary>
        private int mNextPathID = 0;

        /// <summary>
        /// All in-progress path repair jobs.
        /// </summary>
        private readonly List<RepairJob> mPathRepairJobs = new List<RepairJob>();

        /// <summary>
        /// Pending path repair requests.
        /// <para>IMPORTANT: Access to this object must be lock via a lock
        /// on the mAStarRequests object.</para>
        /// </summary>
        private readonly Queue<RepairJob> mPathRepairRequests = new Queue<RepairJob>();
        /// <summary>
        /// The repair search pool.  (Repair jobs available for new requests.)
        /// <para>IMPORTANT: Access to this object must be lock via a lock
        /// on the mAStarRequests object.</para>
        /// </summary>
        private readonly Queue<RepairJob> mRepairPool;

        /// <summary>
        /// The maximum pool Size for repair searches.
        /// </summary>
        private readonly int mRepairPoolMax;

        /// <summary>
        /// A queue holding all cancellation requests.
        /// <para>IMPORTANT: Access to this object must be lock via a lock
        /// on the mAStarRequests object.</para>
        /// </summary>
        private readonly Queue<MasterNavRequest<Path>.NavRequest> mSearchCancellationRequests 
                = new Queue<MasterNavRequest<Path>.NavRequest>();

        /// <summary>
        /// All active "valid location" jobs.
        /// </summary>
        private readonly Queue<ValidLocationJob> mValidLocationJobs 
                = new Queue<ValidLocationJob>();

        /// <summary>
        /// Pending "valid location" requests.
        /// <para>IMPORTANT: Access to this object must be lock via a lock
        /// on the mAStarRequests object.</para>
        /// </summary>
        private readonly Queue<ValidLocationJob> mValidLocationRequests 
                = new Queue<ValidLocationJob>();

        /// <summary>
        /// The mHeuristic to use for A* searches.
        /// </summary>
        public DistanceHeuristicType Heuristic
        {
            get { return mHeuristic; }
        }

        /// <summary>
        /// The maximum age that paths will be cached.  (milliseconds)
        /// When a path expires, it is disposed and removed from the cache.
        /// </summary>
        /// <remarks>Paths will expire, even if still in use by clients, unless the client uses
        /// the <see cref="Navigator.KeepPathAlive(MasterPath.Path)">Navigator.KeepAlive</see> operation to 
        /// reset the path's age.
        /// <para>If set to zero, no path caching will occur, paths will never be disposed,
        /// path sharing will never occur, and path repair will not be possible</para> 
        /// </remarks>
        public int MaxPathAge
        {
            get { return mMaxPathAge; }
        }

        /// <summary>
        ///  The maximum time slice to use during throttling operations. (In ticks as defined by DateTime.Ticks.)
        /// See <see cref="Process(Boolean)">Process(Boolean)</see> for details.
        /// </summary>
        public long MaxProcessingTimeslice
        {
            get { return mMaxProcessingTimeslice; }
        }

        /// <summary>
        /// The maximum search depth to use when attempting to repair paths.
        /// <para>See <see cref="Navigator.RepairPath(float, float, float, MasterPath.Path)">
        /// Navigator.RepairPath</see> for details.</para>
        /// </summary>
        public int RepairSearchDepth
        {
            get { return mRepairSearchDepth; }
        }

        /// <summary>
        /// The number of paths in the path cache.
        /// Will always be zero if <see cref="MaxPathAge">MaxPathAge</see> is zero.
        /// </summary>
        public int ActivePathCount { get { return mActivePaths.Count; } }

        /// <summary>
        /// The number of pending requests to discard path searches.
        /// <para>This value will always be zero following a call
        /// to any of the Process operations.</para> 
        /// </summary>
        public int DiscardPathRequestCount { get { return mSearchCancellationRequests.Count; } }

        /// <summary>
        /// Indicates whether or not this object is disposed.
        /// Once disposed, this object will not process any new
        /// requests.
        /// </summary>
        public Boolean IsDisposed { get { return mIsDisposed; } }

        /// <summary>
        /// The number of pending requests to keep paths in the cache.
        /// <para>This value will always be zero following a call
        /// to any of the Process operations.</para>
        /// </summary>
        public int KeepAliveRequestCount { get { return mKeepAliveRequests.Count; } }

        /// <summary>
        /// The Navigator to distribute to clients needing services from this object.
        /// </summary>
        public Navigator Nav { get { return mNavigator; } }

        /// <summary>
        /// The number of pending "nearest location" requests.
        /// <para>This value will always be zero following a call
        /// to any of the Process operations.</para>
        /// </summary>
        public int NearestLocationRequestCount { get { return mNearestLocationRequests.Count; } }

        /// <summary>
        /// The number of in-progress path searches. (Searches underway but not complete.)
        /// </summary>
        public int PathJobCount { get { return mAStarJobs.Count; } }

        /// <summary>
        /// The number of pending path requests.
        /// (Path searches have not begun.)
        /// <para>This value will always be zero following a call
        /// to any of the Process operations.</para>
        /// </summary>
        public int PathRequestCount { get { return mAStarRequests.Count; } }

        /// <summary>
        /// The size of the path search pool. (Searches staged for use.)
        /// </summary>
        public int PathSearchPoolSize { get { return mAStarPool.Count; } }

        /// <summary>
        /// The number of in-progress path repairs. (Repair in progress but not complete.)
        /// </summary>
        public int RepairJobCount { get { return mPathRepairJobs.Count; } }

        /// <summary>
        /// The Size of the repair job pool.  (Repair jobs staged for use.)
        /// </summary>
        public int RepairPoolSize { get { return mRepairPool.Count; } }

        /// <summary>
        /// The number of pending path repair requests.
        /// <para>This value will always be zero following a call
        /// to any of the Process operations.</para>
        /// </summary>
        public int RepairRequestCount { get { return mPathRepairRequests.Count; } }

        /// <summary>
        /// The number of pending "valid location" requests. 
        /// <para>This value will always be zero following a call
        /// to any of the Process operations.</para>
        /// </summary>
        public int ValidLocationRequestCount { get { return mValidLocationRequests.Count; } }

        /// <summary>
        /// The number of "nearest location" searches currently in progress.
        /// <para>This value will never be greater than zero since all "nearest location" requests
        /// are processed every process cycle.</para>
        /// <para>Exposed for unit testing purposes.</para>
        /// </summary>
        public int NearestLocationJobCount { get { return mNearestLocationJobs.Count; } }

        /// <summary>
        /// The number of "valid location" checks currently in progress.
        /// <para>This value will never be greater than zero since all "valid location" requests
        /// are processed every process cycle.</para>
        /// <para>Exposed for unit testing purposes.</para>
        /// </summary>
        public int ValidLocationJobCount { get { return mValidLocationJobs.Count; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mesh">The navigation mesh.</param>
        /// <param name="heuristic">The distance heuristic to use for A* searches.</param>
        /// <param name="maxUpdateTimeslice">The maximum time slice to use during 
        /// throttling operations. In 'ticks' as defined by DateTime.Ticks.
        /// See <see cref="Process(Boolean)">Process(Boolean)</see> for details.</param>
        /// <param name="maxPathAge">The maximum age that paths will be cached.  In milliseconds.
        /// See <see cref="MaxPathAge">MaxPathAge</see> for details.</param>
        /// <param name="repairSearchDepth">The maximum search depth to use when attempting to repair paths.
        /// Larger values will increase the cost of repairs.  A value between one and three is usually
        /// appropriate.</param>
        /// <param name="searchPoolMax">The maximum size allowed for the search pools.  Search pools
        /// allow re-use of search related objects.  Setting the value too high will waste memory.
        /// Setting the value too low will decrease performance.  Generally, the value is set
        /// to the average number of expected active searches.</param>
        public MasterNavigator(TriNavMesh mesh
                , DistanceHeuristicType heuristic
                , long maxUpdateTimeslice
                , int maxPathAge
                , int repairSearchDepth
                , int searchPoolMax)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");
            mMesh = mesh;
            this.mHeuristic = heuristic;
            mNavigator = new Navigator(this);

            if (maxUpdateTimeslice <= 0)
                this.mMaxProcessingTimeslice = 0;
            else
                this.mMaxProcessingTimeslice = Math.Max(1, maxUpdateTimeslice);
            
            this.mMaxPathAge = Math.Max(0, maxPathAge);
            this.mRepairSearchDepth = Math.Max(1, repairSearchDepth);
            
            // The minimum limit is for code simplification.
            this.mAStarPoolMax = Math.Max(1, searchPoolMax);
            
            if (this.mAStarPoolMax < 3)
                this.mRepairPoolMax = this.mAStarPoolMax;
            else
                this.mRepairPoolMax = this.mRepairPoolMax/2;
            
            if (this.mMaxPathAge > 0)
                mActivePaths = new List<MasterPath>();
            else
                // Setting it to null will break code.  And the code
                // is optimized for Max age > 0.
                mActivePaths = new List<MasterPath>(1);
            
            mAStarPool = new Queue<PathJob>(this.mAStarPoolMax);
            mRepairPool = new Queue<RepairJob>(this.mRepairPoolMax);
        }
        
        /// <summary>
        /// Disposes this object.  All pending requests
        /// are marked as <see cref="NavRequestState.Failed">Failed</see>.
        /// All cached paths are disposed.  No further requests will
        /// be processed.
        /// </summary>
        /// <remarks>
        /// In a multi-threaded environment, a single call to this operation
        /// will not guarantee that all pending requests are disposed.  If
        /// clients depend on being notified of request failures, then this
        /// operation should be called again after a short delay.
        /// </remarks>
        public void Dispose()
        {
            mIsDisposed = true;
            mKeepAliveRequests.Clear();
            mSearchCancellationRequests.Clear();
            lock (mAStarRequests)
            {
                while (mPathRepairRequests.Count > 0)
                {
                    mPathRepairRequests.Dequeue().request.State = NavRequestState.Failed;
                }
                 while (mValidLocationRequests.Count > 0)
                {
                    mValidLocationRequests.Dequeue().request.State =NavRequestState.Failed;
                }  
                while (mNearestLocationRequests.Count > 0)
                {
                    mNearestLocationRequests.Dequeue().request.State = NavRequestState.Failed;
                } 
                while (mAStarRequests.Count > 0)
                {
                    mAStarRequests.Dequeue().request.State = NavRequestState.Failed;
                }            
            }
            foreach (RepairJob job in mPathRepairJobs)
            {
                job.request.State = NavRequestState.Failed;
                if (job.search != null)
                    job.search.Reset();
            }
            mPathRepairJobs.Clear();
            while (mValidLocationJobs.Count > 0)
            {
                mValidLocationJobs.Dequeue().request.State = NavRequestState.Failed;
            }
            while (mNearestLocationJobs.Count > 0)
            {
                mNearestLocationJobs.Dequeue().request.State = NavRequestState.Failed;
            }
            foreach (PathJob job in mAStarJobs)
            {
                job.request.State = NavRequestState.Failed;
                job.Reset();
            }
            mAStarJobs.Clear();
            foreach (MasterPath path in mActivePaths)
            {
                path.Dispose();
            }
            mActivePaths.Clear();
            mAStarPool.Clear();
            mRepairPool.Clear();
        }

        /// <summary>
        /// Process pending requests and searches.  Throttling will occur based
        /// on the value of <see cref="MaxProcessingTimeslice">MaxProcessingTimeSlice</see>.
        /// </summary>
        /// <remarks>
        /// While the operation will attempt to hold its run time below the value
        /// of MaxProcessingTimeslice, there is no guarantee.  This is because:
        /// <ul>
        /// <li>Certain jobs are guaranteed to complete in one Process cycle.  So they
        /// will always be fully processed.</li>
        /// <li>A least one update cycle will be performed for all pending searches.</li>
        /// </ul>
        /// </remarks>
        /// <param name="includeMaintenance">If TRUE, path cache maintenance will be performed.
        /// Maintenance should be run at least once every <see cref="MaxPathAge">MaxPathAge</see> milliseconds. </param>
        /// <returns>The actual length of the operation. In ticks, as defined by DateTime.Ticks.</returns>
        public long Process(Boolean includeMaintenance)
        {

            if (mIsDisposed)
                return 0;
            if (mMaxProcessingTimeslice == long.MaxValue)
                return ProcessAll(includeMaintenance);
                
            long startTick = DateTime.Now.Ticks;

            ProcessAdmin(includeMaintenance);
            ProcessGuarenteedJobs();
            
            Boolean needsMorePathProcessing = false;
            Boolean needsMoreRepairProcessing = false;
            do
            {
                needsMorePathProcessing = ProcessAStarJobs();
                needsMoreRepairProcessing = ProcessPathRepairs();
            }
            while ((needsMorePathProcessing || needsMoreRepairProcessing) 
                    && DateTime.Now.Ticks - startTick < mMaxProcessingTimeslice);
            
            return DateTime.Now.Ticks - startTick;
        }

        /// <summary>
        /// Process all pending requests and searches until they are complete.
        /// No throttling occurs.
        /// </summary>
        /// <param name="includeMaintenance">If TRUE, path cache maintenance will be performed.
        /// Maintenance should be run at least once every <see cref="MaxPathAge">MaxPathAge</see> milliseconds. </param>
        /// <returns>The actual length of the operation. In ticks, as defined by DateTime.Ticks.</returns>
        public long ProcessAll(Boolean includeMaintenance)
        {
            if (mIsDisposed)
                return 0;
            long startTick = DateTime.Now.Ticks;
            ProcessAdmin(includeMaintenance);
            ProcessGuarenteedJobs();
            while (ProcessAStarJobs()) { }
            while (ProcessPathRepairs()) { }
            return (DateTime.Now.Ticks - startTick);
        }

        /// <summary>
        /// Process requests and one cycle of each in-progress search.
        /// The minimum possible time slice of processing will occur while sill
        /// advancing searches.
        /// <para>This is the most expensive version of the process operations.</para>
        /// </summary>
        /// <param name="includeMaintenance">If TRUE, path cache maintenance will be performed.
        /// Maintenance should be run at least once every <see cref="MaxPathAge">MaxPathAge</see> milliseconds.</param>
        public void ProcessOnce(Boolean includeMaintenance)
        {
            if (mIsDisposed)
                return;
            ProcessAdmin(includeMaintenance);
            ProcessGuarenteedJobs();
            ProcessAStarJobs();
            ProcessPathRepairs();
        }

        /// <summary>
        /// Perform periodic potentially costly maintenance operations.
        /// </summary>
        private void PerformMaintenance()
        {
            lock (mKeepAliveRequests) 
            {
                while (mKeepAliveRequests.Count > 0)
                {
                    int iPath = mKeepAliveRequests.Dequeue().Id;
                    foreach (MasterPath path in mActivePaths)
                    {
                        if (path.Id == iPath)
                            path.ResetTimestamp();
                    }
                }
            }
            long minAllowedTimestamp = DateTime.Now.Ticks - mMaxPathAge*10000;
            // Maintain paths.
            // Dev Note: Since the oldest paths are more likely to be at the
            // beginning of the list, this is not going to be efficient.
            for (int i = mActivePaths.Count - 1; i > -1; i--)
            {
                MasterPath path = mActivePaths[i];
                if (path.Timestamp < minAllowedTimestamp)
                {
                    path.Dispose();
                    mActivePaths.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// If there is room in the pool, add the job.
        /// Otherwise the job is discarded.
        /// </summary>
        /// <param name="job">The job to add to the pool.</param>
        private void PoolJob(PathJob job)
        {
            job.Reset();
            if (mAStarPool.Count < mAStarPoolMax)
            {
                lock (mAStarRequests) 
                {
                    mAStarPool.Enqueue(job);
                }
            }
        }

        /// <summary>
        /// If there is room in the pool, add the job.
        /// Otherwise the job is discarded.
        /// </summary>
        /// <param name="job">The job to add to the pool.</param>
        private void PoolJob(RepairJob job)
        {
            job.Reset();
            if (mRepairPool.Count < mRepairPoolMax)
            {
                lock (mAStarRequests) 
                {
                    mRepairPool.Enqueue(job);
                }
            }
        
        }

        /// <summary>
        /// Perform administrative tasks such as Request processing and maintenance.
        /// </summary>
        /// <param name="includeMaintenance">Include maintenance in the administrative tasks.</param>
        private void ProcessAdmin(Boolean includeMaintenance)
        {
            /*
             * Design notes:
             * 
             * Don't perform potentially costly tasks during the 
             * per-update administrative operations.
             */
            TransferRequests();
            ProcessCancellations();
            if (includeMaintenance)
                PerformMaintenance();
        }

        /// <summary>
        /// Returns TRUE if more processing is needed.  Otherwise FALSE.
        /// </summary>
        /// <returns>TRUE if more processing is needed.  Otherwise FALSE.</returns>
        private Boolean ProcessAStarJobs()
        {
            // Process A* searches
            for (int i = 0; i < mAStarJobs.Count;)
            {
                PathJob job = mAStarJobs[i];
                MasterNavRequest<Path> request = job.request;
                AStarSearch search = job.search;

                Vector3 trashV3;
                if (!job.searchIsReady)
                {
                    
                    // This is the first time we have seen this job.
                    // Need to Initialize the search.
                    
                    TriCell startCell = mMesh.GetClosestCell(job.startX, job.startY, job.startZ
                            , true
                            , out trashV3);
                    
                    if (startCell == null)
                        job.request.State = NavRequestState.Failed;
                    else
                    {
                        TriCell goalCell = mMesh.GetClosestCell(job.goalX, job.goalY, job.goalZ
                                , true
                                , out trashV3);
                        
                        if (goalCell == null)
                            job.request.State = NavRequestState.Failed;
                        else
                        {
                            job.InitializeSearch(startCell, goalCell);
                        }
                    }
                }
                
                if (request.State != NavRequestState.Processing)
                {
                    // Something has canceled or invalidated the search.
                    PoolJob(job);
                    mAStarJobs.RemoveAt(i);
                }
                else if (!search.IsActive)
                {
                    PoolJob(job);
                    mAStarJobs.RemoveAt(i);
                }
                else
                {
                    if (search.State == SearchState.Initialized)
                    {
                        // See if existing paths can be re-used.
                        MasterPath evalResult = search.Evaluate(mActivePaths);
                        if (evalResult != null) 
                        {
                            evalResult.ResetTimestamp();
                            request.Set(NavRequestState.Complete
                                    , evalResult.GetPath(search.GoalX, search.GoalY, search.GoalZ));
                            PoolJob(job);
                            mAStarJobs.RemoveAt(i);
                            break;
                        }
                    }
                    SearchState result = search.Process();
                    switch (result)
                    {
                    case SearchState.Complete:
                        MasterPath mpath = new MasterPath(mNextPathID++
                                , search.PathCells
                                , mMesh.PlaneTolerance
                                , mMesh.OffsetScale);
                        request.Set(NavRequestState.Complete
                                , mpath.GetPath(search.GoalX, search.GoalY, search.GoalZ));
                        if (mMaxPathAge > 0)
                            mActivePaths.Add(mpath);
                        PoolJob(job);
                        mAStarJobs.RemoveAt(i);
                        break;
                    case SearchState.Failed:
                        request.State = NavRequestState.Failed;
                        PoolJob(job);
                        mAStarJobs.RemoveAt(i);
                        break;
                    default:
                        i++;
                        break;
                    }
                }
            }
            return (mAStarJobs.Count == 0 ? false : true);
        }

        /// <summary>
        /// Process request cancellations.
        /// </summary>
        private void ProcessCancellations()
        {
            // Handle search cancellations.
            lock (mSearchCancellationRequests) 
            {
                while (mSearchCancellationRequests.Count > 0)
                {
                    MasterNavRequest<Path>.NavRequest requestToRemove = mSearchCancellationRequests.Dequeue();
                    foreach (PathJob job in mAStarJobs)
                    {
                        if (job.request.Request == requestToRemove)
                        {
                            job.request.State = NavRequestState.Failed;
                            job.Reset();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process jobs that are guaranteed to complete within one Process cycle.
        /// </summary>
        private void ProcessGuarenteedJobs()
        {
            while (mNearestLocationJobs.Count > 0)
            {
                VectorJob job = mNearestLocationJobs.Dequeue();
                mMesh.GetClosestCell(job.data.x
                        , job.data.y
                        , job.data.z
                        , false
                        , out job.data);
                job.request.Set(NavRequestState.Complete, job.data);
            }
            
            while (mValidLocationJobs.Count > 0)
            {
                ValidLocationJob job = mValidLocationJobs.Dequeue();
                float y = job.point.y;
                TriCell cell = mMesh.GetClosestCell(job.point.x
                        , job.point.y
                        , job.point.z
                        , true
                        , out job.point);
                if (cell == null)
                    job.request.Set(NavRequestState.Complete, false);
                else if (y < job.point.y - job.tolerance || y > job.point.y + job.tolerance)
                    job.request.Set(NavRequestState.Complete, false);
                else
                    job.request.Set(NavRequestState.Complete, true);
            }
        }

        /// <summary>
        /// Returns TRUE if more processing is needed.  Otherwise false.
        /// </summary>
        /// <returns>TRUE if more processing is needed.  Otherwise false.</returns>
        private Boolean ProcessPathRepairs()
        {
            Vector3 trashV3;
            for (int iJob = 0; iJob < mPathRepairJobs.Count;)
            {
                RepairJob job = mPathRepairJobs[iJob];
                MasterNavRequest<Path> request = job.request;
                // Handle new jobs.
                DijkstraSearch search;
                if (job.search.State == SearchState.Uninitialized)
                {
                    MasterPath path = null;
                    foreach (MasterPath p in mActivePaths)
                    {
                        if (p.Id == job.path.Id)
                        {
                            path = p;
                            break;
                        }
                    }
                    if (path == null)
                    {
                        // No path to repair.
                        request.State = NavRequestState.Failed;
                        PoolJob(job);
                        mPathRepairJobs.RemoveAt(iJob);
                        break;
                    }
                    TriCell startCell = mMesh.GetClosestCell(job.startX
                            , job.startY
                            , job.startZ
                            , true, out trashV3);
                    if (startCell == null)
                    {
                        // Not a valid start point.
                        request.State = NavRequestState.Failed;
                        PoolJob(job);
                        mPathRepairJobs.RemoveAt(iJob);
                        break;
                    }
                    job.InitializeSearch(path, startCell);
                    search = job.search;
                }
                else
                    search = job.search;
                
                if (request.State != NavRequestState.Processing)
                {
                    // Something has canceled or invalidated the search.
                    PoolJob(job);
                    mPathRepairJobs.RemoveAt(iJob);                
                }
                else if (!search.IsActive)
                {
                    PoolJob(job);
                    mPathRepairJobs.RemoveAt(iJob);
                }
                else
                {
                    SearchState state = search.Process();
                    switch (state)
                    {
                    case SearchState.Complete:
                        // Select a path from the search results.
                        // Looking for the path that ends at far along the
                        // original path as possible.
                        MasterPath sourcePath = job.source;
                        TriCell[] selectedCells = search.GetPathCells(0);
                        int maxGoalIndex = sourcePath.GetCellIndex(selectedCells[selectedCells.Length-1]);
                        for (int i = 1; i < search.PathCount; i++)
                        {
                            TriCell[] cells = search.GetPathCells(i);
                            int goalIndex = sourcePath.GetCellIndex(
                                    cells[cells.Length-1]);
                            if (goalIndex > maxGoalIndex)
                            {
                                maxGoalIndex = goalIndex;
                                selectedCells = cells;
                            }
                        }
                        // Merge the two paths to form a new path.
                        // New beginning of path...
                        TriCell[] mergedCells = new TriCell[selectedCells.Length 
                                                            + sourcePath.Size 
                                                            - maxGoalIndex - 1];
                        Array.Copy(selectedCells, 0, mergedCells, 0, selectedCells.Length);
                        if (mergedCells[mergedCells.Length-1] != sourcePath.GoalCell)
                        {
                            // ...old end of path.
                            for (int iSource = maxGoalIndex + 1, iTarget = selectedCells.Length
                                    ; iSource < sourcePath.Size
                                    ; iSource++, iTarget++)
                                mergedCells[iTarget] = sourcePath.GetCell(iSource);                    
                        }
                        MasterPath mergedPath = new MasterPath(mNextPathID++
                                , mergedCells
                                , mMesh.PlaneTolerance
                                , mMesh.OffsetScale);
                        request.Set(NavRequestState.Complete
                                , mergedPath.GetPath(job.path.GoalX
                                        , job.path.GoalY
                                        , job.path.GoalZ));
                        if (mMaxPathAge > 0)
                            mActivePaths.Add(mergedPath);
                        PoolJob(job);
                        mPathRepairJobs.RemoveAt(iJob);
                        break;
                    case SearchState.Failed:
                        request.State = NavRequestState.Failed;
                        PoolJob(job);
                        mPathRepairJobs.RemoveAt(iJob);
                        break;
                    default:
                        iJob++;
                        break;
                    }
                }
            }
            return (mPathRepairJobs.Count == 0 ? false : true);
        }

        /// <summary>
        /// Transfer requests to the job queues.
        /// </summary>
        private void TransferRequests()
        {
            // Transfer new path search requests.
            lock (mAStarRequests) 
            {
                while (mAStarRequests.Count > 0)
                {
                    mAStarJobs.Add(mAStarRequests.Dequeue());
                }
                
                 while (mPathRepairRequests.Count > 0)
                {
                    mPathRepairJobs.Add(mPathRepairRequests.Dequeue());
                }           
                
                 while (mNearestLocationRequests.Count > 0)
                {
                    mNearestLocationJobs.Enqueue(mNearestLocationRequests.Dequeue());
                }           
                
                while (mValidLocationRequests.Count > 0)
                {
                    mValidLocationJobs.Enqueue(mValidLocationRequests.Dequeue());
                }             
            }
        }
        
    }
}

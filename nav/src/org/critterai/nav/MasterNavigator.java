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
package org.critterai.nav;

import java.util.ArrayDeque;
import java.util.ArrayList;

import org.critterai.math.Vector3;
import org.critterai.nav.MasterPath.Path;

/**
 * Provides a means of performing path finding queries and managing paths related to
 * a triangle-based navigation mesh.
 * <p>Primary use case:</p>
 * <ol>
 * <li>The {@literal MasterNavigator} is constructed using the {@link NavUtil} class
 * and assigned to an owner thread to manage processing.</li>
 * <li>Clients are provided with {@link Navigator} references via 
 * {@link #navigator()} as needed.</li>
 * <li>Clients make navigation requests via their {@literal Navigator} reference.</li>
 * <li>The owner thread uses one of the processing operations to process the requests.
 * E.g. {@link #process(boolean)}.</li>
 * </ol>
 * <p>Primary Features:<p>
 * <ul>
 * <li>Can be run in a multi-threaded environment by obtaining a {@link ThreadedNavigator} from
 * {@link NavUtil}.</li>
 * <li>Processing load can be throttled to spread the cost of navigation requests
 * across multiple frames to prevent spikes.</li>
 * <li>Provides caching and re-use of paths.</li>
 * </ul>
 * <p>Instances of this class are not thread-safe but can be used by multiple threads if:</p>
 * <ul>
 * <li>The instance of this class is created using {@link NavUtil}.
 * <li>Only a single (owner) thread holds a reference to the instance of the class.</li>
 * <li>All other threads interact with the instance through a {@link Navigator} reference
 * obtained via {@link #navigator()}.</li> 
 * </ul>
 */
public final class MasterNavigator
{

	/*
	 * Design Notes:
	 * 
	 * Design performance is tuned for minimal
	 * client impact at the expense of navigator
	 * thread.
	 * Only one request type will
	 * be locked at a time by the navigator thread.
	 * For example: If the navigator thread is
	 * emptying the repair request queue, clients
	 * can continue requesting new paths,
	 * keep alive requests, etc.
	 * The down side is that the the navigator
	 * thread must perform 6 locks per call to
	 * the process operations.  This makes the
	 * process once operation the most expensive
	 * process operation.
	 * 
	 * Potential Optimizations:
	 * 
	 * For Path Repair
	 * 
	 * Before starting the the path repair, find the cell
	 * for the new location and check if it is directly
	 * connected to any cells in the existing path.  
	 * If so, consider search complete.
	 * (Select cell with highest index if there is more than one.) 
	 * 
	 * This optimization is based on the expectation that
	 * most repairs are requested when only straying slightly off
	 * the path.
	 */
	
	/**
	 * Provides a means of making navigation requests to a {@link MasterNavigator}.
	 * <p>Primary use case:</p>
	 * <ol>
	 * <li>Client obtains a reference from a {@literal MasterNavigator} via 
	 * the {@link MasterNavigator#navigator()} operation.</li>
	 * <li>Client monitors validity of its {@literal Navigator} via the 
	 * {@link Navigator#isDisposed()} operation.</li>
	 * <li>Client makes navigation requests.</li>
	 * <li>Client monitors pending requests for completion.</li>
	 * <li>When request is complete, client checks the request state.  
	 * If the request completed satisfactorily, client retrieves the data from the request.</li>
	 * </ol>
	 * <p>All operations are thread-safe as long as the {@literal MasterNavigator}
	 * is handling in a thread-safe manner.</p>
	 */
	public class Navigator 
		implements INavigator
	{
		
		private Navigator() { }
		
		/**
		 * {@inheritDoc}
		 */
		public void discardPathRequest(MasterNavRequest<Path>.NavRequest request)
		{
			if (mIsDisposed || request == null)
				return;
			synchronized (mSearchCancellationRequests) 
			{
				mSearchCancellationRequests.add(request);
			}
		}

		/**
		 * {@inheritDoc}
		 */
		public MasterNavRequest<Vector3>.NavRequest getNearestValidLocation(float x, float y ,float z)
		{
			if (mIsDisposed)
				return new MasterNavRequest<Vector3>(NavRequestState.FAILED).request();
			VectorJob job = new VectorJob(new Vector3(x, y, z));
			synchronized (mNearestLocationRequests) 
			{
				mNearestLocationRequests.add(job);
			}
			return job.request.request();
		}

		/**
		 * {@inheritDoc}
		 */
		public MasterNavRequest<Path>.NavRequest getPath(float startX, float startY, float startZ,
				float goalX, float goalY, float goalZ) 
		{
			
			if (mIsDisposed)
				return mFailedPath.request();
			PathJob job = null;
			if (mAStarPool.size() == 0)
				job = new PathJob(startX, startY, startZ
						, goalX, goalY, goalZ);
			synchronized (mAStarRequests) 
			{
				if (job == null)
				{
					job = mAStarPool.pollLast();
					job.initialize(startX, startY, startZ
							, goalX, goalY, goalZ);
				}
				mAStarRequests.add(job);
			}
			return job.request.request();
		}

		/**
		 * {@inheritDoc}
		 */
		public boolean isDisposed() { return mIsDisposed; }

		/**
		 * {@inheritDoc}
		 */
		public MasterNavRequest<Boolean>.NavRequest isValidLocation(float x, float y, float z
				, float yTolerance) 
		{
			if (mIsDisposed)
				return new MasterNavRequest<Boolean>(NavRequestState.FAILED, false).request();
			yTolerance = Math.max(0, yTolerance);
			ValidLocationJob job = new ValidLocationJob(x, y, z, yTolerance);
			synchronized (mValidLocationRequests) 
			{
				mValidLocationRequests.add(job);
			}
			return job.request.request();
		}

		/**
		 * {@inheritDoc}
		 */
		public void keepPathAlive(Path path)
		{
			if (mIsDisposed || path == null || path.isDisposed())
				return;
			synchronized (mKeepAliveRequests) 
			{
				mKeepAliveRequests.add(path);
			}
		}
		
		/**
		 * {@inheritDoc}
		 */
		public MasterNavRequest<Path>.NavRequest repairPath(float startX, float startY, float startZ,
				Path path)
		{
			if (mIsDisposed || path == null || path.isDisposed())
				return mFailedPath.request();
			RepairJob job = null;
			if (mRepairPool.size() == 0)
				job = new RepairJob(startX, startY, startZ, path);
			synchronized (mPathRepairRequests) 
			{
				if (job == null)
				{
					job = mRepairPool.pollLast();
					job.initialize(startX, startY, startZ, path);
				}
				mPathRepairRequests.add(job);
			}
			return job.request.request();
		}
		
	}
	
	/**
	 * A re-usable A* search job.
	 */
	private class PathJob
	{
		public MasterNavRequest<Path> request = null;
		public final AStarSearch search = new AStarSearch(heuristic);
		public boolean searchIsReady = false;		
		public float startX;
		public float startY;
		public float startZ;	
		public float goalX;
		public float goalY;
		public float goalZ;
		public PathJob(float startX, float startY, float startZ
				, float goalX, float goalY, float goalZ)
		{
			initialize(startX, startY, startZ, goalX, goalY, goalZ);
		}
		public void initialize(float startX, float startY, float startZ
				, float goalX, float goalY, float goalZ)
		{
			// Dev Note: The request is re-initialized during the reset.
			reset();
			this.startX = startX;
			this.startY = startY;
			this.startZ = startZ;
			this.goalX = goalX;
			this.goalY = goalY;
			this.goalZ = goalZ;
		}
		public void initializeSearch(TriCell startCell, TriCell goalCell)
		{
			search.initialize(startX, startY, startZ
					, goalX, goalY, goalZ
					, startCell
					, goalCell);
			searchIsReady = true;
		}
		public void reset()
		{
			request = new MasterNavRequest<Path>();
			startX = 0;
			startY = 0;
			startZ = 0;
			goalX = 0;
			goalY = 0;
			goalZ = 0;
			search.reset();
			searchIsReady = false;
		}

	}
	
	/**
	 * A re-usable path repair job.
	 */
	private class RepairJob
	{
		public Path path;
		public MasterNavRequest<Path> request = null;
		public final DijkstraSearch search = new DijkstraSearch();
		public MasterPath source = null;
		public float startX;
		public float startY;
		public float startZ;		
		public RepairJob(float startX, float startY, float startZ, Path path)
		{
			initialize(startX, startY, startZ, path);
		}
		public void initialize(float startX, float startY, float startZ, Path path)
		{
			reset();
			this.path = path;
			this.startX = startX;
			this.startY = startY;
			this.startZ = startZ;
			request = new MasterNavRequest<Path>();
		}
		public void initializeSearch(MasterPath source, TriCell startCell)
		{
			this.source = source;
			TriCell[] goalCells = source.getRawCopy(new TriCell[source.size()]);
			search.initialize(startX, startY, startZ, startCell, goalCells, 2, false);
		}
		public void reset()
		{
			request = null;
			path = null;
			startX = 0;
			startY = 0;
			startZ = 0;
			source = null;
			search.reset();
		}
	}
	
	/**
	 * A job used for {@link Navigator#isValidLocation(float, float, float, float)}
	 * requests.
	 */
	private class ValidLocationJob
	{
		public final Vector3 point;;
		public final MasterNavRequest<Boolean> request = new MasterNavRequest<Boolean>();
		public final float tolerance;
		public ValidLocationJob(float x, float y, float z, float tolerance)
		{
			point = new Vector3(x, y, z);
			this.tolerance = tolerance;
		}
	}
	
	/**
	 * A job used for requests involving {@link Vector3} objects.
	 */
	private class VectorJob
	{
		public final Vector3 data;
		public final MasterNavRequest<Vector3> request = new MasterNavRequest<Vector3>();
		public VectorJob(Vector3 data)
		{
			this.data = data;
		}
	}
	
	/**
	 * The heuristic to use for A* searches.
	 */
	public final DistanceHeuristicType heuristic;
	
	/**
	 * The maximum age that paths will be cached.  (milliseconds)
	 * When a path expires, it is disposed and removed from the cache.
	 * <p>Paths will expire, even if still in use by clients, unless the client uses
	 * the {@link Navigator#keepPathAlive(Path)} operation to reset the path's age.<p>
	 * <p>If set to zero, no path caching will occur, paths will never be disposed,
	 * path sharing will never occur, and path repair will not be possible</p> 
	 */
	public final int maxPathAge;

	/**
	 * The maximum time slice to use during throttling operations. (nanoseconds)
	 * See {@link #process(boolean)} for details.
	 */
	public final long maxProcessingTimeslice;

	/**
	 * The maximum search depth to use when attempting to repair paths.
	 * @see Navigator#repairPath(float, float, float, Path)
	 */
	public final int repairSearchDepth;

	/**
	 * The cached pathes.
	 */
	private final ArrayList<MasterPath> mActivePaths;
	
	/**
	 * The in-progress A* searches.
	 */
	private final ArrayList<PathJob> mAStarJobs  = new ArrayList<PathJob>();
	
	
	/**
	 * The A* search pool. (Searches available for new requests.)
	 * <p>IMPORTANT: Access to this object must be synchronized via a lock
	 * on {@link #mAStarRequests}.</p>
	 */
	private final ArrayDeque<PathJob> mAStarPool;
	
	/**
	 * The maximum pool size for A* searches.
	 */
	private final int mAStarPoolMax;
	
	/**
	 * The request queue for new A* searches.
	 * <p>IMPORTANT: All access to this object must be synchronized.</p>
	 */
	private final ArrayDeque<PathJob> mAStarRequests = new ArrayDeque<PathJob>();

	private final MasterNavRequest<Path> mFailedPath 
			= new MasterNavRequest<Path>(NavRequestState.FAILED);
	
	private boolean mIsDisposed = false;
	
	/**
	 * A queue for requests to keep a path in the cache beyond its normal
	 * expiration.
	 * <p>IMPORTANT: All access to this object must be synchronized.</p>
	 */
	private final ArrayDeque<Path> mKeepAliveRequests = new ArrayDeque<Path>();
	
	/**
	 * The navigation mesh.
	 */
	private final TriNavMesh mMesh;
	
	private final Navigator mNavigator = this.new Navigator();
	
	/**
	 * All active "nearest location" jobs.
	 */
	private final ArrayDeque<VectorJob> mNearestLocationJobs  = new ArrayDeque<VectorJob>();
	
	/**
	 * Pending "nearest location" requests.
	 * <p>IMPORTANT: All access to this object must be synchronized.</p>
	 */
	private final ArrayDeque<VectorJob> mNearestLocationRequests  = new ArrayDeque<VectorJob>();

	/**
	 * The path ID to assign to the next path.
	 */
	private int mNextPathID = 0;
	
	/**
	 * All in-progress path repair jobs.
	 */
	private final ArrayList<RepairJob> mPathRepairJobs = new ArrayList<RepairJob>();
	
	/**
	 * Pending path repair requests.
	 * <p>IMPORTANT: All access to this object must be synchronized.</p>
	 */
	private final ArrayDeque<RepairJob> mPathRepairRequests = new ArrayDeque<RepairJob>();
	/**
	 * The repair search pool.  (Repair jobs available for new requests.)
	 * <p>IMPORTANT: Access to this object must be synchronized via a lock
	 * on {@link #mPathRepairRequests}.</p>
	 */
	private final ArrayDeque<RepairJob> mRepairPool;
	
	/**
	 * The maximum pool size for repair searches.
	 */
	private final int mRepairPoolMax;
	
	/**
	 * A queue holding all cancellation requests.
	 * <p>IMPORTANT: All access to this object must be synchronized.</p>
	 */
	private final ArrayDeque<MasterNavRequest<Path>.NavRequest> mSearchCancellationRequests 
			= new ArrayDeque<MasterNavRequest<Path>.NavRequest>();

	/**
	 * All active "valid location" jobs.
	 */
	private final ArrayDeque<ValidLocationJob> mValidLocationJobs 
			= new ArrayDeque<ValidLocationJob>();

	/**
	 * Pending "valid location" requests.
	 * <p>IMPORTANT: All access to this object must be synchronized.</p>
	 */
	private final ArrayDeque<ValidLocationJob> mValidLocationRequests 
			= new ArrayDeque<ValidLocationJob>();
	
	/**
	 * Constructor
	 * @param mesh The navigation mesh.
	 * @param heuristic The distance heuristic to use for A* searches.
	 * @param maxUpdateTimeslice The maximum time slice to use during 
	 * throttling operations. In nanoseconds.
	 * See {@link #process(boolean)} for details.
	 * @param maxPathAge The maximum age that paths will be cached.  In milliseconds.
	 * See {@link #maxPathAge} for details.
	 * @param repairSearchDepth The maximum search depth to use when attempting to repair paths.
	 * Larger values will increase the cost of repairs.  A value between one and three is usually
	 * appropriate. 
	 * @param searchPoolMax The maximum size allowed for the search pools.  Search pools
	 * allow re-use of search related objects.  Setting the value too high will waste memory.
	 * Setting the value too low will decrease performance.  Generally, the value is set
	 * to the average number of expected active searches.
	 * @throws IllegalArgumentException If any of the arguments are invalid.
	 */
	public MasterNavigator(TriNavMesh mesh
			, DistanceHeuristicType heuristic
			, long maxUpdateTimeslice
			, int maxPathAge
			, int repairSearchDepth
			, int searchPoolMax)
		throws IllegalArgumentException
	{
		if (mesh == null || heuristic == null)
			throw new IllegalArgumentException("One or more arguments is null.");
		mMesh = mesh;
		this.heuristic = heuristic;
		
		if (maxUpdateTimeslice <= 0)
			this.maxProcessingTimeslice = 0;
		else
			this.maxProcessingTimeslice = Math.min(Math.max(10, maxUpdateTimeslice), 9000000000l);
		
		this.maxPathAge = Math.max(0, maxPathAge);
		this.repairSearchDepth = Math.max(1, repairSearchDepth);
		
		// The minimum limit is for code simplification.
		this.mAStarPoolMax = Math.max(1, searchPoolMax);
		
		if (this.mAStarPoolMax < 3)
			this.mRepairPoolMax = this.mAStarPoolMax;
		else
			this.mRepairPoolMax = this.mRepairPoolMax/2;
		
		if (this.maxPathAge > 0)
			mActivePaths = new ArrayList<MasterPath>();
		else
			// Setting it to null will break code.  And the code
			// is optimized for max age > 0.
			mActivePaths = new ArrayList<MasterPath>(1);
		
		mAStarPool = new ArrayDeque<PathJob>(this.mAStarPoolMax);
		mRepairPool = new ArrayDeque<RepairJob>(this.mRepairPoolMax);
	}

	/**
	 * The number of paths in the path cache.
	 * Will always be zero if {@link #maxPathAge} is zero.
	 * @return The number of paths in the path cache.
	 */
	public int activePathCount() { return mActivePaths.size(); }

	/**
	 * The number of pending requests to discard path searches.
	 * <p>This value will always be zero following a call
	 * to any of the process operations.</p> 
	 * @return The number of pending requests to discard path searches.
	 */
	public int discardPathRequestCount() { return mSearchCancellationRequests.size(); }
	
	/**
	 * Disposes this object.  All pending requests
	 * are marked as {@link NavRequestState#FAILED}.
	 * All cached paths are disposed.  No further requests will
	 * be processed.
	 * <p>In a multi-threaded environment, a single call to this operation
	 * will not guarantee that all pending requests are disposed.  If
	 * clients depend on being notified of request failures, then this
	 * operation should be called again after a short delay.</p>
	 */
	public void dispose()
	{
		mIsDisposed = true;
		mKeepAliveRequests.clear();
		mSearchCancellationRequests.clear();
		synchronized (mPathRepairRequests)
		{
			while (mPathRepairRequests.size() > 0)
			{
				mPathRepairRequests.poll().request.setState(NavRequestState.FAILED);
			}
		}
		for (RepairJob job : mPathRepairJobs)
		{
			job.request.setState(NavRequestState.FAILED);
			if (job.search != null)
				job.search.reset();
		}
		mPathRepairJobs.clear();
		synchronized (mValidLocationRequests)
		{
			while (mValidLocationRequests.size() > 0)
			{
				mValidLocationRequests.poll().request.setState(NavRequestState.FAILED);
			}
		}
		while (mValidLocationJobs.size() > 0)
		{
			mValidLocationJobs.poll().request.setState(NavRequestState.FAILED);
		}
		synchronized (mNearestLocationRequests)
		{
			while (mNearestLocationRequests.size() > 0)
			{
				mNearestLocationRequests.poll().request.setState(NavRequestState.FAILED);
			}
		}
		while (mNearestLocationJobs.size() > 0)
		{
			mNearestLocationJobs.poll().request.setState(NavRequestState.FAILED);
		}
		synchronized (mAStarRequests)
		{
			while (mAStarRequests.size() > 0)
			{
				mAStarRequests.poll().request.setState(NavRequestState.FAILED);
			}
		}
		for (PathJob job : mAStarJobs)
		{
			job.request.setState(NavRequestState.FAILED);
			job.reset();
		}
		mAStarJobs.clear();
		for (MasterPath path : mActivePaths)
		{
			path.dispose();
		}
		mActivePaths.clear();
		mAStarPool.clear();
		mRepairPool.clear();
	}

	/**
	 * Indicates whether or not this object is disposed.
	 * Once disposed, this object will not process any new
	 * requests.
	 * @return TRUE if this object is disposed.  Otherwise FALSE.
	 */
	public boolean isDisposed() { return mIsDisposed; }
	
	/**
	 * The number of pending requests to keep paths in the cache.
	 * <p>This value will always be zero following a call
	 * to any of the process operations.</p>
	 * @return The number of pending requests to keep paths in the cache.
	 */
	public int keepAliveRequestCount() { return mKeepAliveRequests.size(); }

	/**
	 * The navigator to distribute to clients needing services from this object.
	 * @return A navigator for clients needing services.
	 */
	public Navigator navigator() { return mNavigator; }

	/**
	 * The number of pending "nearest location" requests.
	 * <p>This value will always be zero following a call
	 * to any of the process operations.</p>
	 * @return
	 */
	public int nearestLocationRequestCount() { return mNearestLocationRequests.size(); }
	
	/**
	 * The number of in-progress path searches. (Searches underway but not complete.)
	 * @return The number of in-progress path searches.
	 */
	public int pathJobCount() { return mAStarJobs.size(); }
	
	/**
	 * The number of pending path requests.
	 * (Path searches have not begun.)
	 * <p>This value will always be zero following a call
	 * to any of the process operations.</p>
	 * @return
	 */
	public int pathRequestCount() { return mAStarRequests.size(); }
	
	/**
	 * The size of the path search pool. (Searches staged for use.)
	 * @return The size of the path search pool.
	 */
	public int pathSearchPoolSize() { return mAStarPool.size(); }

	/**
	 * Process pending requests and searches.  Throttling will occur based
	 * on the value of {@link #maxProcessingTimeslice}.
	 * <p>While the operation will attempt to hold its run time below the value
	 * of {@literal #maxProcessingTimeslice}, there is no guarantee.  This is because:</p>
	 * <ul>
	 * <li>Certain jobs are guaranteed to complete in one process cycle.  So they
	 * will always be fully processed.</li>
	 * <li>A least one update cycle will be performed for all pending searches.</li>
	 * </ul>
	 * @param includeMaintenance If TRUE, path cache maintenance will be performed.
	 * Maintenance should be run at least once every {@link #maxPathAge} milliseconds. 
	 * @return The actual length of the operation. In nanoseconds.
	 */
	public long process(boolean includeMaintenance)
	{

		if (mIsDisposed)
			return 0;
		if (maxProcessingTimeslice == Long.MAX_VALUE)
			return processAll(includeMaintenance);
			
		final long startTick = System.nanoTime();
		
		processAdmin(includeMaintenance);
		processGuarenteedJobs();
		
		boolean needsMorePathProcessing = false;
		boolean needsMoreRepairProcessing = false;
		do
		{
			needsMorePathProcessing = processAStarJobs();
			needsMoreRepairProcessing = processPathRepairs();
		}
		while ((needsMorePathProcessing || needsMoreRepairProcessing) 
				&& System.nanoTime() - startTick < maxProcessingTimeslice);
		
		return System.nanoTime() - startTick;
	}

	/**
	 * Process all pending requests and searches until they are complete.
	 * No throttling occurs.
	 * @param includeMaintenance If TRUE, path cache maintenance will be performed.
	 * Maintenance should be run at least once every {@link #maxPathAge} milliseconds. 
	 * @return The length of the operation. In nanoseconds.
	 */
	public long processAll(boolean includeMaintenance)
	{
		if (mIsDisposed)
			return 0;
		final long startTick = System.nanoTime();
		processAdmin(includeMaintenance);
		processGuarenteedJobs();
		while (processAStarJobs()) { }
		while (processPathRepairs()) { }
		return System.nanoTime() - startTick;
	}

	/**
	 * Process requests and one cycle of each in-progress search.
	 * The minimum possible time slice of processing will occur while sill
	 * advancing in-progress searches.
	 * <p>This is the most expensive of the process operations.</p>
	 * @param includeMaintenance If TRUE, path cache maintenance will be performed.
	 * Maintenance should be run at least once every {@link #maxPathAge} milliseconds.
	 */
	public void processOnce(boolean includeMaintenance)
	{
		if (mIsDisposed)
			return;
		processAdmin(includeMaintenance);
		processGuarenteedJobs();
		processAStarJobs();
		processPathRepairs();
	}

	/**
	 * The number of in-progress path repairs. (Repair in progress but not complete.)
	 * @return The number of in-progress path repairs.
	 */
	public int repairJobCount() { return mPathRepairJobs.size(); }

	/**
	 * The size of the repair job pool.  (Repair jobs staged for use.)
	 * @return The size of the repair job pool.
	 */
	public int repairPoolSize() { return mRepairPool.size(); }

	/**
	 * The number of pending path repair requests.
	 * <p>This value will always be zero following a call
	 * to any of the process operations.</p>
	 * @return The number of pending path repair requests.
	 */
	public int repairRequestCount() { return mPathRepairRequests.size(); }

	/**
	 * The number of pending "valid location" requests. 
	 * <p>This value will always be zero following a call
	 * to any of the process operations.</p>
	 * @return The number of pending "valid location" requests. 
	 */
	public int validLocationRequestCount() { return mValidLocationRequests.size(); }

	/**
	 * Perform periodic potentially costly maintenance operations.
	 */
	private void performMaintenance()
	{
		synchronized (mKeepAliveRequests) 
		{
			while (mKeepAliveRequests.size() > 0)
			{
				int iPath = mKeepAliveRequests.poll().id();
				for (MasterPath path : mActivePaths)
				{
					if (path.id == iPath)
						path.resetTimestamp();
				}
			}
		}
		long minAllowedTimestamp = System.currentTimeMillis() - maxPathAge;
		// Maintain paths.
		// Dev Note: Since the oldest paths are more likely to be at the
		// beginning of the list, this is not going to be efficient.
		for (int i = mActivePaths.size() - 1; i > -1; i--)
		{
			MasterPath path = mActivePaths.get(i);
			if (path.timestamp() < minAllowedTimestamp)
			{
				path.dispose();
				mActivePaths.remove(i);
			}
		}
	}

	/**
	 * If there is room in the pool, add the job.
	 * Otherwise the job is discarded.
	 * @param job The job to add to the pool.
	 */
	private void poolJob(PathJob job)
	{
		job.reset();
		if (mAStarPool.size() < mAStarPoolMax)
		{
			synchronized (mAStarRequests) 
			{
				mAStarPool.add(job);
			}
		}
	}

	/**
	 * If there is room in the pool, add the job.
	 * Otherwise the job is discarded.
	 * @param job The job to add to the pool.
	 */
	private void poolJob(RepairJob job)
	{
		job.reset();
		if (mRepairPool.size() < mRepairPoolMax)
		{
			synchronized (mPathRepairRequests) 
			{
				mRepairPool.add(job);
			}
		}
	
	}

	/**
	 * Perform administrative tasks such as request processing and maintenance.
	 * @param includeMaintenance Include maintenance in the administrative tasks.
	 */
	private void processAdmin(boolean includeMaintenance)
	{
		/*
		 * Design notes:
		 * 
		 * Don't perform potentially costly tasks during the 
		 * per-update administrative operations.
		 */
		transferRequests();
		processCancellations();
		if (includeMaintenance)
			performMaintenance();
	}

	/**
	 * Returns TRUE if more processing is needed.  Otherwise FALSE.
	 * @return TRUE if more processing is needed.  Otherwise FALSE.
	 */
	private boolean processAStarJobs()
	{
		// Process A* searches
		for (int i = 0; i < mAStarJobs.size();)
		{
			PathJob job = mAStarJobs.get(i);
			MasterNavRequest<Path> request = job.request;
			AStarSearch search = job.search;
	
			if (!job.searchIsReady)
			{
				
				// This is the first time we have seen this job.
				// Need to initialize the search.
				
				TriCell startCell = mMesh.getClosestCell(job.startX, job.startY, job.startZ
						, true
						, null);
				
				if (startCell == null)
					job.request.setState(NavRequestState.FAILED);
				else
				{
					TriCell goalCell = mMesh.getClosestCell(job.goalX, job.goalY, job.goalZ
							, true
							, null);
					
					if (goalCell == null)
						job.request.setState(NavRequestState.FAILED);
					else
					{
						job.initializeSearch(startCell, goalCell);
					}
				}
			}
			
			if (request.state() != NavRequestState.PROCESSING)
			{
				// Something has canceled or invalidated the search.
				poolJob(job);
				mAStarJobs.remove(i);
			}
			else if (!search.isActive())
			{
				poolJob(job);
				mAStarJobs.remove(i);
			}
			else
			{
				if (search.state() == SearchState.INITIALIZED)
				{
					// See if existing paths can be re-used.
					MasterPath evalResult = search.evaluate(mActivePaths);
					if (evalResult != null) 
					{
						evalResult.resetTimestamp();
						request.set(NavRequestState.COMPLETE
								, evalResult.getPath(search.goalX(), search.goalY(), search.goalZ()));
						poolJob(job);
						mAStarJobs.remove(i);
						break;
					}
				}
				SearchState result = search.process();
				switch (result)
				{
				case COMPLETE:
					MasterPath mpath = new MasterPath(mNextPathID++
							, search.pathCells()
							, mMesh.planeTolerance()
							, mMesh.offsetScale());
					request.set(NavRequestState.COMPLETE
							, mpath.getPath(search.goalX(), search.goalY(), search.goalZ()));
					if (maxPathAge > 0)
						mActivePaths.add(mpath);
					poolJob(job);
					mAStarJobs.remove(i);
					break;
				case FAILED:
					request.setState(NavRequestState.FAILED);
					poolJob(job);
					mAStarJobs.remove(i);
					break;
				default:
					i++;
				}
			}
		}
		return (mAStarJobs.size() == 0 ? false : true);
	}

	/**
	 * Process request cancellations.
	 */
	private void processCancellations()
	{
		// Handle search cancellations.
		synchronized (mSearchCancellationRequests) 
		{
			while (mSearchCancellationRequests.size() > 0)
			{
				MasterNavRequest<Path>.NavRequest requestToRemove = mSearchCancellationRequests.poll();
				for (PathJob job : mAStarJobs)
				{
					if (job.request.request() == requestToRemove)
					{
						job.request.setState(NavRequestState.FAILED);
						job.reset();
						break;
					}
				}
			}
		}
	}

	/**
	 * Process jobs that are guaranteed to complete within one process cycle.
	 */
	private void processGuarenteedJobs()
	{
		while (mNearestLocationJobs.size() > 0)
		{
			VectorJob job = mNearestLocationJobs.poll();
			mMesh.getClosestCell(job.data.x
					, job.data.y
					, job.data.z
					, false
					, job.data);
			job.request.set(NavRequestState.COMPLETE, job.data);
		}
		
		while (mValidLocationJobs.size() > 0)
		{
			ValidLocationJob job = mValidLocationJobs.poll();
			float y = job.point.y;
			TriCell cell = mMesh.getClosestCell(job.point.x
					, job.point.y
					, job.point.z
					, true
					, job.point);
			if (cell == null)
				job.request.set(NavRequestState.COMPLETE, false);
			else if (y < job.point.y - job.tolerance || y > job.point.y + job.tolerance)
				job.request.set(NavRequestState.COMPLETE, false);
			else
				job.request.set(NavRequestState.COMPLETE, true);
		}
	}

	/**
	 * Returns TRUE if more processing is needed.  Otherwise false.
	 * @return TRUE if more processing is needed.  Otherwise false.
	 */
	private boolean processPathRepairs()
	{
		
		for (int iJob = 0; iJob < mPathRepairJobs.size();)
		{
			RepairJob job = mPathRepairJobs.get(iJob);
			MasterNavRequest<Path> request = job.request;
			// Handle new jobs.
			DijkstraSearch search;
			if (job.search.state() == SearchState.UNINITIALIZED)
			{
				MasterPath path = null;
				for (MasterPath p : mActivePaths)
				{
					if (p.id == job.path.id())
					{
						path = p;
						break;
					}
				}
				if (path == null)
				{
					// No path to repair.
					request.setState(NavRequestState.FAILED);
					poolJob(job);
					mPathRepairJobs.remove(iJob);
					break;
				}
				TriCell startCell = mMesh.getClosestCell(job.startX
						, job.startY
						, job.startZ
						, true, null);
				if (startCell == null)
				{
					// Not a valid start point.
					request.setState(NavRequestState.FAILED);
					poolJob(job);
					mPathRepairJobs.remove(iJob);
					break;
				}
				job.initializeSearch(path, startCell);
				search = job.search;
			}
			else
				search = job.search;
			
			if (request.state() != NavRequestState.PROCESSING)
			{
				// Something has canceled or invalidated the search.
				poolJob(job);
				mPathRepairJobs.remove(iJob);				
			}
			else if (!search.isActive())
			{
				poolJob(job);
				mPathRepairJobs.remove(iJob);
			}
			else
			{
				SearchState state = search.process();
				switch (state)
				{
				case COMPLETE:
					// Select a path from the search results.
					// Looking for the path that ends at far along the
					// original path as possible.
					MasterPath sourcePath = job.source;
					TriCell[] selectedCells = search.getPathCells(0);
					int maxGoalIndex = sourcePath.getCellIndex(selectedCells[selectedCells.length-1]);
					for (int i = 1; i < search.pathCount(); i++)
					{
						TriCell[] cells = search.getPathCells(i);
						int goalIndex = sourcePath.getCellIndex(
								cells[cells.length-1]);
						if (goalIndex > maxGoalIndex)
						{
							maxGoalIndex = goalIndex;
							selectedCells = cells;
						}
					}
					// Merge the two paths to form a new path.
					// New beginning of path...
					TriCell[] mergedCells = new TriCell[selectedCells.length 
					                                    + sourcePath.size() 
					                                    - maxGoalIndex - 1];
					System.arraycopy(selectedCells, 0, mergedCells, 0, selectedCells.length);
					if (mergedCells[mergedCells.length-1] != sourcePath.goalCell())
					{
						// ...old end of path.
						for (int iSource = maxGoalIndex + 1, iTarget = selectedCells.length
								; iSource < sourcePath.size()
								; iSource++, iTarget++)
							mergedCells[iTarget] = sourcePath.getCell(iSource);					
					}
					MasterPath mergedPath = new MasterPath(mNextPathID++
							, mergedCells
							, mMesh.planeTolerance()
							, mMesh.offsetScale());
					request.set(NavRequestState.COMPLETE
							, mergedPath.getPath(job.path.goalX
									, job.path.goalY
									, job.path.goalZ));
					if (maxPathAge > 0)
						mActivePaths.add(mergedPath);
					poolJob(job);
					mPathRepairJobs.remove(iJob);
					break;
				case FAILED:
					request.setState(NavRequestState.FAILED);
					poolJob(job);
					mPathRepairJobs.remove(iJob);
					break;
				default:
					iJob++;
				}
			}
		}
		return (mPathRepairJobs.size() == 0 ? false : true);
	}

	/**
	 * Transfer requests to the job queues.
	 */
	private void transferRequests()
	{
		// Transfer new path search requests.
		synchronized (mAStarRequests) 
		{
			while (mAStarRequests.size() > 0)
			{
				mAStarJobs.add(mAStarRequests.poll());
			}
		}
		
		// Transfer path repair requests.
		synchronized (mPathRepairRequests) 
		{
			while (mPathRepairRequests.size() > 0)
			{
				mPathRepairJobs.add(mPathRepairRequests.poll());
			}
		}
		
		// Transfer nearest location requests.
		synchronized (mNearestLocationRequests) 
		{
			while (mNearestLocationRequests.size() > 0)
			{
				mNearestLocationJobs.add(mNearestLocationRequests.poll());
			}
		}
		
		/*
		 * Transfer valid location requests.
		 */
		synchronized (mValidLocationRequests) 
		{
			while (mValidLocationRequests.size() > 0)
			{
				mValidLocationJobs.add(mValidLocationRequests.poll());
			}
		}
	}

	/**
	 * The number of "nearest location" searches currently in progress.
	 * <p>This value will never be greater than zero since all "nearest location" requests
	 * are processed every process cycle.</p>
	 * <p>Exposed for unit testing purposes.</p>
	 * @return The number of "nearest location" searches in progress.
	 */
	int nearestLocationJobCount() { return mNearestLocationJobs.size(); }

	/**
	 * The number of "valid location" checks currently in progress.
	 * <p>This value will never be greater than zero since all "valid location" requests
	 * are processed every process cycle.</p>
	 * <p>Exposed for unit testing purposes.</p>
	 * @return The number of "valid location" checks currently in progress.
	 */
	int validLocationJobCount() { return mValidLocationJobs.size(); }
	
}

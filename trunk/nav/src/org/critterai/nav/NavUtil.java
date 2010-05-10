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

import org.critterai.nav.MasterNavigator.Navigator;

/**
 * Provides utility operations related to navigation.
 */
public final class NavUtil 
{

	private NavUtil() { }
	
	/**
	 * Creates a navigator for the provided navigation mesh.
	 * Can be used in a threaded environment as long as only a single thread has
	 * a reference to the {@link MasterNavigator} object and all other threads
	 * utilize {@link Navigator} objects.
	 * @param verts The navigation mesh vertices in the form (x, y, z).
	 * @param indices The navigation mesh triangles in the form (vertAIndex, vertBIndex, vertCIndex)
	 * @param spacialDepth The allowed quadtree depth to use when storing navigation mesh cells.
	 * Higher values will tend to improve cell search speed at the cost of more memory use.
	 * @param planeTolerance The tolerance to use when evaluating whether a point is on the
	 * surface of the navigation mesh.  This value should be set above the maximum
	 * the source geometry deviates from the navigation mesh and below 0.5 times the minimum
	 * distance between overlapping navigation mesh polygons. 
	 * @param offsetScale Effects the amount path waypoints are offset from the edge of the
	 * path corridor.  Helpful in keeping clients away from the corridor edges.
	 * Values between 0.05 and 0.1 are normal.  A value of 0.5 will keep path waypoints near the
	 * center of the path corridor.  Values above 0.5 are not appropriate for normal pathfinding.
	 * @param heuristic The heuristic to use for A* searches.
	 * @param maxUpdateTimeslice The maximum time slice to use during 
	 * throttling operations. In nanoseconds.
	 * See {@link MasterNavigator#process(boolean)} for details.
	 * @param maxPathAge The maximum age that paths will be cached.  In milliseconds.
	 * See {@link MasterNavigator#maxPathAge} for details.
	 * @param repairSearchDepth The maximum search depth to use when attempting to repair paths.
	 * Larger values will increase the cost of repairs.  A value between one and three is usually
	 * appropriate. 
	 * @param searchPoolMax The maximum size allowed for the search pools.  Search pools
	 * allow re-use of search related objects.  Setting the value too high will waste memory.
	 * Setting the value too low will decrease performance.  Generally, the value is set
	 * to the average number of expected active searches.
	 * @return A navigator for the navigation mesh.
	 * @throws IllegalArgumentException If any of the arguments result in a construction failure.
	 */
	public static MasterNavigator getNavigator(float[] verts
			, int[] indices
			, int spacialDepth
			, float planeTolerance
			, float offsetScale
			, DistanceHeuristicType heuristic
			, long maxUpdateTimeslice
			, int maxPathAge
			, int repairSearchDepth
			, int searchPoolMax)
		throws IllegalArgumentException
	{
		TriNavMesh mesh = TriNavMesh.build(verts
				, indices
				, spacialDepth
				, planeTolerance
				, offsetScale);
		return new MasterNavigator(mesh
				, heuristic
				, maxUpdateTimeslice
				, maxPathAge
				, repairSearchDepth
				, searchPoolMax);
	}

	/**
	 * Creates a navigator suitable for running on its own thread.
	 * @param verts The navigation mesh vertices in the form (x, y, z).
	 * @param indices The navigation mesh triangles in the form (vertAIndex, vertBIndex, vertCIndex)
	 * @param spacialDepth The allowed quad tree depth to use when storing navigation mesh cells.
	 * Higher values will tend to improve cell search speed at the cost of more memory use.
	 * @param planeTolerance The tolerance to use when evaluating whether a point is on the
	 * surface of the navigation mesh.  This value should be set above the maximum
	 * the source geometry deviates from the navigation mesh, and below 0.5 times the minimum
	 * distance between overlapping navigation mesh polygons. 
	 * @param offsetScale Effects the amount path waypoints are offset from the edge of
	 * path corridors.  Helpful in keeping clients away from corridor edges during locomotion.
	 * Values between 0.05 and 0.1 are normal.  A value of 0.5 will keep path waypoints near the
	 * center of the path corridor.  Values above 0.5 are not appropriate for normal path finding.
	 * @param heuristic The heuristic to use for A* searches.
	 * @param frameLength The length of the frame. In milliseconds.
	 * The process operation will not be called more frequently than this value.
	 * @param maxFrameTimeslice The maximum amount of time per frame the search
	 * is allowed to take.  Used for throttling. In nanoseconds.
	 * This maximum is not guaranteed under heavy load.  See {@link MasterNavigator#process(boolean)}
	 * for details.
	 * @param maxPathAge The maximum age that paths will be cached.  In milliseconds.
	 * See {@link MasterNavigator#maxPathAge} for details.
	 * @param repairSearchDepth The maximum search depth to use when attempting to repair paths.
	 * Larger values will increase the cost of repairs.  A value between one and three is usually
	 * appropriate. 
	 * @param searchPoolMax The maximum size allowed for the search pools.  Search pools
	 * allow re-use of search related objects.  Setting the value too high will waste memory.
	 * Setting the value too low will decrease performance.  Generally, the value is set
	 * to the average number of expected active searches.
	 * @param maintenanceFrequency The frequency, in milliseconds, that maintenance operations
	 * should be performed.  The value should normally be less than maxPathAge.
	 * @return A navigator suitable for running on its own thread.
	 * @throws IllegalArgumentException  If any of the arguments result in a construction failure.
	 */
	public static ThreadedNavigator getThreadedNavigator(float[] verts
			, int[] indices
			, int spacialDepth
			, float planeTolerance
			, float offsetScale
			, DistanceHeuristicType heuristic
			, int frameLength
			, long maxFrameTimeslice
			, int maxPathAge
			, int repairSearchDepth
			, int searchPoolMax
			, long maintenanceFrequency)
		throws IllegalArgumentException
	{
		TriNavMesh mesh = TriNavMesh.build(verts
				, indices
				, spacialDepth
				, planeTolerance
				, offsetScale);
		MasterNavigator navigator = new MasterNavigator(mesh
				, heuristic
				, maxFrameTimeslice
				, maxPathAge
				, repairSearchDepth
				, searchPoolMax);
		return new ThreadedNavigator(navigator, frameLength, maintenanceFrequency);
	}
	
}

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
namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Provides utility operations related to navigation.
    /// </summary>
    public static class PlannerUtil 
    {
        
        /// <summary>
        /// Creates a navigator for the provided navigation mesh.
        /// Can be used in a threaded environment as long as only a single thread has
        /// a reference to the <see cref="MasterNavigator">MasterNavigator</see> object and all other threads
        /// utilize <see cref="MasterNavigator.Navigator">Navigator</see> objects.
        /// </summary>
        /// <param name="verts">The navigation mesh vertices in the form (x, y, z).</param>
        /// <param name="indices">The navigation mesh triangles in the form (vertAIndex, vertBIndex, vertCIndex)</param>
        /// <param name="spacialDepth">The allowed quadtree mDepth to use when storing navigation mesh cells.
        /// Higher values will tend to improve Cell search speed at the cost of more memory use.</param>
        /// <param name="planeTolerance">The tolerance to use when evaluating whether a point is on the
        /// surface of the navigation mesh.  This value should be Set above the maximum
        /// the source geometry deviates from the navigation mesh and below 0.5 times the minimum
        /// distance between overlapping navigation mesh polygons. </param>
        /// <param name="offsetScale">Effects the amount path waypoints are offset from the edge of the
        /// path corridor.  Helpful in keeping clients away from the corridor edges.
        /// Values between 0.05 and 0.1 are normal.  A value of 0.5 will keep path waypoints near the
        /// center of the path corridor.  Values above 0.5 are not appropriate for normal pathfinding.</param>
        /// <param name="heuristic">The heuristic to use for A* searches.</param>
        /// <param name="maxUpdateTimeslice">The maximum time slice to use during 
        /// throttling operations. In nanoseconds.
        /// See {@Link MasterNavigator#Process(boolean)} for details.</param>
        /// <param name="maxPathAge">The maximum age that paths will be cached.  In milliseconds.
        /// See <see cref="MasterNavigator.MaxPathAge">MasterNavigator.MaxPathAge</see> 
        /// for details.</param>
        /// <param name="repairSearchDepth">The maximum search mDepth to use when attempting to repair paths.
        /// Larger values will increase the cost of repairs.  A value between one and three is usually
        /// appropriate. </param>
        /// <param name="searchPoolMax">The maximum Size allowed for the search pools.  Search pools
        /// allow re-use of search related objects.  Setting the value too high will waste memory.
        /// Setting the value too low will decrease performance.  Generally, the value is Set
        /// to the average number of expected active searches.</param>
        /// <returns>A navigator for the navigation mesh.</returns>
        public static MasterPlanner GetPlanner(float[] verts
                , int[] indices
                , int spacialDepth
                , float planeTolerance
                , float offsetScale
                , DistanceHeuristicType heuristic
                , long maxUpdateTimeslice
                , int maxPathAge
                , int repairSearchDepth
                , int searchPoolMax)
        {
            TriNavMesh mesh = TriNavMesh.Build(verts
                    , indices
                    , spacialDepth
                    , planeTolerance
                    , offsetScale);
            return new MasterPlanner(mesh
                    , heuristic
                    , maxUpdateTimeslice
                    , maxPathAge
                    , repairSearchDepth
                    , searchPoolMax);
        }

        /// <summary>
        /// Creates a navigator suitable for running on its own thread. 
        /// </summary>
        /// <param name="verts">The navigation mesh vertices in the form (x, y, z).</param>
        /// <param name="indices">The navigation mesh triangles in the form 
        /// (vertAIndex, vertBIndex, vertCIndex)</param>
        /// <param name="spacialDepth">The allowed quad tree mDepth to use when storing navigation mesh cells.
        /// Higher values will tend to improve Cell search speed at the cost of more memory use.</param>
        /// <param name="planeTolerance">The tolerance to use when evaluating whether a point is on the
        /// surface of the navigation mesh.  This value should be Set above the maximum
        /// the source geometry deviates from the navigation mesh, and below 0.5 times the minimum
        /// distance between overlapping navigation mesh polygons. </param>
        /// <param name="offsetScale">Effects the amount path waypoints are offset from the edge of
        /// path corridors.  Helpful in keeping clients away from corridor edges during locomotion.
        /// Values between 0.05 and 0.1 are normal.  A value of 0.5 will keep path waypoints near the
        /// center of the path corridor.  Values above 0.5 are not appropriate for normal path finding.</param>
        /// <param name="heuristic">The heuristic to use for A* searches.</param>
        /// <param name="frameLength"> The length of the frame. In milliseconds.
        /// The Process operation will not be called more frequently than this value.</param>
        /// <param name="maxFrameTimeslice">The maximum amount of time per frame the search
        /// is allowed to take.  Used for throttling. In nanoseconds.
        /// This maximum is not guaranteed under heavy load.  See 
        /// <see cref="MasterNavigator.Process">MasterNavigator.Process</see>
        /// for details.</param>
        /// <param name="maxPathAge">The maximum age that paths will be cached.  In milliseconds.
        /// See <see cref="MasterNavigator.MaxPathAge">MasterNavigator.MaxPathAge</see> for details.</param>
        /// <param name="repairSearchDepth">The maximum search depth to use when attempting to repair paths.
        /// Larger values will increase the cost of repairs.  A value between one and three is usually
        /// appropriate. </param>
        /// <param name="searchPoolMax">The maximum Size allowed for the search pools.  Search pools
        /// allow re-use of search related objects.  Setting the value too high will waste memory.
        /// Setting the value too low will decrease performance.  Generally, the value is Set
        /// to the average number of expected active searches.</param>
        /// <param name="maintenanceFrequency">The frequency, in milliseconds, that maintenance operations
        /// should be performed.  The value should normally be less than MaxPathAge.</param>
        /// <returns>A navigator suitable for running on its own thread.</returns>
        public static ThreadedPlanner GetThreadedPlanner(float[] verts
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
        {
            TriNavMesh mesh = TriNavMesh.Build(verts
                    , indices
                    , spacialDepth
                    , planeTolerance
                    , offsetScale);
            MasterPlanner navigator = new MasterPlanner(mesh
                    , heuristic
                    , maxFrameTimeslice
                    , maxPathAge
                    , repairSearchDepth
                    , searchPoolMax);
            return new ThreadedPlanner(navigator, frameLength, maintenanceFrequency);
        }
        
    }
}

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

import static org.critterai.math.MathUtil.TOLERANCE_STD;
import static org.critterai.nav.TestUtil.getAllCells;
import static org.critterai.nav.TestUtil.linkAllCells;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

import java.util.ArrayList;

import org.critterai.math.Vector3;
import org.critterai.math.geom.Polygon3;
import org.critterai.nav.DistanceHeuristicType;
import org.critterai.nav.MasterNavRequest;
import org.critterai.nav.MasterNavigator;
import org.critterai.nav.NavRequestState;
import org.critterai.nav.TriCell;
import org.critterai.nav.TriNavMesh;
import org.critterai.nav.MasterNavigator.Navigator;
import org.critterai.nav.MasterPath.Path;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Navigator} class.
 */
public final class NavigatorTest 
{

    /**
     * Design notes;
     * 
     * Limited to single threaded behavior.
     * Does not validate accuracy of throttling.
     * 
     * Internal behavior is being validated in this
     * suite since it is important to detect 
     * potential memory leak issues.
     * 
     * Disposal is tested during the internal tests.
     * 
     * Some tests build on earlier tests.  So fix
     * failures in earlier tests first.
     */
    
    private static final int MAX_PATH_AGE = 60000;
    private static final int SEARCH_DEPTH = 2;
    
    private ITestMesh mMesh;
    private TriCell[] mCells;
    private TriNavMesh mNavMesh;
    private Vector3[] mCents;

    @Before
    public void setUp() throws Exception 
    {
        mMesh = new CorridorMesh();
        float[] verts = mMesh.getVerts();
        int[] indices = mMesh.getIndices();
        mCells = getAllCells(verts, indices);
        linkAllCells(mCells);
        // Dev Note: Keep the offset scale to zero.  Some tests depend on that.
        mNavMesh = TriNavMesh.build(verts, indices, 5, mMesh.getPlaneTolerence(), 0);
        mCents = new Vector3[mCells.length];
        for (int iCell = 0; iCell < mCells.length; iCell++)
        {
            int pCell = iCell*3;
            final Vector3 cent = Polygon3.getCentroid(new Vector3()
                                            , verts[indices[pCell]*3]
                                            , verts[indices[pCell]*3+1]
                                            , verts[indices[pCell]*3+2]
                                            , verts[indices[pCell+1]*3]
                                            , verts[indices[pCell+1]*3+1]
                                            , verts[indices[pCell+1]*3+2]
                                            , verts[indices[pCell+2]*3]
                                            , verts[indices[pCell+2]*3+1]
                                            , verts[indices[pCell+2]*3+2]);
            mCents[iCell] = cent;
        }
    }

    @Test
    public void testIsValidLocation() 
    {
        // Tests locations on the mesh surface or completely outside.
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        MasterNavRequest<Boolean>.NavRequest req;
        MasterNavRequest<Boolean>.NavRequest req2;
        for (int iCell = 1; iCell < mCells.length; iCell++)
        {
            req = nav.isValidLocation(mCents[iCell].x
                                            , mCents[iCell].y
                                            , mCents[iCell].z
                                            , mMesh.getPlaneTolerence());
            req2 = nav.isValidLocation(mCents[iCell-1].x
                    , mCents[iCell-1].y
                    , mCents[iCell-1].z
                    , mMesh.getPlaneTolerence());
            assertTrue(req != null);
            assertTrue(req.isFinished() == false);
            mnav.processAll(false);
            assertTrue(req.isFinished() == true);
            assertTrue(req.state() == NavRequestState.COMPLETE);
            assertTrue(req.data() == true);
            assertTrue(req2.state() == NavRequestState.COMPLETE);
            assertTrue(req2.data() == true);
        }
        // Test standard failure.
        final float[] minPoint = mMesh.getMinVertex();
        req = nav.isValidLocation(minPoint[0] - mMesh.getOffset()
                 , minPoint[1] - mMesh.getOffset()
                 , minPoint[2] - mMesh.getOffset()
                 , mMesh.getPlaneTolerence());
        mnav.processAll(false);
        assertTrue(req.state() == NavRequestState.COMPLETE);
        assertTrue(req.data() == false);
        // Test auto-failure after navigator is disposed.
        mnav.dispose();
        assertTrue(nav.isValidLocation(mCents[0].x, mCents[0].y, mCents[0].z
                , mMesh.getPlaneTolerence()).state() == NavRequestState.FAILED);
    }
    
    @Test
    public void testIsValidLocationTolerence() 
    {
        // Tests the tolerance functionality.
        // Note:  Can't test the exact edge due to floating point
        // errors.  Have to test just above and below the edge.
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        MasterNavRequest<Boolean>.NavRequest req;
        for (int iCell = 1; iCell < mCells.length; iCell++)
        {
            float ytol = mMesh.getPlaneTolerence()*0.5f;
            req = nav.isValidLocation(mCents[iCell].x
                                            , mCents[iCell].y + ytol - TOLERANCE_STD
                                            , mCents[iCell].z
                                            , ytol);
            mnav.processAll(false);
            assertTrue(Integer.toString(iCell), req.state() == NavRequestState.COMPLETE);
            assertTrue(Integer.toString(iCell), req.data() == true);
            req = nav.isValidLocation(mCents[iCell].x
                                            , mCents[iCell].y + ytol + TOLERANCE_STD
                                            , mCents[iCell].z
                                            , ytol);
            mnav.processAll(false);
            assertTrue(Integer.toString(iCell), req.state() == NavRequestState.COMPLETE);
            assertTrue(Integer.toString(iCell), req.data() == false);
            req = nav.isValidLocation(mCents[iCell].x
                                            , mCents[iCell].y - ytol + TOLERANCE_STD
                                            , mCents[iCell].z
                                            , ytol);
            mnav.processAll(false);
            assertTrue(Integer.toString(iCell), req.state() == NavRequestState.COMPLETE);
            assertTrue(Integer.toString(iCell), req.data() == true);
            req = nav.isValidLocation(mCents[iCell].x
                                            , mCents[iCell].y - ytol - TOLERANCE_STD
                                            , mCents[iCell].z
                                            , ytol);
            mnav.processAll(false);
            assertTrue(Integer.toString(iCell), req.state() == NavRequestState.COMPLETE);
            assertTrue(Integer.toString(iCell), req.data() == false);
        }
    }
    
    @Test
    public void testGetNearestValidLocationInternal() 
    {
        // Points are already internal to the mesh.
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        MasterNavRequest<Vector3>.NavRequest req;
        MasterNavRequest<Vector3>.NavRequest req2;
        for (int iCell = 1; iCell < mCells.length; iCell++)
        {
            req = nav.getNearestValidLocation(mCents[iCell].x
                                            , mCents[iCell].y
                                            , mCents[iCell].z);
            req2 = nav.getNearestValidLocation(mCents[iCell-1].x
                    , mCents[iCell-1].y
                    , mCents[iCell-1].z);
            assertTrue(req != null);
            assertTrue(req.isFinished() == false);
            mnav.processAll(false);
            assertTrue(req.isFinished() == true);
            assertTrue(req.state() == NavRequestState.COMPLETE);
            assertTrue(req.data().sloppyEquals(mCents[iCell].x
                    , mCents[iCell].y
                    , mCents[iCell].z
                    , TOLERANCE_STD));
            assertTrue(req2.state() == NavRequestState.COMPLETE);
            assertTrue(req2.data().sloppyEquals(mCents[iCell-1].x
                    , mCents[iCell-1].y
                    , mCents[iCell-1].z
                    , TOLERANCE_STD));
            assertTrue(mnav.nearestLocationJobCount() == 0);
        }
        // Test auto-failure after navigator is disposed.
        mnav.dispose();
        assertTrue(nav.getNearestValidLocation(mCents[0].x, mCents[0].y, mCents[0].z)
                .state() == NavRequestState.FAILED);
    }
    
    @Test
    public void testGetNearestValidLocationExternal() 
    {
        // Point is outside the mesh.
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        MasterNavRequest<Vector3>.NavRequest req;
        float[] minPoint = mMesh.getMinVertex();
        req = nav.getNearestValidLocation(minPoint[0] - mMesh.getOffset()
                 , minPoint[1] - mMesh.getOffset()
                 , minPoint[2] - mMesh.getOffset());
        mnav.processAll(false);
        assertTrue(req.state() == NavRequestState.COMPLETE);
        final int[] minCellIndices = mMesh.getMinVertexPolys();
        boolean found = false;
        for (int i = 0; i < minCellIndices.length; i++)
        {
            TriCell cell = mCells[minCellIndices[i]];
            if (cell.isInColumn(req.data().x, req.data().z))
            {
                found = true;
                break;
            }
        }
        assertTrue(found);
    }
    
    @Test
    public void testDiscardPathRequest() 
    {
        // This test only tests that the functionality works
        // for one version of the process operation.
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        float[] pathPoints = null;
        final ArrayList<MasterNavRequest<Path>.NavRequest> reqs 
            = new ArrayList<MasterNavRequest<Path>.NavRequest>();
        int testCount = 0;
        for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
        {
            pathPoints = mMesh.getPathPoints(iPath);
            reqs.add(nav.getPath(pathPoints[0]
                                 , pathPoints[1]
                                 , pathPoints[2]
                                 , pathPoints[3]
                                 , pathPoints[4]
                                 , pathPoints[5]));
            testCount++;
        }
        // Remember: Test mesh guarantees that all paths are > 2 in length. 
        mnav.processOnce(false);
        pathPoints = mMesh.getPathPoints(mMesh.getPathCount() - 1);
        reqs.add(nav.getPath(pathPoints[0]
                             , pathPoints[1]
                             , pathPoints[2]
                             , pathPoints[3]
                             , pathPoints[4]
                             , pathPoints[5]));
        assertTrue(mnav.pathRequestCount() == 1);
        testCount++;
        if (testCount < 2)
            // Needs two loops to properly validate re-use.
            fail("Mesh doesn't support test.");
        // We now have at least one job and one request.
        for (MasterNavRequest<Path>.NavRequest req : reqs)
        {
            assertTrue(req.isFinished() == false);
            nav.discardPathRequest(req);
        }
        mnav.processOnce(false);
        for (MasterNavRequest<Path>.NavRequest req : reqs)
        {
            assertTrue(req.state() == NavRequestState.FAILED);
        }
        // Test disposal behavior.
        mnav.dispose();
        nav.discardPathRequest(reqs.get(0));
        assertTrue(mnav.discardPathRequestCount() == 0);
    }
    
    @Test
    public void testGetPathStandardProcessOnce() 
    {
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        final ArrayList<MasterNavRequest<Path>.NavRequest> reqs 
            = requestAllPaths(nav);
        final int maxProcessCalls = mMesh.getPolyCount() + 2; // The increment is arbitrary.
        int processCalls = 1;
        final ArrayList<MasterNavRequest<Path>.NavRequest> creqs 
            = new ArrayList<MasterNavRequest<Path>.NavRequest>();
        while (creqs.size() < mMesh.getPathCount() && processCalls <= maxProcessCalls)
        {
            mnav.processOnce(false);
            for (int i = reqs.size() - 1; i > -1; i--)
            {
                MasterNavRequest<Path>.NavRequest req = reqs.get(i);
                if (req.isFinished())
                {
                    assertTrue(req.state() == NavRequestState.COMPLETE);
                    assertTrue(req.data() != null);
                    creqs.add(req);
                    reqs.remove(i);
                }
            }
            processCalls++;
        }
        // Make sure paths are valid by walking them to the end.
        final int maxSteps = 100; // This number is arbitrary.
        final ArrayList<Integer> seenPaths = new ArrayList<Integer>();
        for (MasterNavRequest<Path>.NavRequest req : creqs)
        {
            final Path path = req.data();
            final Vector3 currentPoint = new Vector3();
            final int iPath = getPathStartPoint(path, currentPoint);
            assertTrue(!seenPaths.contains(iPath));
            seenPaths.add(iPath);
            int step = 0;
            while (!currentPoint.equals(path.goalX, path.goalY, path.goalZ)
                    && step < maxSteps)
            {
                path.getTarget(currentPoint.x
                        , currentPoint.y
                        , currentPoint.z
                        , currentPoint);
                step++;
            }
            // Make sure we have reached the goal from the expected
            // start point.
            assertTrue(currentPoint.equals(path.goalX, path.goalY, path.goalZ));
        }
    }
    
    @Test
    public void testGetPathStandardProcessAll() 
    {
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        final ArrayList<MasterNavRequest<Path>.NavRequest> reqs  
                = requestAllPaths(nav);
        final ArrayList<MasterNavRequest<Path>.NavRequest> creqs 
            = new ArrayList<MasterNavRequest<Path>.NavRequest>();
        mnav.processAll(false);
        for (int i = reqs.size() - 1; i > -1; i--)
        {
            MasterNavRequest<Path>.NavRequest req = reqs.get(i);
            assertTrue(req.isFinished());
            assertTrue(req.state() == NavRequestState.COMPLETE);
            assertTrue(req.data() != null);
            creqs.add(req);
            reqs.remove(i);
        }
        // Make sure paths are valid by walking them to the end.
        final int maxSteps = 100; // This number is arbitrary.
        final ArrayList<Integer> seenPaths = new ArrayList<Integer>();
        for (MasterNavRequest<Path>.NavRequest req : creqs)
        {
            final Path path = req.data();
            final Vector3 currentPoint = new Vector3();
            final int iPath = getPathStartPoint(path, currentPoint);
            assertTrue(!seenPaths.contains(iPath));
            seenPaths.add(iPath);
            int step = 0;
            while (!currentPoint.equals(path.goalX, path.goalY, path.goalZ)
                    && step < maxSteps)
            {
                path.getTarget(currentPoint.x
                        , currentPoint.y
                        , currentPoint.z
                        , currentPoint);
                step++;
            }
            // Make sure we have reached the goal from the expected
            // start point.
            assertTrue(currentPoint.equals(path.goalX, path.goalY, path.goalZ));
        }
    }
    
    @Test
    public void testGetPathStandardProcessThrottleBasic() 
    {
        // Only making sure that the processing can complete, not
        // that the throttling is functioning as expected.
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        final ArrayList<MasterNavRequest<Path>.NavRequest> reqs 
            = requestAllPaths(nav);
        final ArrayList<MasterNavRequest<Path>.NavRequest> creqs 
            = new ArrayList<MasterNavRequest<Path>.NavRequest>();
        mnav.process(false);
        for (int i = reqs.size() - 1; i > -1; i--)
        {
            MasterNavRequest<Path>.NavRequest req = reqs.get(i);
            assertTrue(req.isFinished());
            assertTrue(req.state() == NavRequestState.COMPLETE);
            assertTrue(req.data() != null);
            creqs.add(req);
            reqs.remove(i);
        }
        // Make sure paths are valid by walking them to the end.
        final int maxSteps = 100; // This number is arbitrary.
        final ArrayList<Integer> seenPaths = new ArrayList<Integer>();
        for (MasterNavRequest<Path>.NavRequest req : creqs)
        {
            final Path path = req.data();
            final Vector3 currentPoint = new Vector3();
            final int iPath = getPathStartPoint(path, currentPoint);
            assertTrue(!seenPaths.contains(iPath));
            seenPaths.add(iPath);
            int step = 0;
            while (!currentPoint.equals(path.goalX, path.goalY, path.goalZ)
                    && step < maxSteps)
            {
                path.getTarget(currentPoint.x
                        , currentPoint.y
                        , currentPoint.z
                        , currentPoint);
                step++;
            }
            // Make sure we have reached the goal from the expected
            // start point.
            assertTrue(currentPoint.equals(path.goalX, path.goalY, path.goalZ));
        }
    }
    
    @Test
    public void testGetPathStandardThrottling() 
    {
        // Tests whether throttling can complete is forced
        // to single step.
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , 1        // 1 nanosecond. <<<<<<<<<<<<<<<<<
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        final ArrayList<MasterNavRequest<Path>.NavRequest> reqs 
            = requestAllPaths(nav);
        final int maxProcessCalls = mMesh.getPolyCount() + 2; // The increment is arbitrary.
        int processCalls = 1;
        final ArrayList<MasterNavRequest<Path>.NavRequest> creqs 
            = new ArrayList<MasterNavRequest<Path>.NavRequest>();
        while (creqs.size() < mMesh.getPathCount() && processCalls <= maxProcessCalls)
        {
            mnav.process(false);
            for (int i = reqs.size() - 1; i > -1; i--)
            {
                MasterNavRequest<Path>.NavRequest req = reqs.get(i);
                if (req.isFinished())
                {
                    assertTrue(req.state() == NavRequestState.COMPLETE);
                    assertTrue(req.data() != null);
                    creqs.add(req);
                    reqs.remove(i);
                }
            }
            processCalls++;
        }
        // Make sure paths are valid by walking them to the end.
        final int maxSteps = 100; // This number is arbitrary.
        final ArrayList<Integer> seenPaths = new ArrayList<Integer>();
        for (MasterNavRequest<Path>.NavRequest req : creqs)
        {
            final Path path = req.data();
            final Vector3 currentPoint = new Vector3();
            final int iPath = getPathStartPoint(path, currentPoint);
            assertTrue(!seenPaths.contains(iPath));
            seenPaths.add(iPath);
            int step = 0;
            while (!currentPoint.equals(path.goalX, path.goalY, path.goalZ)
                    && step < maxSteps)
            {
                path.getTarget(currentPoint.x
                        , currentPoint.y
                        , currentPoint.z
                        , currentPoint);
                step++;
            }
            // Make sure we have reached the goal from the expected
            // start point.
            assertTrue(currentPoint.equals(path.goalX, path.goalY, path.goalZ));
        }
    }
    
    @Test
    public void testGetPathFailure()
    {
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        MasterNavRequest<Path>.NavRequest reqInOut;
        MasterNavRequest<Path>.NavRequest reqOutIn;
        float[] minPoint = mMesh.getMinVertex();
        minPoint[0] -= 1;
        minPoint[1] -= 1;
        minPoint[2] -= 1;
        for (int iCell = 1; iCell < mCells.length; iCell++)
        {
            reqInOut = nav.getPath(mCents[iCell].x
                                  , mCents[iCell].y
                                  , mCents[iCell].z
                                  , minPoint[0]
                                  , minPoint[1]
                                  , minPoint[2]);
            reqOutIn = nav.getPath(minPoint[0]
                      , minPoint[1]
                      , minPoint[2]
                      , mCents[iCell].x
                      , mCents[iCell].y
                      , mCents[iCell].z);
            mnav.processOnce(false);
            // Guaranteed to fail after one cycle.
            assertTrue(reqInOut.state() == NavRequestState.FAILED);
            assertTrue(reqOutIn.state() == NavRequestState.FAILED);
            assertTrue(reqInOut.data() == null);
            assertTrue(reqOutIn.data() == null);
        }
    }
    
    @Test
    public void testPathReuse()
    {
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        final ArrayList<MasterNavRequest<Path>.NavRequest> reqs 
            = requestAllPaths(nav);
        mnav.processAll(false);
        final ArrayList<MasterNavRequest<Path>.NavRequest> creqs 
            = requestAllPaths(nav);
        mnav.processAll(false);
        for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
        {
            assertTrue(reqs.get(iPath).data().id() == creqs.get(iPath).data().id());
        }
    }
    
    @Test
    public void testInternalPathPool() 
    {
        final int poolSize = (int)(mMesh.getPathCount() + 1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        requestAllPaths(nav);
        mnav.processAll(false);
        assertTrue(mnav.pathSearchPoolSize() == mMesh.getPathCount());
        requestAllPaths(nav);
        requestAllPaths(nav);
        mnav.processAll(false);
        assertTrue(mnav.pathSearchPoolSize() == poolSize);
        mnav.dispose();
        assertTrue(mnav.pathSearchPoolSize() == 0);
    }    
    
    @Test
    public void testInternalRepairPool() 
    {
        final int poolSize = (int)(mMesh.getPathCount() + 1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        
        ArrayList<MasterNavRequest<Path>.NavRequest> reqs = requestAllPaths(nav);
        mnav.processAll(false);
        
        // Create repair requests.
        float[] minVert = mMesh.getMinVertex();
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            nav.repairPath(minVert[0]
                    , minVert[1]
                    , minVert[2]
                    , req.data());
        mnav.processAll(false);
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            nav.repairPath(minVert[0]
                    , minVert[1]
                    , minVert[2]
                    , req.data());
        mnav.processAll(false);
    
        assertTrue(mnav.repairPoolSize() <= poolSize);
        mnav.dispose();
        assertTrue(mnav.repairPoolSize() == 0);
    }
    
    @Test
    public void testRepairPathDepthOne() 
    {
        // Only a search depth of one from the original path.
        // Only one posible link to original path.
        final MasterNavigator mnav = buildRepairNavigator();
        final Navigator nav = mnav.navigator();
        final Vector3 start = new Vector3(2.5f, 0, 0.2f);
        final Vector3 goal = new Vector3(2.5f, 0, 1.8f);
        MasterNavRequest<Path>.NavRequest req = nav.getPath(start.x, start.y, start.z
                , goal.x, goal.y, goal.z);
        mnav.processAll(false);
        Path path = req.data();
        Vector3 target = new Vector3();
        assertTrue(path.getTarget(start.x, start.y, start.z, target));
        assertTrue(target.sloppyEquals(goal, TOLERANCE_STD));
        Vector3 oneOff = new Vector3(1.8f, 0, 0.2f);
        assertFalse(path.getTarget(oneOff.x, oneOff.y, oneOff.z, target));
        req = nav.repairPath(oneOff.x, oneOff.y, oneOff.z, path);
        assertTrue(req.state() == NavRequestState.PROCESSING);
        mnav.processOnce(false);
        assertTrue(req.isFinished() == false);
        mnav.processAll(true);
        assertTrue(req.isFinished());
        assertTrue(req.state() == NavRequestState.COMPLETE);
        path = req.data();
        assertTrue(path.getTarget(oneOff.x, oneOff.y, oneOff.z, target));
        assertTrue(target.sloppyEquals(goal, TOLERANCE_STD));
    }
    
    @Test
    public void testRepairPathDepthTwo() 
    {
        // A search depth of two from the original path.
        // Two possible links to original path.
        final MasterNavigator mnav = buildRepairNavigator();
        final Navigator nav = mnav.navigator();
        final Vector3 start = new Vector3(2.5f, 0, 0.2f);
        final Vector3 goal = new Vector3(2.5f, 0, 1.8f);
        MasterNavRequest<Path>.NavRequest req = nav.getPath(start.x, start.y, start.z
                , goal.x, goal.y, goal.z);
        mnav.processAll(false);
        Path path = req.data();
        Vector3 target = new Vector3();
        assertTrue(path.getTarget(start.x, start.y, start.z, target));
        assertTrue(target.sloppyEquals(goal, TOLERANCE_STD));
        Vector3 twoOff = new Vector3(1.2f, 0, 0.8f);
        assertFalse(path.getTarget(twoOff.x, twoOff.y, twoOff.z, target));
        req = nav.repairPath(twoOff.x, twoOff.y, twoOff.z, path);
        mnav.processAll(true);
        assertTrue(req.isFinished());
        assertTrue(req.state() == NavRequestState.COMPLETE);
        path = req.data();
        assertTrue(path.getTarget(twoOff.x, twoOff.y, twoOff.z, target));
        assertTrue(target.sloppyEquals(goal, TOLERANCE_STD));
    }
    
    @Test
    public void testRepairPathDepthThree() 
    {
        // A search depth of two from the original path.
        // Two possible links to original path.
        final MasterNavigator mnav = buildRepairNavigator();
        final Navigator nav = mnav.navigator();
        final Vector3 start = new Vector3(2.5f, 0, 0.2f);
        final Vector3 goal = new Vector3(2.5f, 0, 1.8f);
        MasterNavRequest<Path>.NavRequest req = nav.getPath(start.x, start.y, start.z
                , goal.x, goal.y, goal.z);
        mnav.processAll(false);
        Path path = req.data();
        Vector3 target = new Vector3();
        assertTrue(path.getTarget(start.x, start.y, start.z, target));
        assertTrue(target.sloppyEquals(goal, TOLERANCE_STD));
        Vector3 threeOff = new Vector3(0.8f, 0, 0.4f);
        assertFalse(path.getTarget(threeOff.x, threeOff.y, threeOff.z, target));
        req = nav.repairPath(threeOff.x, threeOff.y, threeOff.z, path);
        mnav.processAll(true);
        assertTrue(req.isFinished());
        assertTrue(req.state() == NavRequestState.FAILED);
    }
    
    @Test
    public void testPathAgingBasic()
    {
        // Validates that patch cache is cleared.
        final int poolSize = (int)(mMesh.getPathCount() +1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , 500            // Short max age.
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        final ArrayList<MasterNavRequest<Path>.NavRequest> reqs 
            = requestAllPaths(nav);
        mnav.processAll(false);
        assertTrue(mnav.activePathCount() == mMesh.getMultiPathCount());
        try { Thread.sleep(510); }
        catch (InterruptedException e) { fail("Thread sleep interupted."); }
        mnav.processAll(true);  // <<<< Maintenance
        assertTrue(mnav.activePathCount() == 0);
        for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
            assertTrue(reqs.get(iPath).data().isDisposed());
    }

    @Test
    public void testKeepPathAlive() 
    {
        // Validates that patch cache is cleared.
        final int maxAge = 250;
        final int poolSize = (int)(mMesh.getPathCount() + 1);
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , maxAge            // Short max age.
                , SEARCH_DEPTH
                , poolSize);
        final Navigator nav = mnav.navigator();
        final ArrayList<MasterNavRequest<Path>.NavRequest> reqs 
            = requestAllPaths(nav);
        mnav.processAll(false);
        assertTrue(mnav.activePathCount() == mMesh.getMultiPathCount());
        
        try { Thread.sleep(maxAge + 1); }
        catch (InterruptedException e) { fail("Thread sleep interupted."); }
        for (int iPath = 0; iPath < reqs.size(); iPath++)
            nav.keepPathAlive(reqs.get(iPath).data());
        mnav.processAll(true);
        assertTrue(mnav.activePathCount() == reqs.size());
        for (int iPath = 0; iPath < reqs.size(); iPath++)
            assertTrue(reqs.get(iPath).data().isDisposed() == false);
        MasterNavRequest<Path>.NavRequest dreq = null;
        // Let the paths expire one by one.
        while (reqs.size() > 0)
        {
            try { Thread.sleep(maxAge + 1); }
            catch (InterruptedException e) { fail("Thread sleep interupted."); }    
            dreq = reqs.get(reqs.size()-1);
            reqs.remove(reqs.size()-1);
            for (int iPath = 0; iPath < reqs.size(); iPath++)
                nav.keepPathAlive(reqs.get(iPath).data());
            mnav.processAll(true);
            assertTrue(mnav.activePathCount() == reqs.size());
            assertTrue(dreq.data().isDisposed());
            for (int iPath = 0; iPath < reqs.size(); iPath++)
                assertTrue(reqs.get(iPath).data().isDisposed() == false);
        }
    }

    @Test
    public void testInternalPathRequests()
    {
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        
        // Create path requests.
        ArrayList<MasterNavRequest<Path>.NavRequest> reqs = requestAllPaths(nav);
        assertTrue(mnav.pathRequestCount() == reqs.size());
        assertTrue(mnav.pathJobCount() == 0);
        assertTrue(mnav.activePathCount() == 0);
        mnav.dispose();
        assertTrue(mnav.pathJobCount() == 0);
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            assertTrue(req.state() == NavRequestState.FAILED);
    }

    @Test
    public void testInternalPathJobs()
    {
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        
        // Create path jobs
        ArrayList<MasterNavRequest<Path>.NavRequest> reqs = requestAllPaths(nav);
        mnav.processOnce(false);
        assertTrue(mnav.pathRequestCount() == 0);
        assertTrue(mnav.pathJobCount() == reqs.size());
        assertTrue(mnav.activePathCount() == 0);
        mnav.dispose();
        assertTrue(mnav.pathJobCount() == 0);
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            assertTrue(req.state() == NavRequestState.FAILED);
    }

    @Test
    public void testInternalPaths()
    {
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        
        // Create active paths.
        ArrayList<MasterNavRequest<Path>.NavRequest> reqs = requestAllPaths(nav);
        mnav.processAll(false);
        assertTrue(mnav.pathRequestCount() == 0);
        assertTrue(mnav.pathJobCount() == 0);
        assertTrue(mnav.activePathCount() == reqs.size());
        mnav.dispose();
        assertTrue(mnav.activePathCount() == 0);
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            assertTrue(req.data().isDisposed());
    }    
    
    @Test
    public void testInternalRepairRequests()
    {
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        
        // Create active paths.
        ArrayList<MasterNavRequest<Path>.NavRequest> reqs = requestAllPaths(nav);
        mnav.processAll(false);
        // Create repair requests.
        float[] minVert = mMesh.getMinVertex();
        reqs.clear();
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            reqs.add(nav.repairPath(minVert[0]
                    , minVert[1]
                    , minVert[2]
                    , req.data()));
        assertTrue(mnav.repairRequestCount() == reqs.size());
        assertTrue(mnav.repairJobCount() == 0);
        mnav.dispose();
        assertTrue(mnav.repairRequestCount() == 0);
        assertTrue(mnav.repairJobCount() == 0);
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            assertTrue(req.state() == NavRequestState.FAILED);
    }
    
    @Test
    public void testInternalRepairJobs()
    {
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        
        // Create active paths.
        ArrayList<MasterNavRequest<Path>.NavRequest> reqs = requestAllPaths(nav);
        mnav.processAll(false);
        // Create repair requests.
        float[] minVert = mMesh.getMinVertex();
        reqs.clear();
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            reqs.add(nav.repairPath(minVert[0]
                    , minVert[1]
                    , minVert[2]
                    , req.data()));
        mnav.processOnce(false);
        assertTrue(mnav.repairRequestCount() == 0);
        assertTrue(mnav.repairJobCount() == reqs.size());
        mnav.dispose();
        assertTrue(mnav.repairRequestCount() == 0);
        assertTrue(mnav.repairJobCount() == 0);
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            assertTrue(req.state() == NavRequestState.FAILED);
    }
    
    @Test
    public void testInternalKeepAliveRequests()
    {
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        
        // Create active paths.
        ArrayList<MasterNavRequest<Path>.NavRequest> reqs = requestAllPaths(nav);
        mnav.processAll(false);
        // Create keep alive requests.
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            nav.keepPathAlive(req.data());
        assertTrue(mnav.keepAliveRequestCount() == reqs.size());
        mnav.dispose();
        assertTrue(mnav.keepAliveRequestCount() == 0);
    }
    
    @Test
    public void testInternalCancelRequests()
    {
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        
        // Create active paths.
        ArrayList<MasterNavRequest<Path>.NavRequest> reqs = requestAllPaths(nav);
        mnav.processOnce(false);
        // Create cancellations.
        for (MasterNavRequest<Path>.NavRequest req : reqs)
            nav.discardPathRequest(req);
        assertTrue(mnav.discardPathRequestCount() == reqs.size());
        mnav.dispose();
        assertTrue(mnav.discardPathRequestCount() == 0);
    }
    
    @Test
    public void testInternalImmediateRequests()
    {
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        
        // Create location requests (both types.
        MasterNavRequest<Boolean>.NavRequest validLocReq = nav.isValidLocation(mCents[0].x
                , mCents[0].y
                , mCents[0].z
                , mMesh.getPlaneTolerence());
        assertTrue(mnav.validLocationRequestCount() == 1);
        MasterNavRequest<Vector3>.NavRequest nearestLocReq = nav.getNearestValidLocation(mCents[0].x
                , mCents[0].y
                , mCents[0].z);
        assertTrue(mnav.nearestLocationRequestCount() == 1);
        
        mnav.dispose();
        assertTrue(mnav.validLocationRequestCount() == 0);
        assertTrue(mnav.nearestLocationRequestCount() == 0);
        assertTrue(validLocReq.state() == NavRequestState.FAILED);
        assertTrue(nearestLocReq.state() == NavRequestState.FAILED);
    }
    
    @Test
    public void testIsDisposed() 
    {
        // Tests locations on the mesh surface or completely outside.
        final MasterNavigator mnav = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , SEARCH_DEPTH
                , 10);
        final Navigator nav = mnav.navigator();
        assertTrue(nav.isDisposed() == false);
        mnav.dispose();
        assertTrue(nav.isDisposed());
    }
    
    private int getPathStartPoint(Path path, Vector3 out)
    {
        for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
        {
            float[] pathPoints = mMesh.getPathPoints(iPath);
            if (pathPoints[3] == path.goalX
                    && pathPoints[4] == path.goalY
                    && pathPoints[5] == path.goalZ)    
            {
                out.set(pathPoints[0], pathPoints[1], pathPoints[2]);
                return iPath;
            }
                
        }
        return -1;
    }
    
    private ArrayList<MasterNavRequest<Path>.NavRequest> requestAllPaths(Navigator navigator)
    {
        float[] pathPoints;
        final ArrayList<MasterNavRequest<Path>.NavRequest> result 
            = new ArrayList<MasterNavRequest<Path>.NavRequest>();
        for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
        {
            pathPoints = mMesh.getPathPoints(iPath);
            result.add(navigator.getPath(pathPoints[0]
                                 , pathPoints[1]
                                 , pathPoints[2]
                                 , pathPoints[3]
                                 , pathPoints[4]
                                 , pathPoints[5]));
        }
        return result;
    }
    
    private MasterNavigator buildRepairNavigator()
    {
        float[] verts = {
                0, 0, 0        // 0
                , 1, 0, 0
                , 2, 0, 0
                , 3, 0, 0
                , 1, 0, 1
                , 2, 0, 1    // 5
                , 3, 0, 1
                , 2, 0, 2
                , 3, 0, 2    // 8
        };
        int [] indices = {
                0, 4, 1        // 0
                , 1, 4, 5
                , 1, 5, 2
                , 2, 5, 6
                , 2, 6, 3
                , 4, 7, 5    // 5
                , 5, 7, 6
                , 7, 8, 6    // 7
        };
         TriNavMesh mesh = TriNavMesh.build(verts, indices, 2, 0.5f, 0);
        return new MasterNavigator(mesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , MAX_PATH_AGE
                , 2
                , 5);
    }

}

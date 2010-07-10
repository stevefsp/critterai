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

import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

import java.util.ArrayList;

import org.critterai.math.Vector3;
import org.critterai.nav.MasterPath.Path;
import org.junit.Test;

/**
 * Exercises the {@link ThreadedNavigator} search on a moderately complex
 * mesh.  Using a large number of sample points spread across the surface
 * of the mesh, runs searches from each point to all other points.
 * Confirms the searches complete and performs basic path walking checks.
 */
public class IntegrationThreadedSearch 
{
    /*
     * Design Notes:
     * 
     * This is a long running integration test.
     * It should only be run if all non-integration test suites
     * have completed successfully.
     */
    
     private static final int maxConcurrentSearches = 5000;
    
    /**
     * Validates the successful completion of a path search between
     * all sample points.  (Forward and reverse.) 
     */
    @Test
    public void TestThreadedSearch()
    {

        IntegrationMesh mMeshData = null;
        try 
        {
            mMeshData = new IntegrationMesh();
        } catch (Exception e) 
        {
            fail(e.toString());
        }
        
        if (mMeshData.mSamplePoints.length == 0) 
            fail("No sample points.");
        
        Thread thread = new Thread(mMeshData.mMasterNavigator);
        thread.start();

        assertTrue(mMeshData.mMasterNavigator.isDisposed() == false);

        ArrayList<PathSearchInfo> searchInfo 
                = new ArrayList<PathSearchInfo>(mMeshData.mSamplePoints.length);
        MasterNavigator.Navigator nav = mMeshData.mMasterNavigator.navigator();
        int testCount = 0;
        
        for (int pStart = 0
                ; pStart < mMeshData.mSamplePoints.length
                ; pStart += 3)
        {
            // Dev Note: Purposely allowing start and goal to be the same.
            for (int pGoal = pStart
                    ; pGoal < mMeshData.mSamplePoints.length
                    ; pGoal += 3)
            {

                Vector3 start = new Vector3(mMeshData.mSamplePoints[pStart]
                    , mMeshData.mSamplePoints[pStart + 1]
                    , mMeshData.mSamplePoints[pStart + 2]);

                Vector3 goal = new Vector3(mMeshData.mSamplePoints[pGoal]
                    , mMeshData.mSamplePoints[pGoal + 1]
                    , mMeshData.mSamplePoints[pGoal + 2]);

                PathSearchInfo si = new PathSearchInfo(start
                    , goal
                    , nav.getPath(start.x, start.y, start.z
                            , goal.x, goal.y, goal.z));

                searchInfo.add(si);

                goal = new Vector3(mMeshData.mSamplePoints[pStart]
                    , mMeshData.mSamplePoints[pStart + 1]
                    , mMeshData.mSamplePoints[pStart + 2]);

                start = new Vector3(mMeshData.mSamplePoints[pGoal]
                    , mMeshData.mSamplePoints[pGoal + 1]
                    , mMeshData.mSamplePoints[pGoal + 2]);

                si = new PathSearchInfo(start
                    , goal
                    , nav.getPath(start.x, start.y, start.z
                            , goal.x, goal.y, goal.z));

                searchInfo.add(si);
                testCount += 2;
                
                if (searchInfo.size() >= maxConcurrentSearches)
                {
                    // Pause and let searches complete.
                    int maxLoopCount = 12000; // 120 seconds.

                    int loopCount = 0;
                    while (searchInfo.size() > 0 && loopCount < maxLoopCount)
                    {
                        for (int i = searchInfo.size() - 1; i >= 0; i--)
                        {
                            PathSearchInfo sir = searchInfo.get(i);
                            if (sir.pathRequest.isFinished())
                            {
                                String state = sir.start + " -> " + sir.goal;
                                assertTrue(state, sir.pathRequest.state() 
                                        == NavRequestState.COMPLETE);
                                assertTrue(walkPath(sir.pathRequest.data()
                                        , sir.start
                                        , sir.goal));
                                searchInfo.remove(i);
                            }
                        }
                        try 
                        {
                            Thread.sleep(10);
                        } catch (InterruptedException e) 
                        {
                            fail(e.toString());
                        }
                        loopCount++;
                    }
                    assertTrue(searchInfo.size() == 0);
                }
                
                if (testCount % 500000 == 0)
                {
                    System.out.println("ITS: " + (pStart / 3)
                            + " of " + (mMeshData.mSamplePoints.length / 3));
                }
            }
            
        }
    }
    
    private boolean walkPath(Path path, Vector3 start, Vector3 goal)
    {
        if (path.goalX != goal.x
                || path.goalY != goal.y
                || path.goalZ != goal.z)
            return false;
        Vector3 target = new Vector3(start.x, start.y, start.z);
        int loopCount = 0;
        while(!target.equals(goal) && loopCount < 1000)
        {
            if (!path.getTarget(target.x, target.y, target.z, target))
                break;
        }
        return target.equals(goal);
    }
}

package org.critterai.nav;

import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

import java.util.ArrayList;

import org.critterai.math.Vector3;
import org.junit.Test;

/**
 * Performs basic pathfinding tests (can a path be found) on a mesh for which
 * all cells should have a  valid path to all other cells.  The 
 * {@link ThreadedNavigator} is used for path searches from the centroid
 * of each cell to the centroid of every  other cell.
 * <p>This test is especially useful for checking to see if all cells are
 * being properly linked during the build of the navmesh.
 */
public final class IntegrationMeshConnectivity 
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
     * Performs path searches between all cell centroids.
     * This test guarentee's coverage of all cells.
     * (Which is not guaranteed by the sample points.)
     */
    @Test
    public void TestMeshConnectivity()
    {
        
        IntegrationMesh mMeshData = null;
        try 
        {
            mMeshData = new IntegrationMesh();
        } catch (Exception e) 
        {
            fail(e.toString());
        }
        
        if (mMeshData.mCells.length == 0) 
            fail("No cells in mesh.");
        
        Vector3[] centroids = new Vector3[mMeshData.mCells.length];
        for (int iCell = 0; iCell < mMeshData.mCells.length; iCell++)
        {
            TriCell cell = mMeshData.mCells[iCell];
            centroids[iCell] = new Vector3(cell.centroidX
                    , cell.centroidY
                    , cell.centroidZ);
        }

        Thread thread = new Thread(mMeshData.mMasterNavigator);
        thread.start();

        assertTrue(mMeshData.mMasterNavigator.isDisposed() == false);

        ArrayList<PathSearchInfo> searchInfo 
                = new ArrayList<PathSearchInfo>(centroids.length*2);
        MasterNavigator.Navigator nav = mMeshData.mMasterNavigator.navigator();
        
        int testCount = 0;

        for (int pStart = 0; pStart < centroids.length; pStart++)
        {
            // Dev Note: Purposely allowing start and goal to be the same.
            for (int pGoal = pStart; pGoal < centroids.length; pGoal++)
            {
                Vector3 start = centroids[pStart];
                Vector3 goal = centroids[pGoal];

                PathSearchInfo si = new PathSearchInfo(start
                    , goal
                    , nav.getPath(start.x, start.y, start.z
                            , goal.x, goal.y, goal.z));

                searchInfo.add(si);

                start = centroids[pGoal];
                goal = centroids[pStart];

                si = new PathSearchInfo(start
                    , goal
                    , nav.getPath(start.x, start.y, start.z
                            , goal.x, goal.y, goal.z));

                searchInfo.add(si);
                testCount++;
                
                if (searchInfo.size() >= maxConcurrentSearches)
                {
                    // Pause to let searches complete.
                    int maxLoopCount = 12000;    // 120 seconds

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
                
                if (testCount % 10000 == 0)
                {
                    System.out.println("IMC: " + pStart
                            + " of " + (centroids.length));
                }
                
            }
        }

    }
}

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

import org.critterai.math.Vector3;
import org.junit.Test;

/**
 * Exercises the {@link AStarSearch} search on a moderately complex mesh.  
 * Using a large number of sample points spread across the surface of
 * the mesh, runs searches from each point to all other points and
 * confirms the search completes.
 * <p>Does not perform any confirmation that the path returned is
 * good.  It only validates that it completes.  See the 
 * {@link IntegrationThreadedSearch} tests for path confirmation tests.</p>
 */
public final class IntegrationAStarSearch 
{
    /*
     * Design Notes:
     * 
     * This is a long running integration test.
     * It should only be run if all non-integration test suites
     * have completed successfully.
     */
    
    /**
     * Validates the successful completion of an A* search between
     * all sample points.  (Forward and reverse.)
     */
    @Test
    public void TestAStarSearch()
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
        
        AStarSearch search = new AStarSearch(IntegrationMesh.heuristic);
        Vector3 trashV = new Vector3();
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
                TriCell startCell 
                        = mMeshData.mNavMesh.getClosestCell(
                                mMeshData.mSamplePoints[pStart]
                                , mMeshData.mSamplePoints[pStart+1]
                                , mMeshData.mSamplePoints[pStart+2]
                                , true
                                , trashV);
                TriCell goalCell = mMeshData.mNavMesh.getClosestCell(
                    mMeshData.mSamplePoints[pGoal]
                    , mMeshData.mSamplePoints[pGoal + 1]
                    ,mMeshData. mSamplePoints[pGoal + 2]
                    , true
                    , trashV);

                search.initialize(mMeshData.mSamplePoints[pStart]
                    , mMeshData.mSamplePoints[pStart + 1]
                    , mMeshData.mSamplePoints[pStart + 2]
                    , mMeshData.mSamplePoints[pGoal]
                    , mMeshData.mSamplePoints[pGoal + 1]
                    , mMeshData.mSamplePoints[pGoal + 2]
                    , startCell
                    , goalCell);

                int maxLoopCount = mMeshData.mSourceIndices.length / 3 + 1;

                int loopCount = 0;
                while (search.isActive() && loopCount < maxLoopCount)
                {
                    search.process();
                    loopCount++;
                }

                String state = "[" + pStart + ":" + pGoal + "]"
                    + "(" + mMeshData.mSamplePoints[pStart]
                    + ", " + mMeshData.mSamplePoints[pStart + 1]
                    + ", " + mMeshData.mSamplePoints[pStart + 2]
                    + ") -> (" + mMeshData.mSamplePoints[pGoal]
                    + ", " + mMeshData.mSamplePoints[pGoal + 1]
                    + ", " + mMeshData.mSamplePoints[pGoal + 2] + ")";
                assertTrue(state, search.state() == SearchState.COMPLETE);

                // Reverse points.
                search.initialize(mMeshData.mSamplePoints[pGoal]
                    , mMeshData.mSamplePoints[pGoal + 1]
                    , mMeshData.mSamplePoints[pGoal + 2]
                    , mMeshData.mSamplePoints[pStart]
                    , mMeshData.mSamplePoints[pStart + 1]
                    , mMeshData.mSamplePoints[pStart + 2]
                    , goalCell
                    , startCell);

                loopCount = 0;
                while (search.isActive() && loopCount < maxLoopCount)
                {
                    search.process();
                    loopCount++;
                }

                state = "(" + mMeshData.mSamplePoints[pGoal]
                    + ", " + mMeshData.mSamplePoints[pGoal + 1]
                    + ", " + mMeshData.mSamplePoints[pGoal + 2]
                    + ") -> (" + mMeshData.mSamplePoints[pStart]
                    + ", " + mMeshData.mSamplePoints[pStart + 1]
                    + ", " + mMeshData.mSamplePoints[pStart + 2];
                assertTrue(state, search.state() == SearchState.COMPLETE);
                
                testCount++;
                if (testCount % 50000 == 0)
                {
                    System.out.println("IAS: " + (pStart / 3)
                            + " of " + (mMeshData.mSamplePoints.length / 3));
                }
            }
        }
    }
}

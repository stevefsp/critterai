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

import static org.critterai.nav.TestUtil.getAllCells;
import static org.critterai.nav.TestUtil.linkAllCells;
import static org.junit.Assert.*;

import org.critterai.nav.DistanceHeuristicType;
import org.critterai.nav.MasterNavigator;
import org.critterai.nav.TriCell;
import org.critterai.nav.TriNavMesh;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link MasterNavigator} class.
 */
public final class MasterNavigatorTest 
{

    /*
     * Design notes:
     * 
     * Only some very basic checks are performed by this suite.
     * Most validations have to wait until the navigator test suite.
     * 
     * Not validating the count getters.  They are validated all 
     * over the place in the navigator test suite.
     */
    
    private ITestMesh mMesh;
    private float[] verts;
    private int[] indices;
    private TriCell[] cells;
    private TriNavMesh mNavMesh;

    @Before
    public void setUp() throws Exception 
    {
        mMesh = new CorridorMesh();
        verts = mMesh.getVerts();
        indices = mMesh.getIndices();
        cells = getAllCells(verts, indices);
        linkAllCells(cells);
        mNavMesh = TriNavMesh.build(verts, indices, 5, 0.5f, 0.1f);
    }

    @Test
    public void testMasterNavigator() 
    {
        MasterNavigator mn = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , 60000
                , 4
                , 2);
        assertTrue(mn.maxPathAge == 60000);
        assertTrue(mn.maxProcessingTimeslice == Integer.MAX_VALUE);
        assertTrue(mn.repairSearchDepth == 4);
        assertTrue(mn.isDisposed() == false);
    }

    @Test
    public void testNavigator()
    {
        MasterNavigator mn = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , 60000
                , 4
                , 2);
        assertTrue(mn.navigator() != null);
        assertTrue(mn.navigator() == mn.navigator()); // Same reference across multiple calls.
    }

    @Test
    public void testDispose() 
    {
        MasterNavigator mn = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , 60000
                , 4, 2);
        mn.dispose();
        assertTrue(mn.isDisposed() == true);
    }

    @Test
    public void testProcess() 
    {
        MasterNavigator mn = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , 60000
                , 4
                , 2);
        // Just make sure no exceptions are thrown
        // when there is nothing to do.
        mn.process(false);
        mn.process(true);
        mn.dispose();
        mn.process(false);
        mn.process(true);
    }
    
    @Test
    public void testProcessOnce() 
    {
        MasterNavigator mn = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , 60000
                , 4
                , 2);
        // Just make sure no exceptions are thrown
        // when there is nothing to do.
        mn.processOnce(false);
        mn.processOnce(true);
        mn.dispose();
        mn.processOnce(false);
        mn.processOnce(true);
    }
    
    @Test
    public void testProcessAll() 
    {
        MasterNavigator mn = new MasterNavigator(mNavMesh
                , DistanceHeuristicType.LONGEST_AXIS
                , Integer.MAX_VALUE
                , 60000
                , 4
                , 2);
        // Just make sure no exceptions are thrown
        // when there is nothing to do.
        mn.processAll(false);
        mn.processAll(true);
        mn.dispose();
        mn.processAll(false);
        mn.processAll(true);
    }

}

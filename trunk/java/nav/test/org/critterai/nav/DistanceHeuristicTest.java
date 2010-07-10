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

import static org.critterai.nav.DistanceHeuristic.*;

import static org.junit.Assert.*;

import org.critterai.nav.DistanceHeuristicType;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link DistanceHeuristic} class.
 */
public final class DistanceHeuristicTest 
{

    @Before
    public void setUp() throws Exception 
    {
    }

    @Test
    public void testGetManhattan() 
    {
        assertTrue(getManhattan(-3, 0, 0, -1, 0, 0) == 2);
        assertTrue(getManhattan(-2, 0, -1, -2, 0, 2) == 3);
        assertTrue(getManhattan(1, -1, -2, -1, 2, 2) == 9);
        assertTrue(getManhattan(4, 2, 0, 2, 3, -1) == 4);
    }

    @Test
    public void testGetLongestAxis() 
    {
        assertTrue(getLongestAxis(-3, 0, 0, -1, 0, 0) == 2);
        assertTrue(getLongestAxis(-2, 0, -1, -2, 0, 2) == 3);
        assertTrue(getLongestAxis(1, -1, -2, -1, 2, 2) == 4);
        assertTrue(getLongestAxis(1, -1, -2, -4, 2, 2) == 5);
        assertTrue(getLongestAxis(1, -1, -2, -4, 5, 2) == 6);
    }

    @Test
    public void testGetHeuristicValue() 
    {
        assertTrue(getHeuristicValue(DistanceHeuristicType.MANHATTAN, -3, 0, 0, -1, 0, 0) == 2);
        assertTrue(getHeuristicValue(DistanceHeuristicType.MANHATTAN, -2, 0, -1, -2, 0, 2) == 3);
        assertTrue(getHeuristicValue(DistanceHeuristicType.MANHATTAN, 1, -1, -2, -1, 2, 2) == 9);
        assertTrue(getHeuristicValue(DistanceHeuristicType.MANHATTAN, 4, 2, 0, 2, 3, -1) == 4);
        assertTrue(getHeuristicValue(DistanceHeuristicType.LONGEST_AXIS, -3, 0, 0, -1, 0, 0) == 2);
        assertTrue(getHeuristicValue(DistanceHeuristicType.LONGEST_AXIS, -2, 0, -1, -2, 0, 2) == 3);
        assertTrue(getHeuristicValue(DistanceHeuristicType.LONGEST_AXIS, 1, -1, -2, -1, 2, 2) == 4);
        assertTrue(getHeuristicValue(DistanceHeuristicType.LONGEST_AXIS, 1, -1, -2, -4, 2, 2) == 5);
        assertTrue(getHeuristicValue(DistanceHeuristicType.LONGEST_AXIS, 1, -1, -2, -4, 5, 2) == 6);
    }

}

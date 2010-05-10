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

import java.util.ArrayList;

import org.critterai.nav.AStarSearch;
import org.critterai.nav.DistanceHeuristicType;
import org.critterai.nav.MasterPath;
import org.critterai.nav.SearchState;
import org.critterai.nav.TriCell;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link AStarSearch} class.
 */
public final class AStarSearchTest 
{
	
	private ITestMesh mMesh;
	private float[] verts;
	private int[] indices;
	private TriCell[] cells;

	@Before
	public void setUp() throws Exception 
	{
		mMesh = new CorridorMesh();
		verts = mMesh.getVerts();
		indices = mMesh.getIndices();
		cells = getAllCells(verts, indices);
		linkAllCells(cells);
	}

	@Test
	public void testAStarSearch() 
	{
		final AStarSearch search = new AStarSearch(DistanceHeuristicType.LONGEST_AXIS);
		assertTrue(search.goalX() == 0);
		assertTrue(search.goalY() == 0);
		assertTrue(search.goalZ() == 0);
		assertTrue(search.isActive() == false);
		assertTrue(search.state() == SearchState.UNINITIALIZED);
		assertTrue(search.pathCells() == null);
	}
	
	@Test
	public void testInitialize() 
	{
		final AStarSearch search = new AStarSearch(DistanceHeuristicType.LONGEST_AXIS);
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			final float[] pathPoints = mMesh.getPathPoints(iPath);
			final int[] pathCells = mMesh.getPathPolys(iPath);
			final SearchState result = search.initialize(pathPoints[0]
			                             , pathPoints[1]
			                             , pathPoints[2]
			                             , pathPoints[3]
			                             , pathPoints[4]
			                             , pathPoints[5]
			                             , cells[pathCells[0]]
			                             , cells[pathCells[pathCells.length-1]]);
			assertTrue(result == SearchState.INITIALIZED);
			assertTrue(search.goalX() == pathPoints[3]);
			assertTrue(search.goalY() == pathPoints[4]);
			assertTrue(search.goalZ() == pathPoints[5]);
			assertTrue(search.isActive() == true);
			assertTrue(search.state() == SearchState.INITIALIZED);
			assertTrue(search.pathCells() == null);
			testCount++;
		}
		if (testCount < 2)
			// Needs two loops to properly validate re-use.
			fail("Mesh doesn't support test.");
	}
	
	@Test
	public void testReset() 
	{
		final AStarSearch search = new AStarSearch(DistanceHeuristicType.LONGEST_AXIS);
		if (mMesh.getPathCount() < 1)
			fail("Mesh doesn't support test.");
		final float[] pathPoints = mMesh.getPathPoints(0);
		final int[] pathCells = mMesh.getPathPolys(0);
		final SearchState result = search.initialize(pathPoints[0]
		                             , pathPoints[1]
		                             , pathPoints[2]
		                             , pathPoints[3]
		                             , pathPoints[4]
		                             , pathPoints[5]
		                             , cells[pathCells[0]]
		                             , cells[pathCells[pathCells.length-1]]);
		// Duplicated tests.
		assertTrue(result == SearchState.INITIALIZED);
		assertTrue(search.goalX() == pathPoints[3]);
		assertTrue(search.goalY() == pathPoints[4]);
		assertTrue(search.goalZ() == pathPoints[5]);
		assertTrue(search.isActive() == true);
		assertTrue(search.state() == SearchState.INITIALIZED);
		assertTrue(search.pathCells() == null);
		// Primary tests.
		search.reset();
		assertTrue(search.goalX() == 0);
		assertTrue(search.goalY() == 0);
		assertTrue(search.goalZ() == 0);
		assertTrue(search.isActive() == false);
		assertTrue(search.state() == SearchState.UNINITIALIZED);
		assertTrue(search.pathCells() == null);
	}

	@Test
	public void testProcessAndResultSuccess() 
	{
		final AStarSearch search = new AStarSearch(DistanceHeuristicType.LONGEST_AXIS);
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			final float[] pathPoints = mMesh.getPathPoints(iPath);
			final int[] pathCells = mMesh.getPathPolys(iPath);
			SearchState state = search.initialize(pathPoints[0]
			                             , pathPoints[1]
			                             , pathPoints[2]
			                             , pathPoints[3]
			                             , pathPoints[4]
			                             , pathPoints[5]
			                             , cells[pathCells[0]]
			                             , cells[pathCells[pathCells.length-1]]);
			final int maxProcessCalls = mMesh.getPolyCount();
			int processCalls = 1;
			assertTrue(search.process() == SearchState.PROCESSING);
			while (search.isActive() && processCalls <= maxProcessCalls)
			{
				state = search.process();
				if (search.isActive())
					assertTrue(state == SearchState.PROCESSING);
				processCalls++;
			}
			assertTrue(search.state() == SearchState.COMPLETE);
			final TriCell[] path = search.pathCells();
			assertTrue(path != null);
			assertTrue(path.length == pathCells.length);
			for (int i = 0; i < path.length; i++)
			{
				assertTrue(path[i] == cells[pathCells[i]]);
			}
			testCount++;
		}
		if (testCount < 2)
			// Needs two loops to properly validate re-use.
			fail("Mesh doesn't support test.");
	}	
	
	@Test
	public void testProcessAndResultFail() 
	{
		final AStarSearch search = new AStarSearch(DistanceHeuristicType.LONGEST_AXIS);
		int testCount = 0;
		float[] v = { 
				0, 0, 0 
				, 1, 0, 1
				, 1, 0, 0
			};
		TriCell goalCell = new TriCell(v, 0, 1, 2);
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			final float[] pathPoints = mMesh.getPathPoints(iPath);
			final int[] pathCells = mMesh.getPathPolys(iPath);
			SearchState state = search.initialize(pathPoints[0]
			                             , pathPoints[1]
			                             , pathPoints[2]
			                             , v[0]
			                             , v[1]
			                             , v[2]
			                             , cells[pathCells[0]]
			                             , goalCell);
			final int maxProcessCalls = mMesh.getPolyCount();
			int processCalls = 0;
			while (search.isActive() && processCalls <= maxProcessCalls + 1)
			{
				state = search.process();
				if (search.isActive())
					assertTrue(state == SearchState.PROCESSING);
				processCalls++;
			}
			assertTrue(search.state() == SearchState.FAILED);
			assertTrue(search.pathCells() == null);
			testCount++;
		}
		if (testCount < 2)
			// Needs two loops to properly validate re-use.
			fail("Mesh doesn't support test.");
	}	
	
	@Test
	public void testEvaluate() 
	{
		final AStarSearch search = new AStarSearch(DistanceHeuristicType.LONGEST_AXIS);
		int testCount = 0;
		if (mMesh.getPathCount() < 2)
			fail("Mesh doesn't support test.");
		final int[] otherPathCells = mMesh.getPathPolys(0);
		final TriCell[] opc = new TriCell[otherPathCells.length];
		for (int i = 0; i < otherPathCells.length; i++)
			opc[i] = cells[otherPathCells[i]];
		final MasterPath omp = new MasterPath(2, opc, mMesh.getPlaneTolerence(), 0.1f);
		for (int iPath = 1; iPath < mMesh.getPathCount(); iPath++)
		{
			final float[] pathPoints = mMesh.getPathPoints(iPath);
			final int[] pathCells = mMesh.getPathPolys(iPath);
			final TriCell[] pc = new TriCell[pathCells.length];
			for (int i = 0; i < pathCells.length; i++)
				pc[i] = cells[pathCells[i]];
			final MasterPath mp = new MasterPath(3, pc, mMesh.getPlaneTolerence(), 0.1f);
			ArrayList<MasterPath> mpl = new ArrayList<MasterPath>(2);
			mpl.add(omp);
			mpl.add(mp);
			assertTrue(search.evaluate(mpl) == null);
			search.initialize(pathPoints[0]
		                             , pathPoints[1]
		                             , pathPoints[2]
		                             , pathPoints[3]
		                             , pathPoints[4]
		                             , pathPoints[5]
		                             , cells[pathCells[0]]
		                             , cells[pathCells[pathCells.length-1]]);
			assertTrue(search.evaluate(mpl) == mp);
			mpl.remove(mp);
			assertTrue(search.evaluate(mpl) == null);
			assertTrue(search.process() == SearchState.PROCESSING);
			mpl.add(mp);
			assertTrue(search.evaluate(mpl) == mp);
			mpl.remove(mp);
			assertTrue(search.evaluate(mpl) == null);
			testCount++;
		}
		if (testCount < 2)
			// Needs two loops to properly validate re-use.
			fail("Mesh doesn't support test.");
	}


}

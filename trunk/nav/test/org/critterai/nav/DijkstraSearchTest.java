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
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

import org.critterai.nav.DijkstraSearch;
import org.critterai.nav.SearchState;
import org.critterai.nav.TriCell;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link DijkstraSearch} class.
 */
public final class DijkstraSearchTest 
{

	/*
	 * Design notes:
	 * 
	 * This class is opaque.  Direct testing
	 * of loaded values is limited.  So a good amount of testing
	 * is mixed together.
	 */
	
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
	public void testConstructionDefaults() 
	{
		DijkstraSearch search = new DijkstraSearch();
		assertTrue(search.goals() == null);
		assertTrue(search.isActive() == false);
		assertTrue(search.pathCount() == 0);
		assertTrue(search.state() == SearchState.UNINITIALIZED);
	}

	@Test
	public void testGoals() 
	{
		final DijkstraSearch search = new DijkstraSearch();
		final float[] startPoint = mMesh.getMultiPathStartPoint();
		final float[] goalPoints = getGoalPoints();
		search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , goalPoints
		           , getStartCell()
		           , getGoalCells()
		           , 2
		           , false);
		final float[] gps = search.goals();
		assertTrue(gps.length == goalPoints.length);
		for (int i = 0; i < gps.length; i++)
			assertTrue(gps[i] == goalPoints[i]);
		search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , null
		           , getStartCell()
		           , getGoalCells()
		           , 2
		           , false);
		assertTrue(search.goals() == null);
	}
	
	@Test
	public void testProcessAndResultFindAll() 
	{
		// Expect all paths to be found.
		final DijkstraSearch search = new DijkstraSearch();
		final float[] startPoint = mMesh.getMultiPathStartPoint();
		final TriCell[] goalCells = getGoalCells();
		SearchState state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , null
		           , getStartCell()
		           , goalCells
		           , Integer.MAX_VALUE
		           , false);
		assertTrue(state == SearchState.INITIALIZED);
		assertTrue(search.state() == SearchState.INITIALIZED);
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
		assertTrue(state == SearchState.COMPLETE);
		assertTrue(search.state() == SearchState.COMPLETE);
		assertTrue(search.pathCount() == mMesh.getMultiPathCount());
		for (int iExpected = 0; iExpected < mMesh.getMultiPathCount(); iExpected++)
		{
			final TriCell[] expectedPath = getPathCells(iExpected);
			boolean foundMatch = false;
			for (int iResult = 0; iResult < search.pathCount(); iResult++)
			{
				TriCell[] resultPath = search.getPathCells(iResult);
				if (resultPath.length != expectedPath.length)
					continue;
				boolean matched = true;
				for (int iCell = 0; iCell < resultPath.length; iCell++)
				{
					if (resultPath[iCell] != expectedPath[iCell])
					{
						matched = false;
						break;
					}
				}
				if (matched)
				{
					foundMatch = true;
					break;
				}
			}
			if (!foundMatch)
				fail("No search paths match expected path " + iExpected + ".");
		}
	}
	
	@Test
	public void testProcessAndResultFindFirst() 
	{
		// Expect only shortest path to be found.
		final DijkstraSearch search = new DijkstraSearch();
		final float[] startPoint = mMesh.getMultiPathStartPoint();
		final TriCell[] goalCells = getGoalCells();
		SearchState state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , null
		           , getStartCell()
		           , goalCells
		           , Integer.MAX_VALUE
		           , true);
		final int maxProcessCalls = mMesh.getPolyCount();
		int processCalls = 0;
		while (search.isActive() && processCalls <= maxProcessCalls)
		{
			state = search.process();
			if (search.isActive())
				assertTrue(state == SearchState.PROCESSING);
			processCalls++;
		}
		assertTrue(search.pathCount() == 1);
		final TriCell[] resultPath = search.getPathCells(0);
		final TriCell[] expectedPath = getPathCells(mMesh.getShortestMultiPath());
		assertTrue(resultPath.length == expectedPath.length);
		for (int iCell = 0; iCell < resultPath.length; iCell++)
		{
			if (resultPath[iCell] != expectedPath[iCell])
				fail("Search path does not match expected path.");
		}
	}
	
	@Test
	public void testProcessAndResultLimitDepthSuccess() 
	{
		// Shorten the depth of the search so that
		// all paths except longest path(s) are found.
		final DijkstraSearch search = new DijkstraSearch();
		final float[] startPoint = mMesh.getMultiPathStartPoint();
		final TriCell[] goalCells = getGoalCells();
		final int depthLimit = getMaxPathLength() - 2;
		SearchState state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , null
		           , getStartCell()
		           , goalCells
		           , depthLimit
		           , false);
		final int maxProcessCalls = mMesh.getPolyCount();
		int processCalls = 0;
		while (search.isActive() && processCalls <= maxProcessCalls)
		{
			state = search.process();
			if (search.isActive())
				assertTrue(state == SearchState.PROCESSING);
			processCalls++;
		}
		assertTrue(search.pathCount() > 0);
		int expectedPathCount = 0;
		for (int iExpected = 0; iExpected < mMesh.getMultiPathCount(); iExpected++)
		{
			final TriCell[] expectedPath = getPathCells(iExpected);
			if (expectedPath.length > depthLimit + 1)
				// Don't expect to see this path.
				continue;
			expectedPathCount++;
			boolean foundMatch = false;
			for (int iResult = 0; iResult < search.pathCount(); iResult++)
			{
				TriCell[] resultPath = search.getPathCells(iResult);
				if (resultPath.length != expectedPath.length)
					continue;
				boolean matched = true;
				for (int iCell = 0; iCell < resultPath.length; iCell++)
				{
					if (resultPath[iCell] != expectedPath[iCell])
					{
						matched = false;
						break;
					}
				}
				if (matched)
				{
					foundMatch = true;
					break;
				}
			}
			if (!foundMatch)
				fail("No search paths match expected path " + iExpected + ".");
		}
		assertTrue(search.pathCount() == expectedPathCount);
	}

	@Test
	public void testProcessAndResultLimitDepthFail() 
	{
		// Shorten search depth so that no paths are found.
		final DijkstraSearch search = new DijkstraSearch();
		final float[] startPoint = mMesh.getMultiPathStartPoint();
		final TriCell[] goalCells = getGoalCells();
		final int depthLimit = getMinPathLength() - 2;
		SearchState state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , null
		           , getStartCell()
		           , goalCells
		           , depthLimit
		           , false);
		final int maxProcessCalls = mMesh.getPolyCount();
		int processCalls = 0;
		while (search.isActive() && processCalls <= maxProcessCalls)
		{
			state = search.process();
			if (search.isActive())
				assertTrue(state == SearchState.PROCESSING);
			processCalls++;
		}
		assertTrue(search.state() == SearchState.FAILED);
		assertTrue(search.pathCount() == 0);
	}
	
	@Test
	public void testProcessAndResultFail()
	{
		// Give a goal that is not part of the mesh.
		final DijkstraSearch search = new DijkstraSearch();
		final float[] startPoint = mMesh.getMultiPathStartPoint();
		final TriCell[] goalCells = new TriCell[2];
		float[] v = { 
				0, 0, 0 
				, 1, 0, 1
				, 1, 0, 0
				, 0, 0, 1
			};
		goalCells[0] = new TriCell(v, 0, 1, 2);
		goalCells[1] = new TriCell(v, 0, 3, 1);
		goalCells[0].link(goalCells[1], true);
		SearchState state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , null
		           , getStartCell()
		           , goalCells
		           , Integer.MAX_VALUE
		           , false);
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
		assertTrue(search.pathCount() == 0);
	}
	
	@Test
	public void testResetAndReuse() 
	{
		// - Shortening the depth of the search so that
		// all paths except longest path(s) are found.
		// - Reset
		// - Do a shortest path search.
		final DijkstraSearch search = new DijkstraSearch();
		final float[] startPoint = mMesh.getMultiPathStartPoint();
		final TriCell[] goalCells = getGoalCells();
		final int depthLimit = getMaxPathLength() - 2;
		SearchState state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , getGoalPoints()
		           , getStartCell()
		           , goalCells
		           , depthLimit
		           , false);
		final int maxProcessCalls = mMesh.getPolyCount();
		int processCalls = 0;
		while (search.isActive() && processCalls <= maxProcessCalls)
		{
			state = search.process();
			if (search.isActive())
				assertTrue(state == SearchState.PROCESSING);
			processCalls++;
		}
		assertTrue(search.pathCount() > 0);
		int expectedPathCount = 0;
		for (int iExpected = 0; iExpected < mMesh.getMultiPathCount(); iExpected++)
		{
			final TriCell[] expectedPath = getPathCells(iExpected);
			if (expectedPath.length > depthLimit + 1)
				// Don't expect to see this path.
				continue;
			expectedPathCount++;
			boolean foundMatch = false;
			for (int iResult = 0; iResult < search.pathCount(); iResult++)
			{
				TriCell[] resultPath = search.getPathCells(iResult);
				if (resultPath.length != expectedPath.length)
					continue;
				boolean matched = true;
				for (int iCell = 0; iCell < resultPath.length; iCell++)
				{
					if (resultPath[iCell] != expectedPath[iCell])
					{
						matched = false;
						break;
					}
				}
				if (matched)
				{
					foundMatch = true;
					break;
				}
			}
			if (!foundMatch)
				fail("No search paths match expected path " + iExpected + ".");
		}
		assertTrue(search.pathCount() == expectedPathCount);
		// Reset
		search.reset();
		assertTrue(search.pathCount() == 0);
		assertTrue(search.goals() == null);
		// Search for shortest path.
		// Expect only shortest path to be found.
		state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , null
		           , getStartCell()
		           , goalCells
		           , Integer.MAX_VALUE
		           , true);
		processCalls = 0;
		while (search.isActive() && processCalls <= maxProcessCalls)
		{
			state = search.process();
			if (search.isActive())
				assertTrue(state == SearchState.PROCESSING);
			processCalls++;
		}
		assertTrue(search.pathCount() == 1);
		final TriCell[] resultPath = search.getPathCells(0);
		final TriCell[] expectedPath = getPathCells(mMesh.getShortestMultiPath());
		assertTrue(resultPath.length == expectedPath.length);
		for (int iCell = 0; iCell < resultPath.length; iCell++)
		{
			if (resultPath[iCell] != expectedPath[iCell])
				fail("Search path does not match expected path.");
		}
	}
	
	@Test
	public void testReuse() 
	{
		// - Shortening the depth of the search so that
		// all paths except longest path(s) are found.
		// - Do a shortest path search.
		final DijkstraSearch search = new DijkstraSearch();
		final float[] startPoint = mMesh.getMultiPathStartPoint();
		final TriCell[] goalCells = getGoalCells();
		final int depthLimit = getMaxPathLength() - 2;
		SearchState state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , getGoalPoints()
		           , getStartCell()
		           , goalCells
		           , depthLimit
		           , false);
		final int maxProcessCalls = mMesh.getPolyCount();
		int processCalls = 0;
		while (search.isActive() && processCalls <= maxProcessCalls)
		{
			state = search.process();
			if (search.isActive())
				assertTrue(state == SearchState.PROCESSING);
			processCalls++;
		}
		assertTrue(search.pathCount() > 0);
		int expectedPathCount = 0;
		for (int iExpected = 0; iExpected < mMesh.getMultiPathCount(); iExpected++)
		{
			final TriCell[] expectedPath = getPathCells(iExpected);
			if (expectedPath.length > depthLimit + 1)
				// Don't expect to see this path.
				continue;
			expectedPathCount++;
			boolean foundMatch = false;
			for (int iResult = 0; iResult < search.pathCount(); iResult++)
			{
				TriCell[] resultPath = search.getPathCells(iResult);
				if (resultPath.length != expectedPath.length)
					continue;
				boolean matched = true;
				for (int iCell = 0; iCell < resultPath.length; iCell++)
				{
					if (resultPath[iCell] != expectedPath[iCell])
					{
						matched = false;
						break;
					}
				}
				if (matched)
				{
					foundMatch = true;
					break;
				}
			}
			if (!foundMatch)
				fail("No search paths match expected path " + iExpected + ".");
		}
		assertTrue(search.pathCount() == expectedPathCount);
		// Search for shortest path.
		// Expect only shortest path to be found.
		state = search.initialize(startPoint[0], startPoint[1], startPoint[2]
		           , null
		           , getStartCell()
		           , goalCells
		           , Integer.MAX_VALUE
		           , true);
		assertTrue(search.goals() == null);
		processCalls = 0;
		while (search.isActive() && processCalls <= maxProcessCalls)
		{
			state = search.process();
			if (search.isActive())
				assertTrue(state == SearchState.PROCESSING);
			processCalls++;
		}
		assertTrue(search.pathCount() == 1);
		final TriCell[] resultPath = search.getPathCells(0);
		final TriCell[] expectedPath = getPathCells(mMesh.getShortestMultiPath());
		assertTrue(resultPath.length == expectedPath.length);
		for (int iCell = 0; iCell < resultPath.length; iCell++)
		{
			if (resultPath[iCell] != expectedPath[iCell])
				fail("Search path does not match expected path.");
		}
	}
	
	private TriCell[] getGoalCells()
	{
		TriCell[] result = new TriCell[mMesh.getMultiPathCount()];
		for (int i = 0; i < mMesh.getMultiPathCount(); i++)
		{
			int[] pathIndices = mMesh.getMultiPathPolys(i);
			result[i] = cells[pathIndices[pathIndices.length-1]];
		}
		return result;
	}

	private TriCell[] getPathCells(int index)
	{
		int[] pathIndices = mMesh.getMultiPathPolys(index);
		TriCell[] result = new TriCell[pathIndices.length];
		for (int i = 0; i < pathIndices.length; i++)
		{
			result[i] = cells[pathIndices[i]];
		}
		return result;
	}
	
	private float[] getGoalPoints()
	{
		float[] result = new float[mMesh.getMultiPathCount()*3];
		for (int i = 0; i < mMesh.getMultiPathCount(); i++)
		{
			float[] point = mMesh.getMultiPathGoalPoint(i);
			result[i*3+0] = point[0];
			result[i*3+1] = point[1];
			result[i*3+2] = point[2];
		}
		return result;
	}

	private TriCell getStartCell()
	{
		return cells[mMesh.getMultiPathPolys(0)[0]];
	}
	
	private int getMinPathLength()
	{
		int result = mMesh.getMultiPathPolys(0).length;
		for (int i = 1; i < mMesh.getMultiPathCount(); i++)
		{
			result = Math.min(result, mMesh.getMultiPathPolys(i).length);
		}
		return result;
	}
	
	private int getMaxPathLength()
	{
		int result = mMesh.getMultiPathPolys(0).length;
		for (int i = 1; i < mMesh.getMultiPathCount(); i++)
		{
			result = Math.max(result, mMesh.getMultiPathPolys(i).length);
		}
		return result;
	}

}

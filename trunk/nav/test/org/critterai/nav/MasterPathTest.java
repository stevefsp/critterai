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

import org.critterai.math.MathUtil;
import org.critterai.nav.MasterPath;
import org.critterai.nav.TriCell;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link MasterPath} class.
 */
public final class MasterPathTest 
{

	/*
	 * Design notes:
	 * 
	 * Default values and immutable fields are validated in constructor
	 * tests.
	 * Getters for mutable fields are tested with their associated mutators.
	 */
	
	private static final float OFFSET_SCALE = 0.1f;
	private static final int PATH_ID = 5;
	
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
	public void testConstructionSingle() 
	{
		
		MasterPath mp = new MasterPath(PATH_ID
				, cells[0]
				, mMesh.getPlaneTolerence()
				, OFFSET_SCALE);
		assertTrue(mp.size() == 1);
		assertTrue(mp.getCell(0) == cells[0]);
		assertTrue(mp.startCell() == cells[0]);
		assertTrue(mp.goalCell() == cells[0]);	
		assertTrue(mp.id == PATH_ID);
		assertTrue(mp.isDisposed() == false);
	}
	
	@Test
	public void testConstructorMain() 
	{
		TriCell[] pathCells = getPathCells(0);
		MasterPath mp = new MasterPath(PATH_ID
				, pathCells
				, mMesh.getPlaneTolerence()
				, OFFSET_SCALE);
		assertTrue(mp.size() == pathCells.length);
		assertTrue(mp.startCell() == pathCells[0]);
		assertTrue(mp.goalCell() == pathCells[pathCells.length - 1]);		
		assertTrue(mp.id == PATH_ID);
		assertTrue(mp.isDisposed() == false);
	}

//	@Test
//	public void testIncrementClient() 
//	{
//		MasterPath mp = new MasterPath(PATH_ID
//				, cells[0]
//				, mMesh.getPlaneTolerence()
//				, OFFSET_SCALE);
//		assertTrue(mp.clientCount() == 1);
//		mp.incrementClient();
//		assertTrue(mp.clientCount() == 2);
//		mp.incrementClient();
//		assertTrue(mp.clientCount() == 3);
//	}

//	@Test
//	public void testDecrementClient() 
//	{
//		MasterPath mp = new MasterPath(PATH_ID
//				, cells[0]
//				, mMesh.getPlaneTolerence()
//				, OFFSET_SCALE);
//		assertTrue(mp.clientCount() == 1);
//		mp.decrementClient();
//		assertTrue(mp.clientCount() == 0);
//	}

	@Test
	public void testDispose() 
	{
		MasterPath mp = new MasterPath(PATH_ID
				, cells[0]
				, mMesh.getPlaneTolerence()
				, OFFSET_SCALE);
		assertTrue(mp.isDisposed() == false);
		mp.dispose();
		assertTrue(mp.isDisposed() == true);
	}

	@Test
	public void testResetTimestamp() 
	{
		// Can't to an exact test on this one.
		MasterPath mp = new MasterPath(PATH_ID
				, cells[0]
				, mMesh.getPlaneTolerence()
				, OFFSET_SCALE);
		assertTrue(MathUtil.sloppyEquals(mp.timestamp(), System.currentTimeMillis(), 2));
		try 
		{
			Thread.sleep(10);
		}
		catch (Exception e) { }
		long oldts = mp.timestamp();
		mp.resetTimestamp();
		assertTrue(MathUtil.sloppyEquals(mp.timestamp(), System.currentTimeMillis(), 2));
		assertTrue(mp.timestamp() != oldts);
		
	}

	@Test
	public void testGetPath() 
	{
		// Only performs basic validations.  The
		// full accuracy of the returned reference
		// is tested in the Path class's test suite.
		MasterPath mp = new MasterPath(PATH_ID
				, cells[0]
				, mMesh.getPlaneTolerence()
				, OFFSET_SCALE);
		float[] points = mMesh.getPathPoints(0);
		assertTrue(mp.getPath(points[3], points[4], points[5])
				!= mp.getPath(points[3], points[4], points[5]));  // New object per call.
		assertTrue(mp.getPath(points[3], points[4], points[5]).id() == PATH_ID);
	}

	@Test
	public void testGetCell() 
	{
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			TriCell[] pathCells = getPathCells(iPath);
			MasterPath mp = new MasterPath(PATH_ID
					, pathCells
					, mMesh.getPlaneTolerence()
					, OFFSET_SCALE);
			for (int iCell = 0; iCell < mp.size(); iCell++)
			{
				assertTrue(mp.getCell(iCell) == pathCells[iCell]);
				testCount++;
			}
		}
		if (testCount == 0)
			fail("Mesh doesn't support test.");
	}

	@Test
	public void testGetCellIndex() 
	{
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			TriCell[] pathCells = getPathCells(iPath);
			MasterPath mp = new MasterPath(PATH_ID
					, pathCells
					, mMesh.getPlaneTolerence()
					, OFFSET_SCALE);
			for (int iCell = 0; iCell < mp.size(); iCell++)
			{
				assertTrue(mp.getCellIndex(pathCells[iCell]) == iCell);
				testCount++;
			}
		}
		if (testCount == 0)
			fail("Mesh doesn't support test.");
	}

	@Test
	public void testGetRawCopy() 
	{
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			TriCell[] pathCells = getPathCells(iPath);
			MasterPath mp = new MasterPath(PATH_ID
					, pathCells
					, mMesh.getPlaneTolerence()
					, OFFSET_SCALE);
			TriCell[] pc = new TriCell[mp.size()];
			TriCell[] outPathCells = mp.getRawCopy(pc);
			assertTrue(outPathCells == pc);
			assertTrue(outPathCells.length == pathCells.length);
			for (int iCell = 0; iCell < outPathCells.length; iCell++)
			{
				assertTrue(outPathCells[iCell] == pathCells[iCell]);
				testCount++;
			}
		}
		if (testCount == 0)
			fail("Mesh doesn't support test.");
	}
	
	private TriCell[] getPathCells(int pathIndex)
	{
		int[] pathPolys = mMesh.getPathPolys(pathIndex);
		TriCell[] result = new TriCell[pathPolys.length];
		for (int i = 0; i < pathPolys.length; i++)
		{
			result[i] = cells[pathPolys[i]];
		}
		return result;
	}

}

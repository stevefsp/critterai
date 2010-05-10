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
import static org.critterai.math.MathUtil.TOLERANCE_STD;

import org.critterai.math.Vector3;
import org.critterai.nav.MasterPath;
import org.critterai.nav.TriCell;
import org.critterai.nav.MasterPath.Path;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Path} class.
 */
public final class PathTest 
{

	/*
	 * Design notes:
	 * 
	 */
	
	private static final float OFFSET_SCALE = 0.1f;
	private static final int PATH_ID = 5;
	
	private ITestMesh mMesh;
	private TriCell[] mCells;

	@Before
	public void setUp() throws Exception 
	{
		mMesh = new CorridorMesh();
		float[] verts = mMesh.getVerts();
		int[] indices = mMesh.getIndices();
		mCells = getAllCells(verts, indices);
		linkAllCells(mCells);
	}

	@Test
	public void testGetGoal() 
	{
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			final TriCell[] pathCells = getPathCells(iPath);
			final MasterPath mp = new MasterPath(PATH_ID
					, pathCells
					, mMesh.getPlaneTolerence()
					, OFFSET_SCALE);
			final float[] points = mMesh.getPathPoints(iPath);
			final Path p = mp.getPath(points[3], points[4], points[5]);
			assertTrue(p.goalX == points[3]);
			assertTrue(p.goalY == points[4]);
			assertTrue(p.goalZ == points[5]);
			final Vector3 v = new Vector3();
			Vector3 goal = p.getGoal(v);
			assertTrue(goal == v);
			assertTrue(goal.equals(points[3], points[4], points[5]));
			testCount++;
		}
		if (testCount == 0)
			fail("Mesh doesn't support test.");
	}

	@Test
	public void testId() 
	{
		final MasterPath mp = new MasterPath(PATH_ID
				, mCells[0]
				, mMesh.getPlaneTolerence()
				, OFFSET_SCALE);
		assertTrue(mp.getPath(mCells[0].centroidX
				, mCells[0].centroidY
				, mCells[0].centroidZ).id() == mp.id);
	}

	@Test
	public void testIsDisposed() 
	{
		final MasterPath mp = new MasterPath(PATH_ID
				, mCells[0]
				, mMesh.getPlaneTolerence()
				, OFFSET_SCALE);
		final Path p = mp.getPath(mCells[0].centroidX
				, mCells[0].centroidY
				, mCells[0].centroidZ);
		assertTrue(p.isDisposed() == mp.isDisposed());
		mp.dispose();
		assertTrue(p.isDisposed() == mp.isDisposed());
	}
	
	@Test
	public void testIsInPathColumn() 
	{
		/*
		 * Assuming that cell class is functioning properly.
		 * So not testing edge cases.
		 * Only testing to make sure that the entire
		 * column is being searched.
		 */
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			final TriCell[] pathCells = getPathCells(iPath);
			final MasterPath mp = new MasterPath(PATH_ID
					, pathCells
					, mMesh.getPlaneTolerence()
					, OFFSET_SCALE);
			final float[] points = mMesh.getPathPoints(iPath);
			final Path p = mp.getPath(points[3], points[4], points[5]);
			final float[] min = mMesh.getMinVertex();
			assertFalse(p.isInPathColumn(min[0] - 1, min[2] - 1));
			for (int iCell = 0; iCell < pathCells.length; iCell++)
			{
				assertTrue(p.isInPathColumn(pathCells[iCell].centroidX, pathCells[iCell].centroidZ));
				testCount++;	
			}
		}
		if (testCount == 0)
			fail("Mesh doesn't support test.");
	}	
	
	@Test
	public void testForceYToPath() 
	{
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			final TriCell[] pathCells = getPathCells(iPath);
			final MasterPath mp = new MasterPath(PATH_ID
					, pathCells
					, mMesh.getPlaneTolerence()
					, OFFSET_SCALE);
			final float[] points = mMesh.getPathPoints(iPath);
			final Path p = mp.getPath(points[3], points[4], points[5]);
			final float[] min = mMesh.getMinVertex();
			final Vector3 v = new Vector3();
			assertFalse(p.forceYToPath(min[0] - 1, min[1] - 1, min[2] - 1, v));
			for (int iCell = 0; iCell < pathCells.length; iCell++)
			{
				float centX = pathCells[iCell].centroidX;
				float centY = pathCells[iCell].centroidY;
				float centZ = pathCells[iCell].centroidZ;
				assertTrue(p.forceYToPath(centX
						, centY + mMesh.getPlaneTolerence()
						, centZ
						, v));
				assertTrue(v.sloppyEquals(centX, centY, centZ, TOLERANCE_STD));
				testCount++;
			}
		}
		if (testCount == 0)
			fail("Mesh doesn't support test.");
	}	
	
//	@Test
//	public void testIsOnPath() 
//	{
//		int testCount = 0;
//		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
//		{
//			TriCell[] pathCells = getPathCells(iPath);
//			MasterPath mp = new MasterPath(PATH_ID
//					, pathCells
//					, mMesh.getPlaneTolerence()
//					, OFFSET_SCALE);
//			float[] points = mMesh.getPathPoints(iPath);
//			Path p = mp.getPath(points[3], points[4], points[5]);
//			float[] min = mMesh.getMinVertex();
//			assertFalse(p.isOnPath(min[0] - 1, min[1] - 1, min[2] - 1, mMesh.getPlaneTolerence()));
//			for (int iCell = 0; iCell < pathCells.length; iCell++)
//			{
//				float x = pathCells[iCell].centroidX;
//				float y = pathCells[iCell].centroidY;
//				float z = pathCells[iCell].centroidZ;
//				/*
//				 * Algorithm is susceptible to floating point
//				 * errors.  So can only test for a fuzzy edge.
//				 */
//				// Above
//				assertTrue(p.isOnPath(x
//						, y + mMesh.getPlaneTolerence() - 4*TOLERANCE_STD
//						, z
//						, mMesh.getPlaneTolerence()));
//				assertFalse(p.isOnPath(x
//						, y + mMesh.getPlaneTolerence() + 4*TOLERANCE_STD
//						, z
//						, mMesh.getPlaneTolerence()));
//				// Below
//				assertTrue(p.isOnPath(x
//						, y - mMesh.getPlaneTolerence() + 4*TOLERANCE_STD
//						, z
//						, mMesh.getPlaneTolerence()));
//				assertFalse(p.isOnPath(x
//						, y - mMesh.getPlaneTolerence() - 4*TOLERANCE_STD
//						, z
//						, mMesh.getPlaneTolerence()));
//				testCount++;
//			}
//		}
//		if (testCount == 0)
//			fail("Mesh doesn't support test.");
//	}
	
	@Test
	public void testForceToPath() 
	{
		/* 
		 * This is just a wrapper operation for a TriCell operation.
		 * So not testing edge cases.
		 * Basically, we are locating a wall on the each cell that is not
		 * part of the path, then creating a test point just outside that wall.
		 */
		int testCount = 0;
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			final TriCell[] pathCells = getPathCells(iPath);
			final MasterPath mp = new MasterPath(PATH_ID
					, pathCells
					, mMesh.getPlaneTolerence()
					, OFFSET_SCALE);
			final float[] points = mMesh.getPathPoints(iPath);
			Path p = mp.getPath(points[3], points[4], points[5]);
			for (int iCell = 0; iCell < pathCells.length; iCell++)
			{
				final TriCell pathCell = pathCells[iCell];
				int iLinkedWallPre = -1;
				int iLinkedWallNext = -1;
				if (iCell+1 < pathCells.length)
					iLinkedWallNext = pathCells[iCell].getLinkIndex(pathCells[iCell+1]);
				if (iCell-1 >= 0)
					iLinkedWallPre = pathCells[iCell].getLinkIndex(pathCells[iCell-1]);
				for (int iWall = 0; iWall < pathCell.maxLinks(); iWall++)
				{
					if (iWall == iLinkedWallPre || iWall == iLinkedWallNext)
						continue;
					// Get a point that is slightly outside the wall.
					Vector3 u = pathCell.getWallLeftVertex(iWall, new Vector3());
					Vector3 v = pathCell.getWallRightVertex(iWall, new Vector3());
					u.add(v);
					u.divide(2);	// u is now the midpoint of the wall.
					v = Vector3.subtract(u.x, u.y, u.z
							, pathCell.centroidX, pathCell.centroidY
							, pathCell.centroidZ, v).multiply(0.1f);
					u.add(v);
					assertFalse(pathCell.isInColumn(u.x, u.z));  // Only a logic error check.
					u = p.forceToPath(u.x, u.y, u.z, v);
					assertTrue(u == v);
					assertTrue(pathCell.isInColumn(u.x, u.z));
				}
				testCount++;	
			}
		}
		if (testCount == 0)
			fail("Mesh doesn't support test.");
	}
	
	@Test
	public void testGetTargetNoOffset() 
	{
		float[] pverts = {
				-1, -5, -2
				, 0, -5.5f, -1
				, 0, -5, -2
				, -1, -5.5f, -1
				, -0.8f, -6, 0
				, -2, -6, 0
				, -1.2f, -5.5f, 1
				, -2, -5.5f, 2
				, -1, -5.5f, 2
				, 0, -5, 2
				, 0, -5, 1
				, 1, -4.5f, 2
		};
		
		TriCell[] pcells = new TriCell[10];
		pcells[0] = new TriCell(pverts, 0, 1, 2);
		pcells[1] = new TriCell(pverts, 0, 3, 1);
		pcells[2] = new TriCell(pverts, 3, 4, 1);
		pcells[3] = new TriCell(pverts, 3, 5, 4);
		pcells[4] = new TriCell(pverts, 4, 5, 6);
		pcells[5] = new TriCell(pverts, 6, 5, 7);
		pcells[6] = new TriCell(pverts, 7, 8, 6);
		pcells[7] = new TriCell(pverts, 6, 8, 9);
		pcells[8] = new TriCell(pverts, 10, 6, 9);
		pcells[9] = new TriCell(pverts, 9, 11, 10);
		pcells[0].link(pcells[1], true);
		pcells[1].link(pcells[2], true);
		pcells[2].link(pcells[3], true);
		pcells[3].link(pcells[4], true);
		pcells[4].link(pcells[5], true);
		pcells[5].link(pcells[6], true);
		pcells[6].link(pcells[7], true);
		pcells[7].link(pcells[8], true);
		pcells[8].link(pcells[9], true);
		
		Vector3 start = new Vector3(-1, -5, -2);
		Vector3 goal = new Vector3(0.2f, -5, 1.8f);
		
		MasterPath mp = new MasterPath(PATH_ID, pcells, 0.5f, 0);
		Path p = mp.getPath(goal.x, goal.y, goal.z);
		
		Vector3[] targets = new Vector3[3];
		targets[0] = new Vector3(pverts[3*3+0], pverts[3*3+1], pverts[3*3+2]);
		targets[1] = new Vector3(pverts[6*3+0], pverts[6*3+1], pverts[6*3+2]);
		targets[2] = goal;
		
		Vector3 currTarget = new Vector3(start.x, start.y, start.z);
		
		for (int i = 0; i < targets.length; i++)
		{
			assertTrue(p.getTarget(currTarget.x, currTarget.y, currTarget.z, currTarget));
			assertTrue(currTarget.sloppyEquals(targets[i], TOLERANCE_STD));
		}
	}
	
	@Test
	public void testGetTargetWithOffset() 
	{
		float[] pverts = {
				-1, -5, -2
				, 0, -5.5f, -1
				, 0, -5, -2
				, -1, -5.5f, -1
				, -0.8f, -6, 0
				, -2, -6, 0
				, -1.2f, -5.5f, 1
				, -2, -5.5f, 2
				, -1, -5.5f, 2
				, 0, -5, 2
				, 0, -5, 1
				, 1, -4.5f, 2
		};
		
		TriCell[] pcells = new TriCell[10];
		pcells[0] = new TriCell(pverts, 0, 1, 2);
		pcells[1] = new TriCell(pverts, 0, 3, 1);
		pcells[2] = new TriCell(pverts, 3, 4, 1);
		pcells[3] = new TriCell(pverts, 3, 5, 4);
		pcells[4] = new TriCell(pverts, 4, 5, 6);
		pcells[5] = new TriCell(pverts, 6, 5, 7);
		pcells[6] = new TriCell(pverts, 7, 8, 6);
		pcells[7] = new TriCell(pverts, 6, 8, 9);
		pcells[8] = new TriCell(pverts, 10, 6, 9);
		pcells[9] = new TriCell(pverts, 9, 11, 10);
		pcells[0].link(pcells[1], true);
		pcells[1].link(pcells[2], true);
		pcells[2].link(pcells[3], true);
		pcells[3].link(pcells[4], true);
		pcells[4].link(pcells[5], true);
		pcells[5].link(pcells[6], true);
		pcells[6].link(pcells[7], true);
		pcells[7].link(pcells[8], true);
		pcells[8].link(pcells[9], true);
		
		Vector3 start = new Vector3(-1, -5, -2);
		Vector3 goal = new Vector3(0.2f, -5, 1.8f);
		
		MasterPath mp = new MasterPath(PATH_ID, pcells, 0.5f, OFFSET_SCALE);
		Path p = mp.getPath(goal.x, goal.y, goal.z);
		
		Vector3[] targets = new Vector3[3];
		targets[0] = Vector3.translateToward(pverts[3*3+0], pverts[3*3+1], pverts[3*3+2]
		                          		         , pverts[4*3+0], pverts[4*3+1], pverts[4*3+2]                                    
		                          		         , OFFSET_SCALE
		                          		         , new Vector3());
		targets[1] = Vector3.translateToward(pverts[6*3+0], pverts[6*3+1], pverts[6*3+2]
			                          		     , pverts[7*3+0], pverts[7*3+1], pverts[7*3+2]                                    
				                          		 , OFFSET_SCALE
				                          		 , new Vector3());
		targets[2] = goal;
		
		Vector3 currTarget = new Vector3(start.x, start.y, start.z);
		
		for (int i = 0; i < targets.length; i++)
		{
			assertTrue(p.getTarget(currTarget.x, currTarget.y, currTarget.z, currTarget));
			assertTrue(currTarget.sloppyEquals(targets[i], TOLERANCE_STD));
		}
	}
	
	@Test
	public void testGetPathPolys()
	{
		for (int iPath = 0; iPath < mMesh.getPathCount(); iPath++)
		{
			final TriCell[] pathCells = getPathCells(iPath);
			final MasterPath mp = new MasterPath(PATH_ID
					, pathCells
					, mMesh.getPlaneTolerence()
					, OFFSET_SCALE);
			final float[] points = mMesh.getPathPoints(iPath);
			final Path p = mp.getPath(points[3], points[4], points[5]);
			assertTrue(p.pathPolyCount() == pathCells.length);
			assertTrue(p.pathVertCount() == pathCells.length * 3);
			float[] verts = new float[p.pathVertCount()*3];
			int[] indices = new int[p.pathPolyCount()*3];
			p.getPathPolys(verts, indices);
			for (int iCell = 0; iCell < pathCells.length; iCell++)
			{
				int pPoly = iCell*3;
				for (int iPolyVert = 0; iPolyVert < 3; iPolyVert++)
				{
					int pVert = indices[pPoly+iPolyVert]*3;
					assertTrue(pathCells[iCell].getVertexValue(iPolyVert, 0) == verts[pVert+0]);
					assertTrue(pathCells[iCell].getVertexValue(iPolyVert, 1) == verts[pVert+1]);
					assertTrue(pathCells[iCell].getVertexValue(iPolyVert, 2) == verts[pVert+2]);
				}
			}
		}
	}
	
	private TriCell[] getPathCells(int pathIndex)
	{
		final int[] pathPolys = mMesh.getPathPolys(pathIndex);
		final TriCell[] result = new TriCell[pathPolys.length];
		for (int i = 0; i < pathPolys.length; i++)
		{
			result[i] = mCells[pathPolys[i]];
		}
		return result;
	}

}

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
import static org.junit.Assert.*;
import static org.critterai.math.MathUtil.TOLERANCE_STD;

import org.critterai.math.Vector2;
import org.critterai.math.Vector3;
import org.critterai.math.geom.Polygon3;
import org.critterai.nav.TriCell;
import org.critterai.nav.TriNavMesh;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link TriNavMesh} class.
 */
public final class TriNavMeshTests 
{

	/*
	 * Design notes:
	 * 
	 * Validity of these tests are highly dependent on the validity of
	 * the NavPoly and NavPolyQuadTree classes.
	 * 
	 * Full functionality is not being tested.  Tests are abbreviated 
	 * based on knowledge of the internal design of the class.
	 */
	
	private ITestMesh mMesh;
	private static final float PLANE_TOL = 0.5f;
	private static final float OFFSET_SCALE = 0.05f;
	
	@Before
	public void setUp() throws Exception 
	{
		mMesh = new CorridorMesh();
	}

	@Test
	public void testBasics() 
	{
		final float[] verts = mMesh.getVerts();
		final int[] indices = mMesh.getIndices();
		TriNavMesh nm = TriNavMesh.build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
		assertTrue(nm.planeTolerance() == PLANE_TOL);
		assertTrue(nm.offsetScale() == OFFSET_SCALE);
	}

	@Test
	public void testGetClosestCell() 
	{
		final float[] verts = mMesh.getVerts();
		final int[] indices = mMesh.getIndices();
		final TriCell[] cells = getAllCells(verts, indices);
		TriNavMesh nm = TriNavMesh.build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
		Vector3 v = new Vector3();
		Vector3 u = new Vector3();
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			final int pCell = iCell*3;
			final float ax = verts[indices[pCell]*3];
			final float ay = verts[indices[pCell]*3+1];
			final float az = verts[indices[pCell]*3+2];
			final float bx = verts[indices[pCell+1]*3];
			final float by = verts[indices[pCell+1]*3+1];
			final float bz = verts[indices[pCell+1]*3+2];
			final float cx = verts[indices[pCell+2]*3];
			final float cy = verts[indices[pCell+2]*3+1];
			final float cz = verts[indices[pCell+2]*3+2];
			final Vector3 cent = Polygon3.getCentroid(new Vector3()
											, ax, ay, az
											, bx, by, bz
											, cx, cy, cz);
			TriCell cell = nm.getClosestCell(cent.x, cent.y, cent.z, true, v);
			assertTrue(v.sloppyEquals(cent, TOLERANCE_STD));
			for (int i = 0; i < cell.maxLinks(); i++)
			{
				assertTrue(cell.getVertex(i, v).equals(cells[iCell].getVertex(i, u)));
			}
		}
		final float[] va = mMesh.getMinVertex();
		TriCell np = nm.getClosestCell(va[0] - TOLERANCE_STD
				, va[1] - TOLERANCE_STD
				, va[2] - TOLERANCE_STD
				, true
				, v);
		assertTrue(np == null);
	}

	@Test
	public void testIsValidPosition() 
	{
		final float[] verts = mMesh.getVerts();
		final int[] indices = mMesh.getIndices();
		final TriCell[] cells = getAllCells(verts, indices);
		TriNavMesh nm = TriNavMesh.build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			final int pCell = iCell*3;
			final float ax = verts[indices[pCell]*3];
			final float ay = verts[indices[pCell]*3+1];
			final float az = verts[indices[pCell]*3+2];
			final float bx = verts[indices[pCell+1]*3];
			final float by = verts[indices[pCell+1]*3+1];
			final float bz = verts[indices[pCell+1]*3+2];
			final float cx = verts[indices[pCell+2]*3];
			final float cy = verts[indices[pCell+2]*3+1];
			final float cz = verts[indices[pCell+2]*3+2];
			final Vector3 cent = Polygon3.getCentroid(new Vector3()
											, ax, ay, az
											, bx, by, bz
											, cx, cy, cz);
			assertTrue(Integer.toString(iCell)
					, nm.isValidPosition(cent.x, cent.y, cent.z, TOLERANCE_STD));
			assertFalse(Integer.toString(iCell)
					, nm.isValidPosition(cent.x, cent.y + 2*TOLERANCE_STD, cent.z, TOLERANCE_STD));
			assertFalse(Integer.toString(iCell)
					, nm.isValidPosition(cent.x, cent.y - 2*TOLERANCE_STD, cent.z, TOLERANCE_STD));
		}
	}

	@Test
	public void testHasLOSTrue() 
	{
		final float[] verts = mMesh.getVerts();
		final int[] indices = mMesh.getIndices();
		final TriCell[] cells = getAllCells(verts, indices);
		final int[] losPolys = mMesh.getLOSPolysTrue();
		final float[] losPoints = mMesh.getLOSPointsTrue();
		TriNavMesh nm = TriNavMesh.build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
		final int pathCount = losPoints.length / 4;
		for (int iPath = 0; iPath < pathCount; iPath++)
		{
			final int pPathPoint = iPath*4;
			final int pPathPoly = iPath*2;
			TriCell startPoly = cells[losPolys[pPathPoly]];
			startPoly = nm.getClosestCell(startPoly.centroidX
					, startPoly.centroidY
					, startPoly.centroidZ
					, true, null);
			TriCell endPoly = cells[losPolys[pPathPoly+1]];
			endPoly = nm.getClosestCell(endPoly.centroidX
					, endPoly.centroidY
					, endPoly.centroidZ
					, true, null);
			boolean actual = TriNavMesh.hasLOS(losPoints[pPathPoint]
					, losPoints[pPathPoint+1]
					, losPoints[pPathPoint+2]
					, losPoints[pPathPoint+3]
					, startPoly
					, endPoly
					, OFFSET_SCALE
					, new Vector2()
					, new TriCell[1]);
			assertTrue(iPath + ":f", actual);
			// Reverse path.
			actual = TriNavMesh.hasLOS(losPoints[pPathPoint+2]
  					, losPoints[pPathPoint+3]
  					, losPoints[pPathPoint]
  					, losPoints[pPathPoint+1]
  					, endPoly
  					, startPoly
					, OFFSET_SCALE
  					, new Vector2()
  					, new TriCell[1]);
			assertTrue(iPath + ":r", actual);
		}
	}

	@Test
	public void testHasLOSFalse() 
	{
		final float[] verts = mMesh.getVerts();
		final int[] indices = mMesh.getIndices();
		final TriCell[] cells = getAllCells(verts, indices);
		final int[] losPolys = mMesh.getLOSPolysFalse();
		final float[] losPoints = mMesh.getLOSPointsFalse();
		TriNavMesh nm = TriNavMesh.build(verts, indices, 10, PLANE_TOL, OFFSET_SCALE);
		final int pathCount = losPoints.length / 4;
		for (int iPath = 0; iPath < pathCount; iPath++)
		{
			final int pPathPoint = iPath*4;
			final int pPathPoly = iPath*2;
			TriCell startPoly = cells[losPolys[pPathPoly]];
			startPoly = nm.getClosestCell(startPoly.centroidX
					, startPoly.centroidY
					, startPoly.centroidZ
					, true, null);
			TriCell endPoly = cells[losPolys[pPathPoly+1]];
			endPoly = nm.getClosestCell(endPoly.centroidX
					, endPoly.centroidY
					, endPoly.centroidZ
					, true, null);
			boolean actual = TriNavMesh.hasLOS(losPoints[pPathPoint]
					, losPoints[pPathPoint+1]
					, losPoints[pPathPoint+2]
					, losPoints[pPathPoint+3]
					, startPoly
					, endPoly
					, OFFSET_SCALE
					, new Vector2()
					, new TriCell[1]);
			assertFalse(iPath + ":f", actual);
			// Reverse path.
			actual = TriNavMesh.hasLOS(losPoints[pPathPoint+2]
  					, losPoints[pPathPoint+3]
  					, losPoints[pPathPoint]
  					, losPoints[pPathPoint+1]
  					, endPoly
  					, startPoly
					, OFFSET_SCALE
  					, new Vector2()
  					, new TriCell[1]);
			assertFalse(iPath + ":r", actual);
		}
	}
	
}

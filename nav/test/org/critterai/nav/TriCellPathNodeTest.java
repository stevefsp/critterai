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

import static org.critterai.nav.TestUtil.*;
import static org.critterai.math.MathUtil.*;
import static org.junit.Assert.*;

import org.critterai.math.Vector3;
import org.critterai.math.geom.Polygon3;
import org.critterai.nav.TriCell;
import org.critterai.nav.TriCellPathNode;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link TriCellPathNode} class.
 */
public final class TriCellPathNodeTest 
{

	/*
	 * Design notes:
	 * 
	 * There are a lot of dependencies.  The tests are ordered
	 * from minimal to maximal dependence.
	 * 
	 * If the TriCell test suite fails, fix it before fixing any errors
	 * from this test suite.
	 */
	
	private ITestMesh mMesh;
	private float[] verts;
	private int[] indices;
	private TriCell[] cells;
	private Vector3[] cents;
	
	@Before
	public void setUp() throws Exception 
	{
		mMesh = new CorridorMesh();
		verts = mMesh.getVerts();
		indices = mMesh.getIndices();
		cells = getAllCells(verts, indices);
		linkAllCells(cells);
		cents = new Vector3[cells.length];
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			int pCell = iCell*3;
			final Vector3 cent = Polygon3.getCentroid(new Vector3()
											, verts[indices[pCell]*3]
											, verts[indices[pCell]*3+1]
											, verts[indices[pCell]*3+2]
											, verts[indices[pCell+1]*3]
											, verts[indices[pCell+1]*3+1]
											, verts[indices[pCell+1]*3+2]
											, verts[indices[pCell+2]*3]
											, verts[indices[pCell+2]*3+1]
											, verts[indices[pCell+2]*3+2]);
			cents[iCell] = cent;
		}
	}

	@Test
	public void testCell() 
	{
		int testCount = 0;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					break;
				TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
				Vector3 cent = cents[iCell];
				TriCellPathNode child = new TriCellPathNode(childCell
						, parent
						, cent.x, cent.y, cent.z);
				assertTrue(child.cell() == childCell);
				assertTrue(parent.cell() == cells[iCell]);
				testCount++;
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}
	
	@Test
	public void testParent() 
	{
		int testCount = 0;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				// Build a path 3 cells deep.
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					// Can't get to two cells deep.
					break;
				for (int iWallNext = 0; iWallNext < 3; iWallNext++)
				{
					TriCell childChildCell = childCell.getLink(iWallNext);
					if (childChildCell == null)
						// Can't get to three cells deep.
						break;
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					// Test accuracy.
					assertTrue(parent.parent() == null);
					assertTrue(child.parent() == parent);
					assertTrue(childChild.parent() == child);
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}

	@Test
	public void testPathSize() 
	{
		int testCount = 0;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				// Build a path 3 cells deep.
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					// Can't get to two cells deep.
					break;
				for (int iWallNext = 0; iWallNext < 3; iWallNext++)
				{
					TriCell childChildCell = childCell.getLink(iWallNext);
					if (childChildCell == null)
						// Can't get to three cells deep.
						break;
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					// Test accuracy.
					assertTrue(parent.pathSize() == 1);
					assertTrue(child.pathSize() == 2);
					assertTrue(childChild.pathSize() == 3);
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}
	
	@Test
	public void testH()
	{
		int testCount = 0;
		final float parentH = 20;
		final float childH = 10;
		final float childChildH = 5;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				// Build a path 3 cells deep.
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					// Can't get to two cells deep.
					break;
				for (int iWallNext = 0; iWallNext < 3; iWallNext++)
				{
					TriCell childChildCell = childCell.getLink(iWallNext);
					if (childChildCell == null)
						// Can't get to three cells deep.
						break;
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					parent.setH(parentH);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					child.setH(childH);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					childChild.setH(childChildH);
					// Test accuracy.
					assertTrue(parent.h() == parentH);
					float expected = childH;
					float actual = child.h();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					expected = childChildH;
					actual = childChild.h();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}

	@Test
	public void testLocalG() 
	{
		int testCount = 0;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				// Build a path 3 cells deep.
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					// Can't get to two cells deep.
					break;
				for (int iWallNext = 0; iWallNext < 3; iWallNext++)
				{
					TriCell childChildCell = childCell.getLink(iWallNext);
					if (childChildCell == null)
						// Can't get to three cells deep.
						break;
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					// Test accuracy.
					assertTrue(parent.localG() == 0);
					float expected = (float)Math.sqrt(cells[iCell]
					                         .getLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall));
					float actual = child.localG();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					expected = childCell
						.getLinkPointDistance(iWallNext, childCell.getLinkIndex(cells[iCell]));
					actual = childChild.localG();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}
	
	@Test
	public void testG() 
	{
		int testCount = 0;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				// Build a path 3 cells deep.
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					// Can't get to two cells deep.
					break;
				for (int iWallNext = 0; iWallNext < 3; iWallNext++)
				{
					TriCell childChildCell = childCell.getLink(iWallNext);
					if (childChildCell == null)
						// Can't get to three cells deep.
						break;
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					// Test accuracy.
					assertTrue(parent.g() == 0);
					float expected = (float)Math.sqrt(cells[iCell]
					                         .getLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall));
					float actual = child.g();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					expected += childCell
						.getLinkPointDistance(iWallNext, childCell.getLinkIndex(cells[iCell]));
					actual = childChild.g();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}
	
	@Test
	public void testFWithoutHeuristic() 
	{
		int testCount = 0;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				// Build a path 3 cells deep.
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					// Can't get to two cells deep.
					break;
				for (int iWallNext = 0; iWallNext < 3; iWallNext++)
				{
					TriCell childChildCell = childCell.getLink(iWallNext);
					if (childChildCell == null)
						// Can't get to three cells deep.
						break;
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					// Test accuracy of f().
					assertTrue(parent.f() == 0);
					float expected = (float)Math.sqrt(cells[iCell]
					                         .getLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall));
					float actual = child.f();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					expected += childCell
						.getLinkPointDistance(iWallNext, childCell.getLinkIndex(cells[iCell]));
					actual = childChild.f();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}

	@Test
	public void testFWithHeuristic() 
	{
		int testCount = 0;
		final float parentH = 20;
		final float childH = 10;
		final float childChildH = 5;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				// Build a path 3 cells deep.
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					// Can't get to two cells deep.
					break;
				for (int iWallNext = 0; iWallNext < 3; iWallNext++)
				{
					TriCell childChildCell = childCell.getLink(iWallNext);
					if (childChildCell == null)
						// Can't get to three cells deep.
						break;
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					parent.setH(parentH);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					child.setH(childH);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					childChild.setH(childChildH);
					// Test accuracy of f().
					assertTrue(parent.f() == 0 + parentH);
					float expected = (float)Math.sqrt(cells[iCell]
					                         .getLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall))
					                         + childH;
					float actual = child.f();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					expected = (float)Math.sqrt(cells[iCell]
								       .getLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall))
						           + childCell
						               .getLinkPointDistance(iWallNext, childCell.getLinkIndex(cells[iCell]))
						           + childChildH;
					actual = childChild.f();
					assertTrue(sloppyEquals(actual, expected, TOLERANCE_STD));
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}
	
	@Test
	public void testLoadPath() 
	{
		int testCount = 0;
		for (int iCell = 0; iCell < cells.length; iCell++)
		{
			for (int iWall = 0; iWall < 3; iWall++)
			{
				// Build a path 3 cells deep.
				TriCell childCell = cells[iCell].getLink(iWall);
				if (childCell == null)
					// Can't get to two cells deep.
					break;
				for (int iWallNext = 0; iWallNext < 3; iWallNext++)
				{
					TriCell childChildCell = childCell.getLink(iWallNext);
					if (childChildCell == null)
						// Can't get to three cells deep.
						break;
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					// Parent
					TriCell[] pcells = new TriCell[1];
					parent.loadPath(pcells);
					pcells[0] = cells[iCell];
					// Child
					pcells = new TriCell[2];
					child.loadPath(pcells);
					pcells[0] = cells[iCell];
					pcells[1] = childCell;
					// Child Child
					pcells = new TriCell[3];
					childChild.loadPath(pcells);
					pcells[0] = cells[iCell];
					pcells[1] = childCell;
					pcells[2] = childChildCell;
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}
	
	@Test
	public void testEstimateNewGAndSetParent() 
	{
		final float[] verts = {
				-4, 0, -2	// 0
				, 2, 0, 4
				, 0, 0, -2
				, -4, 0, 0
				, -2, 1, 0	// 4
		};
		
		final Vector3 start = new Vector3(-2.8f, 0.2f, -0.4f);
		final Vector3 midSA = new Vector3(-3, 0.5f, 0);
		final Vector3 midSB = new Vector3(-1, 0.5f, -1);
		final Vector3 midAG = new Vector3(-3, 0.5f, 1);
		final Vector3 midBG = new Vector3(0, 0.5f, 2);
		
		final float distSA = (float)Math.sqrt(Vector3.getDistanceSq(start, midSA));
		final float distSB = (float)Math.sqrt(Vector3.getDistanceSq(start, midSB));
		final float distAG = (float)Math.sqrt(Vector3.getDistanceSq(midSA, midAG));
		final float distBG = (float)Math.sqrt(Vector3.getDistanceSq(midSB, midBG));
		
		final float distSAG = distSA + distAG;
		final float distSBG = distSB + distBG;
		
		TriCell cellStart = new TriCell(verts, 3, 4, 2);
		TriCell cellMidA = new TriCell(verts, 3, 0, 4);
		TriCell cellMidB = new TriCell(verts, 1, 2, 4);
		TriCell cellGoal = new TriCell(verts, 1, 4, 0);
		cellStart.link(cellMidA, true);
		cellStart.link(cellMidB, true);
		cellGoal.link(cellMidA, true);
		cellGoal.link(cellMidB, true);
		
		TriCellPathNode nodeStart = new TriCellPathNode(cellStart);
		TriCellPathNode nodeMidA = new TriCellPathNode(cellMidA, nodeStart, start.x, start.y, start.z);
		TriCellPathNode nodeMidB = new TriCellPathNode(cellMidB, nodeStart, start.x, start.y, start.z);
		TriCellPathNode nodeGoal = new TriCellPathNode(cellGoal, nodeMidA, start.x, start.y, start.z);
		
		// S-A-G
		float actual = nodeGoal.g();
		assertTrue(sloppyEquals(actual, distSAG, TOLERANCE_STD));
		// Estimate for S-B-G
		actual = nodeGoal.estimateNewG(nodeMidB, start.x, start.y, start.z);
		assertTrue(sloppyEquals(actual, distSBG, TOLERANCE_STD));
		// Re-assign parent
		nodeGoal.setParent(nodeMidB, start.x, start.y, start.z);
		assertTrue(nodeGoal.parent() == nodeMidB);
		// S-B-G
		actual = nodeGoal.g();
		assertTrue(sloppyEquals(actual, distSBG, TOLERANCE_STD));
	}

}

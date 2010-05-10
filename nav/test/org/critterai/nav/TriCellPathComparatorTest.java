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

import java.util.PriorityQueue;

import org.critterai.math.Vector3;
import org.critterai.math.geom.Polygon3;
import org.critterai.nav.TriCell;
import org.critterai.nav.TriCellPathComparator;
import org.critterai.nav.TriCellPathNode;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link TriCellPathComparator} class.
 */
public final class TriCellPathComparatorTest 
{

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
	public void testCompareSimulatedH() 
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
					// First test will be without h-value set to zero.
					TriCellPathNode parent = new TriCellPathNode(cells[iCell]);
					Vector3 cent = cents[iCell];
					TriCellPathNode child = new TriCellPathNode(childCell
							, parent
							, cent.x, cent.y, cent.z);
					TriCellPathNode childChild =  new TriCellPathNode(childChildCell
							, child
							, cent.x, cent.y, cent.z);
					parent.setH(childChild.g() - parent.g() + 3);
					child.setH(childChild.g() - child.g() + 2);
					childChild.setH(1);
					PriorityQueue<TriCellPathNode> queue 
							= new PriorityQueue<TriCellPathNode>(3, new TriCellPathComparator());
					queue.add(parent);
					queue.add(child);
					queue.add(childChild);
					assertTrue(queue.poll() == childChild);
					assertTrue(queue.poll() == child);
					assertTrue(queue.poll() == parent);
					testCount++;
				}
			}
		}
		if (testCount == 0)
			fail("Mesh can't support test.");
	}

}

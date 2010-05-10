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
import static org.junit.Assert.*;
import static org.critterai.math.MathUtil.TOLERANCE_STD;

import java.util.ArrayList;
import java.util.List;

import org.critterai.math.Vector3;
import org.critterai.math.geom.Polygon3;
import org.critterai.nav.TriCell;
import org.critterai.nav.TriCellQuadTree;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link TriCellQuadTree} class.
 */
public final class TriCellQuadTreeTests 
{

    /*
     * Design Notes:
     * 
     * So far, the tests are purely functional.  No validation is being performed 
     * that internal storage is correct.  Because of this, tests often just compare
     * the results from the tree against results from a full scan of all cell.
     * 
     * Various tests in the suite require the proper functioning of NavPoly.
     * If there is a failure in this suite and in NavPoly, check and fix NavPoly first.
     */
    
    private ITestMesh mMesh;
    
    @Before
    public void setUp() throws Exception 
    {
        mMesh = new CorridorMesh();
    }

    @Test
    public void testFieldBounds() 
    {
        TriCellQuadTree tree = new TriCellQuadTree(-8, 2, 5, 7, 9);
        assertTrue(tree.boundsMinX == -8);
        assertTrue(tree.boundsMinZ == 2);
        assertTrue(tree.boundsMaxX == 5);
        assertTrue(tree.boundsMaxZ == 7);
    }
    
    @Test
    public void testFieldMaxDepth() 
    {
        TriCellQuadTree tree = new TriCellQuadTree(-8, 2, 5, 7, 9);
        assertTrue(tree.maxDepth == 9);
    }

    @Test
    public void testAddInBounds() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final float[] meshBounds = getVertBounds(verts);
        final TriCellQuadTree tree = new TriCellQuadTree(meshBounds[0]
                                             , meshBounds[2]
                                             , meshBounds[3]
                                             , meshBounds[5]
                                             , 10);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            assertTrue(iCell + ":ap", tree.add(cells[iCell]));
        }
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            assertFalse(iCell + ":af", tree.add(cells[iCell]));
        }
    }

    @Test
    public void testAddOutOfBounds() 
    {
        // Only performing basic tests.  Many more are possible.
        // Don't reduce any of the bound deltas to less than 3.
        final float minX = -8;
        final float minZ = 2;
        final float maxX = 5;
        final float maxZ = 7;
        final TriCellQuadTree tree = new TriCellQuadTree(minX, minZ, maxX, maxZ, 10);
        // Corner outside checks.
        // Polygon below tree's minX/minZ bounds.
        float[] bverts = new float[9];
        bverts[0] = minX - TOLERANCE_STD;
        bverts[1] = 5;
        bverts[2] = minZ - TOLERANCE_STD;
        bverts[3] = bverts[0] - 1;
        bverts[4] = 5;
        bverts[5] = bverts[2] - 1;
        bverts[6] = bverts[0] - 0.5f;
        bverts[7] = 5;
        bverts[8] = bverts[2] - 0.5f;
        TriCell cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
        // Polygon above tree's maxX/maxZ bounds.
        bverts[0] = maxX + TOLERANCE_STD;
        bverts[2] = maxZ + TOLERANCE_STD;
        bverts[3] = bverts[0] + 1;
        bverts[5] = bverts[2] + 1;
        bverts[6] = bverts[0] + 0.5f;
        bverts[8] = bverts[2] + 0.5f;
        cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
        // Polygon below minX, above maxZ tree bounds.
        bverts[0] = minX - TOLERANCE_STD;
        bverts[2] = maxZ + TOLERANCE_STD;
        bverts[3] = bverts[0] - 1;
        bverts[5] = bverts[2] + 1;
        bverts[6] = bverts[0] - 0.5f;
        bverts[8] = bverts[2] + 0.5f;
        cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
        // Polygon above maxX, below minZ tree bounds.
        bverts[0] = maxX + TOLERANCE_STD;
        bverts[2] = minZ - TOLERANCE_STD;
        bverts[3] = bverts[0] + 1;
        bverts[5] = bverts[2] - 1;
        bverts[6] = bverts[0] + 0.5f;
        bverts[8] = bverts[2] - 0.5f;
        cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
        // Outside wall checks
        // Polygon below minX
        bverts[0] = minX - TOLERANCE_STD;
        bverts[2] = minZ + 0.5f;
        bverts[3] = bverts[0] - 1;
        bverts[5] = bverts[2] + 1;
        bverts[6] = bverts[0] - 0.5f;
        bverts[8] = bverts[2] + 0.5f;
        cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
        // Polygon above maxX
        bverts[0] = maxX + TOLERANCE_STD;
        bverts[2] = minZ + 0.5f;
        bverts[3] = bverts[0] + 1;
        bverts[5] = bverts[2] + 1;
        bverts[6] = bverts[0] + 0.5f;
        bverts[8] = bverts[2] + 0.5f;
        cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
        // Polygon below minY
        bverts[0] = minX + 0.5f;
        bverts[2] = minZ - TOLERANCE_STD;
        bverts[3] = bverts[0] + 1;
        bverts[5] = bverts[2] - 1;
        bverts[6] = bverts[0] + 0.5f;
        bverts[8] = bverts[2] - 0.5f;
        cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
        // Polygon below minY
        bverts[0] = minX + 0.5f;
        bverts[2] = maxZ + TOLERANCE_STD;
        bverts[3] = bverts[0] + 1;
        bverts[5] = bverts[2] + 1;
        bverts[6] = bverts[0] + 0.5f;
        bverts[8] = bverts[2] + 0.5f;
        cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
        // Overlapping
        bverts[0] = minX - TOLERANCE_STD;
        bverts[2] = minZ + 0.5f;
        bverts[3] = bverts[0] + 1;
        bverts[5] = bverts[2] + 1;
        bverts[6] = bverts[0] + 0.5f;
        bverts[8] = bverts[2] + 0.5f;
        cell = new TriCell(bverts, 0, 1, 2);
        assertFalse(tree.add(cell));
    }
    
    @Test
    public void testGetCells() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final TriCellQuadTree tree = loadTree(verts, cells);
        ArrayList<TriCell> list = new ArrayList<TriCell>();
        List<TriCell> cellList = tree.getCells(list);
        assertTrue(cellList == list);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            assertTrue(iCell + ":missing", cellList.contains(cells[iCell]));
        }
        assertTrue(cells.length == cellList.size());
    }

    @Test
    public void testGetCellsInColumn() 
    {
        /*
         * Not trying to test all edge cases since most of the work
         * in this operation depends on the proper functioning of NavPoly, and NavPoly
         * testing is expected to deal with edge cases.
         */
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final TriCellQuadTree tree = loadTree(verts, cells);
        // Full encompass.
        final ArrayList<TriCell> list = new ArrayList<TriCell>();
        List<TriCell> cellList = tree.getCellsInColumn(tree.boundsMinX, tree.boundsMinZ
                , tree.boundsMaxX, tree.boundsMaxZ
                , list);
        assertTrue(cellList == list);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            assertTrue(iCell + ":missing", cellList.contains(cells[iCell]));
        }
        assertTrue(cells.length == cellList.size());
        // Internal AABB
        final float offsetX = 0.22f * (tree.boundsMaxX - tree.boundsMinX);  // Factor is arbitrary.
        final float offsetZ = 0.22f * (tree.boundsMaxZ - tree.boundsMinZ);
        float minx = tree.boundsMinX + offsetX;
        float minz = tree.boundsMinZ + offsetZ;
        float maxx = tree.boundsMaxX - offsetX;
        float maxz = tree.boundsMaxZ - offsetZ;
        cellList = tree.getCellsInColumn(minx, minz, maxx, maxz, list);    
        assertTrue(isCellListGood(minx, minz, maxx, maxz, cells, cellList));
        // Offset AABB's
        for (int i = 0; i < 3; i++)
        {
            int xFactor = 0;
            int zFactor = 0;
            switch (i)
            {
            case 0: xFactor = 1; zFactor = 1; break;
            case 1: xFactor = -1; zFactor = 1; break;
            case 2: xFactor = -1; zFactor = -1; break;
            case 3: xFactor = 1; zFactor = -1; break;
            }
            minx = tree.boundsMinX + offsetX * xFactor;
            minz = tree.boundsMinZ + offsetZ * zFactor;
            maxx = tree.boundsMaxX + offsetX * xFactor;
            maxz = tree.boundsMaxZ + offsetZ * zFactor;
            cellList = tree.getCellsInColumn(minx, minz, maxx, maxz, list);    
            assertTrue(isCellListGood(minx, minz, maxx, maxz, cells, cellList));
        }
    }

    @Test
    public void testGetCellsForPoint() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final TriCellQuadTree tree = loadTree(verts, cells);
        final ArrayList<TriCell> allPolys = new ArrayList<TriCell>();
        for (TriCell cell : cells)
        {
            allPolys.add(cell);
        }
        // Creating a duplicate mesh translated up the y-axis by 1.5 units.
        // This will improve testing by ensuring that all results return more
        // than a single cell.
        final float[] offsetVerts = mMesh.getVerts();
        for (int pVert = 1; pVert < offsetVerts.length; pVert += 3)
        {
            offsetVerts[pVert] += 1.5f;
        }
        final TriCell[] offsetPolys = getAllCells(offsetVerts, indices);
        for (int iCell = 0; iCell < offsetPolys.length; iCell++)
        {
            tree.add(offsetPolys[iCell]);
            allPolys.add(offsetPolys[iCell]);
        }
        ArrayList<TriCell> list = new ArrayList<TriCell>();
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
            // Test by centroid.  (Internal to cell.)
            List<TriCell> cellList = tree.getCellsForPoint(cent.x, cent.z, list);
            assertTrue(cellList == list);
            assertTrue(cellList.size() == 2);
            // Test by vertex.
            assertTrue(isCellListGood(cent.x, cent.z, allPolys, cellList));
            cellList = tree.getCellsForPoint(ax, az, list);
            assertTrue(isCellListGood(ax, az, allPolys, cellList));
            cellList = tree.getCellsForPoint(bx, bz, list);
            assertTrue(isCellListGood(bx, bz, allPolys, cellList));
            cellList = tree.getCellsForPoint(cx, cz, list);
            assertTrue(isCellListGood(cx, cz, allPolys, cellList));
            // Test by wall midpoint. (Only testing the AB wall.)
            cellList = tree.getCellsForPoint((ax + bx) / 2, (az + bz) / 2, list);
            assertTrue(isCellListGood((ax + bx) / 2, (az + bz) / 2, allPolys, cellList));
        }
    }

    @Test
    public void testGetClosestCell() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final TriCellQuadTree tree = loadTree(verts, cells);
        final ArrayList<TriCell> allPolys = new ArrayList<TriCell>();
        final int[] linkCells = mMesh.getLinkPolys();
        for (TriCell cell : cells)
        {
            allPolys.add(cell);
        }
        // Creating a duplicate mesh translated up the y-axis by 1.5 units.
        final float[] offsetVerts = mMesh.getVerts();
        final float yOffset = 1.5f;
        for (int pVert = 1; pVert < offsetVerts.length; pVert += 3)
        {
            offsetVerts[pVert] += yOffset;
        }
        final TriCell[] offsetPolys = getAllCells(offsetVerts, indices);
        for (int iCell = 0; iCell < offsetPolys.length; iCell++)
        {
            tree.add(offsetPolys[iCell]);
            allPolys.add(offsetPolys[iCell]);
        }
        final Vector3 v = new Vector3();
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
            boolean ttype = false;
            // Loop twice.  Once for exhaustive search.  The next for in-column search.
            for (int i = 0; i < 2; i++)
            {
                // Exact on centroid. (Layer 1)
                TriCell closestPoly = tree.getClosestCell(cent.x, cent.y, cent.z, ttype, v);
                assertTrue(closestPoly == cells[iCell]);
                assertTrue(Integer.toString(iCell), v.sloppyEquals(cent, TOLERANCE_STD));
                // Slightly above centroid. (Layer 1)
                closestPoly = tree.getClosestCell(cent.x, cent.y + 4*TOLERANCE_STD, cent.z, ttype, v);
                assertTrue(closestPoly == cells[iCell]);
                assertTrue(Integer.toString(iCell), v.sloppyEquals(cent, TOLERANCE_STD));
                // Nearer centroid of layer 2.
                closestPoly = tree.getClosestCell(cent.x, cent.y + yOffset - 4*TOLERANCE_STD, cent.z, ttype, v);
                assertTrue(closestPoly == offsetPolys[iCell]);
                assertTrue(Integer.toString(iCell), v.sloppyEquals(cent.x, cent.y + yOffset, cent.z, TOLERANCE_STD));
                // The next set of tests just make sure one of the valid possibilities is returned,
                // since the selection process is arbitrary.
                final float mpx = (ax + bx) / 2;
                final float mpy = (ay + by) / 2;
                final float mpz = (az + bz) / 2;
                closestPoly = tree.getClosestCell(mpx, mpy, mpz, ttype, v);
                if (linkCells[pCell] == -1)
                    assertTrue(Integer.toString(iCell), closestPoly == cells[iCell]);
                else
                    assertTrue(Integer.toString(iCell), closestPoly == cells[iCell] 
                                       || closestPoly == cells[linkCells[pCell]]);
                assertTrue(Integer.toString(iCell), v.sloppyEquals(mpx, mpy, mpz, TOLERANCE_STD));
                // Offset to level 2.
                closestPoly = tree.getClosestCell(mpx, mpy + yOffset - 4*TOLERANCE_STD, mpz, ttype, v);
                if (linkCells[pCell] == -1)
                    assertTrue(Integer.toString(iCell), closestPoly == offsetPolys[iCell]);
                else
                    assertTrue(Integer.toString(iCell), closestPoly == offsetPolys[iCell] 
                                       || closestPoly == offsetPolys[linkCells[pCell]]);
                ttype = true;
            }
        }
        // Test outside of mesh.
        final float[] va = mMesh.getMinVertex();
        // Restrict to column.
        TriCell cp = tree.getClosestCell(va[0] - TOLERANCE_STD
                , va[1] - TOLERANCE_STD
                , va[2] - TOLERANCE_STD
                , true
                , v);
        assertTrue(cp == null);
        // Exhaustive search.
        cp = tree.getClosestCell(va[0] - TOLERANCE_STD
                , va[1] - TOLERANCE_STD
                , va[2] - TOLERANCE_STD
                , false
                , v);
        final int[] minCells = mMesh.getMinVertexPolys();
        boolean found = false;
        for (int i = 0; i < minCells.length; i++)
        {
            if (cp == cells[minCells[i]])
            {
                found = true;
                break;
            }
        }
        assertTrue(found);
    }
    
    /**
     * Scans for cell that intersect the the AABB and makes
     * sure the generated list matches the provided list.
     */
    private boolean isCellListGood(float minx, float minz
            , float maxx, float maxz
            , TriCell[] cells
            , List<TriCell> cellList)
    {
        final ArrayList<TriCell> expectedPolys = new ArrayList<TriCell>();
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            if (cells[iCell].intersects(minx, minz, maxx, maxz))
                expectedPolys.add(cells[iCell]);
        }
        if (cellList.size() != expectedPolys.size())
            return false;
        return cellList.containsAll(expectedPolys);
    }
    
    /**
     * Scans for cell that contain the point (px, pz) and makes
     * sure generated list matches the provided list.
     */
    private boolean isCellListGood(float px, float pz
            , List<TriCell> cells
            , List<TriCell> cellList)
    {
        final ArrayList<TriCell> expectedPolys = new ArrayList<TriCell>();
        for (TriCell cell : cells)
        {
            if (cell.isInColumn(px, pz))
                expectedPolys.add(cell);
        }
        if (cellList.size() != expectedPolys.size())
            return false;
        return cellList.containsAll(expectedPolys);
    }
    
    /**
     * Loads a tree with the provided cell.
     */
    private TriCellQuadTree loadTree(float[] verts, TriCell[] cells)
    {
        final float[] meshBounds = getVertBounds(verts);
        final TriCellQuadTree tree = new TriCellQuadTree(meshBounds[0]
                                             , meshBounds[2]
                                             , meshBounds[3]  // + 0.1f
                                             , meshBounds[5]  // + 0.1f
                                             , 10);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            tree.add(cells[iCell]);
        }
        return tree;
    }

}

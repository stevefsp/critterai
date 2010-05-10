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

import static org.critterai.math.MathUtil.TOLERANCE_STD;
import static org.critterai.nav.TestUtil.getAllCells;
import static org.critterai.nav.TestUtil.linkAllCells;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

import java.util.ArrayList;
import java.util.Random;

import org.critterai.math.MathUtil;
import org.critterai.math.Vector2;
import org.critterai.math.Vector3;
import org.critterai.math.geom.Line2;
import org.critterai.math.geom.LineRelType;
import org.critterai.math.geom.Polygon3;
import org.critterai.math.geom.Triangle2;
import org.critterai.nav.TriCell;
import org.critterai.nav.PathRelType;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link TriCell} class.
 */
public final class TriCellTests 
{
    
    /*
     * Design notes:
     * 
     * The main test locations are around walls, vertices, and centroids.
     * 
     * The biggest assumption in the design is that
     * testing around wall midpoints is all that is needed for wall-based tests.
     * E.g. Testing intersection of a wall in the vicinity of the midpoint covers all needed
     * intersection tests for the wall.  No need to test around other points on the wall.
     * 
     */
    
    private static Random mR = new Random(1);
    private ITestMesh mMesh;
    
    @Before
    public void setUp() throws Exception 
    {
        mMesh = new CorridorMesh();
    }

    @Test
    public void testFieldCentroid()
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            int pCell = iCell*3;
            final TriCell cell = cells[iCell];
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
            assertTrue(cell.centroidX == cent.x);
            assertTrue(cell.centroidY == cent.y);
            assertTrue(cell.centroidZ == cent.z);            
        }
    }
    
    @Test
    public void testFieldBounds()
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            int pCell = iCell*3;
            final TriCell cell = cells[iCell];
            int matchesX = 0;
            int matchesZ = 0;
            for (int i = 0; i < 3; i++)
            {
                assertTrue(verts[indices[pCell+i]*3] >= cell.boundsMinX);
                assertTrue(verts[indices[pCell+i]*3] <= cell.boundsMaxX);
                assertTrue(verts[indices[pCell+i]*3+2] >= cell.boundsMinZ);
                assertTrue(verts[indices[pCell+i]*3+2] <= cell.boundsMaxZ);
                if (verts[indices[pCell+i]*3] == cell.boundsMinX 
                        || verts[indices[pCell+i]*3] == cell.boundsMaxX)
                    matchesX++;
                if (verts[indices[pCell+i]*3+2] == cell.boundsMinZ
                        || verts[indices[pCell+i]*3+2] == cell.boundsMaxZ)
                    matchesZ++;
            }
            assertTrue(matchesX >= 2);
            assertTrue(matchesZ >= 2);
        }
    }
    
    @Test
    public void testGetVertexValue() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        for (int pCell = 0; pCell < indices.length; pCell += 3)
        {
            final TriCell cell = new TriCell(verts
                    , indices[pCell]
                    , indices[pCell+1]
                    , indices[pCell+2]);
            for (int iVert = 0; iVert < 3; iVert++)
            {
                for (int iValue = 0; iValue < 3; iValue++)
                {
                    assertTrue(cell.getVertexValue(iVert, iValue) 
                            == verts[indices[pCell+iVert]*3+iValue]);                        
                }
            }
        }
    }

    @Test
    public void testGetVertex() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final Vector3 v = new Vector3();
        for (int pCell = 0; pCell < indices.length; pCell += 3)
        {
            final TriCell cell = new TriCell(verts
                    , indices[pCell]
                    , indices[pCell+1]
                    , indices[pCell+2]);
            for (int iVert = 0; iVert < 3; iVert++)
            {
                Vector3 vr = cell.getVertex(iVert, v);
                assertTrue(vr == v);
                assertTrue(vr.x == verts[indices[pCell+iVert]*3]);
                assertTrue(vr.y == verts[indices[pCell+iVert]*3+1]);
                assertTrue(vr.z == verts[indices[pCell+iVert]*3+2]);
            }
        }
    }

    @Test
    public void testGetVertexIndex3D() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        for (int pCell = 0; pCell < indices.length; pCell += 3)
        {
            final TriCell cell = new TriCell(verts
                    , indices[pCell]
                    , indices[pCell+1]
                    , indices[pCell+2]);
            for (int iVert = 0; iVert < 3; iVert++)
            {
                // Randomize the tolerance between zero and 0.001;
                // Randomize the offset direction for x, y, and z.
                final float tol = (float)mR.nextFloat() / 1000;
                final float vx = verts[indices[pCell+iVert]*3] + tol * (mR.nextFloat() > 0.5 ? 1 : -1);
                final float vy = verts[indices[pCell+iVert]*3+1] - tol * (mR.nextFloat() > 0.5 ? 1 : -1);
                final float vz = verts[indices[pCell+iVert]*3+2] - tol * (mR.nextFloat() > 0.5 ? 1 : -1);
                final int vi = cell.getVertexIndex(vx, vy, vz, tol);
                assertTrue(vi == iVert);
            }
        }
    }

    @Test
    public void testGetVertexIndex2D() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        for (int pCell = 0; pCell < indices.length; pCell += 3)
        {
            final TriCell cell = new TriCell(verts
                    , indices[pCell]
                    , indices[pCell+1]
                    , indices[pCell+2]);
            for (int iVert = 0; iVert < 3; iVert++)
            {
                // Randomize the tolerance between zero and 0.001;
                // Randomize the offset direction for x and z.
                final float tol = (float)mR.nextFloat() / 1000;
                final float vx = verts[indices[pCell+iVert]*3] + tol * (mR.nextFloat() > 0.5 ? 1 : -1);
                final float vz = verts[indices[pCell+iVert]*3+2] - tol * (mR.nextFloat() > 0.5 ? 1 : -1);
                final int vi = cell.getVertexIndex(vx, vz, tol);
                assertTrue(vi == iVert);
            }
        }
    }

    @Test
    public void testNoLinks()
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        for (int pCell = 0; pCell < indices.length; pCell += 3)
        {
            final TriCell cell = new TriCell(verts
                    , indices[pCell]
                    , indices[pCell+1]
                    , indices[pCell+2]);
            assertNull(cell.getLink(0));
            assertNull(cell.getLink(1));
            assertNull(cell.getLink(2));
            assertTrue(cell.linkCount() == 0);
        }
    }
    
    @Test
    public void testLinkCount() 
    {
        final int[] linkCount = mMesh.getLinkCounts();
        final TriCell[] cells = getAllCells(mMesh.getVerts(), mMesh.getIndices());
        linkAllCells(cells);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            assertTrue(cells[iCell].linkCount() == linkCount[iCell]);
        }
    }

    @Test
    public void testVertCount() 
    {
        final TriCell[] cells = getAllCells(mMesh.getVerts(), mMesh.getIndices());
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            assertTrue(cells[iCell].maxLinks() == 3);
        }
    }

    @Test
    public void testGetLink() 
    {
        final int[] linkCells = mMesh.getLinkPolys();
        final TriCell[] cells = getAllCells(mMesh.getVerts(), mMesh.getIndices());
        linkAllCells(cells);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            for (int iLink = 0; iLink < 3; iLink++)
            {
                TriCell linkedPoly = cells[iCell].getLink(iLink);
                if (linkedPoly == null)
                    assertTrue(iCell + ":" + iLink, linkCells[iCell*3+iLink] == -1);
                else
                    assertTrue(iCell + ":" + iLink, linkedPoly == cells[linkCells[iCell*3+iLink]]);
            }
        }
    }

    @Test
    public void testGetLinkWall() 
    {
        final int[] linkWalls = mMesh.getLinkWalls();
        final TriCell[] cells = getAllCells(mMesh.getVerts(), mMesh.getIndices());
        linkAllCells(cells);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            for (int iLink = 0; iLink < 3; iLink++)
            {
                int linkedWall = cells[iCell].getLinkWall(iLink);
                assertTrue(iCell + ":" + iLink, linkedWall == linkWalls[iCell*3+iLink]);
            }
        }
    }

    @Test
    public void testGetWallRightVertex() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final Vector3 u = new Vector3();
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            for (int iWall = 0; iWall < 3; iWall++)
            {
                Vector3 v = cells[iCell].getWallRightVertex(iWall, u);
                assertTrue(v == u);
                final int iVert = (iWall == 2 ? 0 : iWall + 1);
                assertTrue(v.x == verts[indices[iCell*3+iVert]*3]);
                assertTrue(v.y == verts[indices[iCell*3+iVert]*3+1]);
                assertTrue(v.z == verts[indices[iCell*3+iVert]*3+2]);
            }
        }
    }

    @Test
    public void testGetWallLeftVertex() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final Vector3 u = new Vector3();
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            for (int iWall = 0; iWall < 3; iWall++)
            {
                Vector3 v = cells[iCell].getWallLeftVertex(iWall, u);
                assertTrue(v == u);
                assertTrue(v.x == verts[indices[iCell*3+iWall]*3]);
                assertTrue(v.y == verts[indices[iCell*3+iWall]*3+1]);
                assertTrue(v.z == verts[indices[iCell*3+iWall]*3+2]);
            }
        }
    }

    @Test
    public void testGetWallDistance() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float actual = cells[iCell].getWallDistance(cent.x, cent.z, iWall);
                final float expected = (float)Math.sqrt(Line2.getPointLineDistanceSq(cent.x
                        , cent.z
                        , verts[indices[pCell+iWall]*3]
                        , verts[indices[pCell+iWall]*3+2]
                        , verts[indices[pCell+iNext]*3]
                        , verts[indices[pCell+iNext]*3+2]));
                assertTrue(iCell + ":" + iWall, MathUtil.sloppyEquals(actual, expected, TOLERANCE_STD));
            }
        }
    }

    @Test
    public void testGetLinkPointDistanceSq2D() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float actual = cells[iCell].getLinkPointDistanceSq(cent.x, cent.z, iWall);
                final float midpointX 
                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                final float midpointZ 
                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                final float expected = Vector2.getDistanceSq(cent.x, cent.z, midpointX, midpointZ);
                assertTrue(iCell + ":" + iWall, MathUtil.sloppyEquals(actual, expected, TOLERANCE_STD));
            }
        }
    }

    @Test
    public void testGetLinkPointDistanceSq3D() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float actual = cells[iCell].getLinkPointDistanceSq(cent.x, cent.y, cent.z, iWall);
                final float midpointX 
                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                final float midpointY 
                        = (verts[indices[pCell+iWall]*3+1] + verts[indices[pCell+iNext]*3+1]) / 2;
                final float midpointZ 
                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                final float expected = Vector3.getDistanceSq(cent.x, cent.y, cent.z, midpointX, midpointY, midpointZ);
                assertTrue(iCell + ":" + iWall, MathUtil.sloppyEquals(actual, expected, TOLERANCE_STD));
            }
        }
    }

    @Test
    public void testGetLinkPointDistance() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            int pCell = iCell*3;    
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float midpointX 
                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                final float midpointY 
                        = (verts[indices[pCell+iWall]*3+1] + verts[indices[pCell+iNext]*3+1]) / 2;
                final float midpointZ 
                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                for (int iOtherWall = 0; iOtherWall < 3; iOtherWall++)
                {
                    final int iOtherNext = (iOtherWall == 2 ? 0 : iOtherWall+1);
                    final float otherMidpointX 
                            = (verts[indices[pCell+iOtherWall]*3] + verts[indices[pCell+iOtherNext]*3]) / 2;
                    final float otherMidpointY 
                            = (verts[indices[pCell+iOtherWall]*3+1] + verts[indices[pCell+iOtherNext]*3+1]) / 2;
                    final float otherMidpointZ 
                            = (verts[indices[pCell+iOtherWall]*3+2] + verts[indices[pCell+iOtherNext]*3+2]) / 2;
                    final float actual = cells[iCell].getLinkPointDistance(iWall, iOtherWall);
                    final float expected = (float)Math.sqrt(Vector3.getDistanceSq(midpointX, midpointY, midpointZ
                            , otherMidpointX, otherMidpointY, otherMidpointZ));
                    assertTrue(iCell + ":" + iWall, MathUtil.sloppyEquals(actual, expected, TOLERANCE_STD));
                }
            }
        }
    }

    @Test
    public void testGetPlaneY() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
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
            final float actual = cells[iCell].getPlaneY(cent.x, cent.z);
            assertTrue(MathUtil.sloppyEquals(actual, cent.y, TOLERANCE_STD));
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float midpointX 
                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                final float midpointY 
                        = (verts[indices[pCell+iWall]*3+1] + verts[indices[pCell+iNext]*3+1]) / 2;
                final float midpointZ 
                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                assertTrue(MathUtil.sloppyEquals(cells[iCell]
                                            .getPlaneY(midpointX, midpointZ), midpointY, TOLERANCE_STD));
            }
        }
    }

//    @Test
//    public void testForceToColumn3D() 
//    {
//        final float[] verts = mMesh.getVerts();
//        final int[] indices = mMesh.getIndices();
//        final NavPoly[] cells = getAllPolys(verts, indices);
//        final Vector3 tv = new Vector3();
//        for (int iCell = 0; iCell < cells.length; iCell++)
//        {
//            final int pCell = iCell*3;
//            final float ax = verts[indices[pCell]*3];
//            final float ay = verts[indices[pCell]*3+1];
//            final float az = verts[indices[pCell]*3+2];
//            final float bx = verts[indices[pCell+1]*3];
//            final float by = verts[indices[pCell+1]*3+1];
//            final float bz = verts[indices[pCell+1]*3+2];
//            final float cx = verts[indices[pCell+2]*3];
//            final float cy = verts[indices[pCell+2]*3+1];
//            final float cz = verts[indices[pCell+2]*3+2];
//            final Vector3 cent = Polygon3.getCentroid(new Vector3()
//                                            , ax, ay, az
//                                            , bx, by, bz
//                                            , cx, cy, cz);
//            // First test offsets point from centroid.  Will always
//            // still be within column.
//            float offset = (float)mR.nextFloat() * mMesh.getOffset();
//            if (offset == 0)
//                // Can't let it be zero since we always need
//                // an offset from the centeroid.
//                offset = TOLERANCE_STD;
//            float px = cent.x + offset * (mR.nextFloat() > 0.5 ? 1 : -1);
//            float py = cent.y + offset * (mR.nextFloat() > 0.5 ? 1 : -1);
//            float pz = cent.z + offset * (mR.nextFloat() > 0.5 ? 1 : -1);
//            Vector3 u = cells[iCell].forceToColumn(px, py, pz, tv);
//            assertTrue(u == tv);
//            assertTrue(iCell + ":c", Triangle2.contains(px, pz, ax, az, bx, bz, cx, cz));
//            assertTrue(iCell + ":c", !u.equals(cent.x, cent.y, cent.z));
//            assertTrue(iCell + ":c", px == u.x);
//            assertTrue(iCell + ":c", py == u.y);
//            assertTrue(iCell + ":c", pz == u.z);
//            for (int iWall = 0; iWall < 3; iWall++)
//            {
//                /*
//                 * Second test moves point to outside the wall midpoint.
//                 * Point is expected to be moved to slightly inside the wall.
//                 * This is not a full test.  It does not validate the correct 
//                 * (x, y) position along the wall the point is snapped to.
//                 * It only validates that the point is internal to the column
//                 * and within the expected tolerance of the correct wall.
//                 */
//                final int iNext = (iWall == 2 ? 0 : iWall+1);
//                final float midpointX 
//                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
//                final float midpointY 
//                        = (verts[indices[pCell+iWall]*3+1] + verts[indices[pCell+iNext]*3+1]) / 2;
//                final float midpointZ 
//                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
//                final Vector2 ln = Line2.getNormalAB(verts[indices[pCell+iWall]*3]
//                                             , verts[indices[pCell+iWall]*3+2]
//                                             , verts[indices[pCell+iNext]*3]
//                                             , verts[indices[pCell+iNext]*3+2]
//                                             , new Vector2());                
//                px = midpointX - ln.x;
//                py = midpointY + offset * (mR.nextFloat() > 0.5 ? 1 : -1);
//                pz = midpointZ - ln.y;
////                if (iCell == 14 && iWall == 1)
////                    System.out.println();
//                u = cells[iCell].forceToColumn(px, py, pz, tv);
//                assertTrue(iCell + ":" + iWall, !Triangle2.contains(px, pz, ax, az, bx, bz, cx, cz));
//                assertTrue(iCell + ":" + iWall, !u.equals(cent.x, cent.y, cent.z));
//                assertTrue(iCell + ":" + iWall, py == u.y);
//                assertTrue(iCell + ":" + iWall, Triangle2.contains(u.x, u.z, ax, az, bx, bz, cx, cz));
//                final float maxDist = Vector2.getDistanceSq(cent.x, cent.z, midpointX, midpointZ) * 0.05f;
//                final float actualDist = Line2.getPointSegmentDistanceSq(u.x, u.z
//                                                , verts[indices[pCell+iWall]*3], verts[indices[pCell+iWall]*3+2]
//                                                , verts[indices[pCell+iNext]*3], verts[indices[pCell+iNext]*3+2]);
//                assertTrue(iCell + ":" + iWall, actualDist <= maxDist);
//            }
//        }
//    }

    @Test
    public void testForceToColumn2D() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final Vector2 tv = new Vector2();
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
            // First test offsets point from centroid.  Will always
            // still be within column.
            float offset = (float)mR.nextFloat() * mMesh.getOffset();
            if (offset == 0)
                // Can't let it be zero since we always need
                // an offset from the centeroid.
                offset = TOLERANCE_STD;
            float px = cent.x + offset * (mR.nextFloat() > 0.5 ? 1 : -1);
            float pz = cent.z + offset * (mR.nextFloat() > 0.5 ? 1 : -1);
            final float offsetScale = 0.05f;
            Vector2 u = cells[iCell].forceToColumn(px, pz, offsetScale, tv);
            assertTrue(u == tv);
            assertTrue(iCell + ":c", Triangle2.contains(px, pz, ax, az, bx, bz, cx, cz));
            assertTrue(iCell + ":c", !u.equals(cent.x, cent.z));
            assertTrue(iCell + ":c", px == u.x);
            assertTrue(iCell + ":c", pz == u.y);
            for (int iWall = 0; iWall < 3; iWall++)
            {
                /*
                 * Second test moves point to outside the wall midpoint.
                 * Point is expected to be moved to slightly inside the wall.
                 * This is not a full test.  It does not validate the correct 
                 * (x, y) position along the wall the point is snapped to.
                 * It only validates that the point is internal to the column
                 * and within the expected tolerance of the correct wall.
                 */
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float midpointX 
                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                final float midpointZ 
                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                final Vector2 ln = Line2.getNormalAB(verts[indices[pCell+iWall]*3]
                                             , verts[indices[pCell+iWall]*3+2]
                                             , verts[indices[pCell+iNext]*3]
                                             , verts[indices[pCell+iNext]*3+2]
                                             , new Vector2());                
                px = midpointX - ln.x;
                pz = midpointZ - ln.y;
                u = cells[iCell].forceToColumn(px, pz, offsetScale, tv);
                assertTrue(iCell + ":" + iWall, !Triangle2.contains(px, pz, ax, az, bx, bz, cx, cz));
                assertTrue(iCell + ":" + iWall, !u.equals(cent.x, cent.z));
                assertTrue(iCell + ":" + iWall, Triangle2.contains(u.x, u.y, ax, az, bx, bz, cx, cz));
                final float maxDist = Vector2.getDistanceSq(cent.x, cent.z, midpointX, midpointZ) * offsetScale;
                final float actualDist = Line2.getPointSegmentDistanceSq(u.x, u.y
                                                , verts[indices[pCell+iWall]*3], verts[indices[pCell+iWall]*3+2]
                                                , verts[indices[pCell+iNext]*3], verts[indices[pCell+iNext]*3+2]);
                assertTrue(iCell + ":" + iWall, actualDist <= maxDist);
            }
        }
    }

    @Test
    public void testIsInColumn() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            final int pCell = iCell*3;
            final float ax = verts[indices[pCell]*3];
            final float az = verts[indices[pCell]*3+2];
            final float bx = verts[indices[pCell+1]*3];
            final float bz = verts[indices[pCell+1]*3+2];
            final float cx = verts[indices[pCell+2]*3];
            final float cz = verts[indices[pCell+2]*3+2];
            // Test on vertices
            assertTrue(cells[iCell].isInColumn(ax, az));
            assertTrue(cells[iCell].isInColumn(bx, bz));
            assertTrue(cells[iCell].isInColumn(cx, cz));
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float midpointX 
                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                final float midpointZ 
                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                final Vector2 ln = Line2.getNormalAB(verts[indices[pCell+iWall]*3]
                                             , verts[indices[pCell+iWall]*3+2]
                                             , verts[indices[pCell+iNext]*3]
                                             , verts[indices[pCell+iNext]*3+2]
                                             , new Vector2());    
                ln.scaleTo(2*TOLERANCE_STD);
                // Test slightly outside wall midpoint.
                float px = midpointX - ln.x;
                float pz = midpointZ - ln.y;
                assertTrue(!Triangle2.contains(px, pz, ax, az, bx, bz, cx, cz));
                assertTrue(!cells[iCell].isInColumn(px, pz));
                // Test on midpoint.
                assertTrue(Triangle2.contains(midpointX, midpointZ, ax, az, bx, bz, cx, cz));
                assertTrue(cells[iCell].isInColumn(midpointX, midpointZ));
                // Test slightly inside wall midpoint.
                px = midpointX + ln.x;
                pz = midpointZ + ln.y;
                assertTrue(Triangle2.contains(px, pz, ax, az, bx, bz, cx, cz));
                assertTrue(cells[iCell].isInColumn(px, pz));
            }
        }
    }

    @Test
    public void testGetLinkIndex() 
    {
        final int[] linkCells = mMesh.getLinkPolys();
        final TriCell[] cells = getAllCells(mMesh.getVerts(), mMesh.getIndices());
        linkAllCells(cells);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            for (int iLink = 0; iLink < 3; iLink++)
            {
                int iLinkedPoly = linkCells[iCell*3+iLink];
                if (iLinkedPoly != -1)
                    assertTrue(iCell + ":" + iLink
                            , cells[iCell].getLinkIndex(cells[iLinkedPoly]) == iLink);                    
            }
        }
        final float[] verts = {-1, 0, 0, 1, 0, 1, 0, 0, 1};
        TriCell np = new TriCell(verts, 0, 1, 2);
        assertTrue(cells[0].getLinkIndex(np) == TriCell.NULL_INDEX);
    }
    
    @Test
    public void testLink()
    {
        // This test depends on getLinkIndex() functioning properly.
        final int[] linkCells = mMesh.getLinkPolys();
        TriCell[] cells = getAllCells(mMesh.getVerts(), mMesh.getIndices());
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            for (int iLink = 0; iLink < 3; iLink++)
            {
                int iLinkedPoly = linkCells[iCell*3+iLink];
                if (iLinkedPoly != -1 && iLinkedPoly < iLink)    
                {
                    assertTrue(iCell + ":" + iLink
                            , cells[iCell].link(cells[iLinkedPoly], false) == iLink); // No reverse linking.
                    assertTrue(iCell + ":" + iLink
                            , cells[iCell].getLinkIndex(cells[iLinkedPoly]) == iLink);
                    assertTrue(iCell + ":" + iLink
                            , cells[iLinkedPoly].getLinkIndex(cells[iCell]) == TriCell.NULL_INDEX);
                }
            }
        }
        cells = getAllCells(mMesh.getVerts(), mMesh.getIndices());
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            for (int iLink = 0; iLink < 3; iLink++)
            {
                int iLinkedPoly = linkCells[iCell*3+iLink];
                if (iLinkedPoly != -1 && iLinkedPoly < iLink)    
                {
                    assertTrue(iCell + ":" + iLink
                            , cells[iCell].link(cells[iLinkedPoly], true) == iLink);  // With reverse linking.
                    assertTrue(iCell + ":" + iLink
                            , cells[iCell].getLinkIndex(cells[iLinkedPoly]) == iLink);
                    int iReverseLink = -1;
                    for (int i = 0; i < 3; i++)
                    {
                        int il = linkCells[iLinkedPoly*3+i];
                        if (il == iCell)
                        {
                            iReverseLink = i;
                            break;
                        }    
                    }
                    assertTrue(iReverseLink != -1);  // This is a validity check of test data.
                    assertTrue(iCell + ":" + iLink
                            , cells[iLinkedPoly].getLinkIndex(cells[iCell]) == iReverseLink);
                }
            }
        }
    }
    
    @Test
    public void testIntersectsAABBCentered() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            final int pCell = iCell*3;
            final float ax = verts[indices[pCell]*3];
            final float az = verts[indices[pCell]*3+2];
            final float bx = verts[indices[pCell+1]*3];
            final float bz = verts[indices[pCell+1]*3+2];
            final float cx = verts[indices[pCell+2]*3];
            final float cz = verts[indices[pCell+2]*3+2];
            float minX = MathUtil.min(ax, bx, cx);
            float maxX = MathUtil.max(ax, bx, cx);
            float minZ = MathUtil.min(az, bz, cz);
            float maxZ = MathUtil.max(az, bz, cz);            
            // Polygon fully contained by AABB.
            assertTrue(Integer.toString(iCell)
                    , cells[iCell].intersects(minX, minZ, maxX, maxZ));
            assertTrue(Integer.toString(iCell)
                    , cells[iCell].intersects(minX - TOLERANCE_STD
                        , minZ - TOLERANCE_STD
                        , maxX + TOLERANCE_STD
                        , maxZ + TOLERANCE_STD));
            // AABB slightly smaller than cell.
            assertTrue(Integer.toString(iCell)
                    , cells[iCell].intersects(minX + TOLERANCE_STD
                        , minZ + TOLERANCE_STD
                        , maxX - TOLERANCE_STD
                        , maxZ - TOLERANCE_STD));
            // Polygon fully contains AABB
            boolean firstPass = true;
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float midpointX 
                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                final float midpointZ 
                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                if (firstPass)
                {
                    minX = midpointX;
                    maxX = midpointX;
                    minZ = midpointZ;
                    maxZ = midpointZ;
                    firstPass = false;
                }
                else
                {
                    minX = Math.min(minX, midpointX);
                    maxX = Math.max(maxX, midpointX);
                    minZ = Math.min(minZ, midpointZ);
                    maxZ = Math.max(maxZ, midpointZ);
                }
            }
            assertTrue(Integer.toString(iCell)
                    , cells[iCell].intersects(minX, minZ, maxX, maxZ));
            assertTrue(Integer.toString(iCell)
                    , cells[iCell].intersects(minX - TOLERANCE_STD
                        , minZ - TOLERANCE_STD
                        , maxX + TOLERANCE_STD
                        , maxZ + TOLERANCE_STD));
            assertTrue(Integer.toString(iCell)
                    , cells[iCell].intersects(minX + TOLERANCE_STD
                        , minZ + TOLERANCE_STD
                        , maxX - TOLERANCE_STD
                        , maxZ - TOLERANCE_STD));
            
        }
    }

    @Test
    public void testIntersectsAABBOffset() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            final int pCell = iCell*3;
            final float ax = verts[indices[pCell]*3];
            final float az = verts[indices[pCell]*3+2];
            final float bx = verts[indices[pCell+1]*3];
            final float bz = verts[indices[pCell+1]*3+2];
            final float cx = verts[indices[pCell+2]*3];
            final float cz = verts[indices[pCell+2]*3+2];
            float polyMinX = MathUtil.min(ax, bx, cx);
            float polyMaxX = MathUtil.max(ax, bx, cx);
            float polyMinZ = MathUtil.min(az, bz, cz);
            float polyMaxZ = MathUtil.max(az, bz, cz);            
            // AABB shifted around the edges of cell AABB
            // Not bothering with corner checks.
            assertFalse(Integer.toString(iCell)
                    , cells[iCell].intersects(polyMinX - 1, polyMinZ, polyMinX - TOLERANCE_STD, polyMaxZ));
            assertFalse(Integer.toString(iCell)
                    , cells[iCell].intersects(polyMaxX + TOLERANCE_STD, polyMinZ, polyMaxX + 1, polyMaxZ));
            assertFalse(Integer.toString(iCell)
                    , cells[iCell].intersects(polyMinX, polyMinZ - 1, polyMaxX, polyMinZ - TOLERANCE_STD));
            assertFalse(Integer.toString(iCell)
                    , cells[iCell].intersects(polyMinX, polyMaxZ + TOLERANCE_STD, polyMaxX, polyMaxZ + 1));
        }
    }
    
    @Test
    public void testIntersectsAABBNearVertices() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        /*
         * Shift distance is slightly longer than the length of the diagonal of a square
         * whose sides are 2*TOLERANCE_STD. This length is selected because, if applied
         * to an AABB centered over a vertex, it will shift the AABB completely
         * off the vertex.
         */
        final float shiftDistance 
            = (float)Math.sqrt(4*TOLERANCE_STD*TOLERANCE_STD+4*TOLERANCE_STD*TOLERANCE_STD) + TOLERANCE_STD;
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            final int pCell = iCell*3;
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
            for (int iVert = 0; iVert < 3; iVert++)
            {
                final float vx = verts[indices[pCell+iVert]*3];
                final float vz = verts[indices[pCell+iVert]*3+2];
                // AABB centered on vertex.
                final float minX = vx - TOLERANCE_STD;
                final float minZ = vz - TOLERANCE_STD;
                final float maxX = vx + TOLERANCE_STD;
                final float maxZ = vz + TOLERANCE_STD;
                assertTrue(iCell + ":" + iVert
                        , cells[iCell].intersects(minX, minZ, maxX, maxZ));
                // Slightly outside vertex.
                // Move AABB vertices away from centroid along centroid->vertex vector.
                final Vector2 dirCV = Vector2.subtract(vx, vz, cent.x, cent.z, new Vector2());
                dirCV.scaleTo(shiftDistance);
                float sminX = minX + dirCV.x;
                float sminZ = minZ + dirCV.y;
                float smaxX = maxX + dirCV.x;
                float smaxZ = maxZ + dirCV.y;
                assertFalse(iCell + ":" + iVert
                        , cells[iCell].intersects(sminX, sminZ, smaxX, smaxZ));
                // Slightly inside vertex.
                sminX = minX - dirCV.x;
                sminZ = minZ - dirCV.y;
                smaxX = maxX - dirCV.x;
                smaxZ = maxZ - dirCV.y;
                assertTrue(iCell + ":" + iVert
                        , cells[iCell].intersects(sminX, sminZ, smaxX, smaxZ));
            }
        }
    }
    
    @Test
    public void testIntersectsAABBNearEdgeMidpoints() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        /*
         * Shift distance is slightly longer than the length of the diagonal of a square
         * whose sides are 2*TOLERANCE_STD. This length is selected because, if applied
         * to an AABB centered over a point, it will shift the AABB completely
         * off the point.
         */
        final float shiftDistance 
            = 2*(float)Math.sqrt(4*TOLERANCE_STD*TOLERANCE_STD+4*TOLERANCE_STD*TOLERANCE_STD) + TOLERANCE_STD;
        // shiftDistance = 0.1f;
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            final int pCell = iCell*3;
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float vx 
                        = (verts[indices[pCell+iWall]*3] + verts[indices[pCell+iNext]*3]) / 2;
                final float vz 
                        = (verts[indices[pCell+iWall]*3+2] + verts[indices[pCell+iNext]*3+2]) / 2;
                // AABB centered on midpoint.
                final float minX = vx - TOLERANCE_STD;
                final float minZ = vz - TOLERANCE_STD;
                final float maxX = vx + TOLERANCE_STD;
                final float maxZ = vz + TOLERANCE_STD;
                assertTrue(iCell + ":" + iWall
                        , cells[iCell].intersects(minX, minZ, maxX, maxZ));
                // Slightly outside midpoint.
                // Move AABB vertices away from centroid along centroid->midpoint vector.
                final Vector2 dirCV = Vector2.subtract(vx, vz, cent.x, cent.z, new Vector2());
                dirCV.scaleTo(shiftDistance);
                float sminX = minX + dirCV.x;
                float sminZ = minZ + dirCV.y;
                float smaxX = maxX + dirCV.x;
                float smaxZ = maxZ + dirCV.y;
                assertFalse(iCell + ":" + iWall
                        , cells[iCell].intersects(sminX, sminZ, smaxX, smaxZ));
                // Slightly inside midpoint.  (This test is a little redundant.  But why not, since
                // it is easy enough to do.)
                sminX = minX - dirCV.x;
                sminZ = minZ - dirCV.y;
                smaxX = maxX - dirCV.x;
                smaxZ = maxZ - dirCV.y;
                assertTrue(iCell + ":" + iWall
                        , cells[iCell].intersects(sminX, sminZ, smaxX, smaxZ));
            }
        }
    }
    
    @Test
    public void testGetClosestPolyArray() 
    {
        // This test depends on the proper functioning of getPlaneY().
        final int[] linkCells = mMesh.getLinkPolys();
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        linkAllCells(cells);
        final Vector2 wv2 = new Vector2();
        final Vector3 surfacePoint = new Vector3();
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            int pCell = iCell*3;
            /*
             * Perform centroid test.
             * Doing this to allow for better test of y-axis variance,
             * which can't be done well on edge tests.
             */
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
            assertTrue(iCell + ":c"
                    , TriCell.getClosestCell(cent.x
                            , cent.y + + mMesh.getOffset() * (mR.nextFloat() > 0.5 ? 1 : -1)
                            , cent.z
                            , cells
                            , surfacePoint
                            , wv2) == cells[iCell]);
            assertTrue(iCell + ":c"
                    , surfacePoint.x == cent.x);
            assertTrue(iCell + ":c"
                    , MathUtil.sloppyEquals(surfacePoint.y, cent.y, TOLERANCE_STD));
            assertTrue(iCell + ":c"
                    , surfacePoint.z == cent.z);
            /*
             * Perform edge tests.
             * Only testing close to cell plane to avoid the impact of slope
             * on the result.
             */
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float ax = verts[indices[pCell+iWall]*3];
                final float ay = verts[indices[pCell+iWall]*3+1];
                final float az = verts[indices[pCell+iWall]*3+2];
                final float bx = verts[indices[pCell+iNext]*3];
                final float by = verts[indices[pCell+iNext]*3+1];
                final float bz = verts[indices[pCell+iNext]*3+2];
                final float midpointX = (ax + bx) / 2;
                final float midpointY = (ay + by) / 2;
                final float midpointZ = (az + bz) / 2;
                final Vector2 ln = Line2.getNormalAB(ax, az, bx, bz, new Vector2());
                ln.scaleTo(mMesh.getOffset());
                // Move inward from wall.
                float px = midpointX + ln.x;
                float py = midpointY;
                float pz = midpointZ + ln.y;
                assertTrue(iCell + ":" + iWall, TriCell.getClosestCell(px, py, pz
                        , cells
                        , surfacePoint
                        , wv2) == cells[iCell]);
                assertTrue(iCell + ":" + iWall, surfacePoint.x == px);
                assertTrue(iCell + ":" + iWall, MathUtil.sloppyEquals(surfacePoint.y
                        , cells[iCell].getPlaneY(px, pz)
                        , TOLERANCE_STD));
                assertTrue(iCell + ":" + iWall, surfacePoint.z == pz);
                // Move outward from wall.
                px = midpointX - ln.x;
                py = midpointY;
                pz = midpointZ - ln.y;
                TriCell actual = TriCell.getClosestCell(px, py, pz
                        , cells
                        , surfacePoint
                        , wv2);
                if (linkCells[pCell+iWall] == -1)
                {
                    assertTrue(iCell + ":" + iWall, actual == cells[iCell]);
                    Vector2 ip = new Vector2();
                    assertTrue(iCell + ":" + iWall
                            , Line2.getRelationship(ax, az
                                    , bx, bz
                                    , px, pz
                                    , cells[iCell].centroidX, cells[iCell].centroidZ
                                    , ip) == LineRelType.SEGMENTS_INTERSECT);
                    assertTrue(iCell + ":" + iWall
                            , MathUtil.sloppyEquals(surfacePoint.x,ip.x, TOLERANCE_STD));
                    assertTrue(iCell + ":" + iWall
                            , MathUtil.sloppyEquals(surfacePoint.y
                                    , cells[iCell].getPlaneY(ip.x, ip.y)
                                    , TOLERANCE_STD));
                    assertTrue(iCell + ":" + iWall
                            , MathUtil.sloppyEquals(surfacePoint.z
                                    , ip.y
                                    , TOLERANCE_STD));    
                }
                else
                {
                    assertTrue(iCell + ":" + iWall,  actual == cells[linkCells[pCell+iWall]]);
                    assertTrue(iCell + ":" + iWall, surfacePoint.x == px);
                    assertTrue(iCell + ":" + iWall, MathUtil.sloppyEquals(surfacePoint.y
                            , cells[linkCells[pCell+iWall]].getPlaneY(px, pz)
                            , TOLERANCE_STD));
                    assertTrue(iCell + ":" + iWall, surfacePoint.z == pz);                    
                }
            }
        }
    }

    @Test
    public void testGetClosestCellList() 
    {
        // This test depends on the proper functioning of getPlaneY().
        final int[] linkCells = mMesh.getLinkPolys();
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        linkAllCells(cells);
        final ArrayList<TriCell> cellList = new ArrayList<TriCell>();
        for (TriCell cell : cells)
            cellList.add(cell);
        final Vector2 wv2 = new Vector2();
        final Vector3 surfacePoint = new Vector3();
        for (int iCell = 0; iCell < cells.length; iCell++)
        {
            int pCell = iCell*3;
            /*
             * Perform centroid test.
             * Doing this to allow for better test of y-axis variance,
             * which can't be done well on edge tests.
             */
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
            assertTrue(iCell + ":c"
                    , TriCell.getClosestCell(cent.x
                            , cent.y + + mMesh.getOffset() * (mR.nextFloat() > 0.5 ? 1 : -1)
                            , cent.z
                            , cellList
                            , surfacePoint
                            , wv2) == cells[iCell]);
            assertTrue(iCell + ":c"
                    , surfacePoint.x == cent.x);
            assertTrue(iCell + ":c"
                    , MathUtil.sloppyEquals(surfacePoint.y, cent.y, TOLERANCE_STD));
            assertTrue(iCell + ":c"
                    , surfacePoint.z == cent.z);
            /*
             * Perform edge tests.
             * Only testing close to cell plane to avoid the impact of slope
             * on the result.
             */
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float ax = verts[indices[pCell+iWall]*3];
                final float ay = verts[indices[pCell+iWall]*3+1];
                final float az = verts[indices[pCell+iWall]*3+2];
                final float bx = verts[indices[pCell+iNext]*3];
                final float by = verts[indices[pCell+iNext]*3+1];
                final float bz = verts[indices[pCell+iNext]*3+2];
                final float midpointX = (ax + bx) / 2;
                final float midpointY = (ay + by) / 2;
                final float midpointZ = (az + bz) / 2;
                final Vector2 ln = Line2.getNormalAB(ax, az, bx, bz, new Vector2());
                ln.scaleTo(mMesh.getOffset());
                // Move inward from wall.
                float px = midpointX + ln.x;
                float py = midpointY;
                float pz = midpointZ + ln.y;
                assertTrue(iCell + ":" + iWall, TriCell.getClosestCell(px, py, pz
                        , cellList
                        , surfacePoint
                        , wv2) == cells[iCell]);
                assertTrue(iCell + ":" + iWall, surfacePoint.x == px);
                assertTrue(iCell + ":" + iWall, MathUtil.sloppyEquals(surfacePoint.y
                        , cells[iCell].getPlaneY(px, pz)
                        , TOLERANCE_STD));
                assertTrue(iCell + ":" + iWall, surfacePoint.z == pz);
                // Move outward from wall.
                px = midpointX - ln.x;
                py = midpointY;
                pz = midpointZ - ln.y;
                TriCell actual = TriCell.getClosestCell(px, py, pz
                        , cellList
                        , surfacePoint
                        , wv2);
                if (linkCells[pCell+iWall] == -1)
                {
                    assertTrue(iCell + ":" + iWall, actual == cells[iCell]);
                    Vector2 ip = new Vector2();
                    assertTrue(iCell + ":" + iWall
                            , Line2.getRelationship(ax, az
                                    , bx, bz
                                    , px, pz
                                    , cells[iCell].centroidX, cells[iCell].centroidZ
                                    , ip) == LineRelType.SEGMENTS_INTERSECT);
                    assertTrue(iCell + ":" + iWall
                            , MathUtil.sloppyEquals(surfacePoint.x,ip.x, TOLERANCE_STD));
                    assertTrue(iCell + ":" + iWall
                            , MathUtil.sloppyEquals(surfacePoint.y
                                    , cells[iCell].getPlaneY(ip.x, ip.y)
                                    , TOLERANCE_STD));
                    assertTrue(iCell + ":" + iWall
                            , MathUtil.sloppyEquals(surfacePoint.z
                                    , ip.y
                                    , TOLERANCE_STD));    
                }
                else
                {
                    assertTrue(iCell + ":" + iWall,  actual == cells[linkCells[pCell+iWall]]);
                    assertTrue(iCell + ":" + iWall, surfacePoint.x == px);
                    assertTrue(iCell + ":" + iWall, MathUtil.sloppyEquals(surfacePoint.y
                            , cells[linkCells[pCell+iWall]].getPlaneY(px, pz)
                            , TOLERANCE_STD));
                    assertTrue(iCell + ":" + iWall, surfacePoint.z == pz);                    
                }
            }
        }
    }

    @Test
    public void testGetPathRelationshipInternalSegments() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final Vector2 ip = new Vector2();
        final TriCell[] ep = new TriCell[1];
        final int[] ew = new int[1];
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float ax = verts[indices[pCell+iWall]*3];
                final float az = verts[indices[pCell+iWall]*3+2];
                final float bx = verts[indices[pCell+iNext]*3];
                final float bz = verts[indices[pCell+iNext]*3+2];
                final float midpointX = (ax + bx) / 2;
                final float midpointZ = (az + bz) / 2;
                // All line segments begin or end on a wall or vertex.
                // Edge case: Centroid to wall midpoint.
                PathRelType relType = cells[iCell].getPathRelationship(cent.x, cent.z
                        , midpointX, midpointZ
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                // Edge case: Centroid to vertex.
                relType = cells[iCell].getPathRelationship(cent.x, cent.z
                        , ax, az
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                // Edge case: Wall midpoint to centroid.
                relType = cells[iCell].getPathRelationship(midpointX, midpointZ
                        , cent.x, cent.z
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                // Edge case: Vertex to centroid.
                relType = cells[iCell].getPathRelationship(ax, az
                        , cent.x, cent.z
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                // All line segments are shifted off the walls and vertices.
                // Centroid to just inside wall midpoint.
                final Vector2 offset = Vector2.subtract(cent.x, cent.z
                        , midpointX, midpointZ
                        , new Vector2());
                offset.scaleTo(mMesh.getOffset());
                relType = cells[iCell].getPathRelationship(cent.x, cent.z
                        , midpointX + offset.x, midpointZ + offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                // Edge case: Just inside wall midpoint to centroid.
                relType = cells[iCell].getPathRelationship(midpointX + offset.x, midpointZ + offset.y
                        , cent.x, cent.z
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                Vector2.subtract(cent.x, cent.z
                        , ax, az
                        , offset);
                offset.scaleTo(mMesh.getOffset());
                // Centroid to just inside vertex.
                relType = cells[iCell].getPathRelationship(cent.x, cent.z
                        , ax + offset.x, az + offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                // Edge case: Just inside vertex to centroid.
                relType = cells[iCell].getPathRelationship(ax + offset.x, az + offset.y
                        , cent.x, cent.z
                        , ep
                        , ew
                        , ip
                        , new Vector2());
            }
        }
    }
    
    @Test
    public void testGetPathRelationshipBeginOutEndIn() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final Vector2 ip = new Vector2();
        final TriCell[] ep = new TriCell[1];
        final int[] ew = new int[1];
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float ax = verts[indices[pCell+iWall]*3];
                final float az = verts[indices[pCell+iWall]*3+2];
                final float bx = verts[indices[pCell+iNext]*3];
                final float bz = verts[indices[pCell+iNext]*3+2];
                final float midpointX = (ax + bx) / 2;
                final float midpointZ = (az + bz) / 2;
                // Outside to wall midpoint.
                final Vector2 offset = Vector2.subtract(cent.x, cent.z
                        , midpointX, midpointZ
                        , new Vector2());
                offset.scaleTo(mMesh.getOffset());
                PathRelType relType = cells[iCell].getPathRelationship(midpointX - offset.x, midpointZ - offset.y
                        , midpointX, midpointZ
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                // Outside to just inside wall midpoint.
                relType = cells[iCell].getPathRelationship(midpointX - offset.x, midpointZ - offset.y
                        , midpointX + offset.x, midpointZ + offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                Vector2.subtract(cent.x, cent.z
                        , ax, az
                        , offset);
                offset.scaleTo(mMesh.getOffset());
                // Outside to vertex
                relType = cells[iCell].getPathRelationship(ax - offset.x, az - offset.y
                        , ax, az
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
                // Edge case: Just inside vertex to centroid.
                relType = cells[iCell].getPathRelationship(ax - offset.x, az - offset.y
                        , ax + offset.x, az + offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.ENDING_CELL);
            }
        }
    }

    @Test
    public void testGetPathRelationshipBeginInEndOut() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        linkAllCells(cells);
        final int[] linkCells = mMesh.getLinkPolys();
        final Vector2 ip = new Vector2();
        final TriCell[] ep = new TriCell[1];
        final int[] ew = new int[1];
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float ax = verts[indices[pCell+iWall]*3];
                final float az = verts[indices[pCell+iWall]*3+2];
                final float bx = verts[indices[pCell+iNext]*3];
                final float bz = verts[indices[pCell+iNext]*3+2];
                final float midpointX = (ax + bx) / 2;
                final float midpointZ = (az + bz) / 2;
                // Outside to wall midpoint.
                final Vector2 offset = Vector2.subtract(cent.x, cent.z
                        , midpointX, midpointZ
                        , new Vector2());
                offset.scaleTo(mMesh.getOffset());
                PathRelType relType = cells[iCell].getPathRelationship(midpointX, midpointZ
                        , midpointX - offset.x, midpointZ - offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.EXITING_CELL);
                int iNextPoly = linkCells[pCell+iWall];
                if (iNextPoly == -1)
                    assertTrue(iCell + ":" + iWall, ep[0] == null);
                else
                    assertTrue(iCell + ":" + iWall, ep[0] == cells[iNextPoly]);
                assertTrue(iCell + ":" + iWall, ew[0] == iWall);
                assertTrue(iCell + ":" + iWall, ip.sloppyEquals(midpointX, midpointZ, TOLERANCE_STD));
                // Outside to just inside wall midpoint.
                relType = cells[iCell].getPathRelationship(midpointX + offset.x, midpointZ + offset.y
                        , midpointX - offset.x, midpointZ - offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.EXITING_CELL);
                iNextPoly = linkCells[pCell+iWall];
                if (iNextPoly == -1)
                    assertTrue(iCell + ":" + iWall, ep[0] == null);
                else
                    assertTrue(iCell + ":" + iWall, ep[0] == cells[iNextPoly]);
                assertTrue(iCell + ":" + iWall, ew[0] == iWall);
                assertTrue(iCell + ":" + iWall, ip.sloppyEquals(midpointX, midpointZ, TOLERANCE_STD));
                Vector2.subtract(cent.x, cent.z
                        , ax, az
                        , offset);
                offset.scaleTo(mMesh.getOffset());
                // Outside to vertex
                relType = cells[iCell].getPathRelationship(ax, az
                        , ax - offset.x, az - offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.EXITING_CELL);
                // Not testing exit wall information since which wall is chosen is undefined
                // for vertices.
                assertTrue(iCell + ":" + iWall, ip.sloppyEquals(ax, az, TOLERANCE_STD));
                // Edge case: Just inside vertex to centroid.
                if (iCell == 6 && iWall == 2)
                    System.out.println();                
                relType = cells[iCell].getPathRelationship(ax + offset.x, az + offset.y
                        , ax - offset.x, az - offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.EXITING_CELL);
                // Not testing exit wall information since which wall is chosen is undefined
                // for vertices.
                assertTrue(iCell + ":" + iWall, ip.sloppyEquals(ax, az, TOLERANCE_STD));
            }
        }
    }
    
    @Test
    public void testGetPathRelationshipCrossing() 
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        linkAllCells(cells);
        final int[] linkCells = mMesh.getLinkPolys();
        final Vector2 ip = new Vector2();
        final TriCell[] ep = new TriCell[1];
        final int[] ew = new int[1];
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final int iNextNext = (iNext == 2 ? 0 : iNext+1);
                // Current wall.
                final float wallax = verts[indices[pCell+iWall]*3];
                final float wallaz = verts[indices[pCell+iWall]*3+2];
                final float wallbx = verts[indices[pCell+iNext]*3];
                final float wallbz = verts[indices[pCell+iNext]*3+2];
                final float wallmpx = (wallax + wallbx) / 2;
                final float wallmpz = (wallaz + wallbz) / 2;
                // Next wall.
                final float wallnax = verts[indices[pCell+iNext]*3];
                final float wallnaz = verts[indices[pCell+iNext]*3+2];
                final float wallnbx = verts[indices[pCell+iNextNext]*3];
                final float wallnbz = verts[indices[pCell+iNextNext]*3+2];
                final float wallnmpx = (wallnax + wallnbx) / 2;
                final float wallnmpz = (wallnaz + wallnbz) / 2;
                // Outside next wall midpoint to outside this wall midpoint.
                final Vector2 offset = Vector2.subtract(wallnmpx, wallnmpz
                        , wallmpx, wallmpz
                        , new Vector2());
                offset.scaleTo(mMesh.getOffset());
                PathRelType relType = cells[iCell].getPathRelationship(wallnmpx + offset.x
                        , wallnmpz + offset.y
                        , wallmpx - offset.x
                        , wallmpz - offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.EXITING_CELL);
                int iNextPoly = linkCells[pCell+iWall];
                if (iNextPoly == -1)
                    assertTrue(iCell + ":" + iWall, ep[0] == null);
                else
                    assertTrue(iCell + ":" + iWall, ep[0] == cells[iNextPoly]);
                assertTrue(iCell + ":" + iWall, ew[0] == iWall);
                assertTrue(iCell + ":" + iWall, ip.sloppyEquals(wallmpx, wallmpz, TOLERANCE_STD));
                // Outside cell through centroid to outside vertex.
                Vector2.subtract(wallax, wallaz
                        , cent.x, cent.z
                        , offset);
                Vector2 largeOffset = new Vector2(offset.x, offset.y).scaleTo(20);
                offset.scaleTo(mMesh.getOffset());
                relType = cells[iCell].getPathRelationship(cent.x - largeOffset.x, cent.z - largeOffset.y
                        , wallax + offset.x, wallaz + offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.EXITING_CELL);
                assertTrue(iCell + ":" + iWall, ip.sloppyEquals(wallax, wallaz, TOLERANCE_STD));
            }
        }
    }
    
    @Test
    public void testGetPathRelationshipNoCross()
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final Vector2 ip = new Vector2();
        final TriCell[] ep = new TriCell[1];
        final int[] ew = new int[1];
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
            for (int iWall = 0; iWall < 3; iWall++)
            {
                final int iNext = (iWall == 2 ? 0 : iWall+1);
                final float ax = verts[indices[pCell+iWall]*3];
                final float az = verts[indices[pCell+iWall]*3+2];
                final float bx = verts[indices[pCell+iNext]*3];
                final float bz = verts[indices[pCell+iNext]*3+2];
                final float midpointX = (ax + bx) / 2;
                final float midpointZ = (az + bz) / 2;
                // Outside and away from wall midpoint.
                final Vector2 offset = Vector2.subtract(cent.x, cent.z
                        , midpointX, midpointZ
                        , new Vector2());
                offset.scaleTo(mMesh.getOffset());
                PathRelType relType = cells[iCell].getPathRelationship(midpointX - offset.x
                        , midpointZ - offset.y
                        , midpointX - 2* offset.x
                        , midpointZ - 2* offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.NO_RELATIONSHIP);
                // Outside and away from vertex.
                relType = cells[iCell].getPathRelationship(ax - offset.x, az - offset.y
                        , ax - 2* offset.x, az - 2* offset.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.NO_RELATIONSHIP);
                // Shift wall away from cell so there is no crossing.
                final Vector2 ln = Line2.getNormalAB(ax, az, bx, bz, new Vector2());
                ln.scaleTo(2*TOLERANCE_STD);
                relType = cells[iCell].getPathRelationship(ax - ln.x, az - ln.y
                        , bx - ln.x, bz - ln.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.NO_RELATIONSHIP);
                // Swap direction (BA) and test again.
                relType = cells[iCell].getPathRelationship(bx - ln.x, bz - ln.y
                        , ax - ln.x, az - ln.y
                        , ep
                        , ew
                        , ip
                        , new Vector2());
                assertTrue(iCell + ":" + iWall, relType == PathRelType.NO_RELATIONSHIP);
            }
        }
    }
    
    @Test
    public void testGetSafePoint()
    {
        final float[] verts = mMesh.getVerts();
        final int[] indices = mMesh.getIndices();
        final TriCell[] cells = getAllCells(verts, indices);
        final Vector2 v = new Vector2();
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
            // Test: Point is already safe.
            final float offsetScale = 0.05f;
            Vector2 u = cells[iCell].getSafePoint(cent.x, cent.z, offsetScale, v);
            assertTrue(u == v);
            assertTrue(u.equals(cent.x, cent.z));
            // Test: Point is on vertex. (Not safe.)
            for (int i = 0; i < 3; i++)
            {
                final float vx = verts[indices[pCell+i]*3];
                final float vz = verts[indices[pCell+i]*3+2];
                int iNext = (i + 1 >= 3 ? 0 : i + 1);
                int iNextNext = (iNext + 1 >= 3 ? 0 : iNext + 1);
                u = cells[iCell].getSafePoint(vx, vz, offsetScale, v);
                /*
                 * The following validations are performed:
                 * - Is the point still in the cell?
                 * - Is the point still within an acceptable distance of the vertex? (Not too far.)
                 * - Has the point been moved an acceptable distance from the vertex? (Not too close.)
                 * NOTE: Not really testing that the point was moved along the line toward the centroid.
                 * But this is a good enough set of tests.
                 */
                assertFalse(iCell + ":" + i, u.sloppyEquals(vx, vz, TOLERANCE_STD));
                assertTrue(iCell + ":" + i, Triangle2.contains(u.x, u.y
                        , vx, vz
                        , verts[indices[pCell+iNext]*3], verts[indices[pCell+iNext]*3+2]
                        , verts[indices[pCell+iNextNext]*3], verts[indices[pCell+iNextNext]*3+2]));
                float maxDist = Vector2.getDistanceSq(vx, vz, cent.x, cent.z) * offsetScale;
                float actualDist = Vector2.getDistanceSq(vx, vz, u.x, u.y);
                assertTrue(iCell + ":" + i, actualDist < maxDist);                
            }
        }
    }

}

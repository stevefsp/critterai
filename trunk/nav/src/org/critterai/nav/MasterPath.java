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
import static org.critterai.nav.TriCell.NULL_INDEX;

import org.critterai.math.Vector2;
import org.critterai.math.Vector3;
import org.critterai.math.geom.Triangle2;

/**
 * Represents a path corridor made up of ordered navigation cells.  
 * The corridor is one cell wide and represents a directed path from the start cell
 * to the end cell.
 * <p>Instances of this class are not thread-safe but can be used by multiple threads
 * under the following conditions:</p>
 * <ul>
 * <li>Its {@link TriCell} cells are treated in a thread safe manner.  See {@link TriCell}
 * for details.</li>
 * <li>The instance's reference is not shared between threads.</li>
 * <li>Other threads access path data through instances of {@link Path} objects obtained via 
 * {@link #getPath(float, float, float)}.</li>
 * <li>References to {@link Path} objects are not shared between threads.</li>
 * </ul>
 * <p>In summary, a single thread constructs and manages an instance of {@literal MasterPath}
 * and passes out instances of {@link Path} to other threads as needed.</p>
 */
public final class MasterPath
{
    
    /**
     * Provides operations useful to clients which need to navigate within
     * a path corridor toward a goal point located in the end cell of the corridor. 
     * <p>Sharing instances of this class between clients will negate performance
     * optimizations.</p>
     * <p>Instances of this class are not thread safe.</p>
     */
    public class Path 
    {
        
        /**
         * The x-value of the goal of the path. (goalX, goalY, goalZ)
         */
        public final float goalX;
        
        /**
         * The y-value of the goal of the path. (goalX, goalY, goalZ)
         */
        public final float goalY;
        
        /**
         * The z-value of the goal of the path. (goalX, goalY, goalZ)
         */
        public final float goalZ;
        
        private int mLastCellIndex = 0;
        private final Vector2 mTmpVec2A = new Vector2();
        
        /**
         * Constructor.
         * <p>Behavior is undefined if the goal position is not within 
         * the final cell of the path.</p>
         * @param goalX The x-value of the goal position. (goalX, goalY, goalZ)
         * @param goalY The y-value of the goal position. (goalX, goalY, goalZ)
         * @param goalZ The z-value of the goal position. (goalX, goalY, goalZ)
         */
        private Path(float goalX, float goalY, float goalZ) 
        { 
            this.goalX = goalX;
            this.goalY = goalY;
            this.goalZ = goalZ;
        }
        
        /**
         * Snaps the provided point to the closest location on the surface
         * of the path corridor.  
         * <p>If the point is moved along the xz axes, it will be moved to
         * slightly within the closest edge of the path rather than on the edge.</p>
         * <p>This is a non-trivial operation.</p>
         * @param x The x-value of the point (x, y, z).
         * @param y The y-value of the point (x, y, z).
         * @param z The z-value of the point (x, y, z).
         * @param out The vector to load the result into.
         * @return A reference to the out argument.
         */
        public Vector3 forceToPath(float x, float y, float z, Vector3 out)
        {
            if (out == null)
                out = new Vector3();
            TriCell.getClosestCell(x, y, z, mCells, out, mTmpVec2A);
            return out;
        }
        
        /**
         * Snaps the provided point to the plane of the path corridor.
         * @param x The x-value of the point (x, y, z).
         * @param y The y-value of the point (x, y, z).
         * @param z The z-value of the point (x, y, z).
         * @param out  The point with its y-value snapped to the surface of the path corridor,
         * or the original (x, y, z) value if the provided point is outside of the path
         * corridor.
         * @return TRUE if the the point was within the path corridor.  Otherwise FALSE.
         */
        public boolean forceYToPath(float x, float y, float z, Vector3 out)
        {
            if (out == null)
                out = new Vector3();
            TriCell cell = getCellForPosition(x, y, z);
            if (cell == null)
            {
                out.set(x, y, z);
                return false;
            }
            out.set(x, cell.getPlaneY(x, z), z);
            return true;
        }
        
        /**
         * An version of {@link MasterPath#getCell(float, float, float)}
         * optimized for the current path object.
         * <p>As long as the xz-position is in the path column, it will return
         * a result.  The y-value is used to deal with overlapping cells.</p>
         * @param x The x-value of the point (x, y, z).
         * @param y The y-value of the point (x, y, z).
         * @param z The z-value of the point (x, y, z).
         * @return The best cell for the position or {@link TriCell#NULL_INDEX} if
         * the position is outside the path column.
         */
        private TriCell getCellForPosition(float x, float y, float z)
        {
            // Search optimization:  Check last search location.
            if (isOnCell(x, y, z, mLastCellIndex, mPlaneTolerance))
                // Early exit OK.
                return mCells[mLastCellIndex];
            return getCell(x, y, z);
        }
        
        /**
         * An version of {@link MasterPath#getCellIndex(float, float, float)}
         * optimized for the current path object.
         * <p>As long as the xz-position is in the path column, it will return
         * a result.  The y-value is used to deal with overlapping cells.</p>
         * @param x The x-value of the point (x, y, z).
         * @param y The y-value of the point (x, y, z).
         * @param z The z-value of the point (x, y, z).
         * @return The best cell index for the position or {@link TriCell#NULL_INDEX} if
         * the position is outside the path column.
         */
        private int getCellIndexForPosition(float x, float y, float z)
        {
            // Search optimization:  Check last search location.
            if (isOnCell(x, y, z, mLastCellIndex, mPlaneTolerance))
                // Early exit OK.
                return mLastCellIndex;
            return getCellIndex(x, y, z);
        }
        
        /**
         * The goal point of the path.
         * @param out The vector to load the goal information into.
         * @return A reference to the out argument.
         */
        public Vector3 getGoal(Vector3 out)
        {
            if (out == null)
                out = new Vector3();
            return out.set(goalX, goalY, goalZ);
        }
        
        /**
         * Returns a list of triangles representing the path corridor.
         * Uses standard vertices/indices representation with clockwise wrapping.
         * @param outVerts An array of size {@link #pathVertCount()}*3 to load the
         * vertices into.  The vertices of the triangles in the form 
         * @param outIndices An array of size ({@link #getPathPolys(float[], int[])}*3 to load
         * the results into.
         */
        public void getPathPolys(float[] outVerts, int[] outIndices)
        {
            if (outVerts == null 
                    || outIndices == null
                    || outVerts.length < mCells.length*9
                    || outIndices.length < mCells.length*3)
                return;
            for (int iCell = 0; iCell < mCells.length; iCell++)
            {
                int pVert = iCell*9;
                for (int iv = 0; iv < 3; iv++)
                {
                    for (int ivp = 0; ivp < 3; ivp++)
                    {
                        outVerts[pVert+iv*3+ivp] = mCells[iCell].getVertexValue(iv, ivp);
                    }
                }
                int pPoly = iCell*3;
                outIndices[pPoly+0] = pPoly+0;
                outIndices[pPoly+1] = pPoly+1;
                outIndices[pPoly+2] = pPoly+2;
            }
        }
        
        /**
         * Gets the farthest visible waypoint in the direction of the goal.
         * Effectively, this is on-the-fly string pulling.
         * <p>Will return false if the provided point lies outside of the
         * path column or the path has been disposed.</p>
         * @param fromX The x-value of the position being evaluated. (fromX, fromY, fromZ)
         * @param fromY The y-value of the position being evaluated. (fromX, fromY, fromZ)
         * @param fromZ The z-value of the position being evaluated. (fromX, fromY, fromZ)
         * @param out The vector object to load the target information into.  
         * The vector may be updated even if the operation returns FALSE.  But the data
         * will only be valid if the operation returns TRUE.
         * @return TRUE if a valid waypoint could be found.  FALSE if the "from" point
         * is either outside of the path column or the backing {@link MasterPath} has been
         * disposed.
         */
        public boolean getTarget(float fromX, float fromY, float fromZ, Vector3 out)
        {
            
            /*
             * Design note:
             * 
             * The out argument is also used as a working variable.
             * The true out value is set just before returning. 
             */
            
            if (mIsDisposed)
                return false;
            
            if (out == null)
                out = new Vector3();
            
            int iStart = getCellIndexForPosition(fromX, fromY, fromZ);
            if (iStart == NULL_INDEX)
                return false;
            
            // Handle special cases.
            if (mCells.length == 1)
            {
                // Start and goal must be in the same cell.
                out.set(goalX, goalY, goalZ);
                return true;
            }
            
            // Set the initial funnel.
            // Note: Apex is the start point.
            
            // Right Vertex
            float rightX = 0;
            float rightY = 0;
            float rightZ = 0;
            
            // Left Vertex
            float leftX = 0;
            float leftY = 0;
            float leftZ = 0;
            
            /*
             * This loop detects cases where the apex
             * is on an exit wall vertex.  In such a
             * case the start cell is moved forward
             * in the path until the apex is no longer
             * on an exit wall vertex.
             * 
             * For a properly constructed path,
             * if the apex is on an exit wall vertex,
             * it must be on a non-exit wall vertex of one
             * of the next cells in the path.
             * 
             * At the end of this loop the initial funnel will
             * have been constructed.
             */
            while(iStart < mCells.length)
            {
                
                if (iStart == mCells.length - 1)
                {
                    // We have reached the end cell.  
                    // So the apex is in the end cell.
                    out.set(goalX, goalY, goalZ);
                    return true;
                }
                
                // Left vertex.
                mCells[iStart].getVertex(mWallIndices[iStart], out);
                leftX = out.x;
                leftY = out.y;
                leftZ = out.z;    
                
                // Right vertex.
                // The right vertex index will always be the next index 
                // above the left vertex. (wrapped)
                mCells[iStart].getVertex(
                        (mWallIndices[iStart] + 1 < mCells[iStart].maxLinks() 
                                ? mWallIndices[iStart] + 1 : 0)
                        , out);
                rightX = out.x;
                rightY = out.y;
                rightZ = out.z;        
                
                if ((fromX == leftX && fromY == leftY && fromZ == leftZ)
                        || (fromX == rightX && fromY == rightY && fromZ == rightZ))
                    // The apex is on one of the exit wall vertices.
                    // Move to the next cell.
                    iStart++;                
                else
                    // The funnel is good.
                    break;                    

            }
            
            /*
             * This is a modified funnel algorithm.
             * It gets the farthest edge vertex within the path 
             * that is visible from the current apex.
             * 
             * For a good description see: 
             * http://digestingduck.blogspot.com/2010/03/simple-stupid-funnel-algorithm.html
             */        
                
            /*
             * This loop should never complete without returning a result since,
             * if it gets to the the last cell, it will select the goal,
             * or the funnel right or left vertex.
             */
            for (int iCurrCell = iStart + 1
                    ; iCurrCell < mCells.length
                    ; iCurrCell++)
            {
                float area;  // Signed area x 2.
                // Find the exit edge for this cell.
                if (mWallIndices[iCurrCell] == NULL_INDEX)
                {
                    /*
                     * Special case:  No exit edge.
                     * Should only happen for the last cell of the path.
                     * In this case, the goal point is the final vertex
                     * to test.  If it is visible, then select it.  If it is
                     * not, then select the right or left vertex as appropriate.
                     */
                    area = Triangle2.getSignedAreaX2(fromX, fromZ, goalX, goalZ, rightX, rightZ);
                    if (area < TOLERANCE_STD)
                    {
                        area = Triangle2.getSignedAreaX2(fromX, fromZ, goalX, goalZ, leftX, leftZ);
                        if (area > -TOLERANCE_STD)
                        {
                            // Goal is within the funnel.
                            out.set(goalX, goalY, goalZ);
                            return true;
                        }
                        else
                        {
                            // Goal is to the left of the funnel.
                            // Select left.    
                            Vector3.translateToward(leftX, leftY, leftZ
                                    , rightX, rightY, rightZ
                                    , mOffsetFactor
                                    , out);
                            return true;
                        }
                    }
                    else
                    {
                        // Goal is to the right of the funnel.
                        // Select right.
                        Vector3.translateToward(rightX, rightY, rightZ
                                , leftX, leftY, leftZ
                                , mOffsetFactor
                                , out);
                        return true;
                    }
                }
                // We haven't reached the goal cell.
                else
                {
                    // Test the right vertex.
                    int iCurrVertex = (mWallIndices[iCurrCell] + 1 < mCells[iCurrCell].maxLinks()
                            ? mWallIndices[iCurrCell] + 1 : 0);
                    mCells[iCurrCell].getVertex(iCurrVertex, out);
                    if (out.x != rightX || out.z != rightZ)
                    {
                        // Test point is NOT the same point as
                        // the right vertex.
                        area = Triangle2.getSignedAreaX2(fromX, fromZ
                                , out.x, out.z
                                , rightX, rightZ);
                        /*
                         * If the next test fails, then it means the
                         * test point is to the right of the funnel.
                         * So we keep the current right vertex.
                         */
                        if (area < TOLERANCE_STD)
                        {
                            // Test point is to the left of the right vertex.
                            // (It is toward the inside of the funnel.)
                            area = Triangle2.getSignedAreaX2(fromX, fromZ
                                    , out.x, out.z
                                    , leftX, leftZ);
                            if (area > -TOLERANCE_STD)
                            {
                                /*
                                 * Test point is to the right of the left
                                 *  vertex.  So it is inside the funnel.
                                 *  Narrow the funnel on the right.
                                 */
                                rightX = out.x;
                                rightY = out.y;
                                rightZ = out.z;
                            }
                            else
                            {
                                /*
                                 * Test point is to the left of the left vertex.
                                 * So the right wall veers off to the left of the
                                 * funnel.
                                 * Select the left vertex.
                                 */
                                Vector3.translateToward(leftX, leftY, leftZ
                                        , rightX, rightY, rightZ
                                        , mOffsetFactor
                                        , out);
                                return true;
                            }
                        }                        
                    }
                    // Test the left vertex.
                    iCurrVertex = mWallIndices[iCurrCell];
                    mCells[iCurrCell].getVertex(iCurrVertex, out);
                    if (out.x != leftX || out.z != leftZ)
                    {
                        // Test point is NOT the same point as
                        // the left vertex.
                        area = Triangle2.getSignedAreaX2(fromX, fromZ
                                , out.x, out.z
                                , leftX, leftZ);
                        /*
                         * If the next test fails, then it means the
                         * test point is to the left of the funnel.
                         * So we keep the current left vertex.
                         */
                        if (area > -TOLERANCE_STD)
                        {
                            /*
                             * Test point is to the right of the left
                             * vertex.  (Toward the inside of the funnel.)
                             */
                            area = Triangle2.getSignedAreaX2(fromX, fromZ
                                    , out.x, out.z
                                    , rightX, rightZ);
                            if (area < TOLERANCE_STD)
                            {
                                /*
                                 * Test vertex lies to the left of the right
                                 * vertex.  So it is within the funnel.
                                 * Narrow the funnel on the left.
                                 */
                                leftX = out.x;
                                leftY = out.y;
                                leftZ = out.z;
                            }
                            else
                            {
                                /*
                                 * Test vertex light to the right of the right
                                 * vertex.  So the left wall veers off to the right
                                 * of the funnel.  Select the right vertex.
                                 */
                                Vector3.translateToward(rightX, rightY, rightZ
                                        , leftX, leftY, leftZ
                                        , mOffsetFactor
                                        , out);
                                return true;
                            }
                        }                    
                    }
                }
            }
            
            // Should never get here unless there is a logic or data error.
            return false;
                
        }
        
        /**
         * The id of the {@link MasterPath} that backs this path object.
         * @return The id of the {@link MasterPath} that backs this path object.
         */
        public int id() { return id; }
        
        /**
         * Indicates whether or not the path is valid.  Disposed
         * paths should be discarded.
         * @return TRUE if the path is no longer valid.  Otherwise FALSE.
         */
        public boolean isDisposed() { return mIsDisposed; }
        
        /**
         * Indicates whether or not the point is within the column of the path
         * corridor.
         * <p>It is not necessary or efficient to call this operation before calling the 
         * {@link #getTarget(float, float, float, Vector3)} operation.  Just check that
         * operation's return value to see if a target was obtained.</p>
         * @param x The x-value of the point to evaluate. (x, z) from the point (x, y, z)
         * @param z The z-value of the point to evaluate. (x, z) from the point (x, y, z)
         * @return TRUE if the point is within the column of the path. Otherwise false.
         */
        public boolean isInPathColumn(float x, float z)
        {
            for (TriCell cell : mCells)
            {
                if (cell.isInColumn(x, z))
                    return true;
            }
            return false;
        }
        
        /**
         * The number of triangles expected when calling the 
         * {@link #getPathPolys(float[], int[])} operation.
         * @return The number of triangles expected when calling the 
         * {@link #getPathPolys(float[], int[])} operation.
         */
        public int pathPolyCount() { return mCells.length; }
        
        /**
         * The number of vertices expected when calling the 
         * {@link #getPathPolys(float[], int[])} operation.
         * @return The number of vertices expected when calling the 
         * {@link #getPathPolys(float[], int[])} operation.
         */
        public int pathVertCount() { return mCells.length * 3; }
        
    }
    
    /**
     * The id of the path.
     */
    public final int id;
    
    /**
     * The path's cells. (Ordered)
     * Shares index with the wall indices such that the
     * combination of the two provides the wall of the 
     * cell the path crosses.
     */
    private final TriCell[] mCells;

    /**
     * The path is no longer valid or no longer being used.
     */
    private boolean mIsDisposed = false;

    /**
     * Controls how far target points are offset toward the interior
     * of the path.
     */
    private final float mOffsetFactor;

    /**
     * Used to indicate how close the y-value of a position needs to
     * be to the surface of the path to be considered still on the path.
     */
    private final float mPlaneTolerance;
    
    /**
     * The time the path was created or renewed.
     */
    private long mTimeStamp;

    /**
     * The path's link indices. (Ordered)
     * Shares index with the cell list. Provides the wall of the 
     * cell the path exits.
     * The last value in the array is always expected to
     * be {@link #NULL_INDEX} since there is no exit
     * wall for the last cell in the path.
     * <p>Dev Note TODO: I realized late in the game that this array may
     * not be needed. But it is being left in place in order to minimize
     * unnecessary changes near release.</p>
     */
    private final short[] mWallIndices;

    /**
     * Constructs an path that consists of a single cell.  (Start and goal
     * position is in the same cell.)
     * @param id The id of the path.  (Expected to be unique for the current
     * @param cell The path's single cell.
     * @param planeTolerence How close the y-value of a position needs to
     * be to the surface of the path to be considered still on the path.
     * @param offsetFactor Controls how far target points are offset toward the interior
     * of the path.
     * @throws NullPointerException If the cell argument is null.
     */
    public MasterPath(int id
            , TriCell cell
            , float planeTolerence
            , float offsetFactor)
    throws IllegalArgumentException
    {
        this(id, 1, planeTolerence, offsetFactor);
        mCells[0] = cell;
    }
    
    /**
     * Primary Constructor
     * @param id The id of the path.
     * @param cells  An ordered list of connected cells that
     * represent a path.  The first cell is considered the start cell.
     * The last cell is considered the goal cell.
     * Must contain at least one cell.
     * @param planeTolerence How close the y-value of a position needs to
     * be to the surface of the path to be considered still on the path.
     * @param offsetFactor Controls how far target points are offset toward the interior
     * of the path.
     * @throws IllegalArgumentException If the cells are not properly linked.
     * @throws NullPointerException If the cells argument is null.
     */
    public MasterPath(int id
            , TriCell[] cells
            , float planeTolerence
            , float offsetFactor)
        throws IllegalArgumentException
    {    
        this(id, cells.length, planeTolerence, offsetFactor);
        
        System.arraycopy(cells, 0, mCells, 0, cells.length);

        for (int iCell = 0; iCell < mCells.length - 1; iCell++)
        {
            short exitWall = (short)mCells[iCell].getLinkIndex(mCells[iCell+1]);
            if (exitWall == NULL_INDEX)
                throw new IllegalArgumentException("Invalid path.  No link from cell "
                        + iCell + " to cell " + iCell+1);
            mWallIndices[iCell] = exitWall;
        }
    }
    
    private MasterPath(int id, int pathSize, float planeTolerence, float offsetFactor)
    {
        this.mTimeStamp = System.currentTimeMillis();
        
        this.id = id;
        this.mPlaneTolerance = Math.max(Float.MIN_VALUE, planeTolerence);
        this.mOffsetFactor = Math.min(Math.max(0, offsetFactor), 0.5f);
        
        mCells = new TriCell[pathSize];
        mWallIndices = new short[pathSize];
        mWallIndices[mCells.length - 1] = NULL_INDEX;
    }

    /**
     * Disposes of the path, marking it as no longer valid.
     */
    public void dispose() { mIsDisposed = true; };
    
    /**
     * Gets the requested cell in the path.
     * @param index The index of the cell to retrieve.  (0 <= index < {@link #size()})
     * @return The requested cell in the path.
     */
    public TriCell getCell(int index) {  return mCells[index]; }
    
    /**
     * Discovers the index of the cell in the path.
     * @param cell The cell to search for.
     * @return The index of the cell in the path, or {@link TriCell#NULL_INDEX}
     * if the cell is not in the path.
     */
    public int getCellIndex(TriCell cell)
    {
        for (int i = 0; i < mCells.length; i++)
        {
            if (mCells[i] == cell)
                return i;
        }
        return NULL_INDEX;
    }
    
    /**
     * Returns a new path object configured for the provided goal position.
     * <p>Behavior is undefined if the goal position is not within 
     * the final cell of the path.</p>
     * @param goalX The x-value of the goal position. (goalX, goalY, goalZ)
     * @param goalY The y-value of the goal position. (goalX, goalY, goalZ)
     * @param goalZ The z-value of the goal position. (goalX, goalY, goalZ)
     * @return A new path object.
     */
    public Path getPath(float goalX, float goalY, float goalZ) 
    { 
        return this.new Path(goalX, goalY, goalZ); 
    }

    /**
     * Gets a copy of the ordered list of cells in the path.
     * @param outCells The array to copy the results into.  Must be of length {@link #size()}.
     */
    public TriCell[] getRawCopy(TriCell[] outCells)
    {
        System.arraycopy(mCells, 0, outCells, 0, mCells.length);
        return outCells;
    }
    
    /**
     * The last cell in the path.
     * @return The last cell in the path.
     */
    public TriCell goalCell() { return mCells[mCells.length - 1]; }
    
    /**
     * The path is no longer valid or no longer being used.
     * @return TRUE if the path is no longer valid or no longer being used.
     */
    public boolean isDisposed() { return mIsDisposed; }
    
    /**
     * Sets the time stamp of the path to the current system time.
     */
    public void resetTimestamp() { mTimeStamp = System.currentTimeMillis(); }
    
    /**
     * The number of cells in the path.
     * @return The number of cells in the path.
     */
    public int size() { return mCells.length; }
    
    /**
     * The first cell in the path.
     * @return The start cell of the path.
     */
    public TriCell startCell() { return mCells[0]; }
    
    /**
     * The time the path was created or renewed.
     */
    public long timestamp() { return mTimeStamp; }

    /**
     * Gets the most appropriate cell for a point within the path column.
     * This is a 2D check.  Potential cells are identified using column checks
     * If the y-axis distance for a cell is within {@link #mPlaneTolerance}, then
     * the cell is immediately returned.  Otherwise the cell with the minimum y-axis
     * distance is returned.
     * @param x The x-value of the point to check. (x, y, z)
     * @param y The y-value of the point to check. (x, y, z)
     * @param z The z-value of the point to check. (x, y, z)
     * @return The most appropriate cell for the point, or NULL if the point
     * is outside the path column.
     */
    private TriCell getCell(float x, float y, float z)
    {
        TriCell selectedCell = null;
        float minDistance = Float.MAX_VALUE;
        
        // Loop through cells.
        for (int iCell = 0; iCell < mCells.length; iCell++)
        {
            TriCell cell = mCells[iCell];
            if (cell.isInColumn(x, z))
            {
                float yDistance = Math.abs(cell.getPlaneY(x, z) - y);
                if (yDistance < mPlaneTolerance)
                    return cell;
                if (yDistance < minDistance)
                {
                    selectedCell = mCells[iCell];
                    minDistance = yDistance;
                }                
            }
        }
        
        return selectedCell;
    }

    /**
     * Gets the cell index of the most appropriate cell for a point within the path column.
     * This is a 2D check.  Potential cells are identified using column checks
     * If the y-axis distance for a cell is within {@link #mPlaneTolerance}, then
     * the cell is immediately returned.  Otherwise the cell with the minimum y-axis
     * distance is returned.
     * @param x The x-value of the point to check. (x, y, z)
     * @param y The y-value of the point to check. (x, y, z)
     * @param z The z-value of the point to check. (x, y, z)
     * @return The index for the most appropriate cell for the point,
     * or {@link TriCell#NULL_INDEX} if the point is outside the path column.
     */
    private int getCellIndex(float x, float y, float z)
    {
        int selectedCell = NULL_INDEX;
        float minDistance = Float.MAX_VALUE;
        
        // Loop through cells.
        for (int iCell = 0; iCell < mCells.length; iCell++)
        {
            TriCell cell = mCells[iCell];
            if (cell.isInColumn(x, z))
            {
                float yDistance = Math.abs(cell.getPlaneY(x, z) - y);
                if (yDistance < mPlaneTolerance)
                    return iCell;
                if (yDistance < minDistance)
                {
                    selectedCell = iCell;
                    minDistance = yDistance;
                }                
            }
        }
        
        return selectedCell;
    }

    /**
     * Checks to see if the point is within the given tolerance
     * of the cell plane.
     * This is a 2D check.  The x and z-values are used to determine
     * if the point is in the cell column, then the y-axis distance is checked
     * against the allowed tolerance.
     * @param x The x-value of the point to check. (x, y, z)
     * @param y The y-value of the point to check. (x, y, z)
     * @param z The z-value of the point to check. (x, y, z)
     * @param cellIndex The index of the cell to check. (0 <= index < {@link #size()}
     * @param tolerance The y-axis tolerance.
     * @return TRUE if the point is considered on the surface of the cell.  Otherwise FALSE.
     */
    private boolean isOnCell(float x, float y, float z, int cellIndex, float tolerance)
    {
         if (mCells[cellIndex].isInColumn(x, z))
         {
             /*
              * Is the reference position close enough to be considered
              * on the cell?
              */
             float planeY = mCells[cellIndex].getPlaneY(x, z);
             if (y >= planeY - tolerance && y <= planeY + tolerance)
                  return true;
         }
         return false;
    }
    
}

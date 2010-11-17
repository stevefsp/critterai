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
using System;
using org.critterai.math;
using org.critterai.math.geom;
using UnityEngine;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Represents a path corridor made up of ordered navigation cells.  
    /// The corridor is one Cell wide and represents a directed path from the start Cell
    /// to the end Cell.
    /// </summary>
    /// <remarks>Instances of this class are not thread-safe but can be used by multiple threads
    /// under the following conditions:
    /// <ul>
    /// <li>Its <see cref="TriCell">TriCell</see> cells are treated in a thread safe manner.</li>
    /// <li>The instance's reference is not shared between threads.</li>
    /// <li>Other threads access path data through instances of <see cref="Path">Path</see> 
    /// objects obtained via <see cref="GetPath(float, float, float)">GetPath</see>.</li>
    /// <li>References to <see cref="Path">Path</see> objects are not shared between threads.</li>
    /// </ul>
    /// <para>In summary, a single thread constructs and manages an instance of MasterPath
    /// and passes out instances of <see cref="Path">Path</see> to other threads as needed.</para>
    /// </remarks>
    public sealed class MasterPath
    {
        
        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;
        private const int NULL_INDEX = TriCell.NULL_INDEX;

        /// <summary>
        /// Provides operations useful to clients which need to navigate within
        /// a path corridor toward a goal point located in the end cell of the corridor. 
        /// </summary>
        /// <remarks>
        /// <para>Sharing instances of this class between clients will negate performance
        /// optimizations.</para>
        /// <para>Instances of this class are not thread safe.</para>
        /// </remarks>
        public class Path 
        {

            private readonly MasterPath mRoot;
            private int mLastCellIndex = 0;
            private readonly float mGoalX;
            private readonly float mGoalY;
            private readonly float mGoalZ;

            /// <summary>
            /// The x-value of the goal of the path. (GoalX, GoalY, GoalZ)
            /// </summary>
            public float GoalX
            {
                get { return mGoalX; }
            } 

            /// <summary>
            /// The y-value of the goal of the path. (GoalX, GoalY, GoalZ)
            /// </summary>
            public float GoalY
            {
                get { return mGoalY; }
            } 

            /// <summary>
            /// The z-value of the goal of the path. (GoalX, GoalY, GoalZ)
            /// </summary>
            public float GoalZ
            {
                get { return mGoalZ; }
            }

            /// <summary>
            /// The id of the <see cref="MasterPath">Masterpath</see> that backs 
            /// this path object.
            /// </summary>
            public int Id { get { return mRoot.Id; } }

            /// <summary>
            /// Indicates whether or not the path is valid.  Disposed
            /// paths should be discarded.
            /// </summary>
            public Boolean IsDisposed { get { return mRoot.mIsDisposed; } }

            /// <summary>
            /// The number of triangles expected when calling the 
            /// <see cref="GetPathPolys(float[], int[])">GetPathPolys</see> operation.
            /// </summary>
            public int PathPolyCount { get { return mRoot.mCells.Length; } }

            /// <summary>
            /// The number of vertices expected when calling the 
            /// <see cref="GetPathPolys(float[], int[])">GetPathPolys</see> operation.
            /// </summary>
            public int PathVertCount { get { return mRoot.mCells.Length * 3; } }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <remarks>Behavior is undefined if the goal position is not within 
            /// the final cell of the path.</remarks>
            /// <param name="root">The associated MasterPath object.</param>
            /// <param name="goalX">The x-value of the goal position. (goalX, goalY, goalZ)</param>
            /// <param name="goalY">The y-value of the goal position. (goalX, goalY, goalZ)</param>
            /// <param name="goalZ">The z-value of the goal position. (goalX, goalY, goalZ)</param>
            internal Path(MasterPath root, float goalX, float goalY, float goalZ) 
            {
                this.mRoot = root;
                this.mGoalX = goalX;
                this.mGoalY = goalY;
                this.mGoalZ = goalZ;
            }
            
            /// <summary>
            /// Snaps the provided point to the closest location on the surface
            /// of the path corridor.  
            /// <para>If the point need to be moved along the xz axes, it will be moved to
            /// slightly within the closest edge of the path rather than on the edge.</para>
            /// <para>This is a non-trivial operation.</para>
            /// </summary>
            /// <param name="x">The x-value of the point (x, y, z).</param>
            /// <param name="y">The y-value of the point (x, y, z).</param>
            /// <param name="z">The z-value of the point (x, y, z).</param>
            /// <returns>A point snapped to the surface of the path.</returns>
            public Vector3 ForceToPath(float x, float y, float z)
            {
                Vector3 result = new Vector3();
                TriCell.GetClosestCell(x, y, z, mRoot.mCells, out result);
                return result;
            }
            
            /// <summary>
            /// Snaps the provided point to the plane of the path corridor.
            /// </summary>
            /// <param name="x">The x-value of the point (x, y, z).</param>
            /// <param name="y">The y-value of the point (x, y, z).</param>
            /// <param name="z">The z-value of the point (x, y, z).</param>
            /// <param name="result">The point with its y-value snapped to the surface of the path corridor,
            /// or the original (x, y, z) value if the provided point is outside of the path
            /// corridor.</param>
            /// <returns>TRUE if the the point was within the path corridor.  Otherwise FALSE.</returns>
            public Boolean ForceYToPath(float x, float y, float z, out Vector3 result)
            {
                TriCell cell = GetCellForPosition(x, y, z);
                if (cell == null)
                {
                    result = new Vector3(x, y, z);
                    return false;
                }
                result = new Vector3(x, cell.GetPlaneY(x, z), z);
                return true;
            }
            
            /// <summary>
            /// An version of <see cref="MasterPath.GetCell(float, float, float)">
            /// MasterPath.GetCell</see> that is optimized for the current path object.
            /// </summary>
            /// <remarks>As long as the xz-position is in the path column, it will return
            /// a result.  The y-value is used to deal with overlapping cells.
            /// </remarks>
            /// <param name="x">The x-value of the point (x, y, z).</param>
            /// <param name="y">The y-value of the point (x, y, z).</param>
            /// <param name="z">The z-value of the point (x, y, z).</param>
            /// <returns>The best Cell for the position or <see cref="TriCell.NULL_INDEX">NULL_INDEX</see>
            /// if the position is outside the path column.</returns>
            private TriCell GetCellForPosition(float x, float y, float z)
            {
                // Search optimization:  Check last search location.
                if (mRoot.IsOnCell(x, y, z, mLastCellIndex, mRoot.mPlaneTolerance))
                    // Early exit OK.
                    return mRoot.mCells[mLastCellIndex];
                return mRoot.GetCell(x, y, z);
            }
            
            /// <summary>
            /// An version of <see cref="MasterPath.GetCellIndex(float, float, float)">
            /// MasterPath.GetCellIndex</see> optimized for the current path object.
            /// </summary>
            /// <remarks>As long as the xz-position is in the path column, it will return
            /// a result.  The y-value is used to deal with overlapping cells.
            /// </remarks>
            /// <param name="x">The x-value of the point (x, y, z).</param>
            /// <param name="y">The y-value of the point (x, y, z).</param>
            /// <param name="z">The z-value of the point (x, y, z).</param>
            /// <returns>The best cell index for the position or 
            /// <see cref="TriCell.NULL_INDEX">NULL_INDEX</see> if the position is outside
            /// the path column.</returns>
            private int GetCellIndexForPosition(float x, float y, float z)
            {
                // Search optimization:  Check last search location.
                if (mRoot.IsOnCell(x, y, z, mLastCellIndex, mRoot.mPlaneTolerance))
                    // Early exit OK.
                    return mLastCellIndex;
                return mRoot.GetCellIndex(x, y, z);
            }
            
            /// <summary>
            /// The goal point of the path.
            /// </summary>
            public Vector3 Goal
            {
                get
                {
                    return new Vector3(mGoalX, mGoalY, mGoalZ);
                }
            }
            
            /// <summary>
            /// Returns a list of triangles representing the path corridor.
            /// Uses standard vertices/indices representation with clockwise wrapping.
            /// </summary>
            /// <param name="outVerts">An array of size 
            /// <see cref="PathVertCount">PathVertCount</see>*3 to load the
            /// vertices into.  Form: (x, y, z)</param>
            /// <param name="outIndices">An array of size 
            /// <see cref="PathPolyCount">PathPolyCount</see>*3 to load the
            /// polygons into. Form: (v1, v2, v3)</param>
            public void GetPathPolys(float[] outVerts, int[] outIndices)
            {
                if (outVerts == null 
                        || outIndices == null
                        || outVerts.Length < mRoot.mCells.Length * 9
                        || outIndices.Length < mRoot.mCells.Length * 3)
                    return;
                for (int iCell = 0; iCell < mRoot.mCells.Length; iCell++)
                {
                    int pVert = iCell*9;
                    for (int iv = 0; iv < 3; iv++)
                    {
                        for (int ivp = 0; ivp < 3; ivp++)
                        {
                            outVerts[pVert + iv * 3 + ivp] = mRoot.mCells[iCell].GetVertexValue(iv, ivp);
                        }
                    }
                    int pPoly = iCell*3;
                    outIndices[pPoly+0] = pPoly+0;
                    outIndices[pPoly+1] = pPoly+1;
                    outIndices[pPoly+2] = pPoly+2;
                }
            }
            
            /// <summary>
            /// Gets the farthest visible waypoint in the direction of the goal.
            /// Effectively, this is on-the-fly string pulling.
            /// <para>Will return false if the provided point lies outside of the
            /// path column or the path has been disposed.</para>
            /// </summary>
            /// <param name="fromX">The x-value of the position being evaluated. (fromX, fromY, fromZ)</param>
            /// <param name="fromY">The y-value of the position being evaluated. (fromX, fromY, fromZ)</param>
            /// <param name="fromZ">The z-value of the position being evaluated. (fromX, fromY, fromZ)</param>
            /// <param name="target">The vector object to load the target information into.  
            /// The vector may be updated even if the operation returns FALSE.  But the Data
            /// will only be valid if the operation returns TRUE.</param>
            /// <returns>TRUE if a valid waypoint could be found.  FALSE if the "from" point
            /// is either outside of the path column or the backing 
            /// <see cref="MasterPath">MasterPath</see> has been disposed.</returns>
            public Boolean GetTarget(float fromX, float fromY, float fromZ, out Vector3 target)
            {
                
                /*
                 * Design note:
                 * 
                 * The out argument is also used as a working variable.
                 * The true out value is Set just before returning. 
                 */

                if (mRoot.mIsDisposed)
                {
                    target = Vector3.zero;
                    return false;
                }
                
                int iStart = GetCellIndexForPosition(fromX, fromY, fromZ);
                if (iStart == NULL_INDEX)
                {
                    target = Vector3.zero;
                    return false;
                }
                
                // Handle special cases.
                if (mRoot.mCells.Length == 1)
                {
                    // Start and goal must be in the same Cell.
                    target = new Vector3(mGoalX, mGoalY, mGoalZ);
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
                 * case the start Cell is moved forward
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
                while (iStart < mRoot.mCells.Length)
                {

                    if (iStart == mRoot.mCells.Length - 1)
                    {
                        // We have reached the end Cell.  
                        // So the apex is in the end Cell.
                        target = new Vector3(mGoalX, mGoalY, mGoalZ);
                        return true;
                    }
                    
                    // Left vertex.
                    target = mRoot.mCells[iStart].GetVertex(mRoot.mWallIndices[iStart]);
                    leftX = target.x;
                    leftY = target.y;
                    leftZ = target.z;    
                    
                    // Right vertex.
                    // The right vertex index will always be the next index 
                    // above the left vertex. (wrapped)
                    target = mRoot.mCells[iStart].GetVertex(
                            (mRoot.mWallIndices[iStart] + 1 < mRoot.mCells[iStart].MaxLinks
                                    ? mRoot.mWallIndices[iStart] + 1 : 0));
                    rightX = target.x;
                    rightY = target.y;
                    rightZ = target.z;        
                    
                    if ((fromX == leftX && fromY == leftY && fromZ == leftZ)
                            || (fromX == rightX && fromY == rightY && fromZ == rightZ))
                        // The apex is on one of the exit wall vertices.
                        // Move to the next Cell.
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
                 * if it gets to the the last Cell, it will select the goal,
                 * or the funnel right or left vertex.
                 */
                for (int iCurrCell = iStart + 1
                        ; iCurrCell < mRoot.mCells.Length
                        ; iCurrCell++)
                {
                    float area;  // Signed area x 2.
                    // Find the exit edge for this Cell.
                    if (mRoot.mWallIndices[iCurrCell] == NULL_INDEX)
                    {
                        /*
                         * Special case:  No exit edge.
                         * Should only happen for the last Cell of the path.
                         * In this case, the goal point is the final vertex
                         * to test.  If it is visible, then select it.  If it is
                         * not, then select the right or left vertex as appropriate.
                         */
                        area = Triangle2.GetSignedAreaX2(fromX, fromZ, mGoalX, mGoalZ, rightX, rightZ);
                        if (area < TOLERANCE_STD)
                        {
                            area = Triangle2.GetSignedAreaX2(fromX, fromZ, mGoalX, mGoalZ, leftX, leftZ);
                            if (area > -TOLERANCE_STD)
                            {
                                // Goal is within the funnel.
                                target = new Vector3(mGoalX, mGoalY, mGoalZ);
                                return true;
                            }
                            else
                            {
                                // Goal is to the left of the funnel.
                                // Select left.    
                                target = Vector3Util.TranslateToward(leftX, leftY, leftZ
                                        , rightX, rightY, rightZ
                                        , mRoot.mOffsetFactor);
                                return true;
                            }
                        }
                        else
                        {
                            // Goal is to the right of the funnel.
                            // Select right.
                            target = Vector3Util.TranslateToward(rightX, rightY, rightZ
                                    , leftX, leftY, leftZ
                                    , mRoot.mOffsetFactor);
                            return true;
                        }
                    }
                    // We haven't reached the goal Cell.
                    else
                    {
                        // Test the right vertex.
                        int iCurrVertex = (mRoot.mWallIndices[iCurrCell] + 1 < mRoot.mCells[iCurrCell].MaxLinks
                                ? mRoot.mWallIndices[iCurrCell] + 1 : 0);
                        target = mRoot.mCells[iCurrCell].GetVertex(iCurrVertex);
                        if (target.x != rightX || target.z != rightZ)
                        {
                            // Test point is NOT the same point as
                            // the right vertex.
                            area = Triangle2.GetSignedAreaX2(fromX, fromZ
                                    , target.x, target.z
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
                                area = Triangle2.GetSignedAreaX2(fromX, fromZ
                                        , target.x, target.z
                                        , leftX, leftZ);
                                if (area > -TOLERANCE_STD)
                                {
                                    /*
                                     * Test point is to the right of the left
                                     *  vertex.  So it is inside the funnel.
                                     *  Narrow the funnel on the right.
                                     */
                                    rightX = target.x;
                                    rightY = target.y;
                                    rightZ = target.z;
                                }
                                else
                                {
                                    /*
                                     * Test point is to the left of the left vertex.
                                     * So the right wall veers off to the left of the
                                     * funnel.
                                     * Select the left vertex.
                                     */
                                    target = Vector3Util.TranslateToward(leftX, leftY, leftZ
                                            , rightX, rightY, rightZ
                                            , mRoot.mOffsetFactor);
                                    return true;
                                }
                            }                        
                        }
                        // Test the left vertex.
                        iCurrVertex = mRoot.mWallIndices[iCurrCell];
                        target = mRoot.mCells[iCurrCell].GetVertex(iCurrVertex);
                        if (target.x != leftX || target.z != leftZ)
                        {
                            // Test point is NOT the same point as
                            // the left vertex.
                            area = Triangle2.GetSignedAreaX2(fromX, fromZ
                                    , target.x, target.z
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
                                area = Triangle2.GetSignedAreaX2(fromX, fromZ
                                        , target.x, target.z
                                        , rightX, rightZ);
                                if (area < TOLERANCE_STD)
                                {
                                    /*
                                     * Test vertex lies to the left of the right
                                     * vertex.  So it is within the funnel.
                                     * Narrow the funnel on the left.
                                     */
                                    leftX = target.x;
                                    leftY = target.y;
                                    leftZ = target.z;
                                }
                                else
                                {
                                    /*
                                     * Test vertex light to the right of the right
                                     * vertex.  So the left wall veers off to the right
                                     * of the funnel.  Select the right vertex.
                                     */
                                    target = Vector3Util.TranslateToward(rightX, rightY, rightZ
                                            , leftX, leftY, leftZ
                                            , mRoot.mOffsetFactor);
                                    return true;
                                }
                            }                    
                        }
                    }
                }
                
                // Should never get here unless there is a logic or Data error.
                target = Vector3.zero;
                return false;
                    
            }
            
            /// <summary>
            /// Indicates whether or not the point is within the column of the path
            /// corridor.
            /// </summary>
            /// <remarks>
            /// It is not necessary or efficient to call this operation before calling the 
            /// <see cref="GetTarget">GetTarget</see> operation.  Just check that
            /// operation's return value to see if a target was obtained.
            /// </remarks>
            /// <param name="x">The x-value of the point to Evaluate. (x, z) from the point (x, y, z)</param>
            /// <param name="z">The z-value of the point to Evaluate. (x, z) from the point (x, y, z)</param>
            /// <returns>TRUE if the point is within the column of the path. Otherwise FALSE.</returns>
            public Boolean IsInPathColumn(float x, float z)
            {
                foreach (TriCell cell in mRoot.mCells)
                {
                    if (cell.IsInColumn(x, z))
                        return true;
                }
                return false;
            }
            
        }

        private int mId;
        
        /// <summary>
        /// The path's cells. (Ordered)
        /// Shares index with the wall indices such that the
        /// combination of the two provides the wall of the 
        /// Cell the path crosses.
        /// </summary>
        private readonly TriCell[] mCells;

        /// <summary>
        /// The path is no longer valid or no longer being used.
        /// </summary>
        private Boolean mIsDisposed = false;

        /// <summary>
        /// Controls how far target points are offset toward the interior
        /// of the path.
        /// </summary>
        private readonly float mOffsetFactor;

        /// <summary>
        /// Used to indicate how close the y-value of a position needs to
        /// be to the surface of the path to be considered still on the path.
        /// </summary>
        private readonly float mPlaneTolerance;

        /// <summary>
        /// The time the path was created or renewed.
        /// </summary>
        private long mTimeStamp;

        /// <summary>
        /// The path's Link indices. (Ordered)
        /// Shares index with the Cell list. Provides the wall of the 
        /// Cell the path exits.
        /// The last value in the array is always expected to
        /// be NULL_INDEX since there is no exit
        /// wall for the last cell in the path.
        /// <para>Dev Note TODO: I realized late in the game that this array may
        /// not be needed. But it is being left in place in order to minimize
        /// unnecessary changes near release.</para>
        /// </summary>
        private readonly short[] mWallIndices;

        /// <summary>
        /// The Id of the path.
        /// </summary>
        public int Id { get { return mId; } }

        /// <summary>
        /// The last cell in the path.
        /// </summary>
        public TriCell GoalCell { get { return mCells[mCells.Length - 1]; } }

        /// <summary>
        /// If TRUE, the path is no longer valid or no longer being used.
        /// </summary>
        public Boolean IsDisposed { get { return mIsDisposed; } }

        /// <summary>
        /// The number of cells in the path.
        /// </summary>
        public int Size { get { return mCells.Length; } }

        /// <summary>
        /// The first cell in the path.
        /// </summary>
        public TriCell StartCell { get { return mCells[0]; } }

        /// <summary>
        /// The time the path was created or renewed.
        /// </summary>
        public long Timestamp { get { return mTimeStamp; } }

        /// <summary>
        /// Constructs an path that consists of a single Cell.  (Start and goal
        /// position is in the same Cell.)
        /// </summary>
        /// <param name="id">The id of the path.</param>
        /// <param name="cell">The path's single cell.</param>
        /// <param name="planeTolerence">How close the y-value of a position needs to
        /// be to the surface of the path to be considered still on the path.</param>
        /// <param name="offsetFactor">Controls how far target points are offset toward the interior
        /// of the path.</param>
        public MasterPath(int id
                , TriCell cell
                , float planeTolerence
                , float offsetFactor)
            : this(id, 1, planeTolerence, offsetFactor)
        {
            mCells[0] = cell;
        }
        
        /// <summary>
        /// Primary Constructor
        /// </summary>
        /// <param name="id">The id of the path.</param>
        /// <param name="cells">An ordered list of connected cells that
        /// represent a path.  The first Cell is considered the start Cell.
        /// The last Cell is considered the goal Cell.
        /// Must contain at least one Cell.</param>
        /// <param name="planeTolerence">How close the y-value of a position needs to
        /// be to the surface of the path to be considered still on the path.</param>
        /// <param name="offsetFactor">Controls how far target points are offset toward the interior
        /// of the path.</param>
        public MasterPath(int id
                , TriCell[] cells
                , float planeTolerence
                , float offsetFactor)
            : this(id, cells.Length, planeTolerence, offsetFactor)
        {    
            Array.Copy(cells, 0, mCells, 0, cells.Length);

            for (int iCell = 0; iCell < mCells.Length - 1; iCell++)
            {
                short exitWall = (short)mCells[iCell].GetLinkIndex(mCells[iCell+1]);
                //if (exitWall == NULL_INDEX)
                //    throw new ArgumentException("Invalid path.  No link from cell "
                //            + iCell + " to cell " + iCell+1);
                mWallIndices[iCell] = exitWall;
            }
        }

        /// <summary>
        /// Shared Constructor
        /// </summary>
        /// <param name="id">The id of the path.</param>
        /// <param name="pathSize">The number of cells in the path.</param>
        /// <param name="planeTolerence">How close the y-value of a position needs to
        /// be to the surface of the path to be considered still on the path.</param>
        /// <param name="offsetFactor">Controls how far target points are offset toward the interior
        /// of the path.</param>
        private MasterPath(int id, int pathSize, float planeTolerence, float offsetFactor)
        {
            this.mTimeStamp = DateTime.Now.Ticks;
            
            this.mId = id;
            this.mPlaneTolerance = Math.Max(float.Epsilon, planeTolerence);
            this.mOffsetFactor = Math.Min(Math.Max(0, offsetFactor), 0.5f);
            
            mCells = new TriCell[pathSize];
            mWallIndices = new short[pathSize];
            mWallIndices[mCells.Length - 1] = NULL_INDEX;
        }

        /// <summary>
        /// Disposes of the path, marking it as no longer valid.
        /// </summary>
        public void Dispose() 
        { 
            mIsDisposed = true; 
        }
        
        /// <summary>
        /// Gets the requested Cell in the path.
        /// </summary>
        /// <param name="index">The index of the Cell to retrieve.  Value must be less than the value of
        /// <see cref="Size">Size</see></param>
        /// <returns>The requested Cell in the path.</returns>
        public TriCell GetCell(int index) {  return mCells[index]; }
        
        /// <summary>
        /// Discovers the index of the cell in the path.
        /// </summary>
        /// <param name="cell">The cell to search for.</param>
        /// <returns>The index of the cell in the path, or 
        /// <see cref="TriCell.NULL_INDEX">NULL_INDEX</see>
        /// if the cell is not in the path.</returns>
        public int GetCellIndex(TriCell cell)
        {
            for (int i = 0; i < mCells.Length; i++)
            {
                if (mCells[i] == cell)
                    return i;
            }
            return NULL_INDEX;
        }
        
        /// <summary>
        /// Returns a new path object configured for the provided goal position.
        /// <para>Behavior is undefined if the goal position is not within 
        /// the final cell of the path.</para>
        /// </summary>
        /// <param name="goalX">The x-value of the goal position. (goalX, goalY, goalZ)</param>
        /// <param name="goalY">The y-value of the goal position. (goalX, goalY, goalZ)</param>
        /// <param name="goalZ">The z-value of the goal position. (goalX, goalY, goalZ)</param>
        /// <returns>A new path object.</returns>
        public Path GetPath(float goalX, float goalY, float goalZ) 
        { 
            return new Path(this, goalX, goalY, goalZ); 
        }

        /// <summary>
        /// Gets a copy of the ordered list of cells in the path.
        /// </summary>
        /// <param name="outCells">The array to copy the results into.  
        /// Must be of length <see cref="Size">Size</see>.</param>
        /// <returns>A reference to the <paramref name="outCells"/>array.</returns>
        public TriCell[] GetRawCopy(TriCell[] outCells)
        {
            Array.Copy(mCells, 0, outCells, 0, mCells.Length);
            return outCells;
        }

        /// <summary>
        /// Sets the time stamp of the path to the current system ticks.
        /// </summary>
        public void ResetTimestamp() { mTimeStamp = DateTime.Now.Ticks; }

        /// <summary>
        /// Gets the most appropriate Cell for a point within the path column.
        /// </summary>
        /// <remarks>
        /// This is a 2D check.  Potential cells are identified using column checks
        /// If the y-axis distance for a cell is within mPlaneTolerance, then
        /// the cell is immediately returned.  Otherwise the cell with the minimum y-axis
        /// distance is returned.
        /// </remarks>
        /// <param name="x">The x-value of the point to check. (x, y, z)</param>
        /// <param name="y">The y-value of the point to check. (x, y, z)</param>
        /// <param name="z">The z-value of the point to check. (x, y, z)</param>
        /// <returns>The most appropriate Cell for the point, or NULL if the point
        /// is outside the path column.</returns>
        private TriCell GetCell(float x, float y, float z)
        {
            TriCell selectedCell = null;
            float minDistance = float.MaxValue;
            
            // Loop through cells.
            for (int iCell = 0; iCell < mCells.Length; iCell++)
            {
                TriCell cell = mCells[iCell];
                if (cell.IsInColumn(x, z))
                {
                    float yDistance = Math.Abs(cell.GetPlaneY(x, z) - y);
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

        /// <summary>
        /// Gets the cell index of the most appropriate cell for a point within the path column.
        /// </summary>
        /// <remarks>
        /// This is a 2D check.  Potential cells are identified using column checks
        /// If the y-axis distance for a cell is within mPlaneTolerance, then
        /// the cell is immediately returned.  Otherwise the cell with the minimum y-axis
        /// distance is returned.
        /// </remarks>
        /// <param name="x">The x-value of the point to check. (x, y, z)</param>
        /// <param name="y">The y-value of the point to check. (x, y, z)</param>
        /// <param name="z">The z-value of the point to check. (x, y, z)</param>
        /// <returns>The index for the most appropriate Cell for the point,
        /// or NULL_INDEX if the point is outside the path column.</returns>
        private int GetCellIndex(float x, float y, float z)
        {
            int selectedCell = NULL_INDEX;
            float minDistance = float.MaxValue;
            
            // Loop through cells.
            for (int iCell = 0; iCell < mCells.Length; iCell++)
            {
                TriCell cell = mCells[iCell];
                if (cell.IsInColumn(x, z))
                {
                    float yDistance = Math.Abs(cell.GetPlaneY(x, z) - y);
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

        /// <summary>
        /// Checks to see if the point is within the given tolerance
        /// of the Cell plane.
        /// <para>This is a 2D check.  The x and z-values are used to determine
        /// if the point is in the Cell column, then the y-axis distance is checked
        /// against the allowed tolerance.</para>
        /// </summary>
        /// <param name="x">The x-value of the point to check. (x, y, z)</param>
        /// <param name="y">The y-value of the point to check. (x, y, z)</param>
        /// <param name="z">The z-value of the point to check. (x, y, z)</param>
        /// <param name="cellIndex">The index of the Cell to check. Value must be less than the value of
        /// {@Link #Size()}</param>
        /// <param name="tolerance">The y-axis tolerance.</param>
        /// <returns>TRUE if the point is considered on the surface of the Cell.  Otherwise FALSE.</returns>
        private Boolean IsOnCell(float x, float y, float z, int cellIndex, float tolerance)
        {
             if (mCells[cellIndex].IsInColumn(x, z))
             {
                 /*
                  * Is the reference position close enough to be considered
                  * on the Cell?
                  */
                 float planeY = mCells[cellIndex].GetPlaneY(x, z);
                 if (y >= planeY - tolerance && y <= planeY + tolerance)
                      return true;
             }
             return false;
        }
        
    }
}

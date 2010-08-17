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
using System.Collections.Generic;
using org.critterai.math;
using org.critterai.math.geom;
using UnityEngine;

namespace org.critterai.nav.nmpath
{

    /// <summary>
    /// Represents an triangle cell within a navigation mesh. 
    /// </summary>
    /// <remarks>
    /// Limited to representing polygons with normals that are primarily "up".  This is because
    /// certain operations project onto the xz-plane and will not function properly if the area
    /// of the xz-plan projection is too small.
    /// <para>This class is optimized for speed.  To support this priority, no argument validation is
    /// performed.  E.G. No null checks.  No checks to see whether index arguments are in range.</para>
    /// <para>Instances of this class are not thread-safe but can be used by multiple threads without
    /// synchronization under the following conditions:</para>
    /// <ul>
    /// <li>The <see cref="Link">Link</see> operation is not performed after object references
    /// are shared between threads.  I.e. Perform linking at construction and never again.</li>
    /// <li>Both objects involved in the <see cref="Link">Link</see> operation are under the
    /// control of a single thread during the operation.</li>
    /// </ul>
    /// <para>In summary, objects of this class can be shared safely between threads if they are treated
    /// as immutable after construction and linking.</para>
    /// <para>Static operations are thread safe.</para>
    /// </remarks>
    public sealed class TriCell
    {
        
        /*
         * Design notes:
         * 
         * In general, choosing to use more memory for
         * better local performance.
         * 
         * It is important that the vertex and wall constants 
         * remain in synch.  E.G. VERTA == WALLAB, VERTC = WALLAC
         * Don't mess with them lightly.
         * 
         */

        private const float TOLERANCE_STD = MathUtil.TOLERANCE_STD;

        /// <summary>
        /// How far to offset from the beginning of a mWalls entry
        /// to get to the wall distance information.
        /// </summary>
        private const int DISTANCE_OFFSET = 5;

        /// <summary>
        /// How far to offset from the beginning of a mWalls entry
        /// to get to the wall midpoint information.
        /// </summary>
        private const int MIDPOINT_OFFSET = 2;

        /// <summary>
        /// A value indicating lack of a valid index.
        /// </summary>
        public const int NULL_INDEX = -1;
        private const int VERTA = 0;
        
        private const int VERTB = 1;
        
        private const int VERTC = 2;
        private const int VERTCOUNT = 3;
        private const int WALLAB = 0;
        
        private const int WALLBC = 1;
        private const int WALLCA = 2;
        private const int WALLSTRIDE = 8;
        
        private readonly float mBoundsMaxX;
        private readonly float mBoundsMaxZ;
        private readonly float mBoundsMinX;
        private readonly float mBoundsMinZ;

        private readonly float mCentroidX;
        private readonly float mCentroidY;
        private readonly float mCentroidZ;

        private readonly float mD;

        /// <summary>
        /// References to cells that attach to this cell. A NULL link denotes a solid wall.
        /// <para>Link indices are associated with wall indices which are associated with vertex indices. 
        /// E.G. iLinkA = iWallAB == iVertA</para>
        /// <para>Form: (linkA, linkB, ...linkN) * 1</para>
        /// <para>Stride: 1</para>
        /// </summary>
        private readonly TriCell[] mLinks = new TriCell[VERTCOUNT];

        /// <summary>
        /// References to the walls that attach to this cell's walls. 
        /// A NULL_INDEX denotes an unlinked wall.
        /// <para>Link indices are associated with wall indices which are associated with vertex indices. 
        /// E.G. iLinkA = iWallAB == iVertA</para>
        /// <para>Form: (linkA, linkB, ...linkN) * 1</para>
        /// <para>Stride = 1</para>
        /// </summary>
        private readonly int[] mLinkWalls = new int[VERTCOUNT];

        /// <summary>
        /// The x-value of the cell plane normal 
        /// </summary>
        private readonly float mNormalX;

        /// <summary>
        /// The y-value of the cell plane normal 
        /// </summary>
        private readonly float mNormalY;

        /// <summary>
        /// The c-value of the cell plane normal 
        /// </summary>
        private readonly float mNormalZ;

        /// <summary>
        /// The pointer (not indices) of the cell vertices in the mVerts array.
        /// (pVertA, pVertB, pVertC)
        /// Stride = 1
        /// </summary>
        private readonly int[] mVertPtr = new int[VERTCOUNT];

        /// <summary>
        /// The source vertices of the cell.
        /// Form: (x, y, z)
        /// Stride = 3
        /// </summary>
        private readonly float[] mVerts;

        /// <summary>
        /// Pre-calculated wall Data.
        /// <para>Form: (normalX, normalZ, midpointX, midpointY, midpointZ, 
        ///                 internalDistA, internalDistB, internalDistC) * 3</para>
        /// <para>Stride: WALLSTRIDE</para>
        /// <para>Use DISTANCE_OFFSET and MIDPOINT_OFFSET to offset
        /// to needed Data.</para>
        /// <para>Link indices are associated with wall indices which are associated with vertex indices. 
        /// E.G. iLinkA = iWallAB == iVertA</para>
        /// <para>The order of distance information is consistent with wall ordering. (A, B, C)
        /// A value of zero is used for the position where the wall is self-referencing.</para>
        /// </summary>
        private readonly float[] mWalls = new float[3*WALLSTRIDE];
        
        /// <summary>
        /// The maximum x-bounds of the cell.
        /// </summary>
        public float BoundsMaxX
        {
            get { return mBoundsMaxX; }
        } 

        /// <summary>
        /// The maximum z-bounds of the cell.
        /// </summary>
        public float BoundsMaxZ
        {
            get { return mBoundsMaxZ; }
        } 

        /// <summary>
        ///  The minimum x-bounds of the cell.
        /// </summary>
        public float BoundsMinX
        {
            get { return mBoundsMinX; }
        } 

        /// <summary>
        /// The minimum z-bounds of the cell.
        /// </summary>
        public float BoundsMinZ
        {
            get { return mBoundsMinZ; }
        } 

        /// <summary>
        ///  The x-value of the centroid of the cell. (CentroidX, CentroidY, CentroidZ)
        /// </summary>
        public float CentroidX
        {
            get { return mCentroidX; }
        } 

        /// <summary>
        /// The y-value of the centroid of the cell. (CentroidX, CentroidY, CentroidZ)
        /// </summary>
        public float CentroidY
        {
            get { return mCentroidY; }
        } 

        /// <summary>
        /// The z-value of the centroid of the cell. (CentroidX, CentroidY, CentroidZ)
        /// </summary>
        public float CentroidZ
        {
            get { return mCentroidZ; }
        } 

        /// <summary>
        /// Constant D for the cell's plane.
        /// </summary>
        /// <remarks>Equation: ax + by + cz + mD = 0
        /// where the vector (a, b, c) is the plane's normal
        /// and the point (x, y, z) is a reference point on the plane, in this
        /// case the cell's centroid.</remarks>
        public float D
        {
            get { return mD; }
        } 

        /// <summary>
        /// The number of links actually in use in the cell.
        /// E.G. Links that are non-null.
        /// </summary>
        /// <seealso cref="TriCell.MaxLinks"/>
        public int LinkCount
        {
            get
            {
                return (mLinks[WALLAB] == null ? 0 : 1)
                            + (mLinks[WALLBC] == null ? 0 : 1)
                            + (mLinks[WALLCA] == null ? 0 : 1);
            }
        }
        
        /// <summary>
        /// The maximum possible links for the cell.
        /// </summary>
        /// <seealso cref="TriCell.LinkCount"/>
        public int MaxLinks { get { return VERTCOUNT; } }

        /// <summary>
        /// Constructor. Vertices must be wrapped clockwise.
        /// </summary>
        /// <param name="verts">The vertices array in the form (x, y, z).</param>
        /// <param name="vertAIndex">The index of vertex A.</param>
        /// <param name="vertBIndex">The index of vertex B.</param>
        /// <param name="vertCIndex">The index of vertex C.</param>
        public TriCell(float[] verts, int vertAIndex, int vertBIndex, int vertCIndex)
        {
            
            if (verts == null)
                throw new ArgumentNullException("verts", "Vertices argument is null");
            
            mVerts = verts;
            
            mVertPtr[VERTA] = vertAIndex*3;
            mVertPtr[VERTB] = vertBIndex*3;
            mVertPtr[VERTC] = vertCIndex*3;
            
            if (mVertPtr[VERTA] < 0
                    || mVertPtr[VERTA]+2 >= mVerts.Length
                    || mVertPtr[VERTB] < 0
                    || mVertPtr[VERTB]+2 >= mVerts.Length
                    || mVertPtr[VERTC] < 0
                    || mVertPtr[VERTC]+2 >= mVerts.Length)
                throw new ArgumentOutOfRangeException("vertIndex", "One or more vertex indices are invalid.");
            
            Vector3 workingVector3 = new Vector3();
            
            // Convenience variables
            float ax = mVerts[mVertPtr[VERTA]];
            float ay = mVerts[mVertPtr[VERTA]+1];
            float az = mVerts[mVertPtr[VERTA]+2];
            float bx = mVerts[mVertPtr[VERTB]];
            float by = mVerts[mVertPtr[VERTB]+1];
            float bz = mVerts[mVertPtr[VERTB]+2];
            float cx = mVerts[mVertPtr[VERTC]];
            float cy = mVerts[mVertPtr[VERTC]+1];
            float cz = mVerts[mVertPtr[VERTC]+2];
            
            // Calculate the cells normal.
            workingVector3 = Triangle3.GetNormal(ax, ay, az, bx, by, bz, cx, cy, cz);
            mNormalX = workingVector3.x;
            mNormalY = workingVector3.y;
            mNormalZ = workingVector3.z;
            
            // Calculate the cell's centroid and mD-constant.
            workingVector3 = Polygon3.GetCentroid(ax, ay, az, bx, by, bz, cx, cy, cz);
            mCentroidX = workingVector3.x;
            mCentroidY = workingVector3.y;
            mCentroidZ = workingVector3.z;

            mD = -Vector3Util.Dot(workingVector3.x, workingVector3.y, workingVector3.z, mNormalX, mNormalY, mNormalZ);
            
            // Calculate the xz-plane bounds.
            mBoundsMinX = Math.Min(ax, Math.Min(bx, cx));
            mBoundsMinZ = Math.Min(az, Math.Min(bz, cz));
            mBoundsMaxX = Math.Max(ax, Math.Max(bx, cx));
            mBoundsMaxZ = Math.Max(az, Math.Max(bz, cz));
            
            // Calculate and store the normal each wall.
            // Wall AB
            // Note that a disposable object is being generated here.
            Vector2 v = Line2.GetNormalAB(ax, az, bx, bz);
            mWalls[WALLAB*WALLSTRIDE] = v.x;
            mWalls[WALLAB*WALLSTRIDE+1] = v.y;
            // Wall BC
            v = Line2.GetNormalAB(bx, bz, cx, cz);
            mWalls[WALLBC*WALLSTRIDE] = v.x;
            mWalls[WALLBC*WALLSTRIDE+1] = v.y;
            // Wall CA
            v = Line2.GetNormalAB(cx, cz, ax, az);
            mWalls[WALLCA*WALLSTRIDE] = v.x;
            mWalls[WALLCA*WALLSTRIDE+1] = v.y;
            
            // Calculate the mid-points for each wall.
            // Wall AB
            mWalls[WALLAB*WALLSTRIDE+MIDPOINT_OFFSET] = (ax + bx)/2.0f;
            mWalls[WALLAB*WALLSTRIDE+MIDPOINT_OFFSET+1] = (ay + by)/2.0f;
            mWalls[WALLAB*WALLSTRIDE+MIDPOINT_OFFSET+2] = (az + bz)/2.0f;
            // Wall BC
            mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET] = (bx + cx)/2.0f;
            mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET+1] = (by + cy)/2.0f;
            mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET+2] = (bz + cz)/2.0f;
            // Wall CA
            mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET] = (cx + ax)/2.0f;
            mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET+1] = (cy + ay)/2.0f;
            mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET+2] = (cz + az)/2.0f;
            
            // Calculate and store the distances between wall midpoints. (Between links.)
            // Distance AA
            mWalls[WALLAB+DISTANCE_OFFSET] = 0;
            // Distance BB
            mWalls[WALLBC*WALLSTRIDE+DISTANCE_OFFSET+1] = 0;
            // Distance CC
            mWalls[WALLCA*WALLSTRIDE+DISTANCE_OFFSET+2] = 0;
            // Distance AB
            mWalls[WALLAB+DISTANCE_OFFSET+1] = (float)Math.Sqrt(
                    Vector3Util.GetDistanceSq(mWalls[WALLAB+MIDPOINT_OFFSET]
                            , mWalls[WALLAB+MIDPOINT_OFFSET+1]
                            , mWalls[WALLAB+MIDPOINT_OFFSET+2]
                            , mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET]
                            , mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET+1]
                            , mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET+2]));
            // Distance BA
            mWalls[WALLBC*WALLSTRIDE+DISTANCE_OFFSET] = mWalls[WALLAB+DISTANCE_OFFSET+1];
            // Distance AC
            mWalls[WALLAB+DISTANCE_OFFSET+2] = (float)Math.Sqrt(
                    Vector3Util.GetDistanceSq(mWalls[WALLAB+MIDPOINT_OFFSET]
                             , mWalls[WALLAB+MIDPOINT_OFFSET+1]
                             , mWalls[WALLAB+MIDPOINT_OFFSET+2]
                            , mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET]
                            , mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET+1]
                            , mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET+2]));
            // Distance CA
            mWalls[WALLCA*WALLSTRIDE+DISTANCE_OFFSET] = mWalls[WALLAB+DISTANCE_OFFSET+2];
            // Distance BC
            mWalls[WALLBC*WALLSTRIDE+DISTANCE_OFFSET+2] = (float)Math.Sqrt(
                    Vector3Util.GetDistanceSq(mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET]
                             , mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET+1]
                             , mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET+2]
                            , mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET]
                            , mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET+1]
                            , mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET+2]));
            // Distance CB
            mWalls[WALLCA*WALLSTRIDE+DISTANCE_OFFSET+1] = mWalls[WALLBC*WALLSTRIDE+DISTANCE_OFFSET+2];
            
            mLinkWalls[WALLAB] = NULL_INDEX;
            mLinkWalls[WALLBC] = NULL_INDEX;
            mLinkWalls[WALLCA] = NULL_INDEX;
            
        }
        
        /// <summary>
        /// Forces the point to slightly within the cell column if it is located outside
        /// of the column.  Otherwise no change occurs.
        /// </summary>
        /// <remarks>The movement of the point will be toward the cell along the line 
        /// connecting the point and the cell's centroid.</remarks>
        /// <param name="x">The x-value of point (x, z).</param>
        /// <param name="z">The z-value of point (x, z).</param>
        /// <param name="offsetScale">This value impacts how far
        /// into the cell the point will be moved.  It represents the percentage
        /// distance between the cell centroid and the cell wall.  So setting the 
        /// value to 1 will move the point to the cell wall.  Setting the value to 0.9
        /// will move the point inside the cell wall by 10% of the distance from
        /// the wall to the cell's centroid.</param>
        /// <returns>A points guarenteed to be within the column of the cell.</returns>
        public Vector2 ForceToColumn(float x, float z, float offsetScale)
        {

            Vector2 result;
            TriCell trashCell;
            int trashInt;

            /*
             * Create a test path from the centroid of this cell to the point
             * Compare the test path to the cell.
             */
            PathRelType tpr = GetPathRelationship(mCentroidX, mCentroidZ
                    , x, z
                    , out trashCell
                    , out trashInt
                    , out result);

            if (tpr == PathRelType.ExitingCell)
            {
                
                // The test point is outside the cell or on a wall.
                // Need to pull it into the cell along centroid->point path.
                
                // Create a vector whose end point is the distance from centroid to the
                // exit point on the cell's exit wall.
                float px = result.x - mCentroidX;
                float pz = result.y - mCentroidZ;

                // Scale the vector back so the end point is slightly inside the exit wall of the cell.
                px *= 1 - offsetScale;
                pz *= 1 - offsetScale;

                // Update and return the point.
                result = new Vector2(mCentroidX + px, mCentroidZ + pz);
            }
            else if (tpr == PathRelType.NoRelationship)
            {
                // I don't think this should ever happen since the test path is
                // guaranteed to start in the cell.
                // But just for fun, force to the center of the cell.
                result = new Vector2(mCentroidX, mCentroidZ);
            }
            else
                // The test path does not exit the cell, so the point must already be in the cell.
                // No change necessary.            
                result = new Vector2(x, z);

            return result;
        }
        
        /// <summary>
        /// Gets the cell linked to the wall.
        /// </summary>
        /// <param name="wallIndex">0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <returns>The cell linked to the wall, or NULL if there is no cell linked
        /// to the wall.</returns>
        public TriCell GetLink(int wallIndex)
        {
            return mLinks[wallIndex];
        }
        
        /// <summary>
        /// Returns the index of the wall the provided cell is attached to.
        /// </summary>
        /// <param name="linkedCell">A cell that is linked to this cell.</param>
        /// <returns>The link index the cell is attached to.  Or <see cref="NULL_INDEX">NULL_INDEX</see>
        /// if the cell is not linked to this cell.</returns>
        public int GetLinkIndex(TriCell linkedCell)
        {
            for (int i = WALLAB; i <= WALLCA; i++)
            {
                if (mLinks[i] == linkedCell)
                    return i;
            }
            return NULL_INDEX;
        }

        /// <summary>
        /// Gets the distance from the midpoint of one wall to the midpoint
        /// of another wall.
        /// <para>This is a low cost operation</para>
        /// </summary>
        /// <param name="fromWallIndex">0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <param name="toWallIndex">0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <returns>The distance from the midpoint of one wall to the midpoint
        /// of another wall.</returns>
        public float GetLinkPointDistance(int fromWallIndex, int toWallIndex)
        {
            // Find the start of the distance entry for fromWall, then offset by the toWall.
            return mWalls[fromWallIndex*WALLSTRIDE+DISTANCE_OFFSET+toWallIndex];
        }

        /// <summary>
        /// Get the distance squared from the point to
        /// the wall midpoint. (distance * distance) 
        /// </summary>
        /// <param name="fromX">The x-value of the point to test. (fromX, fromY, fromZ)</param>
        /// <param name="fromY">The y-value of the point to test. (fromX, fromY fromZ)</param>
        /// <param name="fromZ">The z-value of the point to test. (fromX, fromY fromZ)</param>
        /// <param name="toWallIndex">toWallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <returns>Get the distance squared from the point to
        /// the wall midpoint.</returns>
        public float GetLinkPointDistanceSq(float fromX, float fromY, float fromZ, int toWallIndex)
        {
            return (float)(Math.Pow((fromX - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET]), 2.0) 
                    + Math.Pow((fromY - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET+1]), 2.0)
                    + Math.Pow((fromZ - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET+2]), 2.0));
        }
        
        /// <summary>
        /// Get the distance squared from the point to
        /// the wall midpoint. (distance * distance) 
        /// </summary>
        /// <param name="fromX">The x-value of the point to test. (fromX, fromZ)</param>
        /// <param name="fromZ">The z-value of the point to test. (fromX, fromZ)</param>
        /// <param name="toWallIndex">0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <returns>Get the distance squared from the point to
        /// the wall midpoint.</returns>
        public float GetLinkPointDistanceSq(float fromX, float fromZ, int toWallIndex)
        {
            return (float)(Math.Pow((fromX - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET]), 2.0) 
                    + Math.Pow((fromZ - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET+2]), 2.0));
        }

        /// <summary>
        /// The wall index of the cell linked to the wall.
        /// </summary>
        /// <remarks>Example:
        /// <para>If cellQ is connected to wall 1 of cellR<br/>
        /// And cellR is connected to wall 2 of cellQ<br/>
        /// Then cellQ.GetLinkWall(2) == 1</para> 
        /// </remarks>
        /// <param name="wallIndex">0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <returns>The wall index of the cell linked to the wall, or <see cref="NULL_INDEX">NULL_INDEX</see>
        /// is nothing is linked to the wall.</returns>
        public int GetLinkWall(int wallIndex)
        {
            return mLinkWalls[wallIndex];
        }
        
        /// <summary>
        /// Determines the relationship between the path (line segment) and the cell.
        /// </summary>
        /// <param name="ax">The x-value of point A (ax, az).</param>
        /// <param name="az">The z-value of point A (ax, az).</param>
        /// <param name="bx">The x-value of point B (bx, bz).</param>
        /// <param name="bz">The z-value of point B (bx, bz).</param>
        /// <param name="outNextCell">The cell, if any, that is linked to the exit wall.
        /// Will only have a value if the result is <see cref="PathRelType.ExitingCell">ExitingCell</see> 
        /// and the exit wall has a cell linked to it.</param>
        /// <param name="outExitWallIndex">The index of the exit wall, if any.
        /// Only applicable if the result is <see cref="PathRelType.ExitingCell">ExitingCell</see>.</param>
        /// <param name="outIntersectionPoint"> The content is undefined if the return value is
        /// other than <see cref="PathRelType.EndingCell">EndingCell</see>.</param>
        /// <returns>The relationship between the path (line segment) and the cell.</returns>
        public PathRelType GetPathRelationship(float ax, float az
                , float bx, float bz
                , out TriCell outNextCell
                , out int outExitWallIndex
                , out Vector2 outIntersectionPoint)
        {
            // The count of the number of walls the path end point is to the right of.
            // If to the right of all walls, the path ends in the cell.
            int interiorCount = 0;
            
            // Check path against each of the three cell walls.
            for (int iVert = 0; iVert < VERTCOUNT; iVert++)
            {
                /*
                 * This works by checking to see if the path end points are either
                 * on the wall, to the left, or to the right.  If to the left of the
                 * wall or on the wall, the path end point is external to the cell.  
                 * Otherwise the end point is internal.  This works because the vertices were
                 * all wrapped in a clockwise manner.
                 */
                
                int pVert = mVertPtr[iVert];
                
                PointLineRelType endPointRelType = GetRelationship(iVert, bx, bz, TOLERANCE_STD);
                if (endPointRelType ==  PointLineRelType.LeftSide)
                {
                    // For this wall, path end point is NOT inside the cell.
                    // Is the start point outside the cell as well?

                    PointLineRelType startPointRelType = GetRelationship(iVert, ax, az, TOLERANCE_STD);
                    if (startPointRelType == PointLineRelType.RightSide
                            || startPointRelType == PointLineRelType.OnLine)
                    {
                        
                        // For this wall, the path start point is potentially
                        // inside the cell.    
                        
                        int pNextVert = mVertPtr[(iVert+1 >= VERTCOUNT) ? 0 : iVert+1];
                        
                        Vector2 workingVector = new Vector2();

                        LineRelType intersectType = Line2.GetRelationship(ax, az, bx, bz
                                , mVerts[pVert], mVerts[pVert+2], mVerts[pNextVert], mVerts[pNextVert+2]
                                , out workingVector);
                
                        if (intersectType == LineRelType.SegmentsIntersect 
                                || intersectType == LineRelType.ALineCrossesBSeg
                                || ((intersectType == LineRelType.BLineCrossesASeg 
                                || intersectType == LineRelType.LinesIntersect)
                                        /*
                                         * Floating point error special case:
                                         * Need to see if the intersection point is very close to either of the 
                                         * walls vertices.
                                         */
                                        && (Vector2Util.SloppyEquals(workingVector
                                                                           , mVerts[pVert]
                                                                           , mVerts[pVert+2]
                                                                           , TOLERANCE_STD)
                                                || Vector2Util.SloppyEquals(workingVector
                                                                           , mVerts[pNextVert]
                                                                                  , mVerts[pNextVert+2]
                                                                                  , TOLERANCE_STD))))
                        {
                            
                            // The path Intersects this wall.
                            // Also, this is the "exit" wall since the previous two checks
                            // show that the direction of travel is from
                            // the right (inside) to the left (outside) of the wall.
                            
                            outNextCell = mLinks[iVert];
                            outExitWallIndex = iVert;
                            outIntersectionPoint = workingVector;
                            return PathRelType.ExitingCell;
                        }
                    }
                }
                else
                    // The path end point is to the right side of this wall.
                    // So the end point is potentially in the cell.
                    interiorCount++;                
            }

            // No exit wall intersection.  The path either ends in this cell or
            // there is no relationship at all.
            
            outNextCell = null;
            outExitWallIndex = NULL_INDEX;
            outIntersectionPoint = new Vector2();
            
            if (interiorCount == VERTCOUNT)
                return PathRelType.EndingCell;

            return PathRelType.NoRelationship;
        }
        
        /// <summary>
        /// Returns the y-value if the point (x, y, z) is located on the
        /// plane of the cell.
        /// </summary>
        /// <remarks>
        /// This operation does not care whether or not point (x, z)
        /// is within the column of the cell.
        /// <para>This is a low cost operation.</para>
        /// </remarks>
        /// <param name="x">The x-value of point (x, z).</param>
        /// <param name="z">The z-value of point (x, z).</param>
        /// <returns>The y-value if the point (x, y, z) is located on the
        /// plane of the cell.</returns>
        public float GetPlaneY(float x, float z)
        {
            // Proof:
            // ax + by + cz + mD = 0
            // by = -(ax + cz + mD)
            // y = -(ax + cz + mD)/b
            if (mNormalY != 0)
                return ( -((mNormalX * x) + (mNormalZ * z) + mD) / mNormalY );
            return 0;
        }

        /// <summary>
        /// Checks to see if the point is at a problematic location in the cell
        /// and shifts it to a safe location.
        /// </summary>
        /// <remarks>
        /// For vertices, moves the point slightly toward the centeroid.
        /// <para>Behavior is undefined if the point is outside of the cell.</para>
        /// </remarks>
        /// <param name="x">The x-value of point(x, z).</param>
        /// <param name="z">The x-value of point(x, z).</param>
        /// <param name="offsetScale">The scale to offset the point if it is in an unsafe location.</param>
        /// <returns>The point after it has been shifted to a safe location.  (Or the original point
        /// if no shifting was required.)</returns>
        public Vector2 GetSafePoint(float x, float z, float offsetScale)
        {
            Vector2 result = new Vector2(x, z);
            for (int iVert = 0; iVert < VERTCOUNT; iVert++)
            {
                if (Vector2Util.SloppyEquals(x, z
                        , mVerts[mVertPtr[iVert]], mVerts[mVertPtr[iVert]+2]
                        , TOLERANCE_STD))
                {
                    // Found a matching vertex.
                    // Get direction vector vertex->centroid and scale to 5% of its original
                    // length.  Then translate the out point.
                    result.x += (mCentroidX - mVerts[mVertPtr[iVert]]) * offsetScale;
                    result.y += (mCentroidZ - mVerts[mVertPtr[iVert]+2]) * offsetScale;
                }
            }
            return result;
        }
        
        /// <summary>
        /// Gets the value of a vertex.
        /// </summary>
        /// <param name="index">0 = VertA, 1 = VertB, 2 = VertC</param>
        /// <returns>The value of the vertex.</returns>
        public Vector3 GetVertex(int index)
        {
            int pVert = mVertPtr[index];
            return new Vector3(mVerts[pVert], mVerts[pVert+1], mVerts[pVert+2]);
        }
        
        /// <summary>
        /// Gets the vertex index for the vertex that matches the provided
        /// point.  (Only checks the xz axes.)
        /// @param x 
        /// @param z 
        /// @param tolerance 
        /// @return 
        /// </summary>
        /// <param name="x">The x-value of a point that may be a vertex. (x, y, z)</param>
        /// <param name="z">The z-value of a point that may be a vertex. (x, y, z)</param>
        /// <param name="tolerance">The tolerance the elements of each point must be within
        /// to be considered equal.</param>
        /// <returns>The index of the vertex that matches the provided point,
        /// or <see cref="NULL_INDEX">NULL_INDEX</see> if there is no match.</returns>
        public int GetVertexIndex(float x, float z, float tolerance)
        {
            if (Vector2Util.SloppyEquals(mVerts[mVertPtr[VERTA]]
                                            , mVerts[mVertPtr[VERTA]+2]
                                            , x, z
                                            , tolerance))
                return VERTA;
            if (Vector2Util.SloppyEquals(mVerts[mVertPtr[VERTB]]
                                            , mVerts[mVertPtr[VERTB]+2]
                                            , x, z
                                            , tolerance))
                return VERTB;
            if (Vector2Util.SloppyEquals(mVerts[mVertPtr[VERTC]]
                                            , mVerts[mVertPtr[VERTC]+2]
                                            , x, z
                                            , tolerance))
                return VERTC;
            return NULL_INDEX;
        }

        /// <summary>
        /// Gets the vertex index for the vertex that matches the provided
        /// point.
        /// </summary>
        /// <param name="x">The x-value of a point that may be a vertex. (x, y, z)</param>
        /// <param name="y">The y-value of a point that may be a vertex. (x, y, z)</param>
        /// <param name="z">The z-value of a point that may be a vertex. (x, y, z)</param>
        /// <param name="tolerance">The tolerance the elements of each point must be within
        /// to be considered equal.</param>
        /// <returns>The vertex that matches the provided point, or <see cref="NULL_INDEX">NULL_INDEX</see>
        /// if there is no match.</returns>
        public int GetVertexIndex(float x, float y, float z, float tolerance)
        {
            if (Vector3Util.SloppyEquals(mVerts[mVertPtr[VERTA]]
                                            , mVerts[mVertPtr[VERTA]+1]
                                            , mVerts[mVertPtr[VERTA]+2]
                                            , x, y, z
                                            , tolerance))
                return VERTA;
            if (Vector3Util.SloppyEquals(mVerts[mVertPtr[VERTB]]
                                            , mVerts[mVertPtr[VERTB]+1]
                                            , mVerts[mVertPtr[VERTB]+2]
                                            , x, y, z
                                            , tolerance))
                return VERTB;
            if (Vector3Util.SloppyEquals(mVerts[mVertPtr[VERTC]]
                                            , mVerts[mVertPtr[VERTC]+1]
                                            , mVerts[mVertPtr[VERTC]+2]
                                            , x, y, z
                                            , tolerance))
                return VERTC;
            return NULL_INDEX;
        }
        
        /// <summary>
        /// Get the x, y, or z value of a vertex.
        /// </summary>
        /// <param name="vertIndex">0 = VertA, 1 = VertB, 2 = VertC</param>
        /// <param name="valueIndex">0 = x-value, 1 = y-value, 2 = z-value</param>
        /// <returns>The requested vertex value.</returns>
        public float GetVertexValue(int vertIndex, int valueIndex)
        {
            return mVerts[mVertPtr[vertIndex]+valueIndex];
        }

        /// <summary>
        /// Gets the distance from the point to the line formed by the wall.
        /// Since this is not a line segment test, the closest point on the wall's line
        /// may be outside of the cell.
        /// </summary>
        /// <remarks>
        /// This operation uses the cells xz-plane projection.
        /// <para>Due to the method used, this is a low cost operation.</para>
        /// </remarks>
        /// <param name="fromX">The x-value of the point to test. (fromX, fromZ)</param>
        /// <param name="fromZ">The z-value of the point to test. (fromX, fromZ)</param>
        /// <param name="toWallIndex">0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <returns>The distance from the point to the line formed by the wall.</returns>
        public float GetWallDistance(float fromX, float fromZ, int toWallIndex) 
        {
            return Math.Abs(GetSignedDistance(toWallIndex, fromX, fromZ));
        }
        
        /// <summary>
        /// The vertex to the left of the wall when viewed from within the cell.
        /// </summary>
        /// <param name="wallIndex">0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <returns>The vertex to the left of the wall.</returns>
        public Vector3 GetWallLeftVertex(int wallIndex)
        {
            wallIndex = mVertPtr[wallIndex];
            return new Vector3(mVerts[wallIndex], mVerts[wallIndex+1], mVerts[wallIndex+2]);
        }

        /// <summary>
        /// The vertex to the right of the wall when viewed from within the cell.
        /// </summary>
        /// <param name="wallIndex">0 = WallAB, 1 = WallBC, 2 = WallCA</param>
        /// <returns>The vertex to the right of the wall.</returns>
        public Vector3 GetWallRightVertex(int wallIndex)
        {
            wallIndex++;
            if (wallIndex >= VERTCOUNT)
                wallIndex = 0;
            wallIndex = mVertPtr[wallIndex];
            return new Vector3(mVerts[wallIndex], mVerts[wallIndex+1], mVerts[wallIndex+2]);
        }
        
        /// <summary>
        /// Indicates whether or not the cell intersects the AABB. Containment
        /// is considered an intersection. (Only a 2D check.)
        /// <para>This is potentially a non-trivial operation.</para>
        /// </summary>
        /// <param name="minx">The minimum x of the AABB.</param>
        /// <param name="minz">The minimum z of the AABB.</param>
        /// <param name="maxx">The maximum x of the AABB.</param>
        /// <param name="maxz">The maximum Z of the AABB.</param>
        /// <returns>TRUE if the cell and AABB intersect in any manner.</returns>
        public Boolean Intersects(float minx, float minz, float maxx, float maxz)
        {
            // Quick rejection: Does the AABB intersect Triangle-AABB.
            if (!Rectangle2.IntersectsAABB(minx, minz, maxx, maxz
                    , mBoundsMinX, mBoundsMinZ, mBoundsMaxX, mBoundsMaxZ))
                    return false;
            
            // Quick selection: Are any of the triangle vertices contained
            // by the AABB?
            for (int iVert = 0; iVert < VERTCOUNT; iVert++)
            {
                int p = mVertPtr[iVert];
                if (Rectangle2.Contains(minx, minz, maxx, maxz, mVerts[p], mVerts[p+2]))
                    return true;
            }
            
            // More expensive checks.
            // Check for intersection between triangle edges and AABB edges.
            
            // Centroid of AABB.
            float cx = (maxx + minx) / 2;
            float cz = (maxz + minz) / 2;
            
            // Half length for each AABB's axis.
            float hlx = minx - cx;
            float hlz = minz - cz;
            
            // Shift vertices so origin is at centroid.
            float vAx = mVerts[mVertPtr[VERTA]] - cx;
            float vAz = mVerts[mVertPtr[VERTA]+2] - cz;
            float vBx = mVerts[mVertPtr[VERTB]] - cx;
            float vBz = mVerts[mVertPtr[VERTB]+2] - cz;
            float vCx = mVerts[mVertPtr[VERTC]] - cx;
            float vCz = mVerts[mVertPtr[VERTC]+2] - cz;
            
            // NOTE: In most cases in the code comments, "separation axis" 
            // means "potential separation axis".  Same applies to "separation line".
            
            // Is wallAB a valid separation line?
            
            // Get distance from centroid to wallAB.
            float pCL = Vector2Util.Dot(vAx, vAz, mWalls[WALLAB], mWalls[WALLAB+1]);
            
            // Get distance from centroid to vertex opposite wall AB along the
            // separation axis.
            float pCO = Vector2Util.Dot(vCx, vCz, mWalls[WALLAB], mWalls[WALLAB + 1]);
            
            // Project half length of AABB onto separation axis.
            float pBoxHL = Math.Abs(Vector2Util.Dot(hlx, 0, mWalls[WALLAB], mWalls[WALLAB+1]))
                                + Math.Abs(Vector2Util.Dot(0, hlz, mWalls[WALLAB], mWalls[WALLAB + 1]));
            
            if (Math.Min(pCL, pCO) > pBoxHL || Math.Max(pCL, pCO) < -pBoxHL)
                // wallAB is a valid separation axis.
                return false;
            
            // Is wallBC a valid separation axis?
            
            // Get distance from centroid to wallBC.
            pCL = Vector2Util.Dot(vBx, vBz, mWalls[WALLBC*WALLSTRIDE], mWalls[WALLBC*WALLSTRIDE+1]);
            
            // Get distance from centroid to vertex opposite wall BC along the
            // separation axis.
            pCO = Vector2Util.Dot(vAx, vAz, mWalls[WALLBC * WALLSTRIDE], mWalls[WALLBC * WALLSTRIDE + 1]);
            
            // Project half length of AABB onto separation line.
            pBoxHL = Math.Abs(Vector2Util.Dot(hlx
                                , 0
                                , mWalls[WALLBC*WALLSTRIDE]
                                , mWalls[WALLBC*WALLSTRIDE+1]))
                        + Math.Abs(Vector2Util.Dot(0
                                , hlz
                                , mWalls[WALLBC*WALLSTRIDE]
                                , mWalls[WALLBC*WALLSTRIDE+1]));
            
            if (Math.Min(pCL, pCO) > pBoxHL || Math.Max(pCL, pCO) < -pBoxHL)
                // wallBC is a valid separation axis.
                return false;
            
            // Is wallCA a valid separation line?
            
            // Get distance from centroid to wallCA.
            pCL = Vector2Util.Dot(vCx, vCz, mWalls[WALLCA * WALLSTRIDE], mWalls[WALLCA * WALLSTRIDE + 1]);
            
            // Get distance from centroid to vertex opposite wall CA along the
            // separation axis.
            pCO = Vector2Util.Dot(vBx, vBz, mWalls[WALLCA * WALLSTRIDE], mWalls[WALLCA * WALLSTRIDE + 1]);
            
            // Project half length of AABB onto separation axis.
            pBoxHL = Math.Abs(Vector2Util.Dot(hlx
                                , 0
                                , mWalls[WALLCA*WALLSTRIDE]
                                , mWalls[WALLCA*WALLSTRIDE+1]))
                        + Math.Abs(Vector2Util.Dot(0
                                , hlz
                                , mWalls[WALLCA*WALLSTRIDE]
                                , mWalls[WALLCA*WALLSTRIDE+1]));
            
            if (Math.Min(pCL, pCO) > pBoxHL || Math.Max(pCL, pCO) < -pBoxHL)
                // wallCA is a valid separation axis.
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Indicates whether or not the point is within the cell column.
        /// </summary>
        /// <param name="x">The x-value of point (x, z).</param>
        /// <param name="z">The z-value of point (x, z).</param>
        /// <returns>TRUE if the point is within the cell column.  Otherwise FALSE.</returns>
        public Boolean IsInColumn(float x, float z)
        {
            for (int iWall = 0; iWall < VERTCOUNT; iWall++)
            {
                if (GetRelationship(iWall, x, z, TOLERANCE_STD) == PointLineRelType.LeftSide) 
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// Attempts to link the provided cell to this cell.
        /// The link will only succeed if the two cells share a common wall.
        /// <para>If the link location is already occupied, the link operation will fail.</para>
        /// </summary>
        /// <param name="cellToLink">The cell to attempt to link to this cell.</param>
        /// <param name="crossLink">If TRUE, the cells will be linked in both directions.
        /// (I.e. Linked to each other.) If FALSE, no attempt will be made to Link this
        /// cell to the provided cell.</param>
        /// <returns>The wall index the provided cell was linked to.  Or <see cref="NULL_INDEX">NULL_INDEX</see>
        /// if the no valid link could be made.</returns>
        public int Link(TriCell cellToLink, Boolean crossLink)
        {    
            int linkIndex = NULL_INDEX;
            int otherLinkIndex = NULL_INDEX;
            for (int iVert = 2, iNextVert = 0
                    ; iNextVert < VERTCOUNT && linkIndex == NULL_INDEX
                    ; iVert = iNextVert++)
            {
                int pVert = mVertPtr[iVert];
                int pNextVert = mVertPtr[iNextVert];
                for (int iOtherVert = 2, iOtherNextVert = 0
                    ; iOtherNextVert < VERTCOUNT
                    ; iOtherVert = iOtherNextVert++)
                {
                    int pOtherVert = cellToLink.mVertPtr[iOtherVert];
                    int pOtherNextVert = cellToLink.mVertPtr[iOtherNextVert];
                    // Both cells are expected to be wrapped in the same direction.
                    // So the vertices for the 2nd cell are reversed for the check.
                    if (mVerts[pVert+0] == cellToLink.mVerts[pOtherNextVert+0]
                           && mVerts[pVert+1] == cellToLink.mVerts[pOtherNextVert+1]
                           && mVerts[pVert+2] == cellToLink.mVerts[pOtherNextVert+2]
                           && mVerts[pNextVert+0] == cellToLink.mVerts[pOtherVert+0]
                           && mVerts[pNextVert+1] == cellToLink.mVerts[pOtherVert+1]
                           && mVerts[pNextVert+2] == cellToLink.mVerts[pOtherVert+2])
                   {
                       linkIndex = iVert;
                       otherLinkIndex = iOtherVert;
                       break;
                   }
                }
            }
            if (linkIndex != NULL_INDEX)
            {
                if (mLinks[linkIndex] != null)
                    return NULL_INDEX;
                if (crossLink && cellToLink.mLinks[otherLinkIndex] != null)
                    return NULL_INDEX;
                mLinks[linkIndex] = cellToLink;
                mLinkWalls[linkIndex] = otherLinkIndex;
                if (crossLink)
                {
                    cellToLink.mLinks[otherLinkIndex] = this;
                    cellToLink.mLinkWalls[otherLinkIndex] = linkIndex;
                }
            }
            return linkIndex;
        }
        
        private PointLineRelType GetRelationship(int wallIndex, float x, float y, float tolerance)
        {
            
            float distance = GetSignedDistance(wallIndex, x, y);
        
            if (distance > tolerance) 
                return PointLineRelType.RightSide;
            else if (distance < -tolerance) 
                return PointLineRelType.LeftSide;
            
            return PointLineRelType.OnLine;
        }

        private float GetSignedDistance(int wallIndex, float x, float z)
        {
            // The subtraction gets the directional vector from a point on the line 
            // to the reference point.
            // The Dot projects the directional vector onto the normal giving the distance
            // from the line to the point along the normal.
            int pVert = mVertPtr[wallIndex];
            return Vector2Util.Dot(mWalls[wallIndex * WALLSTRIDE]
                               , mWalls[wallIndex*WALLSTRIDE+1]
                               , x - mVerts[pVert]
                               , z - mVerts[pVert+2]);
        }

        /// <summary>
        /// Gets the cell that is closest to the provided point.
        /// </summary>
        /// <remarks>
        /// If the point lies on a shared edge, one of the cells sharing the edge will be returned.
        /// But which one is returned is arbitrary.
        /// <para>This is an estimate for performance reasons.  The closer the point is to cell
        /// edges and the larger the slope differences between adjacent cells, the more likely
        /// an unexpected result will be obtained.</para>
        /// </remarks>
        /// <param name="x">The x-value of point (x, y, z).</param>
        /// <param name="y">The y-value of point (x, y, z).</param>
        /// <param name="z">The z-value of point (x, y, z).</param>
        /// <param name="cells">The cells to search.</param>
        /// <param name="outPointOnCell">The vector will be set to the closest point
        /// on the selected cell's plane.</param>
        /// <returns>The cell that is closest to the provided point.</returns>
        public static TriCell GetClosestCell(float x, float y, float z
                , TriCell[] cells
                , out Vector3 outPointOnCell)
        {
            // WARNING: Any change to this code must also be made to 
            // the overload.
            
            float minDistanceSq = float.MaxValue;
            TriCell selectedCell = null;
            Vector2 workingV2 = new Vector2();
            TriCell trashCell;
            int trashInt;
            outPointOnCell = new Vector3();

            // Loop through all cells.
            foreach (TriCell cell in cells)
            {
                float currentDistanceSq;
        
                // Get the relationship of point to this cell.
                PathRelType relationship = cell.GetPathRelationship(cell.mCentroidX, cell.mCentroidZ
                        , x, z
                        , out trashCell
                        , out trashInt
                        , out workingV2);
                
                if (relationship == PathRelType.ExitingCell)
                {
                    
                    /*
                     * The point is outside the cell column.
                     * The closest point on the cell is the exit's intersection point.
                     * We first need to convert this point to 3D, then figure out the distance
                     * from it to our reference point. 
                     */
                    
                    // Working vector Contains 2D intersection point.  Need y.
                    float ty = cell.GetPlaneY(workingV2.x, workingV2.y);
        
                    // Convert to a direction vector.
                    float dx = workingV2.x - x;
                    float dy = ty - y;
                    float dz = workingV2.y - z;
                    
                    // And determine the length of the direction vector.
                    currentDistanceSq = Vector3Util.GetLengthSq(dx, dy, dz);
        
                    if (currentDistanceSq < minDistanceSq)
                    {
                        // This cell is closer than previous cells.
                        minDistanceSq = currentDistanceSq;
                        selectedCell = cell;
                        outPointOnCell.x = workingV2.x;
                        outPointOnCell.y = ty;
                        outPointOnCell.z = workingV2.y;
                    }
                }
                else
                {
                    // The point is in the cell column.
                    // Use the distance from the point to the target point on the cell surface.
                    // Only measuring y-distance.
                    float cellY = cell.GetPlaneY(x, z);
                    currentDistanceSq = (y - cellY) * (y - cellY);
                    if (currentDistanceSq < minDistanceSq)
                    {
                        // This cell is closer than previous cells.
                        minDistanceSq = currentDistanceSq;
                        selectedCell = cell;
                        outPointOnCell.x = x;
                        outPointOnCell.y = cellY;
                        outPointOnCell.z = z;
                    }    
                }
            }

            return selectedCell;
        }

        /// <summary>
        /// Gets the cell that is closest to the provided point.
        /// </summary>
        /// <remarks>
        /// If the point lies on a shared edge, one of the cells sharing the edge will be returned.
        /// But which one is returned is arbitrary.
        /// <para>This is an estimate for performance reasons.  The closer the point is to cell
        /// edges and the larger the slope differences between adjacent cells, the more likely
        /// an unexpected result will be obtained.</para>
        /// </remarks>
        /// <param name="x">The x-value of point (x, y, z).</param>
        /// <param name="y">The y-value of point (x, y, z).</param>
        /// <param name="z">The z-value of point (x, y, z).</param>
        /// <param name="cells">The cells to search.</param>
        /// <param name="outPointOnCell">The vector will be set to the closest point
        /// on the selected cell's plane.</param>
        /// <returns>The cell that is closest to the provided point.</returns>
        public static TriCell GetClosestCell(float x, float y, float z
                , List<TriCell> cells
                , out Vector3 outPointOnCell)
        {
            // TODO: The code in this operation is exactly the
            // same as in the array overload. Try to clean both up in a way
            // that doesn't hurt performance too much.

            float minDistanceSq = float.MaxValue;
            TriCell selectedCell = null;
            Vector2 workingV2 = new Vector2();
            TriCell trashCell;
            int trashInt;
            outPointOnCell = new Vector3();

            // Loop through all cells.
            foreach (TriCell cell in cells)
            {
                float currentDistanceSq;

                // Get the relationship of point to this cell.
                PathRelType relationship = cell.GetPathRelationship(cell.mCentroidX, cell.mCentroidZ
                        , x, z
                        , out trashCell
                        , out trashInt
                        , out workingV2);

                if (relationship == PathRelType.ExitingCell)
                {

                    /*
                     * The point is outside the cell column.
                     * The closest point on the cell is the exit's intersection point.
                     * We first need to convert this point to 3D, then figure out the distance
                     * from it to our reference point. 
                     */

                    // Working vector Contains 2D intersection point.  Need y.
                    float ty = cell.GetPlaneY(workingV2.x, workingV2.y);

                    // Convert to a direction vector.
                    float dx = workingV2.x - x;
                    float dy = ty - y;
                    float dz = workingV2.y - z;

                    // And determine the length of the direction vector.
                    currentDistanceSq = Vector3Util.GetLengthSq(dx, dy, dz);

                    if (currentDistanceSq < minDistanceSq)
                    {
                        // This cell is closer than previous cells.
                        minDistanceSq = currentDistanceSq;
                        selectedCell = cell;
                        outPointOnCell.x = workingV2.x;
                        outPointOnCell.y = ty;
                        outPointOnCell.z = workingV2.y;
                    }
                }
                else
                {
                    // The point is in the cell column.
                    // Use the distance from the point to the target point on the cell surface.
                    // Only measuring y-distance.
                    float cellY = cell.GetPlaneY(x, z);
                    currentDistanceSq = (y - cellY) * (y - cellY);
                    if (currentDistanceSq < minDistanceSq)
                    {
                        // This cell is closer than previous cells.
                        minDistanceSq = currentDistanceSq;
                        selectedCell = cell;
                        outPointOnCell.x = x;
                        outPointOnCell.y = cellY;
                        outPointOnCell.z = z;
                    }
                }
            }

            return selectedCell;
        }
        
    }

}

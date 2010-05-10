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

import java.text.DecimalFormat;
import java.util.List;

import org.critterai.math.Vector2;
import org.critterai.math.Vector3;
import org.critterai.math.geom.Line2;
import org.critterai.math.geom.LineRelType;
import org.critterai.math.geom.PointLineRelType;
import org.critterai.math.geom.Polygon3;
import org.critterai.math.geom.Rectangle2;
import org.critterai.math.geom.Triangle3;

/**
 * Represents an triangle cell within a navigation mesh. 
 * <p>Limited to representing polygons with normals that are primarily "up".  This is because
 * certain operations project onto the xz-plane and will not function properly if the area
 * of the xz-plan projection is too small.</p>
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No null checks.  No checks to see whether index arguments are in range.</p>
 * <p>Instances of this class are not thread-safe but can be used by multiple threads without
 * synchronization under the following conditions:</p>
 * <ul>
 * <li>The {@link #link(TriCell, boolean)} operation is not performed after object references
 * are shared between threads.  I.e. Perform linking at construction and never again.</li>
 * <li>Both objects involved in the {@link #link(TriCell, boolean)} operation are under the
 * control of a single thread during the operation.</p>
 * </ul>
 * <p>In summary, objects of this class can be shared safely between threads if they are treated
 * as immutable after construction and linking.</p>
 * <p>Static operations are thread safe.</p>
 */
public final class TriCell
{
	
	/*
	 * Design notes:
	 * 
	 * In general, choosing to use more memory for
	 * better local performance.
	 * 
	 * It is important that the vertex and wall constants 
	 * remain in synch.  E.g. VERTA == WALLAB, VERTC = WALLAC
	 * Don't mess with them lightly.
	 * 
	 */
	
	/**
	 * How far to offset from the beginning of a {@link #mWalls} entry
	 * to get to the wall distance information.
	 */
	private static final int DISTANCE_OFFSET = 5;
	
	/**
	 * How far to offset from the beginning of a {@link #mWalls} entry
	 * to get to the wall midpoint information.
	 */
	private static final int MIDPOINT_OFFSET = 2;
	
	/**
	 * A value indicating lack of a valid index.
	 */
	public static final int NULL_INDEX = -1;
	private static final int VERTA = 0;
	
	private static final int VERTB = 1;
	
	private static final int VERTC = 2;
	private static final int VERTCOUNT = 3;
	private static final int WALLAB = 0;
	
	private static final int WALLBC = 1;
	private static final int WALLCA = 2;
	private static final int WALLSTRIDE = 8;
	
	/**
	 * The maximum x-bounds of the cell.
	 */
	public final float boundsMaxX;

	/**
	 * The maximum z-bounds of the cell.
	 */
	public final float boundsMaxZ;
	
	/**
	 * The minimum x-bounds of the cell.
	 */
	public final float boundsMinX;
	
	/**
	 * The minimum z-bounds of the cell.
	 */
	public final float boundsMinZ;
	
	/**
	 * The x-value of the centroid of the cell. (centroidX, centroidY, centroidZ)
	 */
	public final float centroidX;
	
	/**
	 * The y-value of the centroid of the cell. (centroidX, centroidY, centroidZ)
	 */
	public final float centroidY;

	/**
	 * The z-value of the centroid of the cell. (centroidX, centroidY, centroidZ)
	 */
	public final float centroidZ;

	/**
	 * Constant d for the cell's plane.
	 * <p>Equation: ax + by + cz + d = 0
	 * where the vector (a, b, c) is the plane's normal
	 * and the point (x, y, z) is a reference point on the plane, in this
	 * case the cell's centroid.</p>
	 * @return The cell plane's d constant.
	 */
	private final float d;

	/**
	 * References to cells that attach to this cell. A NULL link denotes a solid wall.
	 * <p>Link indices are associated with wall indices which are associated with vertex indices. 
	 * E.g. iLinkA = iWallAB == iVertA</p>
	 * <p>Form: (linkA, linkB, ...linkN) * 1</p>
	 * <p>Stride: 1</p>
	 */	
	private final TriCell[] mLinks = new TriCell[VERTCOUNT];

	/**
	 * References to the walls that attach to this cell's walls. 
	 * A NULL_INDEX denotes an unlinked wall.
	 * <p>Link indices are associated with wall indices which are associated with vertex indices. 
	 * E.g. iLinkA = iWallAB == iVertA</p>
	 * <p>Form: (linkA, linkB, ...linkN) * 1</p>
	 * <p>Stride = 1
	 */	
	private final int[] mLinkWalls = new int[VERTCOUNT];
	
	/**
	 * The x-value of the cell plane normal 
	 */
	private final float mNormalX;
	
	/**
	 * The y-value of the cell plane normal 
	 */
	private final float mNormalY;
	
	/**
	 * The c-value of the cell plane normal 
	 */
	private final float mNormalZ;
	
	/**
	 * The pointer (not indices) of the cell vertices in the {@link #mVerts} array.
	 * (pVertA, pVertB, pVertC)
	 * Stride = 1
	 */
	private final int[] mVertPtr = new int[VERTCOUNT];;

	/**
	 * The source vertices of the cell.
	 * Form: (x, y, z)
	 * Stride = 3
	 */
	private final float[] mVerts;
	
	/**
	 * Pre-calculated wall data.
	 * <p>Form: (normalX, normalZ, midpointX, midpointY, midpointZ, 
	 * 				internalDistA, internalDistB, internalDistC) * 3</p>
	 * <p>Stride: {@link #WALLSTRIDE}.</p>
	 * <p>Use {@link #DISTANCE_OFFSET} and {@link #MIDPOINT_OFFSET} to offset
	 * to needed data.</p>
	 * <p>Link indices are associated with wall indices which are associated with vertex indices. 
	 * E.g. iLinkA = iWallAB == iVertA</p>
	 * <p>The order of distance information is consistent with wall ordering. (A, B, C)
	 * A value of zero is used for the position where the wall is self-referencing.</p>
	 */
	private final float[] mWalls = new float[3*WALLSTRIDE];
	
	/**
	 * Constructor. Vertices must be wrapped clockwise.
	 * @param verts The vertices array in the form (x, y, z).
	 * @param vertAIndex The index of vertex A.
	 * @param vertBIndex The index of vertex B.
	 * @param vertCIndex The index of vertex C.
	 * @throws IllegalArgumentException If the vertices argument is null or 
	 * any of the indices are invalid.
	 */
	public TriCell(float[] verts, int vertAIndex, int vertBIndex, int vertCIndex)
		throws IllegalArgumentException
	{
		
		if (verts == null)
			throw new IllegalArgumentException("Vertices argument is null");
		
		mVerts = verts;
		
		mVertPtr[VERTA] = vertAIndex*3;
		mVertPtr[VERTB] = vertBIndex*3;
		mVertPtr[VERTC] = vertCIndex*3;
		
		if (mVertPtr[VERTA] < 0
				|| mVertPtr[VERTA]+2 >= mVerts.length
				|| mVertPtr[VERTB] < 0
				|| mVertPtr[VERTB]+2 >= mVerts.length
				|| mVertPtr[VERTC] < 0
				|| mVertPtr[VERTC]+2 >= mVerts.length)
			throw new IllegalArgumentException("One or more vertex indices are invalid.");
		
		final Vector3 workingVector3 = new Vector3();
		
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
		Triangle3.getNormal(ax, ay, az, bx, by, bz, cx, cy, cz, workingVector3);
		mNormalX = workingVector3.x;
		mNormalY = workingVector3.y;
		mNormalZ = workingVector3.z;
		
		// Calculate the cell's centroid and d-constant.
		Polygon3.getCentroid(workingVector3, ax, ay, az, bx, by, bz, cx, cy, cz);
		centroidX = workingVector3.x;
		centroidY = workingVector3.y;
		centroidZ = workingVector3.z;
		
		d = -workingVector3.dot(mNormalX, mNormalY, mNormalZ);
		
		// Calculate the xz-plane bounds.
		boundsMinX = Math.min(ax, Math.min(bx, cx));
		boundsMinZ = Math.min(az, Math.min(bz, cz));
		boundsMaxX = Math.max(ax, Math.max(bx, cx));
		boundsMaxZ = Math.max(az, Math.max(bz, cz));
		
		// Calculate and store the normal each wall.
		// Wall AB
		// Note that a disposable object is being generated here.
		Vector2 v = Line2.getNormalAB(ax, az, bx, bz, new Vector2());
		mWalls[WALLAB*WALLSTRIDE] = v.x;
		mWalls[WALLAB*WALLSTRIDE+1] = v.y;
		// Wall BC
		Line2.getNormalAB(bx, bz, cx, cz, v);
		mWalls[WALLBC*WALLSTRIDE] = v.x;
		mWalls[WALLBC*WALLSTRIDE+1] = v.y;
		// Wall CA
		Line2.getNormalAB(cx, cz, ax, az, v);
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
		mWalls[WALLAB+DISTANCE_OFFSET+1] = (float)Math.sqrt(
				Vector3.getDistanceSq(mWalls[WALLAB+MIDPOINT_OFFSET]
						, mWalls[WALLAB+MIDPOINT_OFFSET+1]
						, mWalls[WALLAB+MIDPOINT_OFFSET+2]
						, mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET]
						, mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET+1]
						, mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET+2]));
		// Distance BA
		mWalls[WALLBC*WALLSTRIDE+DISTANCE_OFFSET] = mWalls[WALLAB+DISTANCE_OFFSET+1];
		// Distance AC
		mWalls[WALLAB+DISTANCE_OFFSET+2] = (float)Math.sqrt(
				Vector3.getDistanceSq(mWalls[WALLAB+MIDPOINT_OFFSET]
				     	, mWalls[WALLAB+MIDPOINT_OFFSET+1]
				     	, mWalls[WALLAB+MIDPOINT_OFFSET+2]
						, mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET]
						, mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET+1]
						, mWalls[WALLCA*WALLSTRIDE+MIDPOINT_OFFSET+2]));
		// Distance CA
		mWalls[WALLCA*WALLSTRIDE+DISTANCE_OFFSET] = mWalls[WALLAB+DISTANCE_OFFSET+2];
		// Distance BC
		mWalls[WALLBC*WALLSTRIDE+DISTANCE_OFFSET+2] = (float)Math.sqrt(
				Vector3.getDistanceSq(mWalls[WALLBC*WALLSTRIDE+MIDPOINT_OFFSET]
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
	
	/**
	 * Forces the point to slightly within the cell column if it is located outside
	 * of the column.  Otherwise no change occurs.
	 * @param x The x-value of point (x, z).
	 * @param z The z-value of point (x, z).
	 * @param out A vector to store the result in.
	 * @return A reference to the out argument.
	 */
	public Vector2 forceToColumn(float x, float z, float offsetScale, Vector2 out)
	{
		/*
		 * Create a test path from the centroid of this cell to the point
		 * Compare the test path to the cell.
		 */
		PathRelType tpr = getPathRelationship(centroidX, centroidZ
				, x, z
				, null
				, null
				, null
				, out);

		if (tpr == PathRelType.EXITING_CELL)
		{
			
			// The test point is outside the cell or on a wall.
			// Need to pull it into the cell along centroid->point path.
			
			// Create a vector whose end point is the distance from centroid to the
			// exit point on the cell's exit wall.
			float px = out.x - centroidX;
			float pz = out.y - centroidZ;

			// Scale the vector back so the end point is slightly inside the exit wall of the cell.
			px *= 1 - offsetScale;
			pz *= 1 - offsetScale;

			// Update and return the point.
			out.set(centroidX + px, centroidZ + pz);
		}
		else if (tpr == PathRelType.NO_RELATIONSHIP)
		{
			// I don't think this should ever happen since the test path is
			// guaranteed to start in the cell.
			// But just for fun, force to the center of the cell.
			out.set(centroidX, centroidZ);
		}
		else
			// The test path does not exit the cell, so the point must already be in the cell.
			// No change necessary.			
			out.set(x, z);

		return out;
		
	}
	
	/**
	 * Gets the cell linked to the wall.
	 * @param wallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @return The cell linked to the wall, or NULL if there is no cell linked
	 * to the wall.
	 */
	public TriCell getLink(int wallIndex)
	{
		return mLinks[wallIndex];
	}
	
	/**
	 * Returns the index of the wall the provided cell is attached to.
	 * @param linkedCell A cell that is linked to this cell.
	 * @return The link index the cell is attached to.  Or {@link #NULL_INDEX}
	 * if the cell is not linked to this cell.
	 */
	public int getLinkIndex(TriCell linkedCell)
	{
		for (int i = WALLAB; i <= WALLCA; i++)
		{
			if (mLinks[i] == linkedCell)
				return i;
		}
		return NULL_INDEX;
	}

	/**
	 * Gets the distance from the midpoint of one wall to the midpoint
	 * of another wall.
	 * <p>This is a low cost operation</p>
	 * @param fromWallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @param toWallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @return The distance from the midpoint of one wall to the midpoint
	 * of another wall.
	 */
	public float getLinkPointDistance(int fromWallIndex, int toWallIndex)
	{
		// Find the start of the distance entry for fromWall, then offset by the toWall.
		return mWalls[fromWallIndex*WALLSTRIDE+DISTANCE_OFFSET+toWallIndex];
	}

	/**
	 * Get the distance squared from the point to
	 * the wall midpoint. (distance * distance) 
	 * @param fromX The x-value of the point to test. (fromX, fromY, fromZ)
	 * @param fromY The y-value of the point to test. (fromX, fromY fromZ)
	 * @param fromZ The z-value of the point to test. (fromX, fromY fromZ)
	 * @param toWallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @return Get the distance squared from the point to
	 * the wall midpoint.
	 */
	public float getLinkPointDistanceSq(float fromX, float fromY, float fromZ, int toWallIndex)
	{
		return (float)(Math.pow((fromX - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET]), 2.0) 
				+ Math.pow((fromY - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET+1]), 2.0)
				+ Math.pow((fromZ - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET+2]), 2.0));
	}
	
	/**
	 * Get the distance squared from the point to
	 * the wall midpoint. (distance * distance) 
	 * @param fromX The x-value of the point to test. (fromX, fromZ)
	 * @param fromZ The z-value of the point to test. (fromX, fromZ)
	 * @param toWallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @return Get the distance squared from the point to
	 * the wall midpoint.
	 */
	public float getLinkPointDistanceSq(float fromX, float fromZ, int toWallIndex)
	{
		return (float)(Math.pow((fromX - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET]), 2.0) 
				+ Math.pow((fromZ - mWalls[toWallIndex*WALLSTRIDE+MIDPOINT_OFFSET+2]), 2.0));
	}

	/**
	 * The wall index of the cell linked to the wall.
	 * <p>Example</p>
	 * <p>If cellQ is connected to wall 1 of cellR<br/>
	 * And CellR is connected to wall 2 of cellQ<br/>
	 * Then CellQ.getLinkWall(2) == 1</p> 
	 * @param wallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @return The wall index of the cell linked to the wall, or {@link #NULL_INDEX}
	 * is nothing is linked to the wall.
	 */
	public int getLinkWall(int wallIndex)
	{
		return mLinkWalls[wallIndex];
	}
	
	/**
	 * Determines the relationship between the path (line segment) and the cell.
	 * @param ax The x-value of point A (ax, az).
	 * @param az The z-value of point A (ax, az).
	 * @param bx The x-value of point B (bx, bz).
	 * @param bz The z-value of point B (bx, bz).
	 * @param outNextCell The cell, if any, that is linked to the exit wall.
	 * Only applicable if the result is {@link PathRelType#EXITING_CELL} and the exit
	 * wall has a cell linked to it.  The argument can be null.
	 * @param outExitWallIndex  The index of the exit wall, if any.
	 * Only applicable if the result is {@link PathRelType#EXITING_CELL}.  This argument can be null.
	 * @param outIntersectionPoint The Content is undefined if the return value is
	 * other than {@link PathRelType#ENDING_CELL}. This argument can be null.
	 * @param workingVector  A vector used during the calculations.  Its content at return is undefined.
	 * @return The relationship between the path (line segment) and the cell.
	 */
	public PathRelType getPathRelationship(float ax, float az
			, float bx, float bz
			, TriCell[] outNextCell
			, int[] outExitWallIndex
			, Vector2 outIntersectionPoint
			, Vector2 workingVector)
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
			
			final int pVert = mVertPtr[iVert];
			
			PointLineRelType endPointRelType = getRelationship(iVert, bx, bz, TOLERANCE_STD);
			if (endPointRelType ==  PointLineRelType.LEFT_SIDE)
			{
				// For this wall, path end point is NOT inside the cell.
				// Is the start point outside the cell as well?

				PointLineRelType startPointRelType = getRelationship(iVert, ax, az, TOLERANCE_STD);
				if (startPointRelType == PointLineRelType.RIGHT_SIDE
						|| startPointRelType == PointLineRelType.ON_LINE)
				{
					
					// For this wall, the path start point is potentially
					// inside the cell.	
					
					int pNextVert = mVertPtr[(iVert+1 >= VERTCOUNT) ? 0 : iVert+1];
					
					LineRelType intersectType = Line2.getRelationship(ax, az, bx, bz
							, mVerts[pVert], mVerts[pVert+2], mVerts[pNextVert], mVerts[pNextVert+2]
							, workingVector);
			
					if (intersectType == LineRelType.SEGMENTS_INTERSECT 
							|| intersectType == LineRelType.ALINE_CROSSES_BSEG
							|| ((intersectType == LineRelType.BLINE_CROSSES_ASEG 
							|| intersectType == LineRelType.LINES_INTERSECT)
									/*
									 * Floating point error special case:
									 * Need to see if the intersection point is very close to either of the 
									 * walls vertices.
									 */
									&& (workingVector.sloppyEquals(mVerts[pVert]
									                                   , mVerts[pVert+2]
									                                   , TOLERANCE_STD)
											|| workingVector.sloppyEquals(mVerts[pNextVert]
											                                  , mVerts[pNextVert+2]
											                                  , TOLERANCE_STD))))
					{
						
						// The path intersects this wall.
						// Also, this is the "exit" wall since the previous two checks
						// show that the direction of travel is from
						// the right (inside) to the left (outside) of the wall.
						
						if (outNextCell != null	&& outNextCell.length > 0) 
							outNextCell[0] = mLinks[iVert];
						if (outExitWallIndex != null && outExitWallIndex.length > 0) 
							outExitWallIndex[0] = iVert;
						if (outIntersectionPoint != null)
							outIntersectionPoint.set(workingVector);
						
						return PathRelType.EXITING_CELL;

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
		
		if (outNextCell != null	&& outNextCell.length > 0) 
			outNextCell[0] = null;
		if (outExitWallIndex != null && outExitWallIndex.length > 0) 
			outExitWallIndex[0] = NULL_INDEX;
		
		if (interiorCount == VERTCOUNT)
			return PathRelType.ENDING_CELL;

		return PathRelType.NO_RELATIONSHIP;
		
	}
	
	/**
	 * Returns the y-value if the point (x, y, z) is located on the
	 * plane of the cell.
	 * <p>This operation does not care whether or not point (x, z)
	 * is within the column of the cell.</p>
	 * <p>This is a low cost operation.</p>
	 * @param x The x-value of point (x, z).
	 * @param z The z-value of point (x, z).
	 * @return The y-value if the point (x, y, z) is located on the
	 * plane of the cell.
	 */
	public float getPlaneY(float x, float z)
	{
    	// Proof:
    	// ax + by + cz + d = 0
    	// by = -(ax + cz + d)
    	// y = -(ax + cz + d)/b
    	if (mNormalY != 0)
    		return ( -((mNormalX * x) + (mNormalZ * z) + d) / mNormalY );
    	return 0;
	}

	/**
	 * Checks to see if the point is at a problematic location in the cell
	 * and shifts it to a safe location.
	 * <p>For vertices, moves the point slightly toward the centeroid.</p>
	 * <p>Behavior is undefined if the point is outside of the cell.<p>
	 * @param x The x-value of point(x, z).
	 * @param z The x-value of point(x, z).
	 * @param offsetScale The scale to offset the point if it is in an unsafe location.
	 * @param out The vector to load the result into.
	 * @return A reference to the out argument.
	 */
	public Vector2 getSafePoint(float x, float z, float offsetScale, Vector2 out)
	{
		out.set(x, z);
		for (int iVert = 0; iVert < VERTCOUNT; iVert++)
		{
			if (Vector2.sloppyEquals(x, z
					, mVerts[mVertPtr[iVert]], mVerts[mVertPtr[iVert]+2]
					, TOLERANCE_STD))
			{
				// Found a matching vertex.
				// Get direction vector vertex->centroid and scale to 5% of its original
				// length.  Then translate the out point.
				return out.add((centroidX - mVerts[mVertPtr[iVert]]) * offsetScale
						, (centroidZ - mVerts[mVertPtr[iVert]+2]) * offsetScale);
			}
		}
		return out;
	}
	
	/**
	 * Gets the value of a vertex.
	 * @param index 0 = VertA, 1 = VertB, 2 = VertC
	 * @param out A vector to store the result in.
	 * @return A reference to the out vector.
	 */
	public Vector3 getVertex(int index, Vector3 out)
	{
		final int pVert = mVertPtr[index];
		out.set(mVerts[pVert], mVerts[pVert+1], mVerts[pVert+2]);
		return out;
	}
	
	/**
	 * Gets the vertex index for the vertex that matches the provided
	 * point.  (Only checks the xz axes.)
	 * @param x The x-value of a point that may be a vertex. (x, y, z)
	 * @param z The z-value of a point that may be a vertex. (x, y, z)
	 * @param tolerance The tolerance the elements of each point must be within
	 * to be considered equal.
	 * @return The vertex that matches the provided point, or {@link #NULL_INDEX}
	 * if there is no match.
	 */
	public int getVertexIndex(float x, float z, float tolerance)
	{
		if (Vector2.sloppyEquals(mVerts[mVertPtr[VERTA]]
		                                , mVerts[mVertPtr[VERTA]+2]
		                                , x, z
		                                , tolerance))
			return VERTA;
		if (Vector2.sloppyEquals(mVerts[mVertPtr[VERTB]]
		                                , mVerts[mVertPtr[VERTB]+2]
		                                , x, z
		                                , tolerance))
			return VERTB;
		if (Vector2.sloppyEquals(mVerts[mVertPtr[VERTC]]
		                                , mVerts[mVertPtr[VERTC]+2]
		                                , x, z
		                                , tolerance))
			return VERTC;
		return NULL_INDEX;
	}

	/**
	 * Gets the vertex index for the vertex that matches the provided
	 * point.
	 * @param x The x-value of a point that may be a vertex. (x, y, z)
	 * @param y The y-value of a point that may be a vertex. (x, y, z)
	 * @param z The z-value of a point that may be a vertex. (x, y, z)
	 * @param tolerance The tolerance the elements of each point must be within
	 * to be considered equal.
	 * @return The vertex that matches the provided point, or {@link #NULL_INDEX}
	 * if there is no match.
	 */
	public int getVertexIndex(float x, float y, float z, float tolerance)
	{
		if (Vector3.sloppyEquals(mVerts[mVertPtr[VERTA]]
		                                , mVerts[mVertPtr[VERTA]+1]
		                                , mVerts[mVertPtr[VERTA]+2]
		                                , x, y, z
		                                , tolerance))
			return VERTA;
		if (Vector3.sloppyEquals(mVerts[mVertPtr[VERTB]]
		                                , mVerts[mVertPtr[VERTB]+1]
		                                , mVerts[mVertPtr[VERTB]+2]
		                                , x, y, z
		                                , tolerance))
			return VERTB;
		if (Vector3.sloppyEquals(mVerts[mVertPtr[VERTC]]
		                                , mVerts[mVertPtr[VERTC]+1]
		                                , mVerts[mVertPtr[VERTC]+2]
		                                , x, y, z
		                                , tolerance))
			return VERTC;
		return NULL_INDEX;
	}
	
	/**
	 * Get the x, y, or z value of a vertex.
	 * @param vertIndex 0 = VertA, 1 = VertB, 2 = VertC
	 * @param valueIndex 0 = x-value, 1 = y-value, 2 = z-value
	 * @return The requested vertex value.
	 */
	public float getVertexValue(int vertIndex, int valueIndex)
	{
		return mVerts[mVertPtr[vertIndex]+valueIndex];
	}

	/**
	 * Gets the distance from the point to the line formed by the wall.
	 * Since this is not a line segment test, the closest point on the wall's line
	 * may be outside of the cell.
	 * <p>This operation uses the cells xz-plane projection.</p>
	 * <p>Due to the method used, this is a low cost operation.</p>
	 * @param fromX The x-value of the point to test. (fromX, fromZ)
	 * @param fromZ The z-value of the point to test. (fromX, fromZ)
	 * @param toWallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @return The distance from the point to the line formed by the wall.
	 */
	public float getWallDistance(float fromX, float fromZ, int toWallIndex) 
	{
		return Math.abs(getSignedDistance(toWallIndex, fromX, fromZ));
	}
	
	/**
	 * The vertex to the left of the wall when viewed from within the cell.
	 * @param wallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @param out The vector to load the result into.
	 * @return A reference to the out argument.
	 */
	public Vector3 getWallLeftVertex(int wallIndex, Vector3 out)
	{
		wallIndex = mVertPtr[wallIndex];
		out.set(mVerts[wallIndex], mVerts[wallIndex+1], mVerts[wallIndex+2]);
		return out;
	}

	/**
	 * The vertex to the right of the wall when viewed from within the cell.
	 * @param wallIndex 0 = WallAB, 1 = WallBC, 2 = WallCA
	 * @param out The vector to load the result into.
	 * @return A reference to the out argument.
	 */
	public Vector3 getWallRightVertex(int wallIndex, Vector3 out)
	{
		wallIndex++;
		if (wallIndex >= VERTCOUNT)
			wallIndex = 0;
		wallIndex = mVertPtr[wallIndex];
		out.set(mVerts[wallIndex], mVerts[wallIndex+1], mVerts[wallIndex+2]);
		return out;
	}
	
	/**
	 * Indicates whether or not the cell intersects the AABB. Either fully containing
	 * the other is considered an intersection.  (Only a 2D check.)
	 * <p>This is potentially a non-trivial operation.</p>
	 * @param minx The minimum x of the AABB.
	 * @param minz The minimum z of the AABB.
	 * @param maxx The maximum x of the AABB.
	 * @param maxz The maximum Z of the AABB.
	 * @return TRUE if the cell and AABB intersect in any manner.
	 */
	public boolean intersects(float minx, float minz, float maxx, float maxz)
	{
		// Quick rejection: Does the AABB intersect Triangle-AABB.
		if (!Rectangle2.intersectsAABB(minx, minz, maxx, maxz
				, boundsMinX, boundsMinZ, boundsMaxX, boundsMaxZ))
				return false;
		
		// Quick selection: Are any of the triangle vertices contained
		// by the AABB?
		for (int iVert = 0; iVert < VERTCOUNT; iVert++)
		{
			final int p = mVertPtr[iVert];
			if (Rectangle2.contains(minx, minz, maxx, maxz, mVerts[p], mVerts[p+2]))
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
		float pCL = Vector2.dot(vAx, vAz, mWalls[WALLAB], mWalls[WALLAB+1]);
		
		// Get distance from centroid to vertex opposite wall AB along the
		// separation axis.
		float pCO = Vector2.dot(vCx, vCz, mWalls[WALLAB], mWalls[WALLAB+1]);
		
		// Project half length of AABB onto separation axis.
		float pBoxHL = Math.abs(Vector2.dot(hlx, 0, mWalls[WALLAB], mWalls[WALLAB+1]))
							+ Math.abs(Vector2.dot(0, hlz, mWalls[WALLAB], mWalls[WALLAB+1]));
		
	    if (Math.min(pCL, pCO) > pBoxHL || Math.max(pCL, pCO) < -pBoxHL)
	    	// wallAB is a valid separation axis.
	    	return false;
		
		// Is wallBC a valid separation axis?
		
		// Get distance from centroid to wallBC.
		pCL = Vector2.dot(vBx, vBz, mWalls[WALLBC*WALLSTRIDE], mWalls[WALLBC*WALLSTRIDE+1]);
		
		// Get distance from centroid to vertex opposite wall BC along the
		// separation axis.
		pCO = Vector2.dot(vAx, vAz, mWalls[WALLBC*WALLSTRIDE], mWalls[WALLBC*WALLSTRIDE+1]);
		
		// Project half length of AABB onto separation line.
		pBoxHL = Math.abs(Vector2.dot(hlx
							, 0
							, mWalls[WALLBC*WALLSTRIDE]
							, mWalls[WALLBC*WALLSTRIDE+1]))
					+ Math.abs(Vector2.dot(0
							, hlz
							, mWalls[WALLBC*WALLSTRIDE]
							, mWalls[WALLBC*WALLSTRIDE+1]));
		
	    if (Math.min(pCL, pCO) > pBoxHL || Math.max(pCL, pCO) < -pBoxHL)
	    	// wallBC is a valid separation axis.
	    	return false;
	    
		// Is wallCA a valid separation line?
		
		// Get distance from centroid to wallCA.
		pCL = Vector2.dot(vCx, vCz, mWalls[WALLCA*WALLSTRIDE], mWalls[WALLCA*WALLSTRIDE+1]);
		
		// Get distance from centroid to vertex opposite wall CA along the
		// separation axis.
		pCO = Vector2.dot(vBx, vBz, mWalls[WALLCA*WALLSTRIDE], mWalls[WALLCA*WALLSTRIDE+1]);
		
		// Project half length of AABB onto separation axis.
		pBoxHL = Math.abs(Vector2.dot(hlx
							, 0
							, mWalls[WALLCA*WALLSTRIDE]
							, mWalls[WALLCA*WALLSTRIDE+1]))
					+ Math.abs(Vector2.dot(0
							, hlz
							, mWalls[WALLCA*WALLSTRIDE]
							, mWalls[WALLCA*WALLSTRIDE+1]));
		
	    if (Math.min(pCL, pCO) > pBoxHL || Math.max(pCL, pCO) < -pBoxHL)
	    	// wallCA is a valid separation axis.
	    	return false;
		
		return true;
	}
	
	/**
	 * Indicates whether or not the point is within the cell column.
	 * @param x The x-value of point (x, z).
	 * @param z The z-value of point (x, z).
	 * @return TRUE if the point is within the cell column.  Otherwise FALSE.
	 */
	public boolean isInColumn(float x, float z)
	{
		for (int iWall = 0; iWall < VERTCOUNT; iWall++)
		{
			if (getRelationship(iWall, x, z, TOLERANCE_STD).equals(PointLineRelType.LEFT_SIDE)) 
				return false;
		}
		return true;
	}
	
	/**
	 * Attempts to link the provided cell to this cell.
	 * The link will only succeed if the two cells share a common wall.
	 * <p>If the link location is already occupied, the link operation will fail.</p>
	 * @param cellToLink The cell to attempt to link to this cell.
	 * @param crossLink If TRUE, the cells will be linked in both directions.
	 * (I.e. Linked to each other.) If FALSE, no attempt will be made to link this
	 * cell to the provided cell.
	 * @return The wall index the provided cell was linked to.  Or {@link #NULL_INDEX}
	 * if the no valid link could be made.
	 */
	public int link(TriCell cellToLink, boolean crossLink)
	{	
		int linkIndex = NULL_INDEX;
		int otherLinkIndex = NULL_INDEX;
		for (int iVert = 2, iNextVert = 0
				; iNextVert < VERTCOUNT && linkIndex == NULL_INDEX
				; iVert = iNextVert++)
		{
			final int pVert = mVertPtr[iVert];
			final int pNextVert = mVertPtr[iNextVert];
			for (int iOtherVert = 2, iOtherNextVert = 0
				; iOtherNextVert < VERTCOUNT
				; iOtherVert = iOtherNextVert++)
			{
				final int pOtherVert = cellToLink.mVertPtr[iOtherVert];
				final int pOtherNextVert = cellToLink.mVertPtr[iOtherNextVert];
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

	/**
	 * The number of links actually in use in the cell.
	 * E.g. Links that are non-null.
	 * @return The number of links actually in use in the cell.
	 * @see TriCell#maxLinks()
	 */
	public int linkCount()
	{
		return (mLinks[WALLAB] == null ? 0 : 1) 
					+ (mLinks[WALLBC] == null ? 0 : 1) 
					+ (mLinks[WALLCA] == null ? 0 : 1);
	}
	
	/**
	 * The maximum possible links for the cell.
	 * @return The maximum possible links for the cell.
	 * @see TriCell#linkCount()
	 */
	public int maxLinks() { return VERTCOUNT; }
	
	/**
	 * {@inheritDoc}
	 */
	@Override
	public String toString() 
	{
		DecimalFormat f = new DecimalFormat("0.0000");
		return TriCell.class.getSimpleName() + ": (" 
				+ f.format(centroidX) + ", " 
				+ f.format(centroidY) + ", " 
				+ f.format(centroidZ) + ")";
	}

	/**
	 * Determines the relationship between a point and the line.
	 * @param point The point to compare against the line.
	 * @param tolerance The tolerance to use for determining whether the points is on the line.
	 *     If the point is closer to the line than the tolerance, then the point is considered
	 *     on the line.
	 * @return  The relationship between the point and the line.
	 */
	private PointLineRelType getRelationship(int wallIndex, float x, float y, float tolerance)
	{
		
		float distance = getSignedDistance(wallIndex, x, y);
	
		if (distance > tolerance) 
			return PointLineRelType.RIGHT_SIDE;
		else if (distance < -tolerance) 
			return PointLineRelType.LEFT_SIDE;
		
		return PointLineRelType.ON_LINE;
	}

	private float getSignedDistance(int wallIndex, float x, float z)
	{
		// The subtraction gets the directional vector from a point on the line 
		// to the reference point.
		// The dot projects the directional vector onto the normal giving the distance
		// from the line to the point along the normal.
		int pVert = mVertPtr[wallIndex];
		return Vector2.dot(mWalls[wallIndex*WALLSTRIDE]
		                   , mWalls[wallIndex*WALLSTRIDE+1]
		                   , x - mVerts[pVert]
		                   , z - mVerts[pVert+2]);
	}

	/**
	 * Gets the cell that is closest to the provided point.
	 * <p>If the point lies on a shared edge, one of the cells sharing the edge will be returned.
	 * But which one is returned is arbitrary.</p>
	 * <p>This is an estimate for performance reasons.  The closer the point is to cell
	 * edges and the larger the slope differences between adjacent cells, the more likely
	 * an unexpected result will be obtained.</p>
	 * @param x The x-value of point (x, y, z).
	 * @param y The y-value of point (x, y, z).
	 * @param z The z-value of point (x, y, z).
	 * @param cells The cells to search.
	 * @param outPointOnCell If provided, the vector will be set to the closest point
	 * on the selected cell's plane.
	 * @param workingV2 A vector used during the calculations.  Its content at return is undefined.
	 * @return The cell that is closest to the provided point.
	 */
	public static TriCell getClosestCell(float x, float y, float z
			, TriCell[] cells
			, Vector3 outPointOnCell
			, Vector2 workingV2)
	{
		
		// WARNING: Any change to this code must also be made to 
		// the overload.
		
		float minDistanceSq = Float.MAX_VALUE;
		TriCell selectedCell = null; 
	
		// Loop through all cells.
		for (TriCell cell : cells)
		{
			float currentDistanceSq;
	
			// Get the relationship of point to this cell.
			PathRelType relationship = cell.getPathRelationship(cell.centroidX, cell.centroidZ
					, x, z
					, null
					, null
					, null
					, workingV2);
			
			if (relationship == PathRelType.EXITING_CELL)
			{
				
				/*
				 * The point is outside the cell column.
				 * The closest point on the cell is the exit's intersection point.
				 * We first need to convert this point to 3D, then figure out the distance
				 * from it to our reference point. 
				 */
				
				// Working vector contains 2D intersection point.  Need y.
				final float ty = cell.getPlaneY(workingV2.x, workingV2.y);
	
				// Convert to a direction vector.
				final float dx = workingV2.x - x;
				final float dy = ty - y;
				final float dz = workingV2.y - z;
				
				// And determine the length of the direction vector.
				currentDistanceSq = Vector3.getLengthSq(dx, dy, dz);
	
				if (currentDistanceSq < minDistanceSq)
				{
					// This cell is closer than previous cells.
					minDistanceSq = currentDistanceSq;
					selectedCell = cell;
					if (outPointOnCell != null)
						outPointOnCell.set(workingV2.x, ty, workingV2.y);
				}
			}
			else
			{
				// The point is in the cell column.
				// Use the distance from the point to the target point on the cell surface.
				// Only measuring y-distance.
				float cellY = cell.getPlaneY(x, z);
				currentDistanceSq = (y - cellY) * (y - cellY);
				if (currentDistanceSq < minDistanceSq)
				{
					// This cell is closer than previous cells.
					minDistanceSq = currentDistanceSq;
					selectedCell = cell;
					if (outPointOnCell != null)
						outPointOnCell.set(x, cellY, z);
				}	
			}
		}
	
		return selectedCell;
	}

	/**
	 * Gets the cell that is closest to the provided point.
	 * <p>If the point lies on a shared edge, one of the cells sharing the edge will be returned.
	 * But which one is returned is arbitrary.</p>
	 * <p>This is an estimate for performance reasons.  The closer the point is to cell
	 * edges and the larger the slope differences between adjacent cells, the more likely
	 * an unexpected result will be obtained.</p>
	 * @param x The x-value of point (x, y, z).
	 * @param y The y-value of point (x, y, z).
	 * @param z The z-value of point (x, y, z).
	 * @param cells The cells to search.
	 * @param outPointOnCell If provided, the vector will be set to the closest point
	 * on the selected cell's plane.
	 * @param workingV2 A vector used during the calculations.  Its content at return is undefined.
	 * @return The cell that is closest to the provided point.
	 */
	public static TriCell getClosestCell(float x, float y, float z
			, List<TriCell> cells
			, Vector3 outPointOnCell
			, Vector2 workingV2)
	{
		
		// TODO: The code in this operation is exactly the
		// same as in the array overload. Try to clean both up in a way
		// that doesn't hurt performance too much.
		
		float minDistanceSq = Float.MAX_VALUE;
		TriCell selectedCell = null; 
	
		// Loop through all cells.
		for (TriCell cell : cells)
		{
			float currentDistanceSq;
	
			// Get the relationship of point to this cell.
			PathRelType relationship = cell.getPathRelationship(cell.centroidX, cell.centroidZ
					, x, z
					, null
					, null
					, null
					, workingV2);
			
			if (relationship == PathRelType.EXITING_CELL)
			{
				
				/*
				 * The point is outside the cell column.
				 * The closest point on the cell is the exit's intersection point.
				 * We first need to convert this point to 3D, then figure out the distance
				 * from it to our reference point. 
				 */
				
				// Working vector contains 2D intersection point.  Need y.
				final float ty = cell.getPlaneY(workingV2.x, workingV2.y);
	
				// Convert to a direction vector.
				final float dx = workingV2.x - x;
				final float dy = ty - y;
				final float dz = workingV2.y - z;
				
				// And determine the length of the direction vector.
				currentDistanceSq = Vector3.getLengthSq(dx, dy, dz);
	
				if (currentDistanceSq < minDistanceSq)
				{
					// This cell is closer than previous cells.
					minDistanceSq = currentDistanceSq;
					selectedCell = cell;
					if (outPointOnCell != null)
						outPointOnCell.set(workingV2.x, ty, workingV2.y);
				}
			}
			else
			{
				// The point is in the cell column.
				// Use the distance from the point to the target point on the cell surface.
				// Only measuring y-distance.
				float cellY = cell.getPlaneY(x, z);
				currentDistanceSq = (y - cellY) * (y - cellY);
				if (currentDistanceSq < minDistanceSq)
				{
					// This cell is closer than previous cells.
					minDistanceSq = currentDistanceSq;
					selectedCell = cell;
					if (outPointOnCell != null)
						outPointOnCell.set(x, cellY, z);
				}
			}
		}
	
		return selectedCell;
	}
	
}


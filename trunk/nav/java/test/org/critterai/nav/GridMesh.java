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

import java.util.Random;
import org.critterai.math.MathUtil;

/**
 * NOT ready for use.
 * Dispose if not used by 2011-01-01.
 */
public final class GridMesh 
    implements ITestMesh 
{

    /*
     * Design Notes:
     * 
     * This class is not fully implemented.
     * 
     * The internals of this class are quite brittle
     * with are a lot of dependencies between algorithms.
     * Be especially careful if array storage order is
     * changed. It can break other algorithms which
     * make assumptions about the order.
     */
    
    private final Random mR = new Random(12);
    
    public final float minX;
    public final float minY;
    public final float minZ;
    public final float stepX;
    public final float stepZ;
    public final int stepCountX;
    public final float maxYOffset;
    public final int stepCountZ;
    
    private final int mPolyCount;
    private final float mOffset;
    
    private final float[] mVerts;
    private final int[] mIndices;
    private final float[] mLOSPointsTrue;
    private final int[] mLOSPolysTrue;
    private final int[] mLinkPolys;
    private final int[] mLinkCount;
    private final int[] mLinkWalls;
    
    public GridMesh(float minX, float minY, float minZ
            , float stepX, float stepZ
            , int stepCountX, int stepCountZ
            , float maxYOffset)
        throws IllegalArgumentException
    {
        
        if (stepX < MathUtil.TOLERANCE_STD * 4
                || stepZ < MathUtil.TOLERANCE_STD * 4
                || stepCountX < 1
                || stepCountZ < 1
                || maxYOffset < 0)
            throw new IllegalArgumentException
            		("One or more invalid arguments.");
        
        this.minX = minX;
        this.minY = minY;
        this.minZ = minZ;
        this.stepX = stepX;
        this.maxYOffset = maxYOffset;
        this.stepZ = stepZ;
        this.stepCountX = stepCountX;
        this.stepCountZ = stepCountZ;
        
        mOffset = (stepX < stepZ ? stepX * 0.1f : stepZ * 0.1f);        
        mPolyCount = stepCountX * stepCountZ * 2;
        
        mVerts = getVertsLocal();
        mIndices = getIndicesLocal();
        mLinkCount = getLinkCountLocal();
        
        mLOSPointsTrue = getLOSPointsTrueLocal();
        mLOSPolysTrue = getLOSPolysTrueLocal();
        mLinkPolys = getLinkPolysLocal();
        mLinkWalls = getLinkWallsLocal();
    }
    
    @Override
    public float[] getVerts() 
    {
        float[] result = new float[mVerts.length];
        System.arraycopy(mVerts, 0, result, 0, mVerts.length);
        return result;
    }

    @Override
    public int[] getIndices()
    {
        int[] result = new int[mIndices.length];
        System.arraycopy(mIndices, 0, result, 0, mIndices.length);
        return result;
    }
    
    @Override
    public float[] getMinVertex() 
    {
        float[] result = { minX, minY, minZ };
        return result;
    }

    @Override
    public int[] getMinVertexPolys() 
    {
        int[] result = { 0, 1 };
        return result;
    }

    /**
     * {@inheritDoc}
     * <p>All points in this mesh has LOS to all other points.
     * So this operation will always return null.<p>
     * @return Will always return null.
     */
    @Override
    public float[] getLOSPointsFalse() { return null; }

    /**
     * {@inheritDoc}
     * <p>All points in this mesh has LOS to all other points.
     * So this operation will always return null.<p>
     * @return Will always return null.
     */
    @Override
    public int[] getLOSPolysFalse() { return null; }

    @Override
    public float[] getLOSPointsTrue() 
    {
        float[] result = new float[mLOSPointsTrue.length];
        System.arraycopy(mLOSPointsTrue, 0, result, 0, mLOSPointsTrue.length);
        return result;
    }
    
    @Override
    public int[] getLOSPolysTrue() 
    {
        int[] result = new int[mLOSPolysTrue.length];
        System.arraycopy(mLOSPolysTrue, 0, result, 0, mLOSPolysTrue.length);
        return result;
    }
    
    @Override
    public int[] getLinkCounts() 
    { 
        int[] result = new int[mLinkCount.length];
        System.arraycopy(mLinkCount, 0, result, 0, mLinkCount.length);
        return result;
    }
    
    @Override
    public int[] getLinkPolys() 
    {
        int[] result = new int[mLinkPolys.length];
        System.arraycopy(mLinkPolys, 0, result, 0, mLinkPolys.length);
        return result;
    }

    @Override
    public int[] getLinkWalls() 
    {
        int[] result = new int[mLinkWalls.length];
        System.arraycopy(mLinkWalls, 0, result, 0, mLinkWalls.length);
        return result;
    }

    @Override
    public float getOffset() { return mOffset;}

    @Override
    public int getPolyCount() 
    {
        return mPolyCount;
    }

    private float[] getVertsLocal() 
    {
        float[] result = new float[(stepCountX + 1) * (stepCountZ + 1) * 3];
        int p = 0;
        for (int posZ = 0; posZ <= stepCountZ; posZ++)
        {
            for (int posX = 0; posX <= stepCountX; posX++, p += 3)
            {
                result[p+0] = minX + posX * stepX;
                result[p+1] = minY + mR.nextFloat() * maxYOffset;
                result[p+2] = minZ + posZ * stepZ;
            }
        }
        return result;
    }

    private int[] getIndicesLocal() 
    {
        int cellCount = stepCountX*stepCountZ;
        int[] result = new int[cellCount*2*3];
        for (int posZ = 0, p = 0
                ; posZ < stepCountZ
                ; posZ++)
        {
            for (int posX = 0
                    ; posX < stepCountX
                    ; posX++, p += 6)
            {
                int i = posX+posZ*(stepCountX+1);
                result[p+0] = i;
                result[p+1] = i + stepCountX + 1;
                result[p+2] = result[p+1] + 1;
                result[p+3] = i;
                result[p+4] = result[p+2];
                result[p+5] = i + 1;
            }
        }
        return result;
    }

    private int[] getLinkCountLocal() 
    {
        final int[] result = new int[mPolyCount];
        for (int iPoly = 0; iPoly < mPolyCount; iPoly++)
        {
            int borderCount = 0;
            for (int offsetA = 2, offsetB = 0
                    ; offsetB < 3
                    ; offsetA = offsetB++)
            {
                final int iVertA = mIndices[iPoly*3+offsetA];
                final int iVertB = mIndices[iPoly*3+offsetB];
                if (isBorderWall(iVertA, iVertB))
                    borderCount++;
            }
            if (borderCount == 0)
                 result[iPoly] = 3;
            else if (borderCount == 1)
                 result[iPoly] = 2;
            else
                 result[iPoly] = 1;
        }
        return result;
    }
    
    private boolean isBorderWall(int vertAIndex, int vertBIndex)
    {
        int flag = isBorderVert(vertAIndex) & isBorderVert(vertBIndex);
        return (flag == 1 || flag == 2 || flag == 4 || flag == 8);
    }
    
    private int isBorderVert(int vertIndex)
    {
        int result = 0;
        // Some convenience constants.
        final int rowStride = stepCountX+1;
        final int iMinXMaxZ = rowStride*(stepCountZ);
        if (vertIndex < rowStride)
            // Vertex is on lower bounds.
            result |= 1;
        if (vertIndex >= iMinXMaxZ)
            // Vertex is on upper bounds.
            result |= 2;
        if (vertIndex % rowStride == 0)
            // Vertex is on left bounds.
            result |= 4;
        if ((vertIndex+1) % rowStride == 0)
            // Vertex is on right bounds.
            result |= 8;
        return result;
    }

    private float[] getLOSPointsTrueLocal() 
    {
        final int stride = 16;
        final int rowStride = stepCountX + 1;
        final float offset = getOffset();
        float[] result = new float[stepCountZ*stride];
        for (int iGridZ = 0, p = 0
                ; iGridZ < stepCountZ
                ; iGridZ++, p += stride)
        {
            // Vertext to vertex.
            final int pVert = iGridZ*rowStride*3;
            int pOffset = 0;
            result[p+0] = mVerts[pVert+pOffset+0];
            result[p+1] = mVerts[pVert+pOffset+2];
            pOffset = (2*rowStride-1)*3;
            result[p+2] = mVerts[pVert+pOffset+0];
            result[p+3] = mVerts[pVert+pOffset+2];
            pOffset = rowStride*3;
            result[p+4] = mVerts[pVert+pOffset+0];
            result[p+5] = mVerts[pVert+pOffset+2];
            pOffset = (rowStride-1)*3;
            result[p+6] = mVerts[pVert+pOffset+0];
            result[p+7] = mVerts[pVert+pOffset+2];
            // Wall to wall
            pOffset = 0;
            result[p+8] = mVerts[pVert+pOffset+0] + offset;
            result[p+9] = mVerts[pVert+pOffset+2];
            pOffset = (2*rowStride-1)*3;
            result[p+10] = mVerts[pVert+pOffset+0] - offset;
            result[p+11] = mVerts[pVert+pOffset+2];
            pOffset = 0;
            result[p+8] = mVerts[pVert+pOffset+0];
            result[p+9] = mVerts[pVert+pOffset+2] + offset;
            pOffset = (2*rowStride-1)*3;
            result[p+10] = mVerts[pVert+pOffset+0];
            result[p+11] = mVerts[pVert+pOffset+2] - offset;
            // Inside, and shift axis.
            pOffset = 0;
            result[p+12] = mVerts[pVert+pOffset+0] + offset;
            result[p+13] = mVerts[pVert+pOffset+2] + (stepZ * 0.5f);
            // Clamp to maxZ border.
            pOffset = ((stepCountZ-iGridZ)*rowStride)*3;  
            result[p+14] = mVerts[pVert+pOffset+0] + (stepX * 0.5f);
            result[p+15] = mVerts[pVert+pOffset+2] - offset;
        }
        return result;
    }

    private int[] getLOSPolysTrueLocal() 
    {
        final int stride = 10;
        final int xRowOffset = stepCountX*2;
        int[] result = new int[stepCountZ*stride];
        for (int iGridZ = 0, p = 0
                ; iGridZ < stepCountZ
                ; iGridZ++, p += stride)
        {
            // Vertext to vertex.
            final int iPoly = iGridZ*xRowOffset;
            int iOffset = 1;
            result[p+0] = iPoly+iOffset;
            iOffset = (xRowOffset-2);
            result[p+1] = iPoly+iOffset;
            iOffset = 0;
            result[p+2] = iPoly+iOffset;
            iOffset = (xRowOffset-1);
            result[p+3] = iPoly+iOffset;
            // Wall to wall
            iOffset = 1;
            result[p+4] = iPoly+iOffset;
            iOffset = (xRowOffset-2);
            result[p+5] = iPoly+iOffset;
            iOffset = 0;
            result[p+6] = iPoly+iOffset;
            iOffset = (xRowOffset-1);
            result[p+7] = iPoly+iOffset;
            // Inside, and shift axis.
            iOffset = 0;
            result[p+8] = iPoly+iOffset;
            // Clamp to maxZ border.
            iOffset = ((stepCountZ-1-iGridZ)*xRowOffset);  
            result[p+9] = iPoly+iOffset;
        }
        return result;
    }
    
    private int[] getLinkPolysLocal() 
    {
        final int[] result = new int[mIndices.length];
        final int rowStride = 2*stepCountX;
        for (int iPoly = 0; iPoly < mPolyCount; iPoly += 2)
        {
            int pPoly = iPoly*3;
            int iVertA = mIndices[pPoly+0];
            int iVertB = mIndices[pPoly+1];
            int iVertC = mIndices[pPoly+2];
            if (isBorderWall(iVertA, iVertB))
                result[pPoly+0] = -1;
            else
                result[pPoly+0] = iPoly - 1;
            if (isBorderWall(iVertB, iVertC))
                result[pPoly+1] = -1;
            else
                result[pPoly+1] = iPoly + rowStride + 1;
            if (isBorderWall(iVertC, iVertA))
                result[pPoly+2] = -1;
            else
                result[pPoly+2] = iPoly + 1;
            
            int iNextPoly = iPoly+1;
            pPoly = iNextPoly*3;
            iVertA = mIndices[pPoly+0];
            iVertB = mIndices[pPoly+1];
            iVertC = mIndices[pPoly+2];
            if (isBorderWall(iVertA, iVertB))
                result[pPoly+0] = -1;
            else
                result[pPoly+0] = iNextPoly - 1;
            if (isBorderWall(iVertB, iVertC))
                result[pPoly+1] = -1;
            else
                result[pPoly+1] = iNextPoly + 1;
            if (isBorderWall(iVertC, iVertA))
                result[pPoly+2] = -1;
            else
                result[pPoly+2] = iNextPoly - rowStride - 1;
            
        }
        return result;
    }

    private int[] getLinkWallsLocal() 
    {
        final int[] result = new int[mIndices.length];
        for (int iPoly = 0; iPoly < mPolyCount; iPoly += 2)
        {
            int pPoly = iPoly*3;
            int iVertA = mIndices[pPoly+0];
            int iVertB = mIndices[pPoly+1];
            int iVertC = mIndices[pPoly+2];
            if (isBorderWall(iVertA, iVertB))
                result[pPoly+0] = -1;
            else
                result[pPoly+0] = 1;
            if (isBorderWall(iVertB, iVertC))
                result[pPoly+1] = -1;
            else
                result[pPoly+1] = 2;
            if (isBorderWall(iVertC, iVertA))
                result[pPoly+2] = -1;
            else
                result[pPoly+2] = 0;
            
            pPoly = (iPoly+1)*3;
            iVertA = mIndices[pPoly+0];
            iVertB = mIndices[pPoly+1];
            iVertC = mIndices[pPoly+2];
            if (isBorderWall(iVertA, iVertB))
                result[pPoly+0] = -1;
            else
                result[pPoly+0] = 2;
            if (isBorderWall(iVertB, iVertC))
                result[pPoly+1] = -1;
            else
                result[pPoly+1] = 0;
            if (isBorderWall(iVertC, iVertA))
                result[pPoly+2] = -1;
            else
                result[pPoly+2] = 1;
            
        }
        return result;
    }

    @Override
    public int getPathCount() 
    {
        return 0;
    }

    @Override
    public float[] getPathPoints(int index) {
        return null;
    }

    @Override
    public int[] getPathPolys(int index) {
        return null;
    }

    @Override
    public float getPlaneTolerence() { return 1.0f; }

    @Override
    public int getMultiPathCount() {
        return 0;
    }

    @Override
    public float[] getMultiPathGoalPoint(int index) {
        return null;
    }

    @Override
    public int[] getMultiPathPolys(int index) {
        return null;
    }

    @Override
    public float[] getMultiPathStartPoint() {
        return null;
    }

    @Override
    public int getShortestMultiPath() {

        return 0;
    }

}

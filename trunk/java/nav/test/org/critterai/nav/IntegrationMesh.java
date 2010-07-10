package org.critterai.nav;

import java.io.IOException;
import java.util.ArrayList;

import org.critterai.math.Vector3;

public final class IntegrationMesh 
{
    
    /*
     * Design Notes:
     * 
     * The tests that depend on this mesh expect a mesh in which all cells
     * are  connected to all other cells via a valid path.
     * (No island regions.)
     */
    
    private static final String TEST_FILE_NAME = "BISHomeGAS.obj";
    private static final float sampleFrequency = 0.515f;

    public static final int spacialDepth = 8;
    public static final float planeTolerance = 0.5f;
    public static final float offsetScale = 0.1f;
    public static final DistanceHeuristicType heuristic 
            = DistanceHeuristicType.LONGEST_AXIS;
    public static final int frameLength = 0;   // ns
    public static final long maxFrameTimeslice = 2000000; // ns
    public static final int maxPathAge = 60000; // ms
    public static final int repairSearchDepth = 2;
    public static final int searchPoolMax = 40;
    public static final long maintenanceFrequency = 500; // ms

    public static final boolean mirrorMesh = true;

    public final float[] mSourceVerts;
    public final int[] mSourceIndices;
    public final TriNavMesh mNavMesh;
    public final TriCellQuadTree mQuadTree;
    public final float[] mSamplePoints;
    public final Vector3 mMeshMin;
    public final Vector3 mMeshMax;
    public final TriCell[] mCells;
    public final ThreadedNavigator mMasterNavigator;

    public IntegrationMesh() 
        throws IOException, Exception
    {
        QuickMesh mesh = null;
        mesh = new QuickMesh(TEST_FILE_NAME, true);
        
        mSourceVerts = mesh.verts;
        mSourceIndices = mesh.indices;
        mMeshMin = mesh.minBounds;
        mMeshMax = mesh.maxBounds;
        mesh = null;

        if (mirrorMesh)
        {
            for (int p = 0; p < mSourceVerts.length; p += 3)
            {
                mSourceVerts[p] *= -1;
            }
            for (int p = 0; p < mSourceIndices.length; p += 3)
            {
                int t = mSourceIndices[p + 1];
                mSourceIndices[p + 1] = mSourceIndices[p + 2];
                mSourceIndices[p + 2] = t;
            }
            float tmp = mMeshMin.x * -1;
            mMeshMin.x = mMeshMax.x * -1;
            mMeshMax.x = tmp;
        }

        mCells = TestUtil.getAllCells(mSourceVerts, mSourceIndices);
        TestUtil.linkAllCells(mCells);

        mNavMesh = TriNavMesh.build(mSourceVerts
            , mSourceIndices
            , spacialDepth
            , planeTolerance
            , offsetScale);
        
        mQuadTree = new TriCellQuadTree(mMeshMin.x, mMeshMin.z
                    , mMeshMax.x, mMeshMax.z
                    , spacialDepth);
        for (TriCell cell : mCells)
        {
            if (!mQuadTree.add(cell))
                throw new Exception("Quad tree rejected cell: " + cell);
        }
        
        float x = mMeshMin.x;
        float z = mMeshMin.z;
        ArrayList<TriCell> columnCells = new ArrayList<TriCell>();
        ArrayList<Float> samplePoints = new ArrayList<Float>();
        while (x <= mMeshMax.x)
        {
            while (z <= mMeshMax.z)
            {
                mQuadTree.getCellsForPoint(x, z, columnCells);
                for (TriCell cell : columnCells)
                {
                    samplePoints.add(x);
                    samplePoints.add(cell.getPlaneY(x, z));
                    samplePoints.add(z);
                }
                z += sampleFrequency;
            }
            x += sampleFrequency;
            z = mMeshMin.z;
        }
        
        mSamplePoints = new float[samplePoints.size()];
        for (int i = 0; i < mSamplePoints.length; i++)
            mSamplePoints[i] = samplePoints.get(i);

        mMasterNavigator = NavUtil.getThreadedNavigator(
                mSourceVerts
                , mSourceIndices
                , spacialDepth
                , planeTolerance
                , offsetScale
                , heuristic
                , frameLength
                , maxFrameTimeslice
                , maxPathAge
                , repairSearchDepth
                , searchPoolMax
                , maintenanceFrequency);
        
    }
}

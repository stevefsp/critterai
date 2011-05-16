using System;
using System.Collections.Generic;
using System.Text;
using org.critterai.geom;

namespace org.critterai.nmgen
{
    public sealed class IncrementalBuilder
    {

        private const string pre = "PolyMesh Build: ";
        private const string pret = pre + "Trace: ";

        private NMGenParams mConfig;
        private BuildFlags mFlags;
        private BuildState mState;
        private BuildContext mContext;
        private bool mTrace;

        private Object mPrimary;
        private Object mSecondary;

        public BuildState State { get { return mState; } }

        public bool IsFinished
        {
            get
            {
                return (mState == BuildState.Aborted
                    || mState == BuildState.Complete);
            }
        }

        public PolyMesh PolyMesh
        {
            get
            {
                if (mState == BuildState.Complete)
                    return (PolyMesh)mPrimary;
                return null;
            }
        }

        public PolyMeshDetail DetailMesh
        {
            get
            {
                if (mState == BuildState.Complete)
                    return (PolyMeshDetail)mSecondary;
                return null;
            }
        }

        public int MessageCount { get { return mContext.MessageCount; } }

        public IncrementalBuilder(bool trace
            , NMGenParams config
            , BuildFlags buildFlags
            , TriangleMesh source)
        {
            mContext = new BuildContext(true);

            if (config == null
                || source == null
                || source.triCount < 1)
            {
                mState = BuildState.Aborted;
                mContext.Log(pre + 
                    "Aborted at construction. Null parameters or no geometry.");
                return;
            }

            mTrace = trace;
            mConfig = config.Clone();
            mConfig.DerivedGridSize();

            mFlags = buildFlags;
            mPrimary = source;

            mState = BuildState.MarkWalkableTris;
        }

        public string[] GetMessages() { return mContext.GetMessages(); }

        public BuildState Build()
        {
            switch (mState)
            {
                case BuildState.MarkWalkableTris:
                    MarkWalkableTris();
                    break;
                case BuildState.HeightfieldBuild:
                    BuildHeightfield();
                    break;
                case BuildState.CompactFieldBuild:
                    BuildCompactField();
                    break;
                case BuildState.MarkSpans:
                    MarkSpans();
                    break;
                case BuildState.ErodeWalkableArea:
                    ErodeWalkableArea();
                    break;
                case BuildState.DistanceFieldBuild:
                    BuildDistanceField();
                    break;
                case BuildState.RegionBuild:
                    BuildRegions();
                    break;
                case BuildState.ContourBuild:
                    BuildContours();
                    break;
                case BuildState.PolyMeshBuild:
                    BuildPolyMesh();
                    break;
                case BuildState.DetailMeshBuild:
                    BuildDetailMesh();
                    break;

            }
            return mState;
        }

        private void MarkWalkableTris()
        {
            TriangleMesh source = (TriangleMesh)mPrimary;

            byte[] areas = new byte[source.triCount];

            NMGen.MarkWalkableTriangles(mContext
                , source
                , mConfig.walkableSlope
                , areas);

            if (mTrace)
                mContext.Log(pret + "Marked walkable triangles");

            mSecondary = areas;
            mState = BuildState.HeightfieldBuild;
        }

        private void BuildHeightfield()
        {
            Heightfield hf = new Heightfield(mConfig.width
                , mConfig.depth
                , mConfig.boundsMin
                , mConfig.boundsMax
                , mConfig.xzCellSize
                , mConfig.yCellSize);

            hf.AddTriangles(mContext
                , (TriangleMesh)mPrimary
                , (byte[])mSecondary
                , mConfig.walkableStep);  // Merge for any spans less than step.

            if (mTrace)
                mContext.Log(pret + "Voxelized triangles. Span count: " 
                    + hf.GetSpanCount());

            if (hf.GetSpanCount() < 1)
            {
                mContext.Log("Aborted after heightfield build."
                    + " Heightfield does not have any spans.");
                mState = BuildState.Aborted;
                return;
            }

            mPrimary = hf;
            mSecondary = null;

            if ((mFlags
                & (BuildFlags.LowObstaclesWalkable
                    | BuildFlags.LedgeSpansNotWalkable
                    | BuildFlags.LowHeightSpansNotWalkable)) != 0)
            {
                mState = BuildState.MarkSpans;
            }
            else
                mState = BuildState.CompactFieldBuild;
        }

        private void MarkSpans()
        {
            Heightfield hf = (Heightfield)mPrimary;

            if ((mFlags & BuildFlags.LowObstaclesWalkable) != 0
                && mConfig.walkableStep > 0)
            {
                hf.MarkLowObstaclesWalkable(mContext, mConfig.walkableStep);
                if (mTrace)
                    mContext.Log(pret + "Flagged low obstacles as walkable.");

            }

            if ((mFlags & BuildFlags.LedgeSpansNotWalkable) != 0)
            {
                hf.MarkLedgeSpansNotWalkable(mContext
                    , mConfig.walkableHeight
                    , mConfig.walkableStep);
                if (mTrace)
                    mContext.Log(pret + "Flagged ledge spans as not walklable");
            }

            if ((mFlags & BuildFlags.LowHeightSpansNotWalkable) != 0)
            {
                hf.MarkLowHeightSpansNotWalkable(mContext
                    , mConfig.walkableHeight);
                if (mTrace)
                    mContext.Log(pret
                        + "Flagged low height spans as not walkable.");
            }

            mState = BuildState.CompactFieldBuild;

        }

        private void BuildCompactField()
        {
            Heightfield hf = (Heightfield)mPrimary;

            CompactHeightfield chf = CompactHeightfield.Build(mContext
                , hf
                , mConfig.walkableHeight
                , mConfig.walkableStep);

            hf.RequestDisposal();
            mPrimary = null;

            if (chf == null)
            {
                mContext.Log(pre + "Aborted at compact heightfield build.");
                mState = BuildState.Aborted;
                return;
            }

            if (mTrace)
                mContext.Log(pret + "Built compact heightfield. Spans: " 
                    + chf.SpanCount);

            if (chf.SpanCount < 1)
            {
                mContext.Log("Aborted after compact heightfield build."
                    + " Heightfield does not have any spans.");
                mState = BuildState.Aborted;
                return;
            }

            mSecondary = chf;

            if (mConfig.walkableRadius > 0)
                mState = BuildState.ErodeWalkableArea;
            else
                mState = BuildState.DistanceFieldBuild;
        }

        private void ErodeWalkableArea()
        {
            CompactHeightfield chf = (CompactHeightfield)mSecondary;

            chf.ErodeWalkableArea(mContext, mConfig.walkableRadius);

            if (mTrace)
                mContext.Log(pret + "Eroded walkable area by radius.");

           mState = BuildState.DistanceFieldBuild;
        }

        private void BuildDistanceField()
        {
            CompactHeightfield chf = (CompactHeightfield)mSecondary;

            chf.BuildDistanceField(mContext);
            if (mTrace)
                mContext.Log(pret + "Built distance field. Max Distance: "
                    + chf.MaxDistance);

            mState = BuildState.RegionBuild;
        }

        private void BuildRegions()
        {
            CompactHeightfield chf = (CompactHeightfield)mSecondary;

            if ((mFlags & BuildFlags.UseMonotonePartitioning) != 0)
            {
                chf.BuildRegionsMonotone(mContext
                    , mConfig.borderSize
                    , mConfig.minRegionArea
                    , mConfig.mergeRegionArea);
                if (mTrace)
                    mContext.Log(pret + "Built monotone regions.");
            }
            else
            {
                chf.BuildRegions(mContext
                    , mConfig.borderSize
                    , mConfig.minRegionArea
                    , mConfig.mergeRegionArea);
                if (mTrace)
                    mContext.Log(pret + "Built regions. Region Count: "
                        + chf.MaxRegions);
            }

            if (chf.MaxRegions < 2)
            {
                // Null region counts as a region.  So expect
                // at least 2.
                mContext.Log("Aborted after region build."
                    + " No useable regions formed.");
                mState = BuildState.Aborted;
                return;
            }

            mState = BuildState.ContourBuild;
        }

        private void BuildContours()
        {
            ContourBuildFlags cflags =
                (ContourBuildFlags)((int)mFlags & 0x03);

            ContourSet cset = ContourSet.Build(mContext
                , (CompactHeightfield)mSecondary
                , mConfig.edgeMaxDeviation
                , mConfig.maxEdgeLength
                , cflags);

            if (cset == null)
            {
                mContext.Log(pre + "Aborted at contour set build.");
                mState = BuildState.Aborted;
                return;
            }

            if (mTrace)
                mContext.Log(pret + "Build contour set. Contour count: "
                    + cset.Count);

            if (cset.Count < 1)
            {
                mContext.Log("Aborted after contour build."
                    + " No contours were generated.");
                mState = BuildState.Aborted;
                return;
            }

            mPrimary = cset;

            mState = BuildState.PolyMeshBuild;
        }

        private void BuildPolyMesh()
        {
            ContourSet cset = (ContourSet)mPrimary;

            PolyMesh polyMesh = PolyMesh.Build(mContext
                , cset
                , mConfig.maxVertsPerPoly
                , mConfig.walkableHeight
                , mConfig.walkableRadius
                , mConfig.walkableStep);

            cset.RequestDisposal();

            if (polyMesh == null)
            {
                mContext.Log(pre + "Aborted at poly mesh build.");
                mState = BuildState.Aborted;
                return;
            }

            if (mTrace)
                mContext.Log(pret + "Built poly mesh. PolyCount: "
                    + polyMesh.PolyCount);

            if (polyMesh.PolyCount < 1)
            {
                mContext.Log(pre + "Aborted after poly mesh build."
                    + "No polygons were generated.");
                mState = BuildState.Aborted;
                return;
            }

            if ((mFlags & BuildFlags.ApplyPolyFlags) != 0)
            {
                PolyMeshData data = polyMesh.GetData(false);
                for (int i = 0; i < data.flags.Length; i++)
                {
                    data.flags[i] = 1;
                }

                polyMesh.Load(data);
                if (mTrace)
                    mContext.Log(pret 
                        + "Applied polymesh flag to all polys: 0x01");
            }
            mPrimary = polyMesh;

            mState = BuildState.DetailMeshBuild;
        }

        private void BuildDetailMesh()
        {
            PolyMesh polyMesh = (PolyMesh)mPrimary;
            CompactHeightfield chf = (CompactHeightfield)mSecondary;

            PolyMeshDetail detailMesh = PolyMeshDetail.Build(mContext
                , polyMesh
                , chf
                , mConfig.detailSampleDistance
                , mConfig.detailMaxDeviation);

            chf.RequestDisposal();

            if (detailMesh == null)
            {
                mContext.Log(pre + "Aborted at detail mesh build.");
                polyMesh.RequestDisposal();
                mPrimary = null;
                mSecondary = null;
                mState = BuildState.Aborted;
                return;
            }

            mSecondary = detailMesh;

            if (mTrace)
                mContext.Log(pret + "Built detail mesh. TriangleCount: "
                    + detailMesh.TriCount);

            if (detailMesh.MeshCount < 1)
            {
                mContext.Log(pre + "Aborted after detail mesh build."
                    + "No detail meshes generated.");
                polyMesh.RequestDisposal();
                detailMesh.RequestDisposal();
                mPrimary = null;
                mSecondary = null;
                mState = BuildState.Aborted;
                return;
            }

            mState = BuildState.Complete;
        }

    }
}

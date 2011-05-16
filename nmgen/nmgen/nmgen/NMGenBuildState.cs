using System;
using System.Collections.Generic;
using System.Text;

namespace org.critterai.nmgen
{
    public enum BuildState
    {
        Aborted,
        Complete,
        MarkWalkableTris,
        HeightfieldBuild,
        CompactFieldBuild,
        MarkSpans,
        ErodeWalkableArea,
        DistanceFieldBuild,
        RegionBuild,
        ContourBuild,
        PolyMeshBuild,
        DetailMeshBuild
    }
}

using UnityEngine;
using Array = System.Array;
using org.critterai.nav;
using org.critterai.nmgen;

/// <summary>
/// The base class for a component that provides triangle mesh data for use
/// by other components.
/// </summary>
[System.Serializable]
[AddComponentMenu("CAI/Tile Data Bridge")]
public sealed class NavmeshTileBridge 
    : DSTileData 
{
    public BakedPolyMesh sourcePolyMesh;
    public int tileX;
    public int tileZ;
    public int tileLayer;
    public bool bvTreeEnabled;
    public int userId;

    public override NavmeshTileBuildData GetTileBuildData(bool includeTilePos)
    {
        string pre = this.name + ": NavTileDataBridge: ";

        if (!HasTileBuildData)
        {
            Debug.LogError(pre + "No tile build data available.");
            return null;
        }

        PolyMeshData polyMesh = sourcePolyMesh.GetPolyData();
        PolyMeshDetailData detailMesh = sourcePolyMesh.GetDetailData();

        if (polyMesh == null || polyMesh.polyCount < 1
            || detailMesh == null || detailMesh.meshCount < 1)
        {
            Debug.LogError(pre + "Mesh source provided null or"
                + " invalid meshes.");
            return null;
        }

        NavmeshTileBuildData result = new NavmeshTileBuildData(
            polyMesh.vertCount
            , polyMesh.polyCount
            , polyMesh.maxVertsPerPoly
            , detailMesh.vertCount
            , detailMesh.triCount);

        int x = tileX;
        int z = tileZ;
        int l = tileLayer;
        if (!includeTilePos)
        {
            x = 0;
            z = 0;
            l = 0;
        }

        if (!result.LoadBase(x, z, l, (uint)userId
            , polyMesh.boundsMin
            , polyMesh.boundsMax
            , polyMesh.xzCellSize
            , polyMesh.yCellSize
            , polyMesh.walkableHeight
            , polyMesh.walkableRadius
            , polyMesh.walkableStep
            , bvTreeEnabled))
        {
            Debug.LogError(pre + "Base data load failed. Bad mesh data"
                + " or internal error.");
            return null;
        }

        if (!result.LoadPolys(polyMesh.verts
            , polyMesh.vertCount
            , polyMesh.polys
            , polyMesh.flags
            , polyMesh.areas
            , polyMesh.polyCount))
        {
            Debug.LogError(pre + "Polygon load failed. Bad mesh data"
                + " or internal error.");
            return null;
        }

        if (!result.LoadDetail(detailMesh.verts
            , detailMesh.vertCount
            , detailMesh.tris
            , detailMesh.triCount
            , detailMesh.meshes
            , detailMesh.meshCount))
        {
            Debug.LogError(pre + "Detail load failed. Bad mesh data"
                + " or internal error.");
            return null;
        }

        return result;
    }

    public override bool HasTileBuildData
    {
        get { return (sourcePolyMesh != null && sourcePolyMesh.HasMesh); }
    }
}

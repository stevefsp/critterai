package org.critterai.nav;

import org.critterai.math.Vector3;

final class PathSearchInfo
{
    public Vector3 start;
    public Vector3 goal;
    public MasterNavRequest<MasterPath.Path>.NavRequest pathRequest;

    public PathSearchInfo(Vector3 start
        , Vector3 goal
        , MasterNavRequest<MasterPath.Path>.NavRequest pathRequest)
    {
        this.start = start;
        this.goal = goal;
        this.pathRequest = pathRequest;
    }
}

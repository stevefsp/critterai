/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn.externs
{
    public static class BuildersEx
    {
        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildRCNavMesh")]
        public static extern bool BuildRCNavMesh(RCConfigEx config
            , ref TriMesh3Ex sourceMesh
            , ref MessageBufferEx pMessages
            , ref RCPolyMeshEx polyMesh
            , ref RCPolyMeshDetailEx detailMesh);

        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildStaticDTNavMesh")]
        public static extern uint BuildStaticDTMesh(
            ref RCPolyMeshEx polyMesh
            , ref RCPolyMeshDetailEx detailMesh
            , float walkableHeight
            , float walkableRadius
            , float walkableClimb
            , ref RCNNavWaypointsEx offMeshConnections
            , ref IntPtr resultMesh);

        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildDTNavQuery")]
        public static extern uint BuildNavmeshQuery(IntPtr dtNavMesh
            , int maxNodes
            , ref IntPtr dtQuery);
    }
}

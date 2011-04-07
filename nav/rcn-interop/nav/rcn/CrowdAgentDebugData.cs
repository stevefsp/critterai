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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DTCrowdAgentDebugData
    {
        public const int MaxCorners = 4;
        public const int MaxNeighbors = 6;

	    public byte active;
        public CrowdAgentState state;

	    public PathCorridorData corridor;
        public CrowdLocalBoundaryData boundary;

        public float t;
        public float var;

        public float topologyOptTime;
    	
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNeighbors)]
	    CrowdNeighbor[] neighbors;
	    public int neighborCount;

        public float desiredSpeed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] position;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] disp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] desiredVelocity;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] nvel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] velocity;

        public CrowdAgentParams agentParams;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * MaxCorners)]
        public float[] cornerVerts;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCorners)]
        public byte[] cornerFlags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCorners)]
        public uint[] cornerPolyIds;

        public int cornerCount;

        public void Initialize()
        {
            active = 0;
            state = CrowdAgentState.Walking;
            corridor = PathCorridorData.Initialized;
            boundary = CrowdLocalBoundaryData.Initialized;
            t = 0;
            var = 0;
            topologyOptTime = 0;
            neighbors = new CrowdNeighbor[MaxNeighbors];
            neighborCount = 0;
            desiredSpeed = 0;
            position = new float[3];
            disp = new float[3];
            desiredVelocity = new float[3];
            nvel = new float[3];
            velocity = new float[3];

            agentParams = new CrowdAgentParams();

            cornerVerts = new float[MaxCorners * 3];
            cornerFlags = new byte[MaxCorners];
            cornerPolyIds = new uint[MaxCorners];
            cornerCount = 0;
        }

        public void Reset()
        {
            active = 0;
            state = CrowdAgentState.Walking;
            corridor.Reset();
            boundary.Reset();
            t = 0;
            var = 0;
            topologyOptTime = 0;
            foreach (CrowdNeighbor n in neighbors)
                n.Reset();
            neighborCount = 0;
            desiredSpeed = 0;
            Array.Clear(position, 0, position.Length);
            Array.Clear(disp, 0, disp.Length);
            Array.Clear(desiredVelocity, 0, desiredVelocity.Length);
            Array.Clear(nvel, 0, nvel.Length);
            Array.Clear(velocity, 0, velocity.Length);

            agentParams.Reset();

            Array.Clear(cornerVerts, 0, cornerVerts.Length);
            Array.Clear(cornerFlags, 0, cornerFlags.Length);
            Array.Clear(cornerPolyIds, 0, cornerPolyIds.Length);
            cornerCount = 0;
        }

        public static DTCrowdAgentDebugData Initialized
        {
            get 
            {
                DTCrowdAgentDebugData data = new DTCrowdAgentDebugData();
                data.Initialize();
                return data;
            }
        }

    }
}

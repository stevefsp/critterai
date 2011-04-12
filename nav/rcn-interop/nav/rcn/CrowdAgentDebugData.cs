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

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Provides debug information related to agents managed by 
    /// a <see cref="CrowdManager"/> object.
    /// </summary>
    /// <remarks>
    /// <p>Must be initialized before use.</p>
    /// <p>This data is provided for debug purposes. The purpose of various
    /// fields is this structue is undocumented, but still provided for
    /// advanced users.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct CrowdAgentDebugData
    {
        /// <summary>
        /// The maximum number of corners the corner fields
        /// can hold.
        /// </summary>
        public const int MaxCorners = 4;

        /// <summary>
        /// The maximum number of agent neighbors the neighbor fields
        /// can hold.
        /// </summary>
        public const int MaxNeighbors = 6;

        /// <summary>
        /// Whether or not the agent is being actively managed by its
        /// manager. (1 = active, 0 = inactive.)
        /// </summary>
	    public byte active;

        /// <summary>
        /// The state of the agent.
        /// </summary>
        public CrowdAgentState state;

        /// <summary>
        /// Current path information.
        /// </summary>
	    public PathCorridorData corridor;

        /// <summary>
        /// Local boundary segments. (Content is not documented.)
        /// </summary>
        /// <remarks>This data is used internally by 
        /// <see cref="CrowdManager"/>.  It's content is not currently
        /// documented.</remarks>
        public CrowdLocalBoundaryData boundary;

        /// <summary>
        /// t-value. (Not documented.)
        /// </summary>
        private float t;

        /// <summary>
        /// var-value. (Not documented.)
        /// </summary>
        private float var;

        /// <summary>
        /// topologyOptTime-value. (Not documented.)
        /// </summary>
        private float topologyOptTime;
    	
        /// <summary>
        /// The agent id's of all detected neighbors of the current agent.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNeighbors)]
	    public CrowdNeighbor[] neighbors;

        /// <summary>
        /// The number of neighbors.
        /// </summary>
	    public int neighborCount;

        /// <summary>
        /// The desired speed of the agent.
        /// </summary>
        public float desiredSpeed;

        /// <summary>
        /// The curent position of the agent in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] position;

        /// <summary>
        /// disp-value. (Not documented.)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private float[] disp;

        /// <summary>
        /// The desired velocity of the agent in the form (x, y, z)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] desiredVelocity;

        /// <summary>
        /// nvel-value. (Not documented.)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] nvel;

        /// <summary>
        /// The velocity of the agent in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] velocity;

        /// <summary>
        /// The agent parameters.
        /// </summary>
        public CrowdAgentParams agentParams;

        /// <summary>
        /// The corner vertices of the agent's local path.  (Not documented.)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * MaxCorners)]
        public float[] cornerVerts;

        /// <summary>
        /// The corner flags of the agent's local path.  (Not documented.)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCorners)]
        public byte[] cornerFlags;

        /// <summary>
        /// The polygon ids of the agent's local path. (Not documented.)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCorners)]
        public uint[] cornerPolyIds;

        /// <summary>
        /// The number of corners in the agent's local path.  (Not documented.)
        /// </summary>
        public int cornerCount;

        /// <summary>
        /// Initializes the structure before its first use.
        /// </summary>
        /// <remarks>
        /// Existing references are released and replaced.
        /// </remarks>
        public void Initialize()
        {
            active = 0;
            state = CrowdAgentState.Walking;
            corridor = new PathCorridorData();
            corridor.Initialize();
            boundary = new CrowdLocalBoundaryData();
            boundary.Initialize();
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

        /// <summary>
        /// Resets the structure's fields to their initialized state.
        /// </summary>
        /// <remarks>
        /// <p>Unlike the <see cref="Initialize"/> method, all references are 
        /// kept. (E.g. The content of existing arrays are zeroed.)</p>
        /// <p>The structure must be initialized before using this method.</p>
        /// </remarks>
        public void Reset()
        {
            active = 0;
            state = CrowdAgentState.Walking;
            corridor.Reset();
            boundary.Reset();
            t = 0;
            var = 0;
            topologyOptTime = 0;
            for (int i = 0; i < neighbors.Length; i++)
            {
                neighbors[i].Reset();
            }
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
    }
}

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
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Provides core data for agents managed by a <see cref="CrowdManager"/>
    /// object.
    /// </summary>
    /// <remarks>
    /// <p>Must be initialized before use.</p>
    /// <p>This structure is useful for marshalling information from the
    /// <see cref="CrowdManager"/> back to the actual agent implementation.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct CrowdAgentCoreData
    {
        /// <summary>
        /// The state of the agent.
        /// </summary>
	    public CrowdAgentState state;
    	
        /// <summary>
        /// The desired speed of the agent.
        /// </summary>
	    public float desiredSpeed;

        /// <summary>
        /// The current position of the agent on the navmesh in the form
        /// (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	    public float[] position;

        /// <summary>
        /// The desired velocity of the agent in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] desiredVelocity;

        /// <summary>
        /// The actual velocity of the agent in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] velocity;

        /// <summary>
        /// Initializes the structure before its first use.
        /// </summary>
        /// <remarks>
        /// Existing references are released and replaced.
        /// </remarks>
        public void Initialize()
        {
            state = CrowdAgentState.Walking;
            desiredSpeed = 0;
            position = new float[3];
            desiredVelocity = new float[3];
            velocity = new float[3];
        }

        /// <summary>
        /// Rerturns an array of fully initialized data structures.
        /// </summary>
        /// <param name="length">The length of the array. (>0)</param>
        /// <returns>An array of fully initialized structures.</returns>
        public static CrowdAgentCoreData[] GetInitializedArray(int length)
        {
            CrowdAgentCoreData[] data = new CrowdAgentCoreData[length];
            for (int i = 0; i < length; i++)
            {
                data[i].Initialize();
            }
            return data;
        }
    }
}

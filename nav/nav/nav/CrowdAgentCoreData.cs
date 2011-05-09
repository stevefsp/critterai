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

namespace org.critterai.nav
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
    public struct CrowdAgentCoreState
    {
        /*
         * Design note: This is a structure because it must be passed
         * in an array during interop.
         */

        /// <summary>
        /// The state of the agent.
        /// </summary>
	    public CrowdAgentState state;

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
        private float[] nvel;

        /// <summary>
        /// The velocity of the agent in the form (x, y, z).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] velocity;
    }
}

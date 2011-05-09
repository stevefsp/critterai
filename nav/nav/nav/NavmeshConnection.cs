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
    /// A navigation mesh off-mesh connection.
    /// </summary>
    /// <remarks>This structure is provided for debug purposes.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshConnection
    {
        /// <summary>
        /// The flag that indicates the connection is bi-directional.
        /// </summary>
        public const uint BiDirectionalFlag = 0x01;

        /// <summary>
        /// The endpoints of the connection in the form 
        /// (ax, ay, az, bx, by, bz).
        /// </summary>
        /// <remarks>For a properly built navigation mesh, vertexA
        /// will always be within the bounds of the mesh.
        /// vertexB may or may not be within the bounds of the mesh.
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	    public float[] endpoints;

        /// <summary>
        /// The radius of the endpoints. (>=0)
        /// </summary>
        public float radius;

        /// <summary>
        /// The polygon id of the connection.
        /// </summary>
        /// <remarks>All connections are stored as 2-vertex polygons within
        /// the navigation mesh.</remarks>
        public ushort polyIndex;
        
        /// <summary>
        /// Link flags.
        /// </summary>
        /// <remarks>
        /// These are not the off-mesh connection user flags.  Those
        /// are assigned to the connection's polygon.  These are link flags 
        /// used for internal purposes.
        /// </remarks>
        public byte flags;

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public byte side;

        /// <summary>
        /// The id of the offmesh connection. (User assigned when the
        /// navmesh is built.)
        /// </summary>
        public uint userId;

        /// <summary>
        /// TRUE if the traversal of the connection can start from either 
        /// endpoint.  FALSE if the connection can only be travered only from
        /// vertexA to vertexB.
        /// </summary>
        public bool IsBiDirectional
        {
            get { return (flags & BiDirectionalFlag) != 0; }
        }

        // TODO: CLEANUP: Remove if not back in use by v0.4.
        // Removed this code since the only time the structure is created
        // is during interop.  And initialization is not needed for interop.
        ///// <summary>
        ///// Initializes the structure before its first use.
        ///// </summary>
        ///// <remarks>
        ///// Existing references are released and replaced.
        ///// </remarks>
        //public void Initialize()
        //{
        //    endpoints = new float[6];
        //    radius = 0;
        //    polyIndex = 0;
        //    flags = 0;
        //    side = 0;
        //    userId = 0;
        //}

        ///// <summary>
        ///// Rerturns an array of fully initialized nodes.
        ///// </summary>
        ///// <param name="length">The length of the array. (>0)</param>
        ///// <returns>An array of fully initialized connections.</returns>
        //public static NavmeshConnection[] GetInitializedArray(int length)
        //{
        //    NavmeshConnection[] result = new NavmeshConnection[length];
        //    for (int i = 0; i < result.Length; i++)
        //        result[i].Initialize();
        //    return result;
        //}
    }
}

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
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshConnection
    {
        /// <summary>
        /// The flag that indicates the connection is bi-directional.
        /// </summary>
        public const uint BiDirectionalFlag = 0x01;

        /// <summary>
        /// The endpoints of the connection.
        /// [Form: (ax, ay, az, bx, by, bz)].
        /// </summary>
        /// <remarks>For a properly built navigation mesh, vertex A
        /// will always be within the bounds of the mesh.
        /// Vertex B is not required to be within the bounds of the mesh.
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	    public float[] endpoints;

        /// <summary>
        /// The radius of the endpoints. [Limit: >=0]
        /// </summary>
        public float radius;

        /// <summary>
        /// The polygon reference id of the connection.
        /// </summary>
        /// <remarks>
        /// <para>All connections are stored as 2-vertex polygons within
        /// the navigation mesh.</para></remarks>
        public ushort polyRef;
        
        /// <summary>
        /// Link flags.
        /// </summary>
        /// <remarks>
        /// <para>These are not the user flags.  Those are assigned to the 
        /// connection's polygon.  These are link flags used for internal 
        /// purposes.</para>
        /// </remarks>
        public byte flags;

        /// <summary>
        /// Side.
        /// </summary>
        public byte side;

        /// <summary>
        /// The id of the offmesh connection. (User assigned when the
        /// navmesh is built.)
        /// </summary>
        public uint userId;

        /// <summary>
        /// TRUE if the traversal of the connection can start from either 
        /// endpoint.  FALSE if the connection can only be travered from
        /// vertex A to vertex B.
        /// </summary>
        public bool IsBiDirectional
        {
            get { return (flags & BiDirectionalFlag) != 0; }
        }
    }
}

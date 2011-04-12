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
    /// Defines navigation connections.
    /// </summary>
    /// <remarks>
    /// <p>Must be initialized before use.</p>
    /// <p>All arrays in this structure, even the optional data arrays,
    /// must be sized for MaxConnections.  The actual number of connections 
    /// is specified by determined by the count field.</p>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavConnections
    {
        /// <summary>
        /// The maximum number of connections. (Size of the buffers.)
        /// </summary>
        public const int MaxConnections = 256;

        /// <summary>
        /// The number of connections.
        /// </summary>
        public int count;

        /// <summary>
        /// The connection endpoint vertices in the form 
        /// (startX, startY, startZ, endX, endY, endZ).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6 * MaxConnections)]
        public float[] vertices;

        /// <summary>
        /// The shared radii of each connection's endpoints.
        /// </summary>
        /// <remarks>
        /// The endpoints for a connection both have the same
        /// radius.  So the length of this array is equal to MaxConnections.
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxConnections)]
        public float[] radii;

        /// <summary>
        /// The allowed direction of the connections. (1 = bidirectional,
        /// 0 = Start to end.)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxConnections)]
        public byte[] dirs;

        /// <summary>
        /// The area id of each connection. (Optional)
        /// </summary>
        /// <remarks>
        /// The meaning of this data is user specific. The values can be
        /// used during navigation queries.  (See the 
        /// <see cref="NavmeshQuery"/> class.)</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxConnections)]
        public byte[] areaIds;

        /// <summary>
        /// The flags for each connection.
        /// </summary>
        /// <remarks>
        /// The meaning of this data is user specific. The values can be
        /// used to effect navigation queries.  (See the 
        /// <see cref="NavmeshQueryFilter"/> class.)  A least one flag must
        /// be set in order for connections to function normally.</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxConnections)]
        public ushort[] flags;

        /// <summary>
        /// The id of each connection. (Optional)
        /// </summary>
        /// <remarks>
        /// The meaning of this data is user specific.  It does
        /// not impact the behavior of navigation components.
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxConnections)]
        public uint[] ids;

        public NavConnections(float[] vertices
                , float[] radii
                , byte[] dirs
                , byte[] areaIds
                , ushort[] flags
                , uint[] ids
                , int connectionCount)
        {
            this.vertices = new float[6 * MaxConnections];
            this.radii = new float[MaxConnections];
            this.dirs = new byte[MaxConnections];
            this.areaIds = new byte[MaxConnections];
            this.flags = new ushort[MaxConnections];
            this.ids = new uint[MaxConnections];

            this.count = connectionCount;

            if (count > 0 && count <= MaxConnections
                && vertices != null && vertices.Length >= count * 6
                && radii != null && radii.Length >= count
                && dirs != null && dirs.Length >= count
                && (areaIds == null || areaIds.Length >= count)
                && (flags == null || flags.Length >= count)
                && (ids == null || ids.Length >= count))
            {
                Array.Copy(vertices, this.vertices, count * 6);
                Array.Copy(radii, this.radii, count);
                Array.Copy(dirs, this.dirs, count);

                if (areaIds != null)
                    Array.Copy(areaIds, this.areaIds, count);

                if (flags == null)
                {
                    for (int i = 0; i < MaxConnections; i++)
                    {
                        this.flags[i] = 1;
                    }
                }
                else
                    Array.Copy(flags, this.flags, count);

                if (ids != null)
                    Array.Copy(ids, this.ids, count);
            }
            else
                // Something is wrong with the input data.
                count = 0;
        }

        /// <summary>
        /// Initializes the structure before its first use.
        /// </summary>
        /// <remarks>
        /// <p>If flags are not used, then set initialFlags to 1 to ensure
        /// normal navigation behavior. (A flags value of zero effectively
        /// disables a connection.)</p>
        /// <p>Existing references are released and replaced.</p>
        /// </remarks>
        /// <param name="initialFlags">The initial flags to use for all
        /// connections.</param>
        public void Initialize(ushort initialFlags)
        {
            count = 0;
            vertices = new float[6 * MaxConnections];
            radii = new float[MaxConnections];
            dirs = new byte[MaxConnections];
            areaIds = new byte[MaxConnections];
            flags = new ushort[MaxConnections];
            ids = new uint[MaxConnections];
            for (int i = 0; i < MaxConnections; i++)
            {
                flags[i] = initialFlags;
            }
        }
    }
}

/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using System.Collections.Generic;
using org.critterai.nav;
using org.critterai.geom;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    public sealed class ConnectionSet
    {
        private readonly Vector3[] verts;
        private readonly float[] radii;
        private readonly byte[] dirs;
        private readonly byte[] areas;
        private readonly ushort[] flags;
        private readonly uint[] userIds;

        public int Count { get { return radii.Length; } }

        private ConnectionSet(Vector3[] verts
            , float[] radii
            , byte[] dirs
            , byte[] areas
            , ushort[] flags
            , uint[] userIds) 
        {
            this.verts = verts;
            this.radii = radii;
            this.dirs = dirs;
            this.areas = areas;
            this.flags = flags;
            this.userIds = userIds;
        }

        private ConnectionSet()
        {
            verts = new Vector3[0];
            radii = new float[0];
            dirs = new byte[0];
            areas = new byte[0];
            flags = new ushort[0];
            userIds = new uint[0];
        }

        public int GetConnections(Vector3 bmin, Vector3 bmax
            , out Vector3[] rverts
            , out float[] rradii
            , out byte[] rdirs
            , out byte[] rareas
            , out ushort[] rflags
            , out uint[] ruserIds)
        {
            rverts = null;
            rradii = null;
            rdirs = null;
            rareas = null;
            rflags = null;
            ruserIds = null;

            if (radii.Length == 0)
                return 0;

            List<Vector3> rlverts = new List<Vector3>();
            List<float> rlradii = new List<float>();
            List<byte> rldirs = new List<byte>();
            List<byte> rlareas = new List<byte>();
            List<ushort> rlflags = new List<ushort>();
            List<uint> rluserIds = new List<uint>();

            for (int i = 0; i < radii.Length; i++)
            {
                Vector3 v = verts[i * 2 + 0];
                if (Rectangle2.Contains(bmin.x, bmin.z, bmax.x, bmax.z, v.x, v.z))
                {
                    rlverts.Add(v);
                    rlverts.Add(verts[i * 2 + 1]);

                    rlradii.Add(radii[i]);
                    rldirs.Add(dirs[i]);
                    rlareas.Add(areas[i]);
                    rlflags.Add(flags[i]);
                    rluserIds.Add(userIds[i]);
                }
            }

            if (rlradii.Count == 0)
                return 0;

            rverts = rlverts.ToArray();
            rradii = rlradii.ToArray();
            rdirs = rldirs.ToArray();
            rareas = rlareas.ToArray();
            rflags = rlflags.ToArray();
            ruserIds = rluserIds.ToArray();

            return rradii.Length;
        }

        public static ConnectionSet CreateEmpty()
        {
            return new ConnectionSet();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>This method performs a full validation of the structure
        /// and content of the connection data.</para>
        /// <para>This method cannot be used to create an empty set.  Attempting
        /// to do so will return null. Use <see cref="CreateEmtpy"/> instead.
        /// </para>
        /// </remarks>
        /// <returns>A fully validated connection set, or null on failure.</returns>
        public static ConnectionSet Create(Vector3[] verts
            , float[] radii
            , byte[] dirs
            , byte[] areas
            , ushort[] flags
            , uint[] userIds)
        {
            if (IsValid(verts, radii, dirs, areas, flags, userIds))
            {
                return new ConnectionSet((Vector3[])verts.Clone()
                    , (float[])radii.Clone()
                    , (byte[])dirs.Clone()
                    , (byte[])areas.Clone()
                    , (ushort[])flags.Clone()
                    , (uint[])userIds.Clone());
            }
            return null;
        }

        public static ConnectionSet UnsafeCreate(Vector3[] verts
            , float[] radii
            , byte[] dirs
            , byte[] areas
            , ushort[] flags
            , uint[] userIds)
        {
            return new ConnectionSet(verts
                , radii
                , dirs
                , areas
                , flags
                , userIds);
        }

        public static bool IsValid(Vector3[] verts
            , float[] radii
            , byte[] dirs
            , byte[] areas
            , ushort[] flags
            , uint[] userIds)
        {
            // Will fail is there are zero connections.

            if (verts == null
                || radii == null
                || dirs == null
                || areas == null
                || flags == null
                || userIds == null)
            {
                return false;
            }

            if ((verts.Length < 2 || verts.Length % 2 != 0)
                || radii.Length != verts.Length / 2
                || dirs.Length != radii.Length
                || areas.Length != radii.Length
                || flags.Length != radii.Length
                || userIds.Length != radii.Length)
            {
                return false;
            }

            foreach (float val in radii)
            {
                if (val < MathUtil.Epsilon)
                    return false;
            }

            foreach (byte val in dirs)
            {
                if (val > 1)
                    return false;
            }

            foreach (byte val in areas)
            {
                if (val >= Navmesh.MaxAreas)
                    return false;
            }

            return true;
        }
    }
}

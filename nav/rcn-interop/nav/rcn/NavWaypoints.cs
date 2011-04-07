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
using org.critterai.nav.rcn.externs;
using System.Runtime.Serialization;

namespace org.critterai.nav.rcn
{
    [Serializable]
    public sealed class NavWaypoints
        : IDisposable, ISerializable
    {

        public const int MaxOffMeshConnections 
            = NavWaypointsEx.MaxConnections;

        private const string VertsKey = "v";
        private const string RadiiKey = "r";
        private const string DirsKey = "d";
        private const string AreasKey = "a";
        private const string FlagsKey = "f";
        private const string IdsKey = "i";

        internal NavWaypointsEx root;
        private bool mIsDisposed = false;

        public NavWaypoints() 
        {
            root = NavWaypointsEx.Empty;
        }

        public NavWaypoints(float[] vertices
                , float[] radii
                , byte[] dirs
                , byte[] areas
                , ushort[] flags
                , uint[] ids)
        {
            root = new NavWaypointsEx(vertices
                , radii
                , dirs
                , areas
                , flags
                , ids);
        }

        private NavWaypoints(SerializationInfo info
            , StreamingContext context)
        {
            if (info.MemberCount != 6)
            {
                mIsDisposed = true;
                return;
            }

            float[] verts =
                (float[])info.GetValue(VertsKey, typeof(float[]));
            float[] radii =
                (float[])info.GetValue(RadiiKey, typeof(float[]));
            byte[] dirs =
                (byte[])info.GetValue(DirsKey, typeof(byte[]));
            byte[] areas =
                (byte[])info.GetValue(AreasKey, typeof(byte[]));
            ushort[] flags =
                (ushort[])info.GetValue(FlagsKey, typeof(ushort[]));
            uint[] ids =
                (uint[])info.GetValue(IdsKey, typeof(uint[]));

            root = new NavWaypointsEx(verts
                , radii
                , dirs
                , areas
                , flags
                , ids);
        }

        public float[] GetVertices()
        {
            return root.GetVertices();
        }

        public float[] GetRadii()
        {
            return root.GetRadii();
        }

        public byte[] GetDirs()
        {
            return root.GetDirs();
        }

        public byte[] GetAreas()
        {
            return root.GetAreas();
        }

        public ushort[] GetFlags()
        {
            return root.GetFlags();
        }

        public uint[] GetIds()
        {
            return root.GetIds();
        }

        public int Count { get { return root.Count; } }

        public NavWaypointData GetData()
        {
            NavWaypointData data = new NavWaypointData();
            data.areas = root.GetAreas();
            data.dirs = root.GetDirs();
            data.flags = root.GetFlags();
            data.ids = root.GetIds();
            data.radii = root.GetRadii();
            data.vertices = root.GetVertices();
            data.count = root.Count;
            return data;
        }

        public bool IsDisposed
        {
            get { return mIsDisposed; }
        }

        ~NavWaypoints()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!mIsDisposed)
            {
                NavWaypointsEx.Free(ref root);
                mIsDisposed = true;
            }
        }

        public void GetObjectData(SerializationInfo info
            , StreamingContext context)
        {
            if (mIsDisposed)
                return;

            info.AddValue(VertsKey, root.GetVertices());
            info.AddValue(RadiiKey, root.GetRadii());
            info.AddValue(DirsKey, root.GetDirs());
            info.AddValue(AreasKey, root.GetAreas());
            info.AddValue(FlagsKey, root.GetFlags());
            info.AddValue(IdsKey, root.GetIds());
        }
    }
}

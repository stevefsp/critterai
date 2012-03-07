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
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
	public sealed class ConnectionSetCompiler
	{
        private readonly List<Vector3> mVerts;
        private readonly List<float> mRadii;
        private readonly List<byte> mDirs;
        private readonly List<byte> mAreas;
        private readonly List<ushort> mFlags;
        private readonly List<uint> mUserIds;

        public int Count { get { return mRadii.Count; } }

        public ConnectionSetCompiler(int initialCapacity)
        {
            mVerts = new List<Vector3>(initialCapacity);
            mRadii = new List<float>(initialCapacity);
            mDirs = new List<byte>(initialCapacity);
            mAreas = new List<byte>(initialCapacity);
            mFlags = new List<ushort>(initialCapacity);
            mUserIds = new List<uint>(initialCapacity);
        }

        // Note: Auto-clamps values.
        public void Add(Vector3 start, Vector3 end, float radius
            , bool isBidirectional
            , byte area
            , ushort flags
            , uint userId)
        {
            mVerts.Add(start);
            mVerts.Add(end);
            mRadii.Add(System.Math.Max(MathUtil.Epsilon, radius));
            mDirs.Add((byte)(isBidirectional ? 1 : 0));
            mAreas.Add(NMGen.ClampArea(area));
            this.mFlags.Add(flags);
            this.mUserIds.Add(userId);
        }

        public ConnectionSet GetConnectionSet()
        {
            if (mVerts.Count == 0)
                return ConnectionSet.CreateEmpty();

            Vector3[] lverts = mVerts.ToArray();
            float[] lradii = mRadii.ToArray();
            byte[] ldirs = mDirs.ToArray();
            byte[] lareas = mAreas.ToArray();
            ushort[] lflags = mFlags.ToArray();
            uint[] lids = mUserIds.ToArray();

            if (ConnectionSet.IsValid(lverts, lradii, ldirs, lareas, lflags, lids))
            {
                return ConnectionSet.UnsafeCreate(mVerts.ToArray()
                    , mRadii.ToArray()
                    , mDirs.ToArray()
                    , mAreas.ToArray()
                    , mFlags.ToArray()
                    , mUserIds.ToArray());
            }

            return null;
        }

        public void Reset()
        {
            mVerts.Clear();
            mRadii.Clear();
            mDirs.Clear();
            mAreas.Clear();
            mFlags.Clear();
            mUserIds.Clear();
        }
	}
}

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
using System;
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
	public sealed class AreaBoxMarker
        : AreaMarker
	{
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;

        public Vector3 BoundsMin { get { return mBoundsMin; } }
        public Vector3 BoundsMax { get { return mBoundsMax; } }
        public override bool IsThreadSafe { get { return true; } }

        public AreaBoxMarker(string name, int priority, byte area, Vector3 boundsMin, Vector3 boundsMax)
            : base(name, priority, area)
        {
            mBoundsMin = boundsMin;
            mBoundsMax = boundsMax;
        }

        public override bool ProcessBuild(NMGenState state, NMGenContext context)
        {
            if (state != NMGenState.CompactFieldBuild)
                return true;

            if (context.CompactField.MarkBoxArea(context, mBoundsMin, mBoundsMax, Area))
            {
                context.Log(string.Format("{0} : Marked box area: Area: {1}, Priority: {2}"
                    ,  Name, Area, Priority)
                    , this);
                return true;
            }
            return false;
        }
    }
}

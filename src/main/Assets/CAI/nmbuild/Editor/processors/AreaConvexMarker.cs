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
    public sealed class AreaConvexMarker
        : AreaMarker
    {
        private readonly Vector3[] verts;
        private readonly float ymin;
        private readonly float ymax;

        public override bool IsThreadSafe { get { return true; } }

        private AreaConvexMarker(string name
            , int priority
            , byte area
            , Vector3[] verts
            , float ymin
            , float ymax)
            : base(name, priority, area)
        {
            this.verts = verts;
            this.ymin = ymin;
            this.ymax = ymax;
        }

        public override bool ProcessBuild(NMGenState state, NMGenContext context)
        {
            if (state != NMGenState.HeightfieldBuild)
                return true;

            if (context.CompactField.MarkConvexPolyArea(context, verts, ymin, ymax, Area))
            {
                context.Log(string.Format(
                    "{0}: Marked convex polygon area: Area: {1}, Priority: {2}"
                    , Name, Area, Priority)
                    , this);

                return true;
            }

            context.Log(Name + ": Failed to mark convex polygon area.", this);

            return false;
        }

        public static AreaConvexMarker Create(string name
            , byte area
            , Vector3[] verts
            , float ymin
            , float ymax
            , int priority)
        {
            if (verts == null || verts.Length == 0 || ymin > ymax)
                return null;

            return new AreaConvexMarker(name, priority, area, verts, ymin, ymax);
        }
    }
}

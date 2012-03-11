﻿/*
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

namespace org.critterai.nmbuild
{
    public sealed class ApplyPolygonFlags
        : NMGenProcessor
    {
        private readonly ushort mFlags;

        public ushort Flags { get { return mFlags; } }
        public override bool IsThreadSafe { get { return true; } }

        public ApplyPolygonFlags(string name, int priority, ushort flags)
            : base(name, priority)
        {
            mFlags = flags;
        }

        public override bool ProcessBuild(NMGenState state, NMGenContext context)
        {
            if (state != NMGenState.PolyMeshBuild)
                return true;

            PolyMeshData data = context.PolyMesh.GetData(false);
            for (int i = 0; i < data.flags.Length; i++)
            {
                data.flags[i] |= mFlags;
            }

            context.PolyMesh.Load(data);
            context.Log(string.Format("{0}: Applied flags to all polys. Flags: {1:X}"
                , Name, mFlags)
                , this);

            return true;
        }
    }
}
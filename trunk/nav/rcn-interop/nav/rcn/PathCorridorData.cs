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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PathCorridorData
    {
        public const int MaxPathSize = 256;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] position;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] target;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPathSize)]
        public uint[] path;
        public int pathCount;

        public void Initialize()
        {
            position = new float[3];
            target = new float[3];
            path = new uint[MaxPathSize];
            pathCount = 0;
        }

        public void Reset()
        {
            Array.Clear(position, 0, position.Length);
            Array.Clear(target, 0, target.Length);
            Array.Clear(path, 0, path.Length);
            pathCount = 0;
        }

        public static PathCorridorData Initialized
        {
            get
            {
                PathCorridorData result = new PathCorridorData();
                result.Initialize();
                return result;
            }
        }
    }
}

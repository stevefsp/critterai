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
    public struct NavmeshConnection
    {
        public const uint BiDirectionalFlag = 0x01;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	    public float[] endpoints;							// Both end point locations.

        public float radius;								// Link connection radius.
        public ushort polyId;					// Poly Id
        
        // Note: These are not the off-mesh connection user flags.  Those
        // are assigned to the connections poly.  These flags are used
        // for internal purposes.
        public byte flags;					// Link flags
        public byte side;						// End point side.
        public uint userId;					// User ID to identify this connection.

        public bool IsBiDirectional
        {
            get { return (flags & BiDirectionalFlag) != 0; }
        }

        public void Initialize()
        {
            endpoints = new float[6];
            radius = 0;
            polyId = 0;
            flags = 0;
            side = 0;
            userId = 0;
        }

        public static NavmeshConnection[] GetInitializedArray(int size)
        {
            NavmeshConnection[] result = new NavmeshConnection[size];
            foreach (NavmeshConnection item in result)
                item.Initialize();
            return result;
        }
    }
}

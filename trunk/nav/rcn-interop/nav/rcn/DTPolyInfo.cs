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
    public struct DTPolyInfo
    {

        /// <summary>
        /// Size is DTNavmesh.MaxVertsPerPoly * 3
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
	    public float[] verts;	                    // Indices to vertices of the poly.

	    public ushort flags;						// Flags (see dtPolyFlags).
	    public byte vertCount;					// Number of vertices.
	    
        private byte mAreaAndType;					// Bit packed: Area ID of the polygon, and Polygon type, see dtPolyTypes.
	
        public byte Area
        {
            get { return (byte)(mAreaAndType & 0x3f); }
        }

        public DTPolyType Type
        {
            get { return (DTPolyType)(mAreaAndType >> 6); }
        }

        public static DTPolyInfo Initialized
        {
            get
            {
                DTPolyInfo result = new DTPolyInfo();
                result.verts = new float[DTNavmesh.MaxVertsPerPolygon * 3];
                return result;
            }
        }
    }
}

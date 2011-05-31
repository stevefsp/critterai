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

namespace org.critterai
{
    /// <summary>
    /// Provides various array related utility methods.
    /// </summary>
    public static class ArrayUtil
    {
        /// <summary>
        /// Determines whether or not the elements of the provided vectors 
        /// are equal within the specified tolerance of each other.
        /// </summary>
        /// <remarks>The arrays must be of the same length.</remarks>
        /// <param name="a">An array.</param>
        /// <param name="b">An array.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>TRUE if the associated elements are within the 
        /// specified tolerance of each other</returns>
        public static bool SloppyEquals(float[] a, float[] b, float tolerance)
        {
            tolerance = Math.Max(0, tolerance);
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] < b[i] - tolerance || a[i] > b[i] + tolerance)
                    return false;
            }
            return true;
        }

        // TODO: REMOVE: If not in use by 2012-06-01
        //public static ushort ToUInt16(byte[] source, int index)
        //{
        //    return (ushort)((ushort)(source[index] << 8) + source[index + 1]);
        //}

        //public static uint ToUInt32(byte[] source, int index)
        //{
        //    return (((uint)source[index] << 24) 
        //        + ((uint)source[index + 1] << 16)
        //        + ((uint)source[index + 2] << 8)
        //        + (uint)source[index + 3]);
        //}

        //public static byte[] ToByteArray(uint[] source)
        //{
        //    int targetLength = sizeof(uint) * source.Length;
        //    byte[] tmp = new byte[targetLength];
        //    Buffer.BlockCopy(source, 0, tmp, 0, targetLength);
        //    return tmp;
        //}

        //public static byte[] ToByteArray(ushort[] source)
        //{
        //    int targetLength = sizeof(ushort) * source.Length;
        //    byte[] tmp = new byte[targetLength];
        //    Buffer.BlockCopy(source, 0, tmp, 0, targetLength);
        //    return tmp;
        //}

        //public static uint[] ToUIntArray(byte[] source)
        //{
        //    uint[] tmp = new uint[source.Length / sizeof(uint)];
        //    Buffer.BlockCopy(source, 0, tmp, 0, source.Length);
        //    return tmp;
        //}

        //public static uint[] ToUIntArray(int[] source)
        //{
        //    uint[] tmp = new uint[source.Length];
        //    for (int i = 0; i < source.Length; i++)
        //    {
        //        tmp[i] = (uint)source[i];
        //    }
        //    return tmp;
        //}

        //public static int[] ToIntArray(uint[] source)
        //{
        //    int[] tmp = new int[source.Length];
        //    for (int i = 0; i < source.Length; i++)
        //    {
        //        tmp[i] = (int)source[i];
        //    }
        //    return tmp;
        //}

        //public static ushort[] ToUShortArray(byte[] source)
        //{
        //    ushort[] tmp = new ushort[source.Length / sizeof(ushort)];
        //    Buffer.BlockCopy(source, 0, tmp, 0, source.Length);
        //    return tmp;
        //}
    }
}

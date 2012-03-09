/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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

namespace org.critterai
{
    /// <summary>
    /// Provides array related utility methods.
    /// </summary>
    /// <remarks>
    /// <para>Static methods are thread safe.</para>
    /// </remarks>
    public static class ArrayUtil
    {
        /// <summary>
        /// Compresses an array by removing all null values.
        /// </summary>
        /// <remarks>
        /// <para>Only valid for use with arrays of reference types.</para>
        /// <para>No guarentees are made concerning the order of the items in the returned array.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="items">The array.</param>
        /// <returns>A reference to original array if it contained no nulls, or a
        /// new array with all nulls removed.</returns>
        public static T[] Compress<T>(T[] items)
        {
            if (items == null)
                return null;

            if (items.Length == 0)
                return items;

            int count = 0;

            foreach (T item in items)
            {
                count += (item == null) ? 0 : 1;
            }

            if (count == items.Length)
                return items;

            T[] result = new T[count];

            if (count == 0)
                return result;

            count = 0;
            foreach (T item in items)
            {
                if (item != null)
                    result[count++] = item;
            }

            return result;
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

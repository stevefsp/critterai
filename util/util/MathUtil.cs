/*
 * Copyright (c) 2010-2011 Stephen A. Pratt
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

namespace org.critterai
{
    /// <summary>
    /// Provides various math related utility methods.
    /// </summary>
    /// <remarks>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public static class MathUtil 
    {

        /// <summary>
        /// A standard epsilon value.  (Minimum positive value.)
        /// </summary>
        public const float Epsilon = 0.00001f;

        /// <summary>
        /// A standard tolerance value.
        /// </summary>
        public const float Tolerance = 0.0001f;
        
        /// <summary>
        /// Determines whether the values are within the specified tolerance
        /// of each other. 
        /// </summary>
        /// <param name="a">The a-value to compare the b-value against.
        /// </param>
        /// <param name="b">The b-value to compare against the a-value.
        /// </param>
        /// <param name="tolerance">The tolerance to use for the comparison.
        /// </param>
        /// <returns>TRUE if the values are within the specified tolerance
        /// of each other.</returns>
        public static bool SloppyEquals(float a, float b, float tolerance)
        {
            return !(b < a - tolerance || b > a + tolerance);
        }
        
        /// <summary>
        /// Clamps the value to a positive non-zero value.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The value clamped to a minimum of the smallest possible 
        /// positive value greater  than zero.</returns>
        public static float ClampToPositiveNonZero(float value)
        {
            return Math.Max(float.Epsilon, value);
        }
        
        /// <summary>
        /// Clamps the value to the specified range.  The clamp is inclusive
        /// of the minimum and maximum.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="minimum">The minimum allowed value.</param>
        /// <param name="maximum">The maximum allowed value.</param>
        /// <returns>A value clamped to the specified range.</returns>
        public static int Clamp(int value, int minimum, int maximum)
        {
            return (value < minimum ? minimum : 
                (value > maximum ? maximum : value));
        }
        
        /// <summary>
        /// Returns the maximum value in the list of values.
        /// </summary>
        /// <param name="values">The values to search.</param>
        /// <returns>The maximum value in the list of values.</returns>
        public static float Max(params float[] values)
        {
            float result = values[0];
            for (int i = 1; i < values.Length; i++)
                result = Math.Max(result, values[i]);
            return result;
        }
        
        /// <summary>
        /// Returns the minimum value in the list of values.
        /// </summary>
        /// <param name="values">The values to search.</param>
        /// <returns>The minimum value in the list of values.</returns>
        public static float Min(params float[] values)
        {
            float result = values[0];
            for (int i = 1; i < values.Length; i++)
                result = Math.Min(result, values[i]);
            return result;
        }

        public static byte CleanBits(byte flag, int limit)
        {
            return (byte)CleanBits((ulong)flag, Math.Min(8, limit));
        }

        public static ushort CleanBits(ushort flag, int limit)
        {
            return (ushort)CleanBits((ulong)flag, Math.Min(16, limit));
        }

        public static uint CleanBits(uint flag, int limit)
        {
            return (uint)CleanBits((ulong)flag, Math.Min(32, limit));
        }

        public static ulong CleanBits(ulong flag, int limit)
        {
            limit = Math.Max(0, Math.Min(64, limit));
            ulong mask = 0;
            for (int i = 0; i < (int)limit; i++)
            {
                mask = (mask | (1UL << i));
            }
            return (flag & mask);
        }
    }
}

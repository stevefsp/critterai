/*
 * Copyright (c) 2010 Stephen A. Pratt
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

namespace org.critterai.util
{
    /// <summary>
    /// Provides utility methods useful for reading and mutating 4-bit 
    /// packed unsigned integers.
    /// </summary>
    /// <remarks>
    /// <p>A slot is the zero-based index location of a packed 4-bit 
    /// value. The expected slot range is zero to seven with the zero slot
    /// being the lowest 4-bits.</p>
    /// <p>The range of values permitted for a slot is 0 through 15.</p>
    /// <p>This class is optimized for speed.  To support this priority, no 
    /// argument validation is performed.  E.g. No index or value range
    /// checks.</p>
    /// <p>Static methods are thread safe.</p>
    /// <p>Static methods are thread safe.</p>
    /// </remarks>
    public static class UIntPack4
    {
        /// <summary>
        /// Sets the value of a slot. (Mutates the target.)
        /// </summary>
        /// <param name="target">The packed integer to update.</param>
        /// <param name="slot">The slot to place the value in. (0 - 7)</param>
        /// <param name="value">The value. (0 - 15)</param>
        public static void Set(ref uint target, int slot, uint value)
        {
            target = ((target & ~(0xfU << (slot * 4)) // Clear the slot.
               | ((0xfU & value) << (slot * 4)))); // Set the slot.
        }

        /// <summary>
        /// Sets the value of a slot to zero. (Mutates the target.)
        /// </summary>
        /// <param name="target">The packed integer to update.</param>
        /// <param name="slot">The slot to to set to zero. (0 - 7)</param>
        public static void Zero(ref uint target, int slot)
        {
            target = (target & ~(0xfU << (slot * 4)));
        }

        /// <summary>
        /// Gets the value of a slot.
        /// </summary>
        /// <param name="source">The packed integer.</param>
        /// <param name="slot">The slot.  (0 - 7)</param>
        /// <returns>The value of the specified slot. (0 - 15)</returns>
        public static uint Get(uint source, int slot)
        {
            return (source >> (slot * 4)) & 0xfU;
        }

        /// <summary>
        /// Sets the first slot which has a value of zero to the specified
        /// value.
        /// (Mutates the target.)
        /// </summary>
        /// <remarks>
        /// <p>Starts the search at slot zero. (The lowest 4-bit slot.)</p>
        /// <p>The target will not change if it has no slots with a value
        /// of zero.</p>
        /// </remarks>
        /// <param name="target">The packed integer to update.</param>
        /// <param name="value">The value. (0 - 15)</param>
        public static void SetFirstEmpty(ref uint target, uint value)
        {
            for (int slot = 0; slot < 8; slot++)
            {
                if (Get(target, slot) == 0)
                {
                    Set(ref target, slot, value);
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the index of the first slot with a value of zero.
        /// </summary>
        /// <remarks>
        /// <p>Starts the search at slot zero. (The lowest 4-bit slot.)</p>
        /// </remarks>
        /// <param name="source">A packed integer.</param>
        /// <returns>The index of the first slot with a value of zero, or -1
        /// if there are no empty slots.
        /// </returns>
        public static int FirstEmptySlot(uint source)
        {
            for (int slot = 0; slot < 8; slot++)
            {
                if (Get(source, slot) == 0)
                    return slot;
            }
            return -1;
        }

        /// <summary>
        /// Removes the first slot value and shifts the content of all
        /// other slots one slow lower. (Mutates the target.)
        /// </summary>
        /// <param name="target">The packed integer to update.</param>
        public static void RemoveFirst(ref uint target)
        {
            target = target >> 4;
        }

        /// <summary>
        /// Gets the value of slot zero.
        /// </summary>
        /// <param name="source">A packed integer.</param>
        /// <returns>The value of slot zero.</returns>
        public static uint GetFirst(uint source)
        {
            return source & 0xfU;
        }
    }
}

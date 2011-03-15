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
using UnityEngine;

namespace org.critterai
{
    public static class ColorUtil
    {
        // Returns 1 if the bit at position b in a is 1
        // Otherwise returns 0.
        private static int bit(int a, int b)
        {
            return (a & (1 << b)) >> b;
        }

        public static Color IntToColor(int i, float a)
        {
            // r, g, and b are constrained to between 1 and 4 inclusive.
            float factor = 63 / 255;  // Approximately 0.25.
	        float r = bit(i, 1) + bit(i, 3) * 2 + 1;
	        float g = bit(i, 2) + bit(i, 4) * 2 + 1;
	        float b = bit(i, 0) + bit(i, 5) * 2 + 1;
            return new Color(r * factor, g * factor, b * factor, a);
        }
    }
}

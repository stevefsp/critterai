﻿/*
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

namespace org.critterai.math.geom
{
    /// <summary>
    /// Specifies the position relationship between a point and a line.
    /// </summary>
    /// <remarks>
    /// <p>Static methods are thread safe.</p></remarks>
    public enum PointLineRelType : byte
    {
        /// <summary>
        /// The reference point is on, or very near, the line
        /// </summary>
        OnLine,

        /// <summary>
        /// The reference  point is to the left when looking from point 
        /// A toward B on the line.
        /// </summary>
        LeftSide,

        /// <summary>
        /// The reference point is to the right when looking from point 
        /// A toward B on the line.
        /// </summary>
        RightSide
    }
}
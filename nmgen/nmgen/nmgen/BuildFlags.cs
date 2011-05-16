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

namespace org.critterai.nmgen
{
    [Flags]
    public enum BuildFlags
    {
        /*
         * Design notes:
         * 
         * Keep the base type of this enum an integer in order to remain 
         * compatible with Unity serialization.
         *
         */

        // Keep the first two flags in synch with ContourBuildFlags.
        TessellateWallEdges = 0x001,
        TessellateAreaEdges = 0x002,
        // Leave some room here for future contour flags.
        LedgeSpansNotWalkable = 0x010,
        LowHeightSpansNotWalkable = 0x020,
        LowObstaclesWalkable = 0x040,
        UseMonotonePartitioning = 0x080,
        ApplyPolyFlags = 0x100
    }
}

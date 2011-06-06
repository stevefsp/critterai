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
    /// <summary>
    /// Flags used to control optional build steps. 
    /// </summary>
    [Flags]
    public enum BuildFlags
    {
        /*
         * Design notes:
         * 
         * Keep the base type of this enum an integer in order to remain 
         * compatible with Unity serialization.
         *
         * Keep the two tessellation flags in synch with ContourBuildFlags.
         * Leave some room for future contour flags.
         */

        /// <summary>
        /// Tesselate wall edges during the contour build.
        /// </summary>
        /// <remarks>
        /// <para>Equivalent to the same value in 
        /// <see cref="ContourBuildFlags"/>.</para>
        /// </remarks>
        TessellateWallEdges = 0x001,

        /// <summary>
        /// Tessellate area edges during the contour build.
        /// </summary>
        /// <remarks>
        /// <para>Equivalent to the same value in 
        /// <see cref="ContourBuildFlags"/>.</para>
        /// </remarks>
        TessellateAreaEdges = 0x002,

        /// <summary>
        /// Include <see cref="Heightfield.MarkLedgeSpansNotWalkable"/>
        /// in the build.
        /// </summary>
        LedgeSpansNotWalkable = 0x010,

        /// <summary>
        /// Include <see cref="Heightfield.MarkLowHeightSpansNotWalkable"/>
        /// in the build.
        /// </summary>
        LowHeightSpansNotWalkable = 0x020,

        /// <summary>
        /// Include <see cref="Heightfield.MarkLowObstaclesWalkable"/>
        /// in the build.
        /// </summary>
        LowObstaclesWalkable = 0x040,

        /// <summary>
        /// Use <see cref="CompactHeightfield.BuildRegionsMonotone"/>
        /// to build region data.
        /// </summary>
        UseMonotonePartitioning = 0x080,

        /// <summary>
        /// The 0x01 flag should be applied to all polygons in the
        /// <see cref="PolyMesh"/> object.
        /// </summary>
        ApplyPolyFlags = 0x100
    }
}

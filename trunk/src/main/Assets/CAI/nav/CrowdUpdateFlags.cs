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

namespace org.critterai.nav
{

    /// <summary>
    /// <see cref="CrowdAgent"/> configurtation flags.
    /// </summary>
    [System.Flags]
    public enum CrowdUpdateFlags : byte
    {
        /// <summary>
        /// Undocumented
        /// </summary>
        AnticipateTurns = 0x01,

        /// <summary>
        /// Undocumented.
        /// </summary>
        ObstacleAvoidance = 0x02,

        /// <summary>
        /// Undocumented.
        /// </summary>
        CrowdSeparation = 0x04,

        /// <summary>
        /// Optimize visibility using 
        /// <see cref="PathCorridor.OptimizePathVisibility(Vector3, float)"/> .
        /// </summary>
        OptimizeVis = 0x08,

        /// <summary>
        /// Optimize topology using 
        /// <see cref="PathCorridor.OptimizePathTopology()"/>.
        /// </summary>
        OptimizeTopo = 0x10
    }
}

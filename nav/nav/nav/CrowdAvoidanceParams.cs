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

namespace org.critterai.nav
{
    /// <summary>
    /// Configuration parameters that define steering behaviors for agents
    /// managed by a crowd manager.
    /// </summary>
    /// <remarks>
    /// <p>This type has been implemented as a class with public fields
    /// in order to support Unity serialization. Care must be taken
    /// to not set the fields to invalid values, or to pass references
    /// inappropriatly.</p></remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class CrowdAvoidanceParams
    {

        /*
         * Source: DetourCrowdAvoidance dtObstacleAvoidanceParams (struct)
         * 
         * Design note: 
         * 
         * Class vs Structure
         * 
         * In this case I've decided to go with a class over a structure.
         * I expect this data will be mostly set at design time and remain
         * static at runtime.  So no need to optimize interop calls, and
         * support for Unity serialization is more important.
         * 
         */

        /// <summary>
        /// 
        /// </summary>
	    public float velocityBias;

        /// <summary>
        /// 
        /// </summary>
        public float weightDesiredVelocity;
        public float weightCurrentVelocity;
        public float weightSide;
        public float weightToi;
        public float horizontalTime;
        public byte gridSize;
        public byte adaptiveDivisions;
        public byte adaptiveRings;
        public byte adaptiveDepth;

        public CrowdAvoidanceParams() { }

        public CrowdAvoidanceParams Clone()
        {
            CrowdAvoidanceParams result = new CrowdAvoidanceParams();
            result.velocityBias = velocityBias;
            result.weightDesiredVelocity = weightDesiredVelocity;
            result.weightCurrentVelocity = weightCurrentVelocity;
            result.weightSide = weightSide;
            result.weightToi = weightToi;
            result.horizontalTime = horizontalTime;
            result.gridSize = gridSize;
            result.adaptiveDivisions = adaptiveDivisions;
            result.adaptiveRings = adaptiveRings;
            result.adaptiveDepth = adaptiveDepth;
            return result;
        }
    }
}

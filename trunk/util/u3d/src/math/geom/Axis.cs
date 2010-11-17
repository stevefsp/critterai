using System;
using System.Collections.Generic;
using System.Text;

namespace org.critterai.math.geom
{
    /// <summary>
    /// Represents a euclidean axis.
    /// </summary>
    public enum Axis
    {

        /*
         * Design note:
         * 
         * The value of each enumeration is the standard offset for
         * points.  (x, y, z)
         */

        /// <summary>
        /// X-axis
        /// </summary>
        X = 0,

        /// <summary>
        /// Y-axis
        /// </summary>
        Y = 1,

        /// <summary>
        /// Z-axis
        /// </summary>
        Z = 2
    }
}

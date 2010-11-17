using System;
using System.Collections.Generic;
using System.Text;

namespace org.critterai.nav
{
    public enum MovementState
    {
        /// <summary>
        /// Movement to the goal is complete, or movement is disabled.
        /// </summary>
        Complete,

        /// <summary>
        /// Movement is in progress.
        /// </summary>
        Processing,

        /// <summary>
        /// Movement to the goal failed in an unrecoverable way.
        /// (E.g. No path could be found, movement failed due to an
        /// unexpected obstruction, etc.)
        /// </summary>
        Failed,
    }
}

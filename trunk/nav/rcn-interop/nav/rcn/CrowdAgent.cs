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
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Provides access to detailed debug data for agents that are part of
    /// a <see cref="CrowdManager"/>.
    /// </summary>
    /// <remarks>
    /// <p>This class can only be constructed by <see cref="CrowdManager"/>
    /// objects.  It is opaque since its purpose is to provide debug data
    /// for an agent.</p>
    /// <p>Use the bulk data retrieval methods provided by 
    /// <see cref="CrowdManager"/> for normal access to agent data.</p>
    /// <p>Behavior is undefined if objects of this type are used after 
    /// disposal.</p>
    /// </remarks>
    /// <seealso cref="CrowdManager"/>
    public sealed class CrowdAgent
    {
        /*
         * Design notes:
         * 
         * The memory associated with this class' pointer is managed
         * entirely by the owner object in the external library.  
         * So no need to free it here.
         * 
         * Disposal is provided to allow the owner to indicate it has
         * disposed of the native object.
         * 
         */

        private IntPtr root;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">A pointer to an allocated native 
        /// dtCrowdAgent object.
        /// </param>
        internal CrowdAgent(IntPtr agent)
        {
            root = agent;
        }

        ~CrowdAgent()
        {
            Dispose();
        }

        /// <summary>
        /// Marks the agent as disposed.
        /// </summary>
        internal void Dispose()
        {
            root = IntPtr.Zero;
        }

        /// <summary>
        /// Indicates whether the object's resources have been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Gets the current agent state.
        /// </summary>
        /// <remarks>Behavior of this method is undefined if it is called
        /// after the object is disposed.</remarks>
        /// <param name="data">An <b>initialized</b> data structure to load
        /// the state into.</param>
        public void GetDebugData(ref CrowdAgentDebugData data)
        {
            CrowdManagerEx.GetAgentDebugData(root, ref data);
        }
    }
}

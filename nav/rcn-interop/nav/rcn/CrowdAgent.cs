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
using org.critterai.nav.rcn.externs;

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// Provides access to detailed debug data for DTCrowdManger agents.
    /// </summary>
    /// <remarks>
    /// This class is opaque since its primarily purpose is to get debug data
    /// for an agent.
    /// <p>Use the bulk data retrieval methods on the DTCrowdManager class
    /// for normal agent data access.</p>
    /// </remarks>
    public sealed class CrowdAgent
    {
        /*
         * Design notes:
         * 
         * The memory associated with this class' pointer is managed
         * entirely by the owner object in the external library.  
         * So no need to free it here.
         * 
         */

        private IntPtr root;

        internal CrowdAgent(IntPtr agent)
        {
            root = agent;
        }

        ~CrowdAgent()
        {
            Dispose();
        }

        public void Dispose()
        {
            root = IntPtr.Zero;
        }

        public bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        public void GetDebugData(ref DTCrowdAgentDebugData data)
        {
            CrowdManagerEx.GetAgentDebugData(root, ref data);
        }
    }
}

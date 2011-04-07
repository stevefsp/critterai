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

namespace org.critterai.nav.rcn
{
    /// <summary>
    /// The base class for all RCN objects that contain references to
    /// unmanaged resources.
    /// </summary>
    public abstract class ManagedObject
    {
        protected readonly AllocType resourceType;

        public ManagedObject(AllocType resourceType)
        {
            this.resourceType = resourceType;
        }

        /// <summary>
        /// Request all unmanaged resources controlled by the object be 
        /// immediately freed.
        /// </summary>
        /// <remarks>
        /// Whether or not the resources are actually freed depends on whether
        /// the resources are owned by the current object.
        /// </remarks>
        public abstract void RequestDisposal();
        public abstract bool IsDisposed { get; }
    }
}

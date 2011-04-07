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

namespace org.critterai.nav.rcn
{
    [Flags]
    public enum NavmeshStatus : uint
    {
        // High level status.
        Failure = 1u << 31,			// Operation failed.
        Sucess = 1u << 30,			// Operation succeed.
        InProgress = 1u << 29,		// Operation still in progress.

        WrongMagic = 1 << 0,		// Input data is not recognized.
        WrongVersion = 1 << 1,	// Input data is in wrong version.
        OutOfMemory = 1 << 2,	// Operation ran out of memory.
        InvalidParam = 1 << 3,	// An input parameter was invalid.
        BufferTooSmall = 1 << 4,	// Result buffer for the query was too small to store all results.
        OutOfNodes = 1 << 5,		// Query ran out of nodes during search.
        PartialResult = 1 << 6	// Query did not reach the end location, returning best guess.
    }
}

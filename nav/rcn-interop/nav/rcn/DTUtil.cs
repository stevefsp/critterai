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
    public static class DTUtil
    {
        // Returns true of status is success.
        public static bool Succeeded(DTStatus status)
        {
            return (status & DTStatus.Sucess) != 0;
        }

        // Returns true of status is failure.
        public static bool Failed(DTStatus status)
        {
            return (status & DTStatus.Failure) != 0;
        }

        // Returns true of status is in progress.
        public static bool IsInProgress(DTStatus status)
        {
            return (status & DTStatus.Progress) != 0;
        }

        // Returns true if specific detail is set.
        public static bool HasStatus(DTStatus status, DTStatus flagsToCheck)
        {
            return (status & flagsToCheck) != 0;
        }
    }
}

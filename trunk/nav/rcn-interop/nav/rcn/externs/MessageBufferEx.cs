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

namespace org.critterai.nav.rcn.externs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MessageBufferEx
    {
        public const int None = 0;
        public const int Brief = 1;
        public const int Summary = 2;
        public const int Trace = 4;

        public int messageDetail;
        public int size;
        public IntPtr buffer;

        public MessageBufferEx(int messageDetail)
        {
            this.messageDetail = messageDetail;
            buffer = IntPtr.Zero;
            size = 0;
        }

        public string[] GetMessages()
        {
            if (size == 0 || buffer == IntPtr.Zero)
                return null;

            byte[] buff = new byte[size];
            Marshal.Copy(buffer, buff, 0, size);

            string aggregateMsg =
                ASCIIEncoding.ASCII.GetString(buff);
            char[] delim = { '\0' };
            return aggregateMsg.Split(delim
                , StringSplitOptions.RemoveEmptyEntries);
        }

        [DllImport("cai-nav-rcn", EntryPoint = "rcnFreeMessageBuffer")]
        public static extern void FreeEx(ref MessageBufferEx messages);
    }
}

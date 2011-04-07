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
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn.externs
{
    public static class UtilEx
    {
        public static void Copy(IntPtr source
            , ushort[] destination
            , int length)
        {
            int byteLength = sizeof(ushort) * length;
            byte[] tmp = new byte[byteLength];
            Marshal.Copy(source, tmp, 0, byteLength);
            Buffer.BlockCopy(tmp, 0, destination, 0, byteLength);
        }

        public static void Copy(IntPtr source
            , uint[] destination
            , int length)
        {
            int byteLength = sizeof(uint) * length;
            byte[] tmp = new byte[byteLength];
            Marshal.Copy(source, tmp, 0, byteLength);
            Buffer.BlockCopy(tmp, 0, destination, 0, byteLength);
        }

        public static void Copy(ushort[] source
            , int startIndex
            , IntPtr destination
            , int length)
        {
            int size = sizeof(ushort);
            int byteLength = size * length;
            int byteStart = size * startIndex;
            byte[] tmp = new byte[byteLength];
            Buffer.BlockCopy(source, byteStart, tmp, 0, byteLength);
            Marshal.Copy(tmp, 0, destination, byteLength);
        }

        public static void Copy(uint[] source
            , int startIndex
            , IntPtr destination
            , int length)
        {
            int size = sizeof(uint);
            int byteLength = size * length;
            int byteStart = size * startIndex;
            byte[] tmp = new byte[byteLength];
            Buffer.BlockCopy(source, byteStart, tmp, 0, byteLength);
            Marshal.Copy(tmp, 0, destination, byteLength);
        }

        public static IntPtr GetBuffer(int size, bool zeroMemory)
        {
            IntPtr result = Marshal.AllocHGlobal(size);
            if (zeroMemory)
                ZeroMemory(result, size);
            return result;
        }

        public static IntPtr GetFilledBuffer(ushort[] source, int length)
        {
            int size = sizeof(ushort) * length;
            IntPtr result = Marshal.AllocHGlobal(size);
            Copy(source, 0, result, length);
            return result;
        }

        public static IntPtr GetFilledBuffer(uint[] source, int length)
        {
            int size = sizeof(uint) * length;
            IntPtr result = Marshal.AllocHGlobal(size);
            Copy(source, 0, result, length);
            return result;
        }

        public static IntPtr GetFilledBuffer(float[] source, int length)
        {
            int size = sizeof(float) * length;
            IntPtr result = Marshal.AllocHGlobal(size);
            Marshal.Copy(source, 0, result, length);
            return result;
        }

        public static IntPtr GetFilledBuffer(int[] source, int length)
        {
            int size = sizeof(int) * length;
            IntPtr result = Marshal.AllocHGlobal(size);
            Marshal.Copy(source, 0, result, length);
            return result;
        }

        public static IntPtr GetFilledBuffer(byte[] source, int length)
        {
            IntPtr result = Marshal.AllocHGlobal(length);
            Marshal.Copy(source, 0, result, length);
            return result;
        }

        public static ushort[] ExtractArrayUShort(IntPtr source, int length)
        {
            ushort[] result = new ushort[length];
            Copy(source, result, length);
            return result;
        }

        public static uint[] ExtractArrayUInt(IntPtr source, int length)
        {
            uint[] result = new uint[length];
            Copy(source, result, length);
            return result;
        }

        public static int[] ExtractArrayInt(IntPtr source, int length)
        {
            int[] result = new int[length];
            Marshal.Copy(source, result, 0, length);
            return result;
        }

        public static byte[] ExtractArrayByte(IntPtr source, int length)
        {
            byte[] result = new byte[length];
            Marshal.Copy(source, result, 0, length);
            return result;
        }

        public static float[] ExtractArrayFloat(IntPtr source, int length)
        {
            float[] result = new float[length];
            Marshal.Copy(source, result, 0, length);
            return result;
        }

        public static void ZeroMemory(IntPtr target, int size)
        {
            byte[] tmp = new byte[size];
            Marshal.Copy(tmp, 0, target, size);
        }
    }
}

/*
 * Copyright (c) 2010 Stephen A. Pratt
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace org.critterai
{
    [TestClass()]
    public class MathUtilTest
    {
        [TestMethod()]
        public void TestStaticSloppyEquals()
        {
            float tol = 0.1f;
            Assert.IsTrue(MathUtil.SloppyEquals(5, 5.09f, tol));
            Assert.IsTrue(MathUtil.SloppyEquals(5, 4.91f, tol));
            Assert.IsTrue(MathUtil.SloppyEquals(5, 5.10f, tol));
            Assert.IsTrue(MathUtil.SloppyEquals(5, 4.90f, tol));
            Assert.IsFalse(MathUtil.SloppyEquals(5, 5.101f, tol));
            Assert.IsFalse(MathUtil.SloppyEquals(5, 4.899f, tol));
        }

        [TestMethod()]
        public void TestStaticMin()
        {
            Assert.IsTrue(MathUtil.Min(2) == 2);
            Assert.IsTrue(MathUtil.Min(-1, 0, 1, 2) == -1);
            Assert.IsTrue(MathUtil.Min(2, 2, -1, 0) == -1);
        }
        [TestMethod()]
        public void TestStaticMax()
        {
            Assert.IsTrue(MathUtil.Max(2) == 2);
            Assert.IsTrue(MathUtil.Max(-1, 0, 1, 2) == 2);
            Assert.IsTrue(MathUtil.Max(-1, 2, -1, 0) == 2);
        }

        [TestMethod()]
        public void TestStaticClampToPositiveNonZero()
        {
            Assert.IsTrue(MathUtil.ClampToPositiveNonZero(0.1f) == 0.1f);
            Assert.IsTrue(MathUtil.ClampToPositiveNonZero(float.Epsilon) 
                == float.Epsilon);
            Assert.IsTrue(MathUtil.ClampToPositiveNonZero(0) == float.Epsilon);
            Assert.IsTrue(MathUtil.ClampToPositiveNonZero(-float.Epsilon) 
                == float.Epsilon);
        }

        [TestMethod()]
        public void TestStaticClampInt()
        {
            Assert.IsTrue(MathUtil.Clamp(5, 4, 6) == 5);
            Assert.IsTrue(MathUtil.Clamp(4, 4, 6) == 4);
            Assert.IsTrue(MathUtil.Clamp(3, 4, 6) == 4);
            Assert.IsTrue(MathUtil.Clamp(6, 4, 6) == 6);
            Assert.IsTrue(MathUtil.Clamp(7, 4, 6) == 6);
        }

        [TestMethod]
        public void CleanFlagsULong()
        {
            ulong flag = 0xffffffff;

            ulong expected = 0x00000000000fffff;
            ulong actual = MathUtil.CleanBits(flag, 20);
            Assert.IsTrue(actual == expected);

            flag = 0xffffffffffffffff;
            expected = flag;
            actual = MathUtil.CleanBits(flag, 64);
            Assert.IsTrue(actual == expected);

            flag = 0xffffff0fffffffff;
            expected = flag;
            actual = MathUtil.CleanBits(flag, 68);
            Assert.IsTrue(actual == expected);

            flag = 0xffffffffffffffff;
            expected = 0;
            actual = MathUtil.CleanBits(flag, -1);
            Assert.IsTrue(actual == expected);

            flag = 0xffffffffffffffff;
            expected = 0;
            actual = MathUtil.CleanBits(flag, 0);
            Assert.IsTrue(actual == expected);

            flag = 0xffffffffffffffff;
            expected = 3;
            actual = MathUtil.CleanBits(flag, 2);
            Assert.IsTrue(actual == expected);
        }

        [TestMethod]
        public void CleanFlagsUInt()
        {
            // Not performing full tests based on knowledge that
            // this function calls the ulong version.

            uint flag = 0xffffffff;
            uint expected = 0x00ffffff;
            uint actual = MathUtil.CleanBits(flag, 24);
            Assert.IsTrue(actual == expected);

            flag = 0xfffff0ff;
            expected = flag;
            actual = MathUtil.CleanBits(flag, 33);
            Assert.IsTrue(actual == expected);
        }

        [TestMethod]
        public void CleanFlagsUShort()
        {
            // Not performing full tests based on knowledge that
            // this function calls the ulong version.

            ushort flag = 0xffff;
            ushort expected = 0x0fff;
            ushort actual = MathUtil.CleanBits(flag, 12);
            Assert.IsTrue(actual == expected);

            flag = 0xf0ff;
            expected = flag;
            actual = MathUtil.CleanBits(flag, 17);
            Assert.IsTrue(actual == expected);
        }

        [TestMethod]
        public void CleanFlagsByte()
        {
            // Not performing full tests based on knowledge that
            // this function calls the ulong version.

            byte flag = 0xff;
            byte expected = 0x0f;
            byte actual = MathUtil.CleanBits(flag, 4);
            Assert.IsTrue(actual == expected);

            flag = 0xf7;
            expected = flag;
            actual = MathUtil.CleanBits(flag, 9);
            Assert.IsTrue(actual == expected);
        }
    }
}

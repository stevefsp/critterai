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
    public class UIntPack4Test
    {
        // Design note: Values must not repeat.
        private const uint slotValue0 = 12;
        private const uint slotValue1 = 8;
        private const uint slotValue2 = 1;
        private const uint slotValue3 = 14;
        private const uint slotValue4 = 2;
        private const uint slotValue5 = 10;
        private const uint slotValue6 = 6;
        private const uint slotValue7 = 9;

        // Design note: Must not ot equal to any slot value.
        private const uint stdValue = 3;

        private readonly uint[] slotValue = new uint[8];

        [TestInitialize()]
        public void Setup()
        {
            ResetSlotArray();
        }

        [TestMethod()]
        public void TestStaticSetGetClean()
        {
            for (int i = 0; i < 8; i++)
            {
                uint pi = 0;
                UIntPack4.Set(ref pi, i, slotValue[i]);
                Assert.IsTrue(UIntPack4.Get(pi, i) == slotValue[i]);
            }
        }

        [TestMethod()]
        public void TestStaticSetGetDirty()
        {
            uint pki = GetFullPack();
            for (int i = 0; i < 8; i++)
            {
                Assert.IsTrue(UIntPack4.Get(pki, i) == slotValue[i]);
            }
        }

        [TestMethod()]
        public void TestStaticSetGetMaxPack()
        {
            uint pki = 0;
            for (int i = 0; i < 8; i++)
            {
                UIntPack4.Set(ref pki, i, 0xfU);
            }
            for (int i = 0; i < 8; i++)
            {
                Assert.IsTrue(UIntPack4.Get(pki, i) == 0xfU);
            }
        }

        [TestMethod()]
        public void TestStaticZeroSlot()
        {
            for (int i = 0; i < 8; i++)
            {
                ResetSlotArray();
                uint pki = GetFullPack();
                UIntPack4.Zero(ref pki, i);
                slotValue[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                    Assert.IsTrue(UIntPack4.Get(pki, j) == slotValue[j]);
                }
            }
        }

        [TestMethod()]
        public void TestStaticGetFirst()
        {
            Assert.IsTrue(UIntPack4.GetFirst(GetFullPack()) == slotValue[0]);
        }

        [TestMethod()]
        public void TestStaticRemoveFirst()
        {
            uint pki = GetFullPack();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    uint actual = UIntPack4.Get(pki, j);
                    Assert.IsTrue(actual == slotValue[j]
                        , "Actual: " + actual + ", Expected: " + slotValue[j]);
                }
                UIntPack4.RemoveFirst(ref pki);
                Assert.IsTrue(UIntPack4.Get(pki, 7) == 0);
                DequeSlotArray();
            }
            Assert.IsTrue(pki == 0);
        }

        [TestMethod()]
        public void TestStaticFirstEmptySlot()
        {
            uint pki = GetFullPack();
            Assert.IsTrue(UIntPack4.FirstEmptySlot(pki) == -1);
            for (int i = 7; i > -1; i--)
            {
                UIntPack4.Zero(ref pki, i);
                Assert.IsTrue(UIntPack4.FirstEmptySlot(pki) == i);
            }
        }

        [TestMethod()]
        public void TestStaticSetFirstEmpty()
        {
            uint pki = GetFullPack();
            UIntPack4.SetFirstEmpty(ref pki, stdValue); 
            Assert.IsTrue(pki == GetFullPack());
            for (int i = 0; i < 8; i++)
            {
                ResetSlotArray();
                pki = GetFullPack();
                UIntPack4.Zero(ref pki, i);
                slotValue[i] = stdValue;
                UIntPack4.SetFirstEmpty(ref pki, stdValue);
                for (int j = 0; j < 8; j++)
                {
                    uint actual = UIntPack4.Get(pki, j);
                    Assert.IsTrue(actual == slotValue[j]
                        , "Actual: " + actual + ", Expected: " + slotValue[j]);
                }
            }
        }

        private uint GetFullPack()
        {
            uint pki = 0;
            for (int i = 0; i < 8; i++)
            {
                UIntPack4.Set(ref pki, i, slotValue[i]);
            }
            return pki;
        }

        private void DequeSlotArray()
        {
            for (int i = 1; i < 8; i++)
            {
                slotValue[i - 1] = slotValue[i];
            }
            slotValue[7] = 0;
        }

        private void ResetSlotArray()
        {
            slotValue[0] = slotValue0;
            slotValue[1] = slotValue1;
            slotValue[2] = slotValue2;
            slotValue[3] = slotValue3;
            slotValue[4] = slotValue4;
            slotValue[5] = slotValue5;
            slotValue[6] = slotValue6;
            slotValue[7] = slotValue7;
        }
    }
}

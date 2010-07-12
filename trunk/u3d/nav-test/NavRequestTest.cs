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
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace org.critterai.nav
{
    /// <summary>
    /// Summary description for NavRequestTest
    /// </summary>
    [TestClass]
    public sealed class NavRequestTest
    {
 
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void TestPrimaryGetters()
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>();
            MasterNavRequest<Boolean>.NavRequest nr = mnr.Request;
            Assert.IsTrue(mnr.Data == nr.Data);
            Assert.IsTrue(mnr.State == nr.State);
            mnr.Set(NavRequestState.Complete, true);
            Assert.IsTrue(mnr.Data == nr.Data);
            Assert.IsTrue(mnr.State == nr.State);
        }

        [TestMethod]
        public void TestIsComplete()
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>(NavRequestState.Processing);
            MasterNavRequest<Boolean>.NavRequest nr = mnr.Request;
            Assert.IsTrue(nr.IsFinished == false);
            mnr.State = NavRequestState.Failed;
            Assert.IsTrue(nr.IsFinished == true);
            mnr.State = NavRequestState.Complete;
            Assert.IsTrue(nr.IsFinished == true);
        }

    }
}

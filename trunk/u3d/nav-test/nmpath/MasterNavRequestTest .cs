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

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Summary description for MasterNavRequestTest
    /// </summary>
    [TestClass]
    public sealed class MasterNavRequestTest
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
        public void TestConstructorDefault() 
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>();
            Assert.IsTrue(mnr.State == NavRequestState.Processing);
        }

        [TestMethod]
        public void TestConstructorState() 
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>(NavRequestState.Failed);
            Assert.IsTrue(mnr.State == NavRequestState.Failed);
        }

        [TestMethod]
        public void TestConstructorFull() 
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>(NavRequestState.Complete, true);
            Assert.IsTrue(mnr.Data == true);
            Assert.IsTrue(mnr.State == NavRequestState.Complete);
        }

        [TestMethod]
        public void TestGetRequest() 
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>(NavRequestState.Complete, true);
            Assert.IsTrue(mnr.Request == mnr.Request);  // Same ref on multiple calls.
        }

        [TestMethod]
        public void TestSetData() 
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>(NavRequestState.Complete, true);
            Assert.IsTrue(mnr.Data == true);
            mnr.SetData(false);
            Assert.IsTrue(mnr.Data == false);
        }

        [TestMethod]
        public void TestSetState() 
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>();
            Assert.IsTrue(mnr.State == NavRequestState.Processing);
            mnr.State = NavRequestState.Failed;
            Assert.IsTrue(mnr.State == NavRequestState.Failed);
        }

        [TestMethod]
        public void TestSet() 
        {
            MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>();
            Assert.IsTrue(mnr.State == NavRequestState.Processing);
            mnr.Set(NavRequestState.Complete, true);
            Assert.IsTrue(mnr.State == NavRequestState.Complete);
            Assert.IsTrue(mnr.Data == true);
        }

    }
}

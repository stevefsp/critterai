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
using System.Collections.Generic;

namespace org.critterai.nmgen
{
    public sealed class ProcessorSet
    {
        private readonly IHFProcessor[] mHFS;
        private readonly ICHFProcessor[] mCHFS;
        private readonly IPMProcessor[] mPMS;
        private readonly IDMProcessor[] mDMS;
        private readonly bool mIsThreadSafe;

        public int HFCount { get { return mHFS.Length; } }
        public int CHFCount { get { return mCHFS.Length; } }
        public int PMCount { get { return mPMS.Length; } }
        public int DMCount { get { return mDMS.Length; } }
        public bool IsThreadSafe { get { return mIsThreadSafe; } }

        private ProcessorSet(IHFProcessor[] hfs
            , ICHFProcessor[] chfs
            , IPMProcessor[] pms
            , IDMProcessor[] dms) 
        {
            this.mHFS = hfs;
            this.mCHFS = chfs;
            this.mPMS = pms;
            this.mDMS = dms;

            foreach (IHFProcessor p in mHFS)
            {
                if (!p.IsThreadSafe)
                {
                    mIsThreadSafe = false;
                    break;
                }
            }

            if (!mIsThreadSafe)
                return;

            foreach (ICHFProcessor p in mCHFS)
            {
                if (!p.IsThreadSafe)
                {
                    mIsThreadSafe = false;
                    break;
                }
            }

            if (!mIsThreadSafe)
                return;

            foreach (IPMProcessor p in mPMS)
            {
                if (!p.IsThreadSafe)
                {
                    mIsThreadSafe = false;
                    break;
                }
            }

            if (!mIsThreadSafe)
                return;

            foreach (IDMProcessor p in mDMS)
            {
                if (!p.IsThreadSafe)
                {
                    mIsThreadSafe = false;
                    break;
                }
            }
        }

        private ProcessorSet()
        {
            mHFS = new IHFProcessor[0];
            mCHFS = new ICHFProcessor[0];
            mPMS = new IPMProcessor[0];
            mDMS = new IDMProcessor[0];
            mIsThreadSafe = true;
        }

        public IHFProcessor GetHF(int index)
        {
            return mHFS[index];
        }

        public ICHFProcessor GetCHF(int index)
        {
            return mCHFS[index];
        }

        public IPMProcessor GetPM(int index)
        {
            return mPMS[index];
        }

        public IDMProcessor GetDM(int index)
        {
            return mDMS[index];
        }

        public static ProcessorSet CreateEmpty()
        {
            return new ProcessorSet();
        }

        public static ProcessorSet Create(IHFProcessor[] hfs
            , ICHFProcessor[] chfs
            , IPMProcessor[] pms
            , IDMProcessor[] dms)
        {
            IHFProcessor[] nhfs = 
                (hfs == null) ?  new IHFProcessor[0] : ArrayUtil.Compress(hfs);
            if (nhfs == hfs)
                nhfs = (IHFProcessor[])hfs.Clone();

            ICHFProcessor[] nchfs =
                (chfs == null) ? new ICHFProcessor[0] : ArrayUtil.Compress(chfs);
            if (nchfs == chfs)
                nchfs = (ICHFProcessor[])chfs.Clone();

            IPMProcessor[] npms =
                (pms == null) ? new IPMProcessor[0] : ArrayUtil.Compress(pms);
            if (npms == pms)
                npms = (IPMProcessor[])pms.Clone();

            IDMProcessor[] ndms =
                (dms == null) ? new IDMProcessor[0] : ArrayUtil.Compress(dms);
            if (ndms == dms)
                ndms = (IDMProcessor[])dms.Clone();


            return new ProcessorSet(nhfs, nchfs, npms, ndms);
        }

        internal static ProcessorSet UnsafeCreate(IHFProcessor[] hfs
            , ICHFProcessor[] chfs
            , IPMProcessor[] pms
            , IDMProcessor[] dms)
        {
            return new ProcessorSet(hfs, chfs, pms, dms);
        }
    }
}

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
        private readonly IHFProcessor[] hfs;
        private readonly ICHFProcessor[] chfs;
        private readonly IPMProcessor[] pms;
        private readonly IDMProcessor[] dms;

        public int HFCount { get { return hfs.Length; } }
        public int CHFCount { get { return chfs.Length; } }
        public int PMCount { get { return pms.Length; } }
        public int DMCount { get { return dms.Length; } }

        public IHFProcessor GetHF(int index)
        {
           return hfs[index];
        }

        public ICHFProcessor GetCHF(int index)
        {
            return chfs[index];
        }

        public IPMProcessor GetPM(int index)
        {
            return pms[index];
        }

        public IDMProcessor GetDM(int index)
        {
            return dms[index];
        }

        private ProcessorSet(IHFProcessor[] hfs
            , ICHFProcessor[] chfs
            , IPMProcessor[] pms
            , IDMProcessor[] dms) 
        {
            this.hfs = hfs;
            this.chfs = chfs;
            this.pms = pms;
            this.dms = dms;
        }

        private ProcessorSet()
        {
            hfs = new IHFProcessor[0];
            chfs = new ICHFProcessor[0];
            pms = new IPMProcessor[0];
            dms = new IDMProcessor[0];
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

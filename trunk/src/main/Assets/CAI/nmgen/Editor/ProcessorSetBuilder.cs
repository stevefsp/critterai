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
	public sealed class ProcessorSetBuilder
	{
        public readonly List<IHFProcessor> hfs;
        public readonly List<ICHFProcessor> chfs;
        public readonly List<IPMProcessor> pms;
        public readonly List<IDMProcessor> dms;

        public ProcessorSetBuilder()
        {
            hfs = new List<IHFProcessor>();
            chfs = new List<ICHFProcessor>();
            pms = new List<IPMProcessor>();
            dms = new List<IDMProcessor>();
        }

        public ProcessorSet GetProcessorSet()
        {
            return ProcessorSet.UnsafeCreate(
                ArrayUtil.Compress(hfs.ToArray())
                , ArrayUtil.Compress(chfs.ToArray())
                , ArrayUtil.Compress(pms.ToArray())
                , ArrayUtil.Compress(dms.ToArray()));
        }

        public void Reset()
        {
            hfs.Clear();
            chfs.Clear();
            pms.Clear();
            dms.Clear();
        }
	}
}

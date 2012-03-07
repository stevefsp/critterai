/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using org.critterai.nmgen;

namespace org.critterai.nmbuild
{
    public sealed class ProcessorSet
    {
        private readonly bool mIsThreadSafe;
        private readonly INMGenProcessor[] mProcessors;
        private readonly NMGenAssetFlag mPreserveAssets;

        public NMGenAssetFlag PreserveAssets { get { return mPreserveAssets; } } 
        public bool IsThreadSafe { get { return mIsThreadSafe; } }

        public int Count { get { return mProcessors.Length; } }

        private ProcessorSet(INMGenProcessor[] processors) 
        {
            mProcessors = processors;

            PriorityComparer<INMGenProcessor> comp = 
                new PriorityComparer<INMGenProcessor>(true);

            System.Array.Sort(mProcessors, comp);

            mIsThreadSafe = true;  // This is correct.

            foreach (INMGenProcessor p in mProcessors)
            {
                mPreserveAssets |= p.PreserveAssets;

                if (!p.IsThreadSafe)
                    mIsThreadSafe = false;
            }
        }

        /// <remarks>
        /// <para>A return value of false indicates the build should be aborted.</para>
        /// <para>The caller is expected to check and handle 
        /// <see cref="TileBuildIntermediates.NoResult"/> on <paramref name="item"/> return.
        /// </para>
        /// <para>The processor may set intermeidates to a new object, but will never set
        /// one to null.  The caller can treat unexpected null's as an error on the processors part.
        /// </para>
        /// </remarks>
        public bool Process(NMGenState state, NMGenContext logger)
        {
            foreach (INMGenProcessor p in mProcessors)
            {
                if (!p.ProcessBuild(state, logger))
                    return false;
            }
            return true;
        }

        public void LogProcessors(NMGenContext context)
        {
            foreach (INMGenProcessor p in mProcessors)
            {
                context.Log(string.Format("Processor: {0} ({1})", p.Name, p.GetType().Name)
                    , this);
            }
        }

        public static INMGenProcessor[] GetStandard(NMGenFlag options)
        {
            List<INMGenProcessor> ps = new List<INMGenProcessor>();

            if ((options & NMGenFlag.ApplyPolyFlags) != 0)
            {
                ps.Add(new ApplyPolygonFlags("ApplyDefaultPolyFlag"
                    , CAIUtil.MinPriority, NMGen.DefaultFlag));
            }

            if ((options & NMGenFlag.LedgeSpansNotWalkable) != 0)
                ps.Add(FilterLedgeSpans.Instance);

            if ((options & NMGenFlag.LowHeightSpansNotWalkable) != 0)
                ps.Add(FilterLowHeightSpans.Instance);

            if ((options & NMGenFlag.LowObstaclesWalkable) != 0)
                ps.Add(LowObstaclesWalkable.Instance);

            return ps.ToArray();
        }

        public static ProcessorSet CreateStanard(NMGenFlag options)
        {
            return Create(GetStandard(options));
        }

        // Will return null if there are no processors.
        public static ProcessorSet Create(INMGenProcessor[] processors)
        {
            INMGenProcessor[] lprocessors = ArrayUtil.Compress(processors);

            if (lprocessors == null)
                lprocessors = new INMGenProcessor[0];
            else if (lprocessors == processors)
                lprocessors = (INMGenProcessor[])processors.Clone();

            return new ProcessorSet(lprocessors);
        }
    }
}

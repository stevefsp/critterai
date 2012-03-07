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
using org.critterai.nmgen;
using org.critterai.nav;

namespace org.critterai.nmbuild
{
    public sealed class NMGenTask
        : BuildTask<NMGenAssets>
    {
        private readonly IncrementalBuilder mBuilder;

        public override bool IsThreadSafe { get { return mBuilder.IsThreadSafe; }}

        public int TileX { get { return mBuilder.TileX; } }
        public int TileZ { get { return mBuilder.TileZ; } }

        private NMGenTask(IncrementalBuilder builder, int priority)
            : base(priority)
        {
            mBuilder = builder;
        }

        public static NMGenTask Create(IncrementalBuilder builder, int priority)
        {
            if (builder == null || builder.IsFinished)
                return null;

            return new NMGenTask(builder, priority);
        }

        protected override bool LocalRun()
        {
            mBuilder.Build();
            return !mBuilder.IsFinished;  // Go to false when IsFinished.
        }

        protected override bool GetResult(out NMGenAssets result)
        {
            AddMessages(mBuilder.GetMessages());

            switch (mBuilder.State)
            {
                case NMGenState.Complete:

                    result = mBuilder.Result;
                    return true;

                case NMGenState.NoResult:

                    result = new NMGenAssets(mBuilder.TileX, mBuilder.TileZ, null, null);
                    return true;

                default:

                    result = new NMGenAssets();
                    return false;
            }
        }
    }
}

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
using UnityEditor;
using UnityEngine;

namespace org.critterai.nmbuild.u3d
{
    public class UnityBuildContext
        : BuildContext
    {
        internal const string TraceKey = "org.critterai.nmbuild.TraceMessages";

        private static bool mTraceEnabled;

        public static bool TraceEnabled
        {
            get { return mTraceEnabled; }
            set
            {
                if (mTraceEnabled != value)
                {
                    mTraceEnabled = value;
                    EditorPrefs.SetBool(TraceKey, mTraceEnabled);
                }
            }
        }

        public void PostError(string summary, Object context)
        {
            Debug.LogError(string.Format("{0}\n{1}"
                    , summary
                    , GetMessagesFlat())
                , context);
            ResetLog();
        }

        public void PostError(string summary, string[] messages, Object context)
        {
            Log(messages);
            PostError(summary, context);
        }

        public void PostTrace(string summary, Object context)
        {
            if (mTraceEnabled)
            {
                Debug.Log(string.Format("{0}\n{1}"
                        , summary
                        , GetMessagesFlat())
                    , context);
            }
            ResetLog();
        }

        public void PostTrace(string summary, string[] messages, Object context)
        {
            if (mTraceEnabled)
            {
                Log(messages);
                PostTrace(summary, context);
            }
        }
    }
}

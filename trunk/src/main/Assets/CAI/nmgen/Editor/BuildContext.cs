/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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
using System.Text;
using org.critterai.interop;
using org.critterai.nmgen.rcn;

namespace org.critterai.nmgen
{
    /// <summary>
    /// Provides logging functionality.
    /// </summary>
    /// <remarks>
    /// <para>The message buffer can hold a maximum of 1000 messages
    /// comprised of 64K characters.  Any messages added after the
    /// buffer limit is reached will be ignored.</para>
    /// </remarks>
    public class BuildContext
    {
        /*
         * Design notes:
         * 
         * File name mismatch is needed for Unity support.
         *
         */

        public const string InfoLabel = "INFO";
        public const string ErrorLabel = "ERROR";
        public const string WarningLabel = "WARNING";

        private const int MessagePoolSize = 65536;

        internal IntPtr root = IntPtr.Zero;

        /// <summary>
        /// The number of messages in the buffer.
        /// </summary>
        public int MessageCount
        {
            get { return BuildContextEx.nmbcGetMessageCount(root); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public BuildContext()
        {
            root = BuildContextEx.nmbcAllocateContext(true);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~BuildContext()
        {
            if (root != IntPtr.Zero)
            {
                BuildContextEx.nmbcFreeContext(root);
                root = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Clears all messages from the message buffer.
        /// </summary>
        public void ResetLog()
        {
            BuildContextEx.nmbcResetLog(root);
        }

        /// <summary>
        /// Posts a message to the message buffer.
        /// </summary>
        /// <param name="message">The message to post.</param>
        public void Log(string message, Object context)
        {
            Log(InfoLabel, message, context);
        }

        public void Log(string[] messages)
        {
            if (messages == null || messages.Length == 0)
                return;

            foreach (string msg in messages)
            {
                Log(msg, null);
            }
        }

        public void Log(string category, string message, Object context)
        {
            if (message != null && message.Length > 0)
            {
                BuildContextEx.nmbcLog(root, string.Format("{0}: {1}{2}"
                    , category, message, ContextPart(context)));
            }
        }

        public void LogWarning(string message, Object context)
        {
            Log(WarningLabel, message, context);
        }

        public void LogError(string message, Object context)
        {
            Log(ErrorLabel, message, context);
        }

        /// <summary>
        /// Gets all messages in the message buffer.
        /// </summary>
        /// <remarks>
        /// The length of the result will always equal 
        /// <see cref="MessageCount"/>.
        /// </remarks>
        /// <returns>All messages in the message buffer, or a zero length
        /// array if there are no messages.</returns>
        public string[] GetMessages()
        {
            byte[] buffer = new byte[MessagePoolSize];

            int messageCount = BuildContextEx.nmbcGetMessagePool(root
                , buffer
                , buffer.Length);

            if (messageCount == 0)
                return new string[0];

            string aggregateMsg =
                ASCIIEncoding.ASCII.GetString(buffer);
            char[] delim = { '\0' };
            return aggregateMsg.Split(delim
                , StringSplitOptions.RemoveEmptyEntries);
        }

        public string GetMessagesFlat()
        {
            string[] msgs = GetMessages();

            if (msgs.Length == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            foreach (string msg in msgs)
            {
                sb.AppendLine(msg);
            }

            return sb.ToString();
        }

        public void AppendMessages(BuildContext fromContext)
        {
            if (fromContext == null || fromContext.MessageCount == 0)
                return;

            string[] msgs = fromContext.GetMessages();
            foreach (string msg in msgs)
            {
                BuildContextEx.nmbcLog(root, msg);
            }
        }

        private static string ContextPart(Object context)
        {
            return (context == null ? "" : " (" + context.GetType().Name + ")");
        }

        /// <summary>
        /// Tests the operation of the context by adding test messages
        /// </summary>
        /// <param name="context">The context to test.</param>
        /// <param name="count">The number of messages to add. 
        /// (Limit &lt;100)</param>
        public static void LoadTestMessages(BuildContext context, int count)
        {
            if (context != null)
                BuildContextEx.nmgTestContext(context.root, Math.Min(100, count));
        }

    }
}

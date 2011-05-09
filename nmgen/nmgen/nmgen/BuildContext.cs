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
    /// <p>The message buffer can hold a maximum of 1000 messages
    /// comprised of 12,000 characters.  Any messages added after the
    /// buffer limit is reached will be ignored.</p>
    /// <p>Behavior is undefined if an object is used after disposal.</p>
    /// </remarks>
    public sealed class BuildContext
        : IManagedObject
    {
        // TODO: EVAL: Allow a 'null' version that that has no buffers
        // allocated and will ever log anything.

        internal IntPtr root = IntPtr.Zero;

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// TRUE if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (root == IntPtr.Zero); } }

        /// <summary>
        /// Enableds/Disables logging functionality.
        /// </summary>
        public bool LogEnabled
        {
            get 
            {
                if (IsDisposed)
                    return false;
                return BuildContextEx.GetLogEnabled(root); 
            }
            set 
            {
                if (!IsDisposed)
                    BuildContextEx.SetLogEnabled(root, value); 
            }
        }

        /// <summary>
        /// The number of messages in the buffer.
        /// </summary>
        public int MessageCount
        {
            get 
            {
                if (IsDisposed)
                    return 0;
                return BuildContextEx.GetMessageCount(root); 
            }
        }

        ~BuildContext()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Frees all resources and marks the object as disposed.
        /// </summary>
        public void RequestDisposal()
        {
            if (root != IntPtr.Zero)
            {
                BuildContextEx.FreeEx(root);
                root = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Clears all messages from the message buffer.
        /// </summary>
        public void ResetLog()
        {
            if (!IsDisposed)
                BuildContextEx.ResetLog(root);
        }

        /// <summary>
        /// Posts a message to the message buffer.
        /// </summary>
        /// <param name="message">The message to post.</param>
        public void Log(string message)
        {
            if (!IsDisposed && message != null && message.Length > 0)
                BuildContextEx.Log(root, message);
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
            if (IsDisposed)
                return null;

            byte[] buffer = new byte[12000];

            int messageCount = BuildContextEx.GetMessagePool(root
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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logEnabled">The initial logging state.</param>
        public BuildContext(bool logEnabled)
        {
            root = BuildContextEx.Alloc(logEnabled);
        }

        /// <summary>
        /// Tests the operation of the context by adding test messages
        /// </summary>
        /// <param name="context">The context to test.</param>
        /// <param name="count">The number of messages to add. 
        /// (Limit &lt;100)</param>
        public static void LoadTestMessages(BuildContext context, int count)
        {
            if (context != null && !context.IsDisposed)
                BuildContextEx.TestContext(context.root, Math.Min(100, count));
        }
    }
}

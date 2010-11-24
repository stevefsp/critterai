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

/// <summary>
/// Defines the detail to use when generating Unity Editor debug messages.
/// </summary>
public enum CAIMessageStyle
{
    /*
     * Design notes:
     * 
     * This enumeration is kept free of Unity references in order to permit
     * dual use in .NET.
     */

    /// <summary>
    /// No messages should be sent to the console.
    /// </summary>
    None = 0,

    /// <summary>
    /// Send only minimal messages, usually only errors and warnings,
    /// to the console.
    /// </summary>
    Brief = 1, 

    /// <summary>
    /// Send only summary messages to the console.
    /// </summary>
    Summary = 2,

    /// <summary>
    /// Send detailed messages suitable for tracing the progress of an operation
    /// to the console.
    /// (If available.)
    /// </summary>
    Trace = 3
}

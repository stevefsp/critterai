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
/// Provides various constants and utility methods for CAI classes.
/// </summary>
public static class CAIUtil
{
    /// <summary>
    /// Converts DateTime ticks to seconds. 
    /// (ticks * value = seconds)
    /// </summary>
    public const float TicksToSec = 1E-7f;

    /// <summary>
    /// Converts DateTime ticks to milliseconds.
    /// (ticks * value = ms)
    /// </summary>
    public const float TicksToMS = 1E-4f;

    /// <summary>
    /// Converts seconds to DateTime ticks.
    /// (seconds * value = ticks)
    /// </summary>
    public const int SecToTicks = 10000000;

    /// <summary>
    /// Converts milliseconds to DateTime ticks.
    /// (ms * value = ticks)
    /// </summary>
    public const int MSToTicks = 10000;

    /// <summary>
    /// Gets the number of milliseconds since the start ticks to now.
    /// </summary>
    /// <remarks>Useful for performance tracking.</remarks>
    /// <param name="start">The start time in DateTime ticks.</param>
    /// <returns>The number of milliseconds since the start tick.</returns>
    public static int GetNowDeltaMS(long start)
    {
        return (int)((System.DateTime.Now.Ticks - start) * TicksToMS);
    }

    /// <summary>
    /// Gets the number of seconds since the start ticks to now.
    /// </summary>
    /// <remarks>Useful for performance tracking.</remarks>
    /// <param name="start">The start time in DateTime ticks.</param>
    /// <returns>The number of seconds since the start tick.</returns>
    public static int GetNowDeltaSec(long start)
    {
        return (int)((System.DateTime.Now.Ticks - start) * TicksToSec);
    }
}

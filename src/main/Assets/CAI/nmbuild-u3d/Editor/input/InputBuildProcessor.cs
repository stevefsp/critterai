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
using org.critterai;
using org.critterai.nmbuild.u3d.editor;
using UnityEngine;

/// <summary>
/// A processor used build input for an NMGen build.
/// </summary>
/// <remarks>
/// <para>Processors are called during every step of the input build in an ascending order.</para>
/// <para></para>
/// </remarks>
public abstract class InputBuildProcessor
    : ScriptableObject, IPriorityItem
{
    /// <summary>
    /// Processes the context.
    /// </summary>
    /// <param name="state">The current state of the input build.</param>
    /// <param name="context">The input context to process.</param>
    /// <returns>False if the input build should abort.</returns>
    public abstract bool ProcessInput(InputBuildState state, InputBuildContext context);

    /// <summary>
    /// The priority of the processor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Processors are called in ascending order.
    /// </para>
    /// </remarks>
    public abstract int Priority { get; }

    /// <summary>
    /// True if the multiple processors of the same type can be included in a build.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this property is false, the input builder will discard duplicate processors of the
    /// same type.  Which duplicate is discarded is undefined.
    /// </para>
    /// <para>This restricts same type processors only.  The input builder never accepts duplicate
    /// objects.</para>
    /// </remarks>
    public abstract bool DuplicatesAllowed { get; } 
}

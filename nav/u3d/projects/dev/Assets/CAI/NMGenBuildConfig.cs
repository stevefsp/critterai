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
using UnityEngine;
using org.critterai.nmgen.u3d;

/// <summary>
/// A configuration used to build polygon mesh data.
/// </summary>
/// <remarks>
/// <p>The primary purpose of this class is for editing configurations
/// in the Unity Editor.</p>
/// </remarks>
[System.Serializable]
[AddComponentMenu("CAI/NMGen Build Config")]
public sealed class NMGenBuildConfig
    : MonoBehaviour
{
    /// <summary>
    /// The configuation to use.
    /// </summary>
    [SerializeField]
    public NMGenBuildParams config = new NMGenBuildParams();

    /// <summary>
    /// A clone of the configuration.
    /// </summary>
    /// <returns>A clone of the configuration.</returns>
    public NMGenBuildParams GetBuildConfig()
    {
        return config.Clone();
    }
}

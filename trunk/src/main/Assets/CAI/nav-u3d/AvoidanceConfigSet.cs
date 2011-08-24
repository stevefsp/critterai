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
using org.critterai.nav;

/// <summary>
/// A set of <see cref="CrowdAvoidanceParams"/> configurations to be used
/// with a <see cref="CrowdManager"/>.
/// </summary>
[AddComponentMenu("CAI/Avoidance Config Set")]
public class AvoidanceConfigSet 
    : MonoBehaviour 
{
    /*
     * Design notes:
     * 
     * Purposefully letting runtime exceptions be thrown on invalid indices.
     * 
     * Considered linking this directly to a CrowdManager object.  But
     * CrowdManager objects are not serializable.  So they can't exist until 
     * runtime.
     * 
     */

    /// <summary>
    /// The maximum permitted avoidance configurations.
    /// </summary>
    /// <remarks><para>Based on the maximum number of configurations permited
    /// for <see cref="CrowdManager"/> objects.</para></remarks>
    public const int MaxCount = CrowdManager.MaxAvoidanceParams;

    /// <summary>
    /// The name used configurations that are not considered to be in-use.
    /// </summary>
    public const string DefaultName = "Unused";

    [SerializeField]
    private string[] mNames;

    [SerializeField]
    private CrowdAvoidanceParams[] mConfigs;

    /// <summary>
    /// Constructor.  (DO NOT USE DIRECLTY IN CODE.)
    /// </summary>
    /// <remarks>
    /// <para>This constructor is provided to work around a Unity Editor
    /// peculiarity. It is not meant for use in script code.</para></remarks>
    public AvoidanceConfigSet()
    {
        Reset();
    }

    /// <summary>
    /// A reference to the configuration at the specified index.
    /// </summary>
    /// <remarks>
    /// <para>Attempting to set a configuration to null will have no effect.</para>
    /// <para>References are stored.  No cloning occurs.</para>
    /// </remarks>
    /// <param name="index">The index of the configuration.</param>
    /// <returns></returns>
    /// <value>A reference to a configuration.</value>
    public CrowdAvoidanceParams this[int index]
    {
        get { return mConfigs[index]; }
        set
        {
            if (value != null)
                mConfigs[index] = value;
        }
    }

    /// <summary>
    /// Restores the object to its default state, releasing all
    /// references to existing configurations.
    /// </summary>
    public void Reset()
    {
        mNames = new string[MaxCount];
        mConfigs = new CrowdAvoidanceParams[MaxCount];

        for (int i = 0; i < MaxCount; i++)
        {
            mNames[i] = DefaultName;
            mConfigs[i] = new CrowdAvoidanceParams();
        }

        mNames[0] = "Low";
        mConfigs[0].velocityBias = 0.5f;
        mConfigs[0].adaptiveDivisions = 5;
        mConfigs[0].adaptiveRings = 2;
        mConfigs[0].adaptiveDepth = 1;

        mNames[1] = "Medium";
        mConfigs[1].velocityBias = 0.5f;
        mConfigs[1].adaptiveDivisions = 5;
        mConfigs[1].adaptiveRings = 2;
        mConfigs[1].adaptiveDepth = 2;

        mNames[2] = "Good";
        mConfigs[2].velocityBias = 0.5f;
        mConfigs[2].adaptiveDivisions = 7;
        mConfigs[2].adaptiveRings = 2;
        mConfigs[2].adaptiveDepth = 3;

        mNames[3] = "High";
        mConfigs[3].velocityBias = 0.5f;
        mConfigs[3].adaptiveDivisions = 7;
        mConfigs[3].adaptiveRings = 3;
        mConfigs[3].adaptiveDepth = 3;
    }

    /// <summary>
    /// Gets the name of a configuration.
    /// </summary>
    /// <param name="index">The index of the configuration.</param>
    /// <returns>The name of the configuration.</returns>
    public string GetName(int index)
    {
        return mNames[index];
    }

    /// <summary>
    /// Sets the name of a configuration.
    /// </summary>
    /// <remarks>
    /// <para>The name will be auto-trimmed.</para>
    /// <para>If the name is an empty string it will be automatically set to 
    /// <see cref="DefaultName"/>.</para></remarks>
    /// <param name="index">The index of the configuration.</param>
    /// <param name="name">The new name of the configuration.</param>
    public void SetName(int index, string name)
    {
        if (name == null || name.Trim().Length == 0)
            name = DefaultName;
        mNames[index] = name.Trim();
    }
}

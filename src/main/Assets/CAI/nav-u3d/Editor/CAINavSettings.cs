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
using UnityEngine;
using System.Collections.Generic;
using org.critterai.nav;

/// <summary>
/// Global navigation related settings. (Editor Only)
/// </summary>
[System.Serializable]
public sealed class CAINavSettings
    : ScriptableObject
{
    private const string UnWalkable = "Not Walkable";
    private const string Default = "Default";
    private const string Unknown = "Unknown";

    [SerializeField]
    internal List<string> areaNames = new List<string>();

    [SerializeField]
    internal List<byte> areas = new List<byte>();

    [SerializeField]
    internal string[] flagNames;

    void OnEnable()
    {
        if (areaNames.Count == 0)
            Reset();
    }

    /// <summary>
    /// Resets the settings to the default state.
    /// </summary>
    public void Reset()
    {
        areaNames.Clear();
        areas.Clear();

        areaNames.Add(UnWalkable);
        areas.Add(Navmesh.NullArea);

        areaNames.Add(Default);
        areas.Add(Navmesh.MaxArea);

        areaNames.Add("Water");
        areas.Add((byte)(Navmesh.MaxArea - 1));

        flagNames = new string[16];

        flagNames[0] = Default;
        flagNames[1] = "Jump";
        flagNames[2] = "Swim";
        flagNames[3] = "Blocked";

        for (int i = 4; i < flagNames.Length; i++)
        {
            flagNames[i] = string.Format("Flag 0x{0:X}", 1 << i);
        }
    }

    /// <summary>
    /// Gets a clone of the defined well known area names.
    /// </summary>
    /// <returns>A clone of the area names.</returns>
    public string[] GetAreaNames()
    {
        return areaNames.ToArray();
    }

    /// <summary>
    /// True if the area has a defined well known name.
    /// </summary>
    /// <param name="area">The area.</param>
    /// <returns>True if the area is well known.</returns>
    public bool HasAreaName(byte area)
    {
        return (areas.IndexOf(area) >= 0);
    }

    /// <summary>
    /// Gets the name of the area, or a default name if none has been defined.
    /// </summary>
    /// <param name="area">The area.</param>
    /// <returns>The area's well known name.</returns>
    public string GetAreaName(byte area)
    {
        int i = areas.IndexOf(area);
        return (i == -1) ? Unknown : areaNames[i];
    }

    /// <summary>
    /// Gets the index of the area's well known name. (Or -1 if it is not well known.)
    /// </summary>
    /// <param name="area">The area.</param>
    /// <returns>The index of the well known name.</returns>
    public int GetAreaNameIndex(byte area)
    {
        return areas.IndexOf(area);
    }

    /// <summary>
    /// Gets the area associated with the well known name. (Or 0 if not a valid name.)
    /// </summary>
    /// <param name="name">The well known name.</param>
    /// <returns>The area associated with the name.</returns>
    public byte GetArea(string name)
    {
        int i = areaNames.IndexOf(name);
        return (i == -1) ? (byte)0 : areas[i];
    }

    /// <summary>
    /// Gets a clone of the well known names for flags, index on flag bit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All flags have a well known name.
    /// </para>
    /// <para>
    /// The order of the names match the order of the flags. So the name at index 5 is the name
    /// for flag (1 &lt;&lt; 5).
    /// </para>
    /// </remarks>
    /// <returns>A clone of the flag names.</returns>
    public string[] GetFlagNames()
    {
        return (string[])flagNames.Clone();
    }

    ///// <summary>
    ///// Gets the well known name of the flag. (Only one should be set.)
    ///// </summary>
    ///// <param name="flag">The flag.</param>
    ///// <returns></returns>
    //public string GetFlagName(ushort flag)
    //{
    //    for (int i = 0; i < 16; i++)
    //    {
    //        if ((flag & (1 << i)) != 0)
    //            return flagNames[i];
    //    }
    //    return "";
    //}

    //public int GetFlagNameIndex(ushort flag)
    //{
    //    for (int i = 0; i < 16; i++)
    //    {
    //        if ((flag & (1 << i)) != 0)
    //            return i;
    //    }
    //    return -1;
    //}
}

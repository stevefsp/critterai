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
using UnityEngine;
using UnityEditor;

/// <summary>
/// Defines menu items applicable only to the Unity Pro version of NMGen.
/// </summary>
public static class NMGenProMenu
{
    [MenuItem("GameObject/Create CAI", false, 2)]
    public static void CreateCAIMenu() { }

    [MenuItem("GameObject/Create CAI/Navmesh (Array Sourced)", false, 0)]
    public static void AddNavmeshArray()
    {
        GameObject go = new GameObject("Navmesh");
        CAIArraySource source = go.AddComponent<CAIArraySource>();
        GeneratedNavmesh nm = go.AddComponent<GeneratedNavmesh>();
        nm.sourceGeometry = source;
    }

    [MenuItem("GameObject/Create CAI/Navmesh (Tag Sourced)", false, 0)]
    public static void AddGeneratedNavmeshTag()
    {
        GameObject go = new GameObject("Navmesh");
        CAITagSource source = go.AddComponent<CAITagSource>();
        GeneratedNavmesh nm = go.AddComponent<GeneratedNavmesh>();
        nm.sourceGeometry = source;
    }
}

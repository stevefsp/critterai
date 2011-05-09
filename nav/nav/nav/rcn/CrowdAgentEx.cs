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
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn
{
    internal static class CrowdAgentEx
    {
        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetAgentParams")]
        public static extern void GetParams(IntPtr agent
            , ref CrowdAgentParams config);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetAgentCorners")]
        public static extern void GetCorners(IntPtr agent
            , [In, Out] CrowdCornerData resultData);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetAgentCoreData")]
        public static extern void GetCoreData(IntPtr agent
            , [In, Out] CrowdAgentCoreState resultData);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetAgentNeighbors")]
        public static extern int GetNeighbors(IntPtr agent
            , [In, Out] CrowdNeighbor[] neighbors
            , int neighborsSize);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetPathCorridorData")]
        public static extern void GetPathCorridor(IntPtr agent
            , [In, Out] PathCorridorData corridor);

        [DllImport("cai-nav-rcn", EntryPoint = "dtcaGetLocalBoundary")]
        public static extern void GetLocalBoundary(IntPtr agent
            , [In, Out] LocalBoundaryData boundary);
    }
}

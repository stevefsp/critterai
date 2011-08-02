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
    internal static class PathCorridorEx
    {
        /*
         * Design note: In order to stay compatible with Unity iOS, all
         * extern methods must be unique and match DLL entry point.
         * (Can't use EntryPoint.)
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtpcAlloc(int maxPath);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcFree(IntPtr corridor);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcReset(IntPtr corridor
            , uint polyRef
            , [In] float[] position);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcFindCorners(IntPtr corridor
            , [In, Out] float[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcOptimizePathVisibility(IntPtr corridor
             , [In] float[] next
             , float pathOptimizationRange
             , IntPtr navquery
             , IntPtr filter);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcOptimizePathTopology(IntPtr corridor
             , IntPtr navquery
             , IntPtr filter);
    	
	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcMoveOverOffmeshConnection(IntPtr corridor
             , uint offMeshConRef
             , [In, Out] uint[] refs // size 2
             , [In, Out] float[] startPos
             , [In, Out] float[] endPos
             , IntPtr navquery);
    	
	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcMovePosition(IntPtr corridor
            , [In] float[] npos
            , IntPtr navquery
            , IntPtr filter
            , [In, Out] float[] pos);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcMoveTargetPosition(IntPtr corridor
            , [In] float[] npos
            , IntPtr navquery
            , IntPtr filter
            , [In, Out] float[] pos);
    	
	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcSetCorridor(IntPtr corridor
             , [In] float[] target
             , [In] uint[] path
             , int pathCount);
    	
	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcGetPos(IntPtr corridor
             , [In] float[] position);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcGetTarget(IntPtr corridor
             , [In] float[] target);
         	
	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern uint dtpcGetFirstPoly(IntPtr corridor);
    	
	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcGetPath(IntPtr corridor
             , [In, Out] uint[] path
             , int maxPath);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcGetPathCount(IntPtr corridor);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcGetData(IntPtr corridor
            , [In, Out] PathCorridorData data);
    }
}

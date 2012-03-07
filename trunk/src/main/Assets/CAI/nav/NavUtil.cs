/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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
using Math = System.Math;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Provides utilities related to navigation.
    /// </summary>
    public static class NavUtil
    {
        /// <summary>
        /// Returns TRUE if the status includes the success flag.
        /// </summary>
        /// <param name="status">The status to check.</param>
        /// <returns>TRUE if the status includes the success flag.</returns>
        public static bool Succeeded(NavStatus status)
        {
            return (status & NavStatus.Sucess) != 0;
        }

        /// <summary>
        /// Returns TRUE if the status includes the failure flag.
        /// </summary>
        /// <param name="status">The status to check.</param>
        /// <returns>TRUE if the status includes the failure flag.</returns>
        public static bool Failed(NavStatus status)
        {
            return (status & NavStatus.Failure) != 0;
        }

        /// <summary>
        /// Returns TRUE if the status includes the in-progress flag.
        /// </summary>
        /// <param name="status">The status to check.</param>
        /// <returns>TRUE if the status includes the in-progress flag.
        /// </returns>
        public static bool IsInProgress(NavStatus status)
        {
            return (status & NavStatus.InProgress) != 0;
        }

        public static byte ClampArea(byte value)
        {
            return Math.Min(Navmesh.WalkableArea, value);
        }

        public static byte ClampArea(int value)
        {
            return (byte)Math.Min(Navmesh.WalkableArea, Math.Max(0, value));
        }

        public static NavmeshParams GetConfig(NavmeshTileData tile)
        {
            NavmeshTileHeader header = tile.GetHeader();

            return new NavmeshParams(header.boundsMin
                    , header.boundsMax.x - header.boundsMin.x
                    , header.boundsMax.z - header.boundsMin.z
                    , 1  // Max tiles.
                    , header.polyCount);
        }

        public static Vector3 TestVector(Vector3 v)
        {
            Vector3 result = new Vector3();
            rcn.InteropUtil.dtvlVectorTest(ref v, ref result);
            return result;
        }

        public static void TestVectorArray(Vector3[] vectors
            , int vectorCount
            , Vector3[] result)
        {
            rcn.InteropUtil.dtvlVectorArrayTest(vectors, vectorCount, result);
        }
    }
}

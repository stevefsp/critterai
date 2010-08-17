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

namespace org.critterai.nav.nmpath
{
    ///
    /// A mesh representing connected corridors.  The mesh layout and
    /// tests paths cover almost all test and edge cases.  The only notable
    /// exceptions are:
    /// <para>No overlapping cells.<br/>
    /// No large open areas</para>
    /// 
    ///
    public sealed class CorridorMesh
        : ITestMesh 
    {
        
        ///
        /// {@inheritDoc}
        ///
        public int PolyCount { get { return 27; } }    
        
        ///
        /// {@inheritDoc}
        ///
        public float Offset { get { return 0.1f; } }
        
        ///
        /// {@inheritDoc}
        ///
        public float[] MinVertex
        {
            get
            {
                float[] result = { -5, 0.7f, -3 };
                return result;
            }
        }
        
        ///
        /// {@inheritDoc}
        ///
        public int[] GetMinVertexPolys()
        {
            int[] result = { 11, 12, 13 };
            return result;
        }
        
        ///
        /// {@inheritDoc}
        ///

        public int[] GetIndices() 
        {
            int[] result = {
                    1, 6, 5            // 0
                    , 1, 2, 6
                    , 6, 2, 7
                    , 6, 7, 12
                    , 6, 12, 8
                    , 8, 12, 15        // 5
                    , 12, 16, 15    
                    , 15, 16, 19
                    , 19, 16, 25
                    , 19, 25, 18
                    , 18, 25, 22    // 10
                    , 22, 25, 24    
                    , 10, 22, 24
                    , 1, 10, 24
                    , 1, 5, 10
                    , 5, 11, 10        // 15
                    , 10, 11, 14    
                    , 12, 13, 16
                    , 13, 17, 16
                    , 3, 4, 9
                    , 9, 4, 21        // 20
                    , 13, 9, 17        
                    , 17, 9, 21
                    , 17, 21, 20
                    , 20, 21, 0
                    , 20, 0, 26        // 25
                    , 23, 20, 26    // 26
            };
            return result;
        }
        
        ///
        /// {@inheritDoc}
        ///
        public float[] GetVerts() 
        {
            float[] result = { 
                      5, 0.5f, -3    // 0
                    , -5, 1, 4
                    , 3, 0.5f, 4
                    , 4, -1, 4
                    , 5, -1, 4
                    , -4, 1, 3        // 5
                    , 0, 0.5f, 3
                    , 3, 0.5f, 3
                    , -1, 0.5f, 2
                    , 4, -0.5f, 2
                    , -4, 1, 1        // 10
                    , -2, 1.5f, 1
                    , 1, 0.5f, 1
                    , 3, 0, 1
                    , -3, 1.5f, 0
                    , -1, 0.3f, 0    // 15
                    , 1, 0.5f, 0
                    , 3, 0, 0
                    , -3, 0.5f, -1
                    , 0, 0.5f, -1
                    , 3, 0, -1        // 20
                    , 5, -1, -1
                    , -4, 0.7f, -2
                    , 2, 0.4f, -2
                    , -5, 0.7f, -3
                    , 1, 0.5f, -3    // 25
                    , 2, 0.5f, -3    // 26
                };
            return result;
        }
        
        ///
        /// {@inheritDoc}
        ///
        public int[] GetLinkCounts()
        {
            int[] result = {
                2, 2, 2, 2, 2
                , 2, 3, 2, 2, 2
                , 2, 2, 2, 2, 3
                , 2, 1, 2, 2, 1
                , 2, 2, 3, 2, 2
                , 2, 1
            };
            return result;
        }
        
        ///
        /// {@inheritDoc}
        ///
        public int[] GetLinkWalls()
        {
            int[] result = {
                2, -1, 0    // 0
                , -1, 0, 0
                , 1, -1, 0
                , 2, -1, 0
                , 2, 0, -1
                , 1, 2, -1    // 5
                , 2, 0, 1
                , 1, 0, -1
                , 1, -1, 0
                , 2, 0, -1
                , 1, 0, -1    // 10
                , 1, -1, 1
                , -1, 2, 1
                , 2, 2, -1
                , 2, 2, 0
                , -1, 0, 1    // 15
                , 1, -1, -1
                , -1, 2, 0
                , 2, -1, 1
                , -1, 0, -1
                , 1, -1, 1    // 20
                , -1, 0, 0
                , 1, 2, 0
                , 2, 0, -1
                , 1, -1, 0
                , 2, -1, 1    // 25
                , -1, 2, -1    // 26
            };
            return result;
        }
        
        ///
        /// {@inheritDoc}
        ///
        public int[] GetLinkPolys()
        {
            int[] result = {
                1, -1, 14        // 0
                , -1, 2, 0
                , 1, -1, 3
                , 2, -1, 4
                , 3, 5, -1
                , 4, 6, -1        // 5
                , 17, 7, 5
                , 6, 8, -1
                , 7, -1, 9
                , 8, 10, -1
                , 9, 11, -1        // 10
                , 10, -1, 12
                , -1, 11, 13
                , 14, 12, -1
                , 0, 15, 13
                , -1, 16, 14    // 15
                , 15, -1, -1
                , -1, 18, 6
                , 21, -1, 17
                , -1, 20, -1
                , 19, -1, 22    // 20
                , -1, 22, 18
                , 21, 20, 23
                , 22, 24, -1
                , 23, -1, 25
                , 24, -1, 26    // 25
                , -1, 25, -1    // 26
            };
            return result;
        }

        ///
        /// {@inheritDoc}
        ///
        public float[] GetLOSPointsTrue() 
        {
            float[] result = { 
                    -4.4f, 3.6f, -3, 0            // 0  End is vertex
                    , 0.2f, -1, -1.4f, -2.8f
                    , 0, 0, 1, -1                //    Start and end on wall.
                    , 3.4f, 0.2f, -0.2f, 1.2f
                    , 0, 3, 1, 0                // 5  Start and end on vertex.
            };
            return result;
        }

        ///
        /// {@inheritDoc}
        ///
        public int[] GetLOSPolysTrue() 
        {
            int[] result = {
                    0, 16        // 0
                    , 8, 11
                    , 6, 8
                    , 22, 5
                    , 4, 6
            };
            return result;
        }

        ///
        /// {@inheritDoc}
        ///
        public float[] GetLOSPointsFalse() 
        {
            float[] result = {    
                      -3.8f, 2.4f, -1.4f, 3.6f    // 0
                    , -5, 2.2f, 0, 0            //     Edge of mesh (wall) to wall.
                    , -2.6f, 0.6f, 3.6f, 0.8f    
                    , -0.2f, 2.4f, 2.8f, -2.6f
                    , 0, 3, 3, 0                // 4 Starts and end on vertex.
                    
            };
            return result;
        }

        ///
        /// {@inheritDoc}
        ///
        public int[] GetLOSPolysFalse() 
        {
            int[] result = {
                      15, 1        // 0
                    , 13, 7
                    , 16, 22
                    , 4, 25
                    , 3, 18        // 4
                    
            };
            return result;
        }

        ///
        /// {@inheritDoc}
        ///
        public int GetPathCount 
        {
            get { return 4; }
        }

        ///
        /// {@inheritDoc}
        /// <para>If searching for this points, search by column.  Otherwise
        /// the corresponding start/goal Cell is not guaranteed.</para>
        ///
        public float[] GetPathPoints(int index) 
        {
            switch (index)
            {
            case 0:
                float[] result0 = { 2.4f, 0.4f, -1.8f, 0.8f, 0.5f, -2 };
                return result0;
            case 1:
                float[] result1 = { 4.2f, -0.75f, 3.2f, 1.2f, 0.5f, 3.2f };
                return result1;
            case 2:
                float[] result2 = { -2.6f, 1.5f, 0.8f, -1.4f, 0.5f, -1.2f };
                return result2;
            case 3:
                float[] result3 = { 4.8f, -0.5f, 2.4f, -4.2f, 0.7f, -1.4f };
                return result3;
            }
            return null;
        }

        ///
        /// {@inheritDoc}
        ///
        public int[] GetPathPolys(int index) 
        {
            switch (index)
            {
            case 0:
                int[] result0 = { 26, 25, 24, 23, 22, 21, 18, 17, 6, 7, 8 };
                return result0;
            case 1:
                int[] result1 = { 19, 20, 22, 21, 18, 17, 6, 5, 4, 3, 2 };
                return result1;
            case 2:
                int[] result2 = { 16, 15, 14, 13, 12, 11, 10, 9 };
                return result2;
            case 3:
                int[] result3 = { 20, 22, 21, 18, 17, 6, 7, 8, 9, 10, 11, 12 };
                return result3;
            }
            return null;
        }

        ///
        /// {@inheritDoc}
        /// <para>This mesh does not have any overlapping cells.  To any value is valid.</para>
        ///
        public float PlaneTolerance { get { return 0.5f; } }

        ///
        /// {@inheritDoc}
        ///
        public int MultiPathCount { get { return 4; } }

        ///
        /// {@inheritDoc}
        ///
        public float[] GetMultiPathGoalPoint(int index) 
        {
            switch (index)
            {
            case 0:
                float[] result0 = { 0.8f, 0.5f, -2 };
                return result0;
            case 1:
                float[] result1 = { 1.2f, 0.5f, 3.2f };
                return result1;
            case 2:
                float[] result2 = { -1.4f, 0.5f, -1.2f };
                return result2;
            case 3:
                float[] result3 = { -4.2f, 0.7f, -1.4f };
                return result3;
            }
            return null;
        }

        ///
        /// {@inheritDoc}
        ///
        public int[] GetMultiPathPolys(int index) 
        {
            switch (index)
            {
            case 0:
                int[] result0 = { 16, 15, 14, 13, 12, 11, 10, 9, 8 };
                return result0;
            case 1:
                int[] result1 = { 16, 15, 14, 0, 1, 2 };
                return result1;
            case 2:
                int[] result2 = { 16, 15, 14, 13, 12, 11, 10, 9 };
                return result2;
            case 3:
                int[] result3 = { 16, 15, 14, 13, 12 };
                return result3;
            }
            return null;
        }

        ///
        /// {@inheritDoc}
        ///
        public float[] MultiPathStartPoint
        {
            get
            {
                float[] result = { -2.6f, 1.5f, 0.8f };
                return result;
            }
        }

        public int GetShortestMultiPath() { return 3; }   
    }
}

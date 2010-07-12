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
using System;

namespace org.critterai.nav
{
    ///
    ///Utility functions used by multiple tests.
    ///
    public static class TestUtil 
    {
        
        public static TriCell[] GetAllCells(float[] verts, int[] indices)
        {
            int polyCount = indices.Length / 3;
            TriCell[] polys = new TriCell[polyCount];
            for (int iPoly = 0; iPoly < polyCount; iPoly++)
            {
                int pPoly = iPoly*3;
                polys[iPoly] = new TriCell(verts
                        , indices[pPoly]
                        , indices[pPoly+1]
                        , indices[pPoly+2]);
            }
            return polys;
        }

        public static void LinkAllCells(TriCell[] polys)
        {
            for (int iPoly = 0; iPoly < polys.Length; iPoly++)
            {
                for (int iPolyNext = iPoly+1; iPolyNext < polys.Length; iPolyNext++)
                {
                    polys[iPoly].Link(polys[iPolyNext], true);
                }
            }
        }
        
        public static float[] GetVertBounds(float[] verts)
        {
            if (verts == null || verts.Length % 3 != 0)
                return null;
            float[] result = new float[6];
            result[0] = verts[0];
            result[1] = verts[1];
            result[2] = verts[2];
            result[3] = verts[0];
            result[4] = verts[1];
            result[5] = verts[2];
            for (int pPoly = 3; pPoly < verts.Length; pPoly += 3)
            {
                result[0] = Math.Min(result[0], verts[pPoly]);
                result[1] = Math.Min(result[1], verts[pPoly+1]);
                result[2] = Math.Min(result[2], verts[pPoly+2]);
                result[3] = Math.Max(result[3], verts[pPoly]);
                result[4] = Math.Max(result[4], verts[pPoly+1]);
                result[5] = Math.Max(result[5], verts[pPoly+2]);
            }
            return result;
        }
    }
}

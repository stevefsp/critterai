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
package org.critterai.nav;

import org.critterai.nav.TriCell;

/**
 * Utility functions used by multiple tests.
 */
public final class TestUtil 
{
	
	private TestUtil() { }
	
	public static TriCell[] getAllCells(float[] verts, int[] indices)
	{
		int polyCount = indices.length / 3;
		final TriCell[] polys = new TriCell[polyCount];
		for (int iPoly = 0; iPoly < polyCount; iPoly++)
		{
			final int pPoly = iPoly*3;
			polys[iPoly] = new TriCell(verts
					, indices[pPoly]
					, indices[pPoly+1]
					, indices[pPoly+2]);
		}
		return polys;
	}

	public static void linkAllCells(TriCell[] polys)
	{
		for (int iPoly = 0; iPoly < polys.length; iPoly++)
		{
			for (int iPolyNext = iPoly+1; iPolyNext < polys.length; iPolyNext++)
			{
				polys[iPoly].link(polys[iPolyNext], true);
			}
		}
	}
	
	public static float[] getVertBounds(float[] verts)
	{
		if (verts == null || verts.length % 3 != 0)
			return null;
		float[] result = new float[6];
		result[0] = verts[0];
		result[1] = verts[1];
		result[2] = verts[2];
		result[3] = verts[0];
		result[4] = verts[1];
		result[5] = verts[2];
		for (int pPoly = 3; pPoly < verts.length; pPoly += 3)
		{
			result[0] = Math.min(result[0], verts[pPoly]);
			result[1] = Math.min(result[1], verts[pPoly+1]);
			result[2] = Math.min(result[2], verts[pPoly+2]);
			result[3] = Math.max(result[3], verts[pPoly]);
			result[4] = Math.max(result[4], verts[pPoly+1]);
			result[5] = Math.max(result[5], verts[pPoly+2]);
		}
		return result;
	}
}

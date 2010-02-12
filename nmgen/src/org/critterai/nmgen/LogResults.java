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
package org.critterai.nmgen;

/**
 * Holds performance and log related data.
 * <p>All time values are in nanoseconds (ns).
 */
public class LogResults 
{

	/**
	 * The data is undefined. (Has not been set.)
	 */
	public static final long UNDEFINED = -1;
	
	/**
	 * The time to perform voxelization. (ns)
	 */
	public long voxelizationTime;
	
	/**
	 * The time to perform region generation. (ns)
	 */
	public long regionGenTime;
	
	/**
	 * The time to perform contour generation. (ns)
	 */
	public long contourGenTime;
	
	/**
	 * The time to perform polygon generation. (ns)
	 */
	public long polyGenTime;
	
	/**
	 * The time to perform the final triangulation. (ns)
	 */
	public long finalMeshGenTime;
	
	/**
	 * Returns the total time to generate the navigation mesh. (ns)
	 * @return The total time to generate the navigation mesh. (ns)
	 */
	public long getTotalGenTime()
	{
		if (finalMeshGenTime == UNDEFINED)
			return UNDEFINED;
		return voxelizationTime 
			+ regionGenTime
			+ contourGenTime
			+ polyGenTime
			+ finalMeshGenTime;
	}
	
	/**
	 * Prepares the object to be reused.
	 */
	public void reset()
	{
		voxelizationTime = UNDEFINED;
		regionGenTime = UNDEFINED;
		contourGenTime = UNDEFINED;
		polyGenTime = UNDEFINED;
		finalMeshGenTime = UNDEFINED;
	}
	
}

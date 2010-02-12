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
 * A class used to hold intermediate data used to build a navigation mesh.
 * <p>When this data is combined with the source geometry and final navigation mesh,
 * the entire build process is represented.</p>
 */
public final class IntermediateData 
{
	
	/*
	 * Recast Reference: None
	 * 
	 * Doc State: Complete
	 * Standards Check: Complete
	 */
	
	private SolidHeightfield mSolidHeightfield;
	private OpenHeightfield mOpenHeightfield;
	private ContourSet mContours;
	private PolyMeshField mPolyMesh;
	
	/**
	 * The solid heightfield associated with the source geometry.
	 * @return The solid heightfield derived from the source geometry.
	 */
	public SolidHeightfield solidHeightfield() { return mSolidHeightfield; }
	
	/**
	 * The open heightfield associated with the solid heightfield.
	 * @return The open heightfield associated with the solid heightfield.
	 */
	public OpenHeightfield openHeightfield() { return mOpenHeightfield; }
	
	/**
	 * The contour set associated with the open heightfield.
	 * @return The contours associated with the open heightfield.
	 */
	public ContourSet contours() { return mContours; }
	
	/**
	 * The polygon mesh associated with the contour set.
	 * @return The polygon mesh associated with the contour set.
	 */
	public PolyMeshField polyMesh() { return mPolyMesh; }
	
	/**
	 * Sets the solid height field.
	 * @param field The solid heightfield.
	 */
	public void setSolidHeightfield(SolidHeightfield field)
	{
		mSolidHeightfield = field;
	}
	
	/**
	 * Sets the open heightfield.
	 * @param field The open heightfield.
	 */
	public void setOpenHeightfield(OpenHeightfield field)
	{
		mOpenHeightfield = field;
	}
	
	/**
	 * Sets the contour set.
	 * @param contours The contour set.
	 */
	public void setContours(ContourSet contours)
	{
		mContours = contours;
	}
	
	/**
	 * Sets the polygon mesh.
	 * @param mesh The polygon mesh.
	 */
	public void setPolyMesh(PolyMeshField mesh)
	{
		mPolyMesh = mesh;
	}
	
	/**
	 * Resets all data to null.
	 */
	public void reset()
	{
		mSolidHeightfield = null;
		mOpenHeightfield = null;
		mContours = null;
		mPolyMesh = null;
	}
}

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
 * Implements a common interface for heightfields.
 * <p>At this point this class is just a convenience class..Unless is ends up serving
 * a real purpose in the future it will likely be obsoleted.</p>
 */
public abstract class BaseHeightfield<T>
	extends BoundedField
{
	
	/*
	 * Recast Reference: None
	 * 
	 * Doc State: Complete
	 * Standards Check: Complete
	 */
	
	/**
	 * Constructor
	 * @param gridBoundsMin The minimum bounds of the field in the form (minX, minY, minZ).
	 * @param gridBoundsMax The maximum bounds of the field in the form (maxX, maxY, maxZ).
	 * @param cellSize The size of the cells.  (The grid that forms the base of the field.)
	 * @param cellHeight The height increment of the field.
	 * @throws IllegalArgumentException If the bounds are null or the wrong size.
	 */
	public BaseHeightfield(float[] gridBoundsMin, float[] gridBoundsMax, float cellSize, float cellHeight)
		throws IllegalArgumentException
	{
		super(gridBoundsMin, gridBoundsMax, cellSize, cellHeight);
	}
	
	/**
	 * Constructor - Partial
	 * <p>The bounds of the field will default to min(0, 0, 0) and max(1, 1, 1).</p>
	 * @param cellSize The size of the cells.  (The grid that forms the base of the field.)
	 * @param cellHeight The height increment of the field.
	 */
	public BaseHeightfield(float cellSize, float cellHeight)
	{
		super(cellSize, cellHeight);
	}
	
	/**
	 * Constructor - Default
	 * <p>The bounds of the field will default to min(0, 0, 0) and max(1, 1, 1).</p>
	 * <p>The cell size and height will default to 0.1.</p>
	 */
	public BaseHeightfield()
	{
		super();
	}
	
	/**
	 * Returns the cell data for the provided width and depth index.
	 * <p>Behavior is undefined if the width or depth indices are invalid.</p>
	 * @param widthIndex The width index. (0 <= index < {@link BaseHeightfield#width()})
	 * @param depthIndex The depth index. (0 <= index < {@link BaseHeightfield#depth()})
	 * @return The cell data for the provided width and depth index.
	 */
	public abstract T getData(int widthIndex, int depthIndex);
	
	/**
	 * An iterator for the data contained within the field. 
	 * @return An iterator for the data contained within the field. 
	 */
	public abstract IHeightfieldIterator<T> dataIterator();
	
}

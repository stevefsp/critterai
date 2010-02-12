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
 * Represents the open space above a solid span within the cell column of a heightfield.
 * <p>Design Note: Structurally this class is so similar with {@link HeightSpan} that this 
 * class could extend HeightSpan.  But this class is being kept separate for clarity.  
 * As an independent class it can use the {@link #floor()} and {@link #ceiling()} nomenclature that
 * makes reading code much easier.</p>
 * @see <a href="http://www.critterai.org/?q=nmgen_hfintro" target="_parent">Introduction to Heightfields</a>
 * @see HeightSpan
 */
public final class OpenHeightSpan 
{
	/*
	 * Doc State: Complete
	 * Standards Check: Complete
	 */
	
	/**
	 * Flags used for for temporary flagging.
	 * The value is meaningless outside the operation in which the flags are needed.
	 * Since various operations may use this flag for their own purpose, always reset
	 * the flag prior to use.
	 */
	public int flags = 0;
	
	private int mRegionID = 0;
	private int mDistanceToRegionCore = 0;
	private int mDistanceToBorder = 0;
	
	private int mFloor;
	private int mHeight;
	
	private OpenHeightSpan mNext = null;
	private OpenHeightSpan mNeighborConnection0 = null;
	private OpenHeightSpan mNeighborConnection1 = null;
	private OpenHeightSpan mNeighborConnection2 = null;
	private OpenHeightSpan mNeighborConnection3 = null;
	
	/**
	 * Constructor
	 * @param floor The base height of the span.
	 * @param height The height of the unobstructed space above the floor. 
	 * {@link Integer#MAX_VALUE} is generally used to indicate no obstructions exist above the floor. 
	 * @throws IllegalArgumentException If the floor is below zero or the height is less than 1.
	 */
	public OpenHeightSpan(int floor, int height)
		throws IllegalArgumentException
	{
		if (floor < 0)
			throw new IllegalArgumentException("Floor is less than zero.");
		if (height < 1)
			throw new IllegalArgumentException("Height is less than one.");
		this.mFloor = floor;
		this.mHeight = height;
	}
	
	/**
	 * The base height of the span.
	 * @return The base height of the span.
	 */
	public int floor() { return mFloor; }
	
	/**
	 * The height of the unobstructed space above the floor. 
	 * <p>{@link Integer#MAX_VALUE} is generally used to indicate no obstructions exist above the floor.</p> 
	 * @return The height of the unobstructed space above the floor. 
	 */
	public int height() { return mHeight; }
	
	/**
	 * The height of the ceiling.
	 * @return The height of the ceiling.
	 */
	public int ceiling() { return mFloor + mHeight; }
	
	/**
	 * The next span higher in the span's heightfield column.
	 * <p>The space between this span's ceiling and the next span's floor is
	 * considered to be obstructed space.</p>
	 * @return The next higher span in the span's heightfield column.  Or null if there
	 * is no heigher span.
	 */
	public OpenHeightSpan next() { return mNext; }
	
	/**
	 * Set the next heigher span in the span's heightfield column.
	 * @param value The new value.  null is an acceptable value.
	 */
	public void setNext(OpenHeightSpan value) { mNext = value; }
	
	/**
	 * The distance this span is from the nearest border of the heightfield it belongs to.
	 * @return The distance this span is from the nearest heightfield border.
	 */
	public int distanceToBorder() { return mDistanceToBorder; }
	
	/**
	 * Set the distance this span is from the nearest border of the heightfield it belongs to.
	 * @param value The new distance.  Auto-clamped at a minimum of zero.
	 */
	public void setDistanceToBorder(int value)
	{
		mDistanceToBorder = Math.max(value, 0);
	}
	
	/**
	 * The distance this span is from the core of the heightfield region it belongs to.
	 * @return The distance this span is from the core of the heightfield region it belongs to.
	 */
	public int distanceToRegionCore() { return mDistanceToRegionCore; }
	
	/**
	 * Set the distance this span is from the core of the heightfield region it belongs to.
	 * @param value The new distance.  Auto-clamped at a minimum of zero.
	 */
	public void setDistanceToRegionCore(int value)
	{
		mDistanceToRegionCore = Math.max(value, 0);
	}
	
	/**
	 * The heightfield region this span belongs to.
	 * @return The heightfield region this span belongs to.
	 */
	public int regionID() { return mRegionID; }
	
	/**
	 * The heightfield region this span belongs to.
	 * @param value The new value.
	 */
	public void setRegionID(int value) { mRegionID = value; }
	
	/**
	 * Gets a reference to the span that is considered an axis-neighbor to this span for
	 * the specified direction.  Uses the standard direction indices (0 through 3) where
	 * Zero is the neighbor offset at (-1, 0) and the search proceeds clockwise.
	 * @param direction  The direction to search.
	 * @return A reference to the axis-neighbor in the specified direction.  Or null if there
	 * is no neighbor in the direction or the direction index is invalid.
	 * @see <a href="http://www.critterai.org/?q=nmgen_hfintro#nsearch" target="_parent">Neighbor Searches</a>
	 */
	public OpenHeightSpan getNeighbor(int direction)
	{
		switch (direction)
		{
		case 0: return mNeighborConnection0;
		case 1: return mNeighborConnection1;
		case 2: return mNeighborConnection2;
		case 3: return mNeighborConnection3;
		default: return null;
		}
	}
	
	/**
	 * Sets the specified span at the neighbor of the current span.
	 * <p>Uses the standard direction indices (0 through 3) where
	 * Zero is the neighbor offset at (-1, 0) and the search proceeds clockwise.</p>
	 * @param direction The direction of the neighbor.
	 * @param neighbor The neighbor of this span.
	 * @see <a href="http://www.critterai.org/?q=nmgen_hfintro#nsearch" target="_parent">Neighbor Searches</a>
	 */
	public void setNeighbor(int direction, OpenHeightSpan neighbor)
	{
		switch (direction)
		{
		case 0: mNeighborConnection0 = neighbor; break;
		case 1: mNeighborConnection1 = neighbor; break;
		case 2: mNeighborConnection2 = neighbor; break;
		case 3: mNeighborConnection3 = neighbor; break;
		}
	}
	
	/**
	 * {@inheritDoc}
	 */
	@Override
	public String toString() 
	{
		return "floor: " + mFloor + ", ceiling: " + mHeight + ", Region: " + mRegionID;
	}
	
}

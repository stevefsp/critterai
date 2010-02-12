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

import java.util.Hashtable;
import java.util.NoSuchElementException;

/**
 * Provides a representation of the open (unobstructed) space above solid surfaces in a voxel field.
 * <p>For this type of heightfield, spans represent the floor and ceiling of the open spaces.</p> 
 * <p>WARNING: This class has very little protections build into it.  It is basically an uncontrolled
 * data structure with convenience functions.</p>
 * @see <a href="http://www.critterai.org/?q=nmgen_hfintro" target="_parent">Introduction to Height Fields</a>
 */
public final class OpenHeightfield 
	extends BaseHeightfield<OpenHeightSpan>
{
	
	/*
	 * Recast Reference: rcCompactHeightfield in Recast.h
	 * The internal structure of this class is very different from Recast.
	 * See: http://www.critterai.org/nmgen_diffs for the reason.
	 * 
	 * Doc State: Complete
	 * Standards Check: Complete
	 */
	
	/**
	 * A value representing the lack of a region assignment.
	 * <p>Spans in the null-region are often skipped during processing. 
	 * Other processing is only applied when the null-region is involved.</p>
	 */
	public static final int NULL_REGION_ID = 0;
	
	/**
	 * Indicates that a value is unknown and need to be derived.
	 */
	private static final int UNKNOWN = -1;
	
	private int mSpanCount = 0;
	private int mRegionCount = 0;	

	// These next fields are derived only when the value is needed.
	
	/**
	 * The maximum distance a span is from a border span.
	 */
	private int mMaxBorderDistance = UNKNOWN;
	
	/**
	 * The minimum distance a span is from a border span. 
	 */
	private int mMinBorderDistance = UNKNOWN;

	/**
	 * Key = Grid index from {@link #gridIndex(int, int)}.
	 * Value = The first (lowest) span in the grid column.
	 */
	private final Hashtable<Integer, OpenHeightSpan> mSpans = new Hashtable<Integer, OpenHeightSpan>();
	
	/**
	 * Constructor
	 * @param gridBoundsMin The minimum bounds of the field in the form (minX, minY, minZ).
	 * @param gridBoundsMax The maximum bounds of the field in the form (maxX, maxY, maxZ).
	 * @param cellSize The size of the cells.  (The grid that forms the base of the field.)
	 * @param cellHeight The height increment of the field.
	 * @throws IllegalArgumentException If the bounds are null or the wrong size.
	 */
	public OpenHeightfield(float[] gridBoundsMin, float[] gridBoundsMax, float cellSize, float cellHeight)
		throws IllegalArgumentException
	{
		super(gridBoundsMin, gridBoundsMax, cellSize, cellHeight);
	}
	
	/**
	 * The number of spans in the heightfield.
	 * <p>A value of zero indicates an empty heightfield.</p>
	 */
	public int spanCount() { return mSpanCount; }
	
	/**
	 * Increments the span count.
	 * <p>IMPORTANT: There is no automatic span count updates.  Span count
	 * must be managed manually.</p>
	 * @return The new span count.
	 */
	public int incrementSpanCount() { return ++mSpanCount; }
	
	/**
	 * The number of regions in the height field.
	 * <p>Includes the null region.  So unless all spans are in the null region,
	 * this value will be >= 2.  (E.g. The null region and at least one other region.)</p>
	 */
	public int regionCount() { return mRegionCount; }
	
	/**
	 * Sets the region count.
	 * <p>IMPORTANT: There is no automatic region count management.  Region count
	 * must be managed manually.</p>
	 * @param value The new region count.
	 */
	public void setRegionCount(int value) { mRegionCount = value; }
	
	/**
	 * The maximum distance a span in the heightfield is from its nearest border.
	 * @return The maximum distance a span in the heightfield is from its nearest border.
	 */
	public int maxBorderDistance() 
	{ 
		if (mMaxBorderDistance == UNKNOWN)
			calcBorderDistanceBounds();
		return mMaxBorderDistance; 
	}
	
	/**
	 * The minimum distance a span in the height field is from its nearest border.
	 * (Usually zero.  But can depend on the generation method.)
	 * @return The minimum distance a span in the height field is from its nearest border.
	 */
	public int minBorderDistance() 
	{ 
		if (mMinBorderDistance == UNKNOWN)
			calcBorderDistanceBounds();
		return mMinBorderDistance; 
	}
	
	/**
	 * Puts the span at the grid location, replacing any spans already at the location.
	 * The added span becomes the new base span for the location.
	 * <p>WARNING: The span count must be manually updated to reflect changes in span
	 * count</p> 
	 * <p>Behavior is undefined if the indices are invalid.</p>
	 * @param widthIndex The width index of the grid location to add the span to. 
	 * (0 <= value < {@link #width()})
	 * @param depthIndex The depth index of the grid location to add the span to. 
	 * (0 <= value < {@link #depth()})
	 * @param span The span to put at the grid location.
	 * @return The original base span that was in the grid location, or null if there
	 * was no pre-existing span in the location.
	 */
	public OpenHeightSpan addData(int widthIndex, int depthIndex, OpenHeightSpan span)
	{
		return mSpans.put(gridIndex(widthIndex, depthIndex), span);
	}
	
	/**
	 * Retrieves the base (lowest) grid for the specified grid location.
	 * <p>Behavior is undefined if the indices are invalid.</p>
	 * @param widthIndex The width index of the grid location the span is located in. 
	 * (0 <= value < {@link #width()})
	 * @param depthIndex The depth index of the grid location the span is located in.
	 * (0 <= value < {@link #depth()})
	 * @return The base (lowest) span for the specified grid location.  Null if there is no data
	 * for the grid location.
	 */
	public OpenHeightSpan getData(int widthIndex, int depthIndex)
	{
		return mSpans.get(gridIndex(widthIndex, depthIndex));
	}
	
	/**
	 * {@inheritDoc}
	 * The returned iterator does not support the {@link IHeightfieldIterator#remove()} operation.
	 */
	@Override
	public IHeightfieldIterator<OpenHeightSpan> dataIterator() 
	{
		return new OpenHeightFieldIterator();
	}

	/**
	 * Sends a tab delimited table of the distance field values to standard out.
	 * <p>Only the lowest spans in the field are output.  So this operation
	 * is really only suitable for simple tests on fields
	 * that don't contain overlapping spans.</p> 
	 * <p>Columns: Width<br/>
	 * Rows: Depth</p>
	 */
	public void printDistanceField()
	{
		System.out.println("Distance Field (Spans: " + mSpanCount + ")");
		final OpenHeightFieldIterator iter = new OpenHeightFieldIterator();
		int depth = -1;
		System.out.print("\t");
		for (int width = 0; width < width(); width++)
			System.out.print(width + "\t");
		while (iter.hasNext())
		{
			OpenHeightSpan span = iter.next();
			if (iter.depthIndex() != depth)
				System.out.print("\n" + ++depth + "\t");
			System.out.print(span.distanceToBorder() + "\t");
		}
		System.out.println();
	}

	/**
	 * Sends a tab delimited table of the region ID values to standard out.
	 * <p>Only the lowest spans in the field are output.  So this operation
	 * is really only suitable for simple tests on fields
	 * that don't contain overlapping spans.</p> 
	 * <p>Columns: Width<br/>
	 * Rows: Depth</p>
	 */
	public void printRegionField()
	{
		System.out.println("Distance Field (Spans: " + mSpanCount + ")");
		final OpenHeightFieldIterator iter = new OpenHeightFieldIterator();
		int depth = -1;
		System.out.print("\t");
		for (int width = 0; width < width(); width++)
			System.out.print(width + "\t");
		while (iter.hasNext())
		{
			OpenHeightSpan span = iter.next();
			if (iter.depthIndex() != depth)
				System.out.print("\n" + ++depth + "\t");
			System.out.print(span.regionID() + "\t");
		}
		System.out.println();
	}

	/**
	 * Resets the border distance values so they will
	 * be recacluated the next time they are needed.
	 */
	public void clearBorderDistanceBounds()
	{
		mMaxBorderDistance = UNKNOWN;
		mMinBorderDistance = UNKNOWN;
	}
	
	/**
	 * Calculates the min/max distance a span in the field is from it nearest border.
	 * Allows on-demand calculation of this information.
	 */
	private void calcBorderDistanceBounds()
	{
		if (mSpanCount == 0) 
			return;
	
		// Default the values.
		mMinBorderDistance = Integer.MAX_VALUE;
		mMaxBorderDistance = UNKNOWN;
		
		// Iterate through all spans and reset the values if new min/max's are
		// found.
		IHeightfieldIterator<OpenHeightSpan> iter = new OpenHeightFieldIterator();
		while (iter.hasNext())
		{
			OpenHeightSpan span = iter.next();
			mMinBorderDistance = Math.min(mMinBorderDistance, span.distanceToBorder());
			mMaxBorderDistance = Math.max(mMaxBorderDistance, span.distanceToBorder());		
		}
		if (mMinBorderDistance == Integer.MAX_VALUE)
			// There ware a problem locating the maximum.  So set it back to unknown.
			mMinBorderDistance = UNKNOWN;
		
	}

	/**
	 * The iterator returned by {@link OpenHeightfield#dataIterator()}.
	 */
	private class OpenHeightFieldIterator
		implements IHeightfieldIterator<OpenHeightSpan>
	{
	
		// See reset() for initialization information.
		
		private int mNextWidth;
		private int mNextDepth;
		private OpenHeightSpan mNext;
		
		private int mLastWidth;
		private int mLastDepth;
		
		/**
		 * Constructor.
		 */
		public OpenHeightFieldIterator()
		{
			reset();
		}
		
		/**
		 * {@inheritDoc}
		 */
		@Override
		public int depthIndex() { return mLastDepth; }

		/**
		 * {@inheritDoc}
		 */
		@Override
		public int widthIndex() { return mLastWidth; }

		/**
		 * {@inheritDoc}
		 */
		@Override
		public boolean hasNext() 
		{
			return (mNext != null);
		}

		/**
		 * {@inheritDoc}
		 */
		@Override
		public OpenHeightSpan next() 
		{
			if (mNext == null) 
				throw new NoSuchElementException();
			// Select value cursor.
			OpenHeightSpan next = mNext;
			mLastWidth = mNextWidth;
			mLastDepth = mNextDepth;
			// Move the cursor to the next value.
			moveToNext();
			return next;
		}

		/**
		 * {@inheritDoc}
		 * This operation is not supported.
		 */
		@Override
		public void remove()
		{
			throw new UnsupportedOperationException();
		}

		/**
		 * {@inheritDoc}
		 */
		@Override
		public void reset()
		{
			mNextWidth = 0;
			mNextDepth = 0;
			mNext = null;
			mLastWidth = 0;
			mLastDepth = 0;
			moveToNext();
		}
		
		/**
		 * This operation is called in order to move the
		 * cursor to the next span after the {@link #next()} operation
		 * has selected its span to return.
		 */
		private void moveToNext()
		{
			if (mNext != null)
			{
				// There is a current cell selected.
				if (mNext.next() != null)
				{
					// The current cell has a next. Select it
					mNext = mNext.next();
					return;					
				}
				else mNextWidth++;  // Move to next cell.
			}
			// Need to find the next grid location that contains a span.
			// Loop until one is found or no more are available.
			for (int depthIndex = mNextDepth; depthIndex < depth(); depthIndex++)
			{
				for (int widthIndex = mNextWidth; widthIndex < width(); widthIndex++)
				{
					OpenHeightSpan span = mSpans.get(gridIndex(widthIndex, depthIndex));
					if (span != null)
					{
						// A span was found.  Set the cursor to it.
						mNext = span;
						mNextWidth = widthIndex;
						mNextDepth = depthIndex;
						return;
					}
				}
				mNextWidth = 0;
			}
			// If got here then there are no more spans.
			// Set values to indicate the end of iteration.
			mNext = null;
			mNextDepth = -1;
			mNextWidth = -1;
		}
	}
	
}

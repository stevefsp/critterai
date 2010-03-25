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
 * Searches for region spans that are adjacent to null region corners and
 * ensures they are in the most appropriate region.
 * <p>This algorithm reduces the likelihood of a region flowing slightly around a null region
 * corner in a manner that results in a contour that is a self-intersecting polygon.</p>
 * <p>This is an experimental algorithm.  It has only been validated in a limited number of test scenarios.</p>
 * <p>Example: Before the algorithm is applied.</p> 
 * <p><a href="http://www.critterai.org/sites/default/files/study/nmgen/ohfg_08_cornerwrapbefore.png" target="_blank">
 * <img alt="" src="http://www.critterai.org/sites/default/files/study/nmgen/ohfg_08_cornerwrapbefore.jpg" style="width: 620px; height: 353px; " />
 * </a></p> 
 * <p>Example: After the algorithm is applied.</p> 
 * <p><a href="http://www.critterai.org/sites/default/files/study/nmgen/ohfg_09_cornerwrapafter.png" target="_blank">
 * <img alt="" src="http://www.critterai.org/sites/default/files/study/nmgen/ohfg_09_cornerwrapafter.jpg" style="width: 620px; height: 353px; " />
 * </a></p> 
 * @see <a href="http://www.critterai.org/nmgen_regiongen" target="_parent">Region Generation</a>
 * 
 */
public final class CleanNullRegionCorners
	implements IOpenHeightFieldAlgorithm
{

	/*
	 * Design note: 
	 * 
	 * I investigated several simpler algorithms that did not actually 
	 * care about corners or whether the region flows around the corner.  
	 * For example, one looked at every span connected to the null region
	 * and re-assigned them to the region they have the most connections to.
	 * But while these simpler algorithms fixed the contour issue, they resulted in
	 * a significant, unnecessary increase in thin triangles in the vicinity of corners.
	 * 
	 * Recast Reference: None
	 * 
	 * Doc State: Complete.
	 * Standards Check: Complete
	 */
	
	/**
	 * {@inheritDoc}
	 * <p>Behavior is undefined if the height field does not contain 
	 * valid region information.</p>
	 */
	@Override
	public void apply(OpenHeightfield field) 
	{
		
		/*
		 * Scan for regions that flow around the corner of a null region.
		 * and determine if the spans that flow around the corner should be
		 * assigned to another region.
		 */
		
		IHeightfieldIterator<OpenHeightSpan> iter = field.dataIterator();

		/*
		 * Contains the region IDs of the eight neighbors of the current span.
		 * 
		 * Reference: Neighbor searches and nomenclature.
		 * http://www.critterai.org/?q=nmgen_hfintro#nsearch
		 * 
		 * The first four values are the axis neighbors using the standard
		 * 0-3 direction values.  For example, the axis neighbor at direction
		 * 2 is stored at index 2.
		 * 
		 * The next four values are the diagonal neighbors clockwise to the axis
		 * neighbors.  For example, the neighbor clockwise to axis neighbor
		 * 2 is stored at index 6 (2+4).
		 * 
		 * The lack of a neighbor at a position is marked as a null region.
		 */
		final int[] nRegions = new int[8];
		
		// Loop through all spans.
		while (iter.hasNext())
		{
			/*
			 * Note: The content of the neighbor region array is properly re-populated
			 * during this process.  So there is no need to re-initialize its content
			 * at the beginning of each loop.
			 */
			
			final OpenHeightSpan span = iter.next();
			
			if (span.regionID() == OpenHeightfield.NULL_REGION_ID)
				// Don't care about null regions spans.
				continue;
			
			OpenHeightSpan nSpan;
			boolean onBorder = false;
			
			// Search through this span's axis neighbors, gathering regions IDs and determining
			// if any are in the null region.
			for (int dir = 0; dir < 4; dir++)
			{
				nSpan = span.getNeighbor(dir);
				if (nSpan == null
						|| nSpan.regionID() == OpenHeightfield.NULL_REGION_ID)
				{
					// There is a border in this direction.  Mark is as a null region.
					onBorder = true;
					nRegions[dir] = OpenHeightfield.NULL_REGION_ID;
				}
				else
					// There is a standard region span in this direction.  Store its region id.
					nRegions[dir] = nSpan.regionID();	
			}
			
			if (!onBorder)
				// Early exit.  We only care about spans that connect to the null region.
				continue;
			
			// Need to get information for diagonal neighbors.
			for (int dir = 0; dir < 4; dir++)
			{
				final int dirCW = (dir+1)&0x03;  // Rotated clockwise from current direction.
				nSpan = span.getNeighbor(dir);
				if (nSpan == null)
				{
					// No span in this direction.  Try to find diagonal neighbor
					// for this direction via the other axis neighbor that borders it.
					nSpan = span.getNeighbor(dirCW);
					if (nSpan != null)
						// Get diagonal from this neighbor.
						nSpan = nSpan.getNeighbor(dir);
				}
				else
					// Get diagonal from this neighbor.
					nSpan = nSpan.getNeighbor(dirCW);					
				if (nSpan == null)
					// No detectable diagonal neighbor in this direction.
					nRegions[dir+4] = OpenHeightfield.NULL_REGION_ID;
				else
					// Get diagonal from this neighbor.
					nRegions[dir+4] = nSpan.regionID();
			}
			
			// Detect null region corners wrapped by spans in the same region.
			int targetRegion = -1;
			int nullNeighborCount = 0;
			for (int dir = 0; dir < 4; dir++)
			{
				final int dirCW = (dir+1)&0x03;  // The direction clockwise from current direction.
				final int dirOPP = (dir+2)&0x03;  // The direction that is opposite from the current direction.
				final int dirCCW = (dir+3)&0x03; // The direction counter clockwise from the current direction.
				if (nRegions[dir] == OpenHeightfield.NULL_REGION_ID)
				{
					nullNeighborCount++;
					// Current span is connected to null region in this direction.
					// Check the diagonal spans on each side of the neighbor in this direction.
					final int regionCW = nRegions[dir+4];
					final int regionCCW = nRegions[dirCCW+4];
					/*
					 * There are two possible corner styles.  Each are detected and handled 
					 * in a slightly different manner.
					 *   
					 * Note: Technically the next checks don't guarantee that this is a true wrap
					 * since a true wrap requires three spans from the same region to wrap the corner 
					 * and only two are validated. But the extra check adds unneeded complexity for
					 * no real benefit.
					 */
					if (regionCW == OpenHeightfield.NULL_REGION_ID && regionCCW == span.regionID())
					{
						// This corner is wrapped.
						if (nRegions[dirCW] != span.regionID() && nRegions[dirCW] == nRegions[dirOPP])
						{
							// The current span has two axis neighbor connections
							// to another region and only one (maximum) to its current region.
							// Re-assign to the other region.
							targetRegion = nRegions[dirCW];
							break;
						}
					}
					else if (regionCW == span.regionID() && regionCCW == OpenHeightfield.NULL_REGION_ID)
					{
						// This corner is wrapped.
						if (nRegions[dirCCW] != span.regionID() && nRegions[dirCCW] == nRegions[dirOPP])
						{
							// The current span has two axis neighbor connections
							// to another region and only one (maximum) to its current region.
							// Re-assign to the other region.
							targetRegion = nRegions[dirCCW];
							break;
						}
					}
				}
			}
			if (nullNeighborCount != 4 && targetRegion != -1)
			{
				/*
				 * The span is not an island surrounded by the null region
				 * and it needs re-assignment.
				 * 
				 * The island check is needed because this algorithm is not set up to
				 * handle complete removal of regions.
				 * 
				 * Reassign region.
				 */
				span.setRegionID(targetRegion);
			}
		}
	}

}

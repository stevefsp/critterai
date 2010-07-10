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

import java.util.ArrayList;

/**
 * Builds a set of contours from the region information contained by an
 * {@link OpenHeightfield}.  It does this by locating and "walking" the edges
 * <p>
 * <a href="http://www.critterai.org/sites/default/files/nmgen/cont_11_simplified_full.png" target="_parent">
 * <img class="insert" height="465" src="http://www.critterai.org/sites/default/files/nmgen/cont_11_simplified_full.jpg" width="620" />
 * </a></p>
 * @see <a href="http://www.critterai.org/nmgen_contourgen" target="_parent">Contour Generation</a> 
 * @see Contour
 * @see ContourSet
 */
public final class ContourSetBuilder 
{
    
    /*
     * Design notes:
     * 
     * Not adding configuration getters until they are needed.
     * Never add setters.  Configuration should remain immutable to keep
     * the class thread friendly.
     * 
     * Recast Reference: rcBuildContours() in RecastContour.cpp
     * 
     * Doc State: Text Complete.
     * Standards Check: Complete
     */
    
    /**
     * The post-processing algorithms to apply to the contours.
     */
    private final ArrayList<IContourAlgorithm> mAlgorithms = new ArrayList<IContourAlgorithm>();
    
    /**
     * Contructor
     * @param algorithms The post-processing algorithms to apply to the contours.
     */
    public ContourSetBuilder(ArrayList<IContourAlgorithm> algorithms)
    {
        if (algorithms == null) 
            return;
        this.mAlgorithms.addAll(algorithms);
    }
    
    /**
     * Generates a contour set from the provided {@link OpenHeightfield}
     * <p>The provided field is expected to contain region information.
     * Behavior is undefined if the provided field is malformed or incomplete.</p>
     * <p>This operation overwrites the flag fields for all spans in the provided field.
     * So the flags must be saved and restored if they are important.</p>
     * @param sourceField  A fully generated field.
     * @return The contours generated from the field.
     */
    public ContourSet build(OpenHeightfield sourceField)
    {
        if (sourceField == null || sourceField.regionCount() == 0) 
            return null;
        
        // Initialize the contour set.
        final ContourSet result = new ContourSet(sourceField.boundsMin()
                , sourceField.boundsMax()
                , sourceField.cellSize()
                , sourceField.cellHeight()
                , sourceField.regionCount());
        
        /*
         *  Set the flags on all spans in non-null regions to indicate which edges 
         *  are connected to external regions.
         *  
         *  Reference:  Neighbor search and nomenclature.
         *  http://www.critterai.org/?q=nmgen_hfintro#nsearch
         *  
         *  If a span has no connections to external regions or is completely surrounded by other
         *  regions (an island), its flag will be zero.
         *  
         *  If a span is connected to one or more external regions then the flag will be a 4 bit
         *  value where connections are recorded as follows:
         *      bit1 = neighbor0
         *      bit2 = neighbor1
         *      bit3 = neighbor2
         *      bit4 = neighbor3
         *  With the meaning of the bits as follows:
         *      0 = neighbor in same region.
         *      1 = neighbor not in same region. (Neighbor may be the null region
         *          or a real region.)
         */
        final IHeightfieldIterator<OpenHeightSpan> iter = sourceField.dataIterator();
        while(iter.hasNext())
        {
            // Note:  This algorithm first sets the flag bits such that 1 = "neighbor is in the same
            // region".  At the end it inverts the bits so flags are as expected.
            final OpenHeightSpan span = iter.next();
            // Default to "not connected to any external region".
            span.flags = 0;
            if (span.regionID() == OpenHeightfield.NULL_REGION_ID)
                // Don't care about spans in the null region.
                continue;
            // Loop through all directions.
            for (int dir = 0; dir < 4; dir++)
            {
                // Default to show neighbor is in null region.
                int nRegionID = OpenHeightfield.NULL_REGION_ID;  
                final OpenHeightSpan nSpan = span.getNeighbor(dir);
                if (nSpan != null)
                    // There is a neighbor in the current direction.
                    // Get its region ID.
                    nRegionID = nSpan.regionID();
                if (span.regionID() == nRegionID)
                    // Neighbor is in same region as this span.  Set the bit for this
                    // neighbor to 1.  (Will be inverted later.)
                    span.flags |= (1 << dir);
            }
            // Invert the bits so a bit value of 1 indicates neighbor NOT in same region.
            span.flags ^= 0xf;
            if (span.flags == 0xf)
                // This is an island span.  (All neighbors are other regions.)
                // Get rid of flags.
                span.flags = 0;
        }
        
        /*
         * These are working lists whose content changes with each iteration of the up coming loop.
         * They represent the detailed and simple contour vertices.
         * Initial sizing is arbitrary.
         */
        final ArrayList<Integer> workingRawVerts = new ArrayList<Integer>(256);
        final ArrayList<Integer> workingDetailVerts = new ArrayList<Integer>(64);
        
        /*
         * Loop through all spans looking for spans on the edge of a region.
         * 
         * At this point, only spans with flags != 0 are edge spans that
         * are part of a region contour.
         * 
         * The process of building a contour will clear the flags on all spans
         * that make up the contour.  This ensures that the spans that make up a contour are only 
         * processed once.
         */
        iter.reset();
        while(iter.hasNext())
        {
            final OpenHeightSpan span = iter.next();
            if (span.regionID() == OpenHeightfield.NULL_REGION_ID || span.flags == 0)
                // Span is either: Part of the null region, does not represent an edge span,
                // or was already processed during an earlier iteration.
                continue;
            workingRawVerts.clear();
            workingDetailVerts.clear();
            // The span is part of an unprocessed region's contour.
            // Locate a direction of the span's edge which points toward another region.
            // (We know there is at least one.)
            int startDir = 0;
            while ((span.flags & (1 << startDir)) == 0)
                // This is not an edge direction.  Try the next one.
                startDir++;
            // We now have a span that is part of a contour and a direction that points 
            // to a different region (null or real).
            // Build the contour.
            buildRawContours(span, iter.widthIndex(), iter.depthIndex(), startDir, workingRawVerts);
            // Perform post processing on the contour  E.g. Non-null region edges made straight.
            // Apply various algorithms per configuration.
            performContourPostProcessing(workingRawVerts, workingDetailVerts);
            // Remove segments that will cause problems for the triangulation.
            removeInvalidSegments(workingDetailVerts);
            
            if (workingDetailVerts.size() / 4 < 3)
                // Contour is two small to form a shape.  Skip it.
                // TODO: EVAL: This is currently a silent error.  Add logging?
                continue;
            result.add(new Contour(span.regionID(), workingRawVerts, workingDetailVerts));
        }
        
        if (result.size() != sourceField.regionCount())
            /*
             * The only time this should occur is if an invalid contour was formed
             * or if a region somehow resulted in multiple contours (bad region data).
             * While it may not be a fatal error, it should be addressed since
             * this can result in odd, hard to spot anomalies later in the pipeline.
             */
            return null;
        
        return result;
        
    }

    /**
     * Walk around the edge of this span's region gathering vertices that represent the corners of each span
     * on the sides that are external facing.  
     * <p>There will be two or three vertices for each edge span:
     * Two for spans that don't represent a change in edge direction.  Three for spans
     * that represent a change in edge direction.<p>
     * <p>The output array will contain vertices ordered as follows: (x, y, z, regionID)
     * where regionID is the region (null or real) that this vertex is considered to be connected to.</p>
     * <p>WARNING: Only run this operation on spans that are already known to be on a region edge.
     * The direction must also be pointing to a valid edge.  Otherwise behavior will be undefined.</p>
     * @param startSpan A span that is known to be on the edge of a region.  (Part of a region contour.)
     * @param startWidthIndex The width index of the starting span.
     * @param startDepthIndex The depth index of the starting span.
     * @param startDirection The direction of the edge of the span that is known to point
     * across the region edge.
     * @param outContourVerts The list of vertices that represent the edge of the region. (Plus region information.)
     */
    private void buildRawContours(OpenHeightSpan startSpan
            , int startWidthIndex
            , int startDepthIndex
            , int startDirection
            , ArrayList<Integer> outContourVerts)
    {
        
        /*
         * Flaw in Algorithm:
         * 
         * This method of contour generation can result in an inappropriate impassable seam 
         * between two adjacent regions in the following case:
         * 
         * 1. One region connects to another region on two sides in an uninterrupted manner.  (Visualize
         * one region wrapping in an L shape around the corner of another.)
         * 2. At the corner shared by the two regions, a change in height occurs.
         * 
         * In this case, the two regions should share a corner vertex.  (An obtuse corner vertex for one region and
         * an acute corner vertex for the other region.)
         * 
         * In reality, though this algorithm will select the same (x, z) coordinates for each region's corner vertex,
         * the vertex heights may differ, eventually resulting in an impassable seam.
         * 
         */
        
        /*
         * It is a bit hard to describe the stepping portion of this algorithm.  One way to visualize
         * it is to think of a robot sitting on the floor facing a known wall.  It then does the
         * following to skirt the wall:
         * 1. If there is a wall in front of it, turn clockwise in 90 degrees increments until
         *    it finds the wall is gone.
         * 2. Move forward one step.
         * 3. Turn counter-clockwise by 90 degrees.
         * 4. Repeat from step 1 until it finds itself at its original location facing its
         *    original direction.
         * 
         * See also: http://www.critterai.org/nmgen_contourgen#robotwalk
         */
        
        // Initialize to current span pointing to the edge.
        OpenHeightSpan span = startSpan;
        int dir = startDirection;
        int spanX = startWidthIndex;
        int spanZ = startDepthIndex;
        
        int loopCount = 0;
        /*
         * The loop limit is arbitrary.  It exists only to guarantee that bad input data
         * doesn't result in an infinite loop.
         * The only down side of this loop limit is that it limits the number of detectable
         * edge vertices.  (The longer the region edge and the higher the number of "turns" 
         * in a region's edge, the less edge vertices can be detected for that region.)
         */
        while (++loopCount < 65535)
        {
            
            // Note: The design of this loop is such that the span variable will always 
            // reference an edge span from the same region as the start span.
                
            if ((span.flags & (1 << dir)) != 0)
            {
                
                // The current direction is pointing toward an edge.
                // Get this edge's vertex.
                int px = spanX;
                final int py = getCornerHeight(span, dir);
                int pz = spanZ;
                /*
                 * Update the px and pz values based on current direction.
                 * The update is such that the corner being represented is
                 * clockwise from the edge the direction is currently pointing 
                 * toward.
                 */
                switch(dir)
                {
                    case 0: pz++; break;
                    case 1: px++; pz++; break;
                    case 2: px++; break;
                }
                int regionThisDirection = OpenHeightfield.NULL_REGION_ID;  // Default in case no neighbor.
                final OpenHeightSpan nSpan = span.getNeighbor(dir);
                if (nSpan != null)
                    // There is a neighbor in this direction.  Get its region ID.
                    regionThisDirection = nSpan.regionID();
                // Add the vertex to the contour.
                outContourVerts.add(px);
                outContourVerts.add(py);
                outContourVerts.add(pz);
                outContourVerts.add(regionThisDirection);
                
                // Remove the flag for this edge.  We never need to consider it again since
                // we have a vertex for this edge.
                span.flags &= ~(1 << dir);
                dir = (dir+1) & 0x3; // Rotate in clockwise direction.
            }
            else
            {
                /*
                 * The current direction does not point to an edge.  So it must point
                 * to a neighbor span in the same region as the current span.
                 * Move to the neighbor and swing the search direction back one
                 * increment (counterclockwise).  By moving the direction back one increment we
                 * guarantee we don't miss any edges.
                 */
                span = span.getNeighbor(dir);
                // Update the span index based on the direction traveled to get to this neighbor.
                switch(dir)
                {
                    case 0: spanX--; break;
                    case 1: spanZ++; break;
                    case 2: spanX++; break;
                    case 3: spanZ--; break;
                }
                dir = (dir+3) & 0x3; // Rotate counterclockwise.
            }
            
            if (span == startSpan && dir == startDirection)
                // We have returned to the starting point.  Time to stop the walk.
                // The contour is complete.
                break;            
            
        }
        
    }

    /**
     * Takes a group of vertices that represent a region contour and changes it in the following
     * manner:
     * <ul>
     * <li>For any edges that connect to non-null regions, remove all vertices except the start and end
     * vertices for that edge.  (This smoothes the edges between non-null regions into a straight line.)</li>
     * <li>Runs all algorithm's in {@link #mAlgorithms} against the contour.<li>
     * </ul>
     * @param sourceVerts  The source vertices that represent the complex contour in the form (x, y, z, regionID)
     * @param outVerts The simplified contour vertices in the form: (x, y, z, regionID)
     */
    private void performContourPostProcessing(ArrayList<Integer> sourceVerts, ArrayList<Integer> outVerts)
    {
        
        /*
         * NOTE: The forth vertex entry position in the output list contains the index of its corresponding
         * source vertex until the end of this operation.  Only at the very end is the region information
         * copied from the source to the output list.
         */
        
        boolean noConnections = true;
        // Determine if this contour has any connections to non-null regions.
        for (int pVert = 0; pVert < sourceVerts.size(); pVert += 4)
        {
            if (sourceVerts.get(pVert+3) != OpenHeightfield.NULL_REGION_ID)
            {
                // Found a non-null region connection.
                noConnections = false;
                break;
            }
        }
        
        // Seed the simplified contour with the mandatory edges.  (At least one edge.)
        if (noConnections)
        {
            /*
             * This contour represents an island region surrounded only by the null region.
             * Seed the simplified contour with the source's lower left (ll) and 
             * upper right (ur) vertices.
             */
            int llx = sourceVerts.get(0);
            int lly = sourceVerts.get(1);
            int llz = sourceVerts.get(2);
            int lli = 0;    // Will be index of source vertex, not region.
            int urx = sourceVerts.get(0);
            int ury = sourceVerts.get(1);
            int urz = sourceVerts.get(2);
            int uri = 0;    // Will be index of source vertex, not region.
            // Loop through the source contour vertices and find the ur and ll vertices.
            // TODO: EVAL: Consider switching from pointer to index format to avoid divisions.
            for (int pVert = 0; pVert < sourceVerts.size(); pVert += 4)
            {
                int x = sourceVerts.get(pVert);
                int y = sourceVerts.get(pVert+1);
                int z = sourceVerts.get(pVert+2);
                if (x < llx || (x == llx && z < llz))
                {
                    // This the new lower left vertex.
                    llx = x;
                    lly = y;
                    llz = z;
                    lli = pVert / 4;
                }
                if (x >= urx || (x == urx && z > urz))
                {
                    // This is the new upper right vertex.
                    urx = x;
                    ury = y;
                    urz = z;
                    uri = pVert / 4;
                }
            }
            // Seed the simplified contour with this edge.
            outVerts.add(llx);
            outVerts.add(lly);
            outVerts.add(llz);
            outVerts.add(lli);
            
            outVerts.add(urx);
            outVerts.add(ury);
            outVerts.add(urz);
            outVerts.add(uri);
        }
        else
        {
            // The contour shares edges with other non-null regions.
            // Seed the simplified contour with a new vertex for every location where
            // the region connection changes.  These are vertices that are important
            // because they represent portals to other regions.
            for (int iVert = 0, vCount = sourceVerts.size() / 4
                    ; iVert < vCount
                    ; iVert++)
            {
                // TODO: EVAL: Find a way to get rid of the modulus.
                if (sourceVerts.get(iVert*4+3) != sourceVerts.get(((iVert+1)%vCount)*4+3))
                {
                    // The current vertex has a different region than the
                    // next vertex.  So there is a change in vertex region.
                    outVerts.add(sourceVerts.get(iVert*4));
                    outVerts.add(sourceVerts.get(iVert*4+1));
                    outVerts.add(sourceVerts.get(iVert*4+2));
                    outVerts.add(iVert);
                }
            }
//            if (outVerts.size() < 12)
//                System.out.println("Unexpected: Out verts < 3.");
        }
        
        // Run all post processing algorithms.  These will build the final
        // simplified contour from the seeded edges.
        for (IContourAlgorithm algorithm : mAlgorithms)
        {
            algorithm.apply(sourceVerts, outVerts);
        }
        
        // Replace the index pointers in the output list with region IDs.
        final int sourceVertCount = sourceVerts.size() / 4;
        final int simplifiedVertCount = outVerts.size() / 4;
        for (int iVert = 0; iVert < simplifiedVertCount; ++iVert)
        {
            // The connected region id is taken from the next source point.
            // TODO: EVAL: Can the modulus used for wrapping be optimized out?
            final int sourceVertIndex = (outVerts.get(iVert * 4 + 3) + 1) % sourceVertCount;
            outVerts.set(iVert * 4 + 3, sourceVerts.get(sourceVertIndex * 4 + 3));
        }
    }

    /**
     * Finds the correct height to use for a particular span vertex.
     * (The vertex to the the right (clockwise) of the specified direction.)
     * @param span A span on a region edge.
     * @param direction  A direction that points to a neighbor in a different region.
     * (I.e. Crosses the border to a new region.)
     * @return The height (y-value) to use for the vertex for this edge.
     */
    private int getCornerHeight(OpenHeightSpan span, int direction)
    {
        
        /*
         * This algorithm, while it uses similar processes as Recast,
         * has been significantly adjusted.
         * 
         * Examples:  
         * - Only 3 vertices are surveyed instead of the 4
         *   the recast uses.
         * - This algorithm searches more thoroughly for the diagonal neighbor.
         * 
         * Reference: Neighbor search and nomenclature.
         * http://www.critterai.org/?q=nmgen_hfintro#nsearch
         * 
         * See also: http://www.critterai.org/nmgen_contourgen#yselection
         */
        
        // Default height to the current floor.
        int maxFloor = span.floor();
        
        // The diagonal neighbor span to this corner.
        OpenHeightSpan dSpan = null;
        
        // Rotate clockwise from original direction.
        int directionOffset = (direction + 1) & 0x3;
        
        // Check axis neighbor in current direction.
        OpenHeightSpan nSpan = span.getNeighbor(direction);
        if (nSpan != null)
        {
            // Select for maximum floor using this neighbor.
            maxFloor = Math.max(maxFloor, nSpan.floor());
            // Get diagonal neighbor.  (By looking clockwise from this neighbor.)
            dSpan = nSpan.getNeighbor(directionOffset);
        }
        // Check original span's axis-neighbor in clockwise direction.
        nSpan = span.getNeighbor(directionOffset);
        if (nSpan != null)
        {
            // Select for maximum floor using this neighbor.
            maxFloor = Math.max(maxFloor, nSpan.floor());
            if (dSpan == null)
                // Haven't found the diagonal neighbor yet.
                // Try to get it by looking counter-clockwise 
                // from this neighbor. 
                dSpan = nSpan.getNeighbor(direction);
        }
        
        if (dSpan != null)
            // The diagonal neighbor was found.
            // Select for maximum floor using this neighbor.
            maxFloor = Math.max(maxFloor, dSpan.floor());
    
        return maxFloor;
        
    }

    /**
     * The triangulation of contours (performed elsewhere) does not support certain vertex configurations.
     * This operation cleans up the contour so it can be triangulated.
     * <p>Specifically:  Merges segments such that no vertical segments exist.  (A vertical segment
     * is a segment with end points with duplicate (x, z) coordinates.)
     * @param verts  Contour vertices in the following format: (x, y, z, regionID)
     */
    private void removeInvalidSegments(ArrayList<Integer> verts)
    {
        if (verts.size() == 0)
            return;
        // Loop through all vertices starting with the last vertex. 
        for (int pVert = verts.size() - 4, pNextVert = 0
                ; pVert < verts.size()
                ; pVert += 4)
        {
            if (verts.get(pVert) == verts.get(pNextVert) &&
                    verts.get(pVert + 2) == verts.get(pNextVert + 2))
            {
                // This segment represents a vertical line.
                // Remove the first vertex in the pair.
                // TODO: EVAL: Review for optimization.  Cheaper to manually remove through
                // iteration & copy?
                verts.remove(pNextVert);
                verts.remove(pNextVert); // +1
                verts.remove(pNextVert); // +2
                verts.remove(pNextVert); // +3
            }
        }
    }

}

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

/**
 * Provides various distance heuristic operations used
 * for A* searches.
 * <p>Static operations are thread-safe.</p>
 */
public final class DistanceHeuristic 
{
    
    private DistanceHeuristic() { }

    /**
     * Returns the distance estimate from point A and point B using the 
     * requested heuristic.
     * @param type The heuristic to use.
     * @param ax The x-value of point (ax, ay, az).
     * @param ay The y-value of point (ax, ay, az).
     * @param az The z-value of point (ax, ay, az).
     * @param bx The x-value of point (bx, by, bz).
     * @param by The y-value of point (bx, by, bz).
     * @param bz The z-value of point (bx, by, bz).
     * @return The estimated distance from point A to point B.
     */
    public static float getHeuristicValue(DistanceHeuristicType type
            , float ax, float ay, float az
            , float bx, float by, float bz)
    {
        switch (type)
        {
        case LONGEST_AXIS:
            return getLongestAxis(ax, ay, az, bx, by, bz);
        case MANHATTAN:
            return getManhattan(ax, ay, az, bx, by, bz);
        }
        return 0;
    }
    
    /**
     * Returns the longest axis distance from point A to point B.
     * I.e. The longest of (ax - bx), (ay - by), or (az - bz).
     * @param ax The x-value of point (ax, ay, az).
     * @param ay The y-value of point (ax, ay, az).
     * @param az The z-value of point (ax, ay, az).
     * @param bx The x-value of point (bx, by, bz).
     * @param by The y-value of point (bx, by, bz).
     * @param bz The z-value of point (bx, by, bz).
     * @return The longest axis-distance from point A to point B.
     */
    public static float getLongestAxis(float ax, float ay, float az, float bx, float by, float bz)
    {
        return Math.max(Math.max(Math.abs(ax - bx), Math.abs(ay - by)), Math.abs(az - bz));
    }
    
    /**
     * Determines the Manhattan distance from point A to point B.
     * @param ax The x-value of point (ax, ay, az).
     * @param ay The y-value of point (ax, ay, az).
     * @param az The z-value of point (ax, ay, az).
     * @param bx The x-value of point (bx, by, bz).
     * @param by The y-value of point (bx, by, bz).
     * @param bz The z-value of point (bx, by, bz).
     * @return The Manhattan distance from point A to point B.
     * @see <a href="http://en.wikipedia.org/wiki/Manhattan_distance" target="_blank">Manhattan Distance (Wikipedia)</a>
     */
    public static float getManhattan(float ax, float ay, float az, float bx, float by, float bz)
    {
        return (Math.abs(ax - bx) + Math.abs(ay - by)) +  Math.abs(az - bz);
    }
    
}

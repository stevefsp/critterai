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
 * The supported distance heuristics.
 */
public enum DistanceHeuristicType 
{
    /**
     * The longest axis distance between two points.
     * E.g. The longest of (ax - bx), (ay - by), or (az - bz).
     */
    LONGEST_AXIS,
    
    /**
     * The Manhattan distance between two points.
     * @see <a href="http://en.wikipedia.org/wiki/Manhattan_distance" target="_blank">Manhattan Distance (Wikipedia)</a>
     */
    MANHATTAN;
    
    /**
     * A safe version of {@link #valueOf(String)} that will return
     * null if the argument is invalid.
     * @param value A string representing the exact {@link #toString()} value
     * of a valid enumeration.
     * @return The enumeration associated with the string value, or null
     * if there is no match.
     */
    public static DistanceHeuristicType valueOfSafe(String value)
    {
        try
        {
            return valueOf(value);
        }
        catch (Exception e)
        {
            return null;
        }
    }
}

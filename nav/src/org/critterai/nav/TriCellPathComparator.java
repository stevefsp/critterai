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

import java.io.Serializable;
import java.util.Comparator;

/**
 * Sorts based on cell movement cost. {@link TriCellPathNode#f()}
 * <p>This comparator imposes orderings that are inconsistent with equals.
 * E.g. (c.compare(e1, e2)==0) does not have the same boolean value as e1.equals(e2)
 * This is because a subset of fields are used for ordering.</p>
 * <p>Instances of this class are not thread-safe.</p>
 */
public final class TriCellPathComparator 
    implements Comparator<TriCellPathNode>, Serializable 
{

    private static final long serialVersionUID = 1L;

    /**
     * Compares its two arguments for order.
     * <p>Returns a value of zero if both objects have the same estimated
     * path cost.<br/>
     * Returns a value greater than zero if the path cost of nodeA 
     * exceeds the path cost of nodeB.<br/>
     * Returns a value less than zero if the path cost of nodeB exceeds
     * the cost of nodeA.<br/></p>
     * @param nodeA The first node to be compared.
     * @param nodeB The second node to be compared.
     * @return A negative integer, zero, or positive integer if the first
     * argument has lower, same, or higher path cost than the second argument.
     */
    @Override
    public int compare(TriCellPathNode nodeA, TriCellPathNode nodeB) 
        throws NullPointerException
    {
        float delta = nodeA.f() - nodeB.f();
        
        // Handle special cases.
        if (delta > 0 && delta < 1.0f) 
            return 1;
        else if (delta < 0 && delta > -1.0f) 
            return -1;
        
        return (int)delta;
    }

}

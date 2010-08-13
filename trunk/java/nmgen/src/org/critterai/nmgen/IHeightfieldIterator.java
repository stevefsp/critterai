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

import java.util.Iterator;

/**
 * Implements an iterator that will iterate through all spans within a
 * height field. (Not just the base spans.)
 * <p>Behavior of the iterator is undefined if the interator's source
 * is changed during iteration.</p>
 * @param <E> The type of data held by the heightfield.
 */
public interface IHeightfieldIterator<E>
    extends Iterator<E>
{
    /**
     * The depth index of the last span returned by {@link #next()}
     * @return The depth index of the last span returned by {@link #next()}
     */
    int depthIndex();
    
    /**
     * Resets the iterator so that it can be re-used.
     */
    void reset();
    
    /**
     * The width index of the last span returned by {@link #next()}
     * @return The width index of the last span returned by {@link #next()}
     */
    int widthIndex();
}

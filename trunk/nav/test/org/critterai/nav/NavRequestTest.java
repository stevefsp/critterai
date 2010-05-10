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

import static org.junit.Assert.assertTrue;

import org.critterai.nav.MasterNavRequest;
import org.critterai.nav.NavRequestState;
import org.critterai.nav.MasterNavRequest.NavRequest;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link NavRequest} class.
 */
public final class NavRequestTest 
{

	@Before
	public void setUp() throws Exception 
	{
	}

	@Test
	public void testPrimaryGetters() 
	{
		MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>();
		MasterNavRequest<Boolean>.NavRequest nr = mnr.request();
		assertTrue(mnr.data() == nr.data());
		assertTrue(mnr.state() == nr.state());
		mnr.set(NavRequestState.COMPLETE, true);
		assertTrue(mnr.data() == nr.data());
		assertTrue(mnr.state() == nr.state());
	}

	@Test
	public void testIsComplete() 
	{
		MasterNavRequest<Boolean> mnr = new MasterNavRequest<Boolean>(NavRequestState.PROCESSING);
		MasterNavRequest<Boolean>.NavRequest nr = mnr.request();
		assertTrue(nr.isFinished() == false);
		mnr.setState(NavRequestState.FAILED);
		assertTrue(nr.isFinished() == true);
		mnr.setState(NavRequestState.COMPLETE);
		assertTrue(nr.isFinished() == true);
	}

}

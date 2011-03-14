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
#include <string.h>
#include "Rcn.h"

RCNBuildContext::RCNBuildContext()
    : rcContext(false), mMessageCount(0), mTextPoolSize(0), messageDetail(1)
{
    m_logEnabled = true;
}

RCNBuildContext::~RCNBuildContext()
{
}

void RCNBuildContext::doResetLog()
{
    mMessageCount = 0;
    mTextPoolSize = 0;
}

void RCNBuildContext::doLog(const rcLogCategory category
    , const char* message
    , const int messageLength)
{
    // Design Note: The category is not used.

    // Process early exits.
    if (messageLength == 0)
        return;
    if (mMessageCount >= MAX_MESSAGES)
        return;
    int remainingSpace = MESSAGE_POOL_SIZE - mTextPoolSize;
    if (remainingSpace < 1)
	    return;

    // Store message
    const int realLength = rcMin(messageLength+1, remainingSpace-1);
    char* pEntry = &mTextPool[mTextPoolSize];
    memcpy(pEntry, message, realLength);
    pEntry[realLength-1] = '\0';
    mTextPoolSize += realLength;
    mMessages[mMessageCount++] = pEntry;
}

int RCNBuildContext::getMessageCount() const
{
    return mMessageCount;
}

const char* RCNBuildContext::getMessage(const int i) const
{
    return mMessages[i];
}

int RCNBuildContext::getMessagePoolLength() const { return mTextPoolSize; }
const char* RCNBuildContext::getMessagePool() const { return mTextPool; }
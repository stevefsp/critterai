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
#include "Recast.h"
#pragma once

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

namespace org 
{
namespace critterai
{
namespace nmgen
{
    /*
     * Documentation Standard: When possible, API documentation will be located 
     * with the definition rather than the declaration.
     */

    static const float EPSILON = 0.00001f;
    static const float TOLERANCE = 0.0001f;
    static const float MAX_ALLOWED_SLOPE = 85.0f;
    static const int MAX_SMOOTHING = 4;

    struct Configuration
    {
        // Design Note: Don't change the order without good reason.
        // Interop implementations depend on this order.
        float xzResolution;
        float yResolution;
        float minTraversableHeight;
        float maxTraversableStep;
        float maxTraversableSlope;
        float traversableAreaBorderSize;
        float heightfieldBorderSize;
        float maxEdgeLength;
        float edgeMaxDeviation;
        float contourSampleDistance;
        float contourMaxDeviation;
        int smoothingThreshold;
        int minUnconnectedRegionSize;
        int mergeRegionSize;
        int maxVertsPerPoly;
        bool clipLedges;

        /// Apply the mandatory limits to the configuration.
        /**
         * 
        */
        void applyLimits()
        {
            xzResolution = rcMax(EPSILON, xzResolution);
            yResolution = rcMax(EPSILON, yResolution);
            minTraversableHeight = rcMax(EPSILON, minTraversableHeight);
            maxTraversableStep = rcMax(0.0f, maxTraversableStep);
            maxTraversableSlope = 
                rcMin(MAX_ALLOWED_SLOPE, rcMax(0.0f, maxTraversableSlope));
            traversableAreaBorderSize = rcMax(0.0f, traversableAreaBorderSize);
            smoothingThreshold = 
                rcMin(MAX_SMOOTHING, rcMax(0, smoothingThreshold));
            minUnconnectedRegionSize = rcMax(1, minUnconnectedRegionSize);
            mergeRegionSize = rcMax(0, mergeRegionSize) ;
            maxEdgeLength = rcMax(0.0f, maxEdgeLength);
            edgeMaxDeviation = rcMax(0.0f, edgeMaxDeviation);
            maxVertsPerPoly = rcMax(3, maxVertsPerPoly);
            contourSampleDistance = contourSampleDistance < 0.9f ? 
                0 : contourSampleDistance;
            contourMaxDeviation  = rcMax(0.0f, contourMaxDeviation);
            heightfieldBorderSize = rcMax(0.0f, heightfieldBorderSize);
        }
    };

    class BuildContext : public rcContext
    {	
    public:
	    static const int MAX_MESSAGES = 1000;
	    static const int MESSAGE_POOL_SIZE = 12000;

	    BuildContext();
	    virtual ~BuildContext();
    	
	    int getMessageCount() const;
	    const char* getMessage(const int i) const;

        int getMessagePoolLength() const;
        const char* getMessagePool() const;

        bool getLogEnabled() const { return m_logEnabled; }

    protected:
	    virtual void doResetLog();
	    virtual void doLog(const rcLogCategory category
            , const char* msg
            , const int len);

    private:
	    const char* mMessages[MAX_MESSAGES];
	    int mMessageCount;

	    char mTextPool[MESSAGE_POOL_SIZE];
	    int mTextPoolSize;
    };

    template<class T> inline bool sloppyEquals(T a, T b) 
    { 
        return !(b < a - TOLERANCE || b > a + TOLERANCE);
    };

extern "C"
{

    extern "C" EXPORT_API void freeMesh(float** vertices, int** triangles);

    extern "C" EXPORT_API void applyStandardLimits(Configuration* config);

    extern "C" EXPORT_API bool buildSimpleMesh(Configuration config
        , float* sourceVerts
        , int sourceVertsLength
        , int* sourceTriangles
        , int sourceTrianglesLength
        , float** resultVerts
        , int* resultVertLength
        , int** resultTriangles
        , int* resultTrianglesLength
        , char* messages
        , int messagesLength
        , int messageDetail);

}

}
}
}
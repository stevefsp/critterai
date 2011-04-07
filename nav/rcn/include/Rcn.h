/*
 * Copyright (c) 2011 Stephen A. Pratt
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
#ifndef CAIRCN_EX_H
#define CAIRCN_EX_H

#include "Recast.h"
#include "DetourNavmesh.h"
#include "DetourNavMeshQuery.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

static const float RCN_EPSILON = 0.00001f;
static const float RCN_TOLERANCE = 0.0001f;

static const int MDETAIL_NONE = 0;
static const int MDETAIL_BRIEF = 1;
static const int MDETAIL_SUMMARY = 2;
static const int MDETAIL_TRACE = 4;

struct RCNMessageBuffer
{
    int messageDetail;
    int size;
    char* buffer;
};

struct RCNMesh3
{
    int vertCount;
    int polyCount;
    float* vertices;
    int* indices;
};

struct RCNOffMeshConnections
{
    static const int MAX_CONNECTIONS = 256;
    int count;
    float* verts;
    float* radii;
    unsigned char* dirs;
    unsigned char* areas;
    unsigned short* flags;
    unsigned int* ids;
};

static const float RCN_MAX_ALLOWED_SLOPE = 85.0f;
static const int RCN_MAX_SMOOTHING = 4;

struct RCNNavMeshConfig
{
    // Design Note: Don't change the order without good reason.
    // Interop implementations depend on this order.
    float xzCellSize;
    float yCellSize;
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
    int minIslandRegionSize;
    int mergeRegionSize;
    int maxVertsPerPoly;
    bool clipLedges;

    // Apply the mandatory limits to the configuration.
    void applyLimits()
    {
        xzCellSize = rcMax(RCN_EPSILON, xzCellSize);
        yCellSize = rcMax(RCN_EPSILON, yCellSize);
        minTraversableHeight = rcMax(RCN_EPSILON, minTraversableHeight);
        maxTraversableStep = rcMax(0.0f, maxTraversableStep);
        maxTraversableSlope = 
            rcMin(RCN_MAX_ALLOWED_SLOPE, rcMax(0.0f, maxTraversableSlope));
        traversableAreaBorderSize = rcMax(0.0f, traversableAreaBorderSize);
        smoothingThreshold = 
            rcMin(RCN_MAX_SMOOTHING, rcMax(0, smoothingThreshold));
        minIslandRegionSize = rcMax(1, minIslandRegionSize);
        mergeRegionSize = rcMax(0, mergeRegionSize) ;
        maxEdgeLength = rcMax(0.0f, maxEdgeLength);
        edgeMaxDeviation = rcMax(0.0f, edgeMaxDeviation);
        maxVertsPerPoly = rcClamp(maxVertsPerPoly, 3, DT_VERTS_PER_POLYGON);
        contourSampleDistance = contourSampleDistance < 0.9f ? 
            0 : contourSampleDistance;
        contourMaxDeviation  = rcMax(0.0f, contourMaxDeviation);
        heightfieldBorderSize = rcMax(0.0f, heightfieldBorderSize);
        
    }
};

class RCNBuildContext : public rcContext
{
public:
    static const int MAX_MESSAGES = 1000;
    static const int MESSAGE_POOL_SIZE = 12000;

    RCNBuildContext();
    virtual ~RCNBuildContext();
	
    int getMessageCount() const;
    const char* getMessage(const int i) const;

    int getMessagePoolLength() const;
    const char* getMessagePool() const;

    bool getLogEnabled() const { return m_logEnabled; }

    int messageDetail;

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

void rcnTransferMessages(RCNBuildContext& context
    , RCNMessageBuffer& messages);

template<class T> inline bool rcnSloppyEquals(T a, T b) 
{ 
    return !(b < a - RCN_TOLERANCE || b > a + RCN_TOLERANCE);
};

bool rcnBuildBaseRCNavMesh(RCNNavMeshConfig config
        , RCNMesh3* sourceMesh
        , unsigned char* pAreas
        , RCNBuildContext* context
        , rcPolyMesh& polyMesh
        , rcPolyMeshDetail& detailMesh);

extern "C"
{
    EXPORT_API void rcnFreeMessageBuffer(RCNMessageBuffer* pMessages);
    EXPORT_API void rcnApplyNavMeshConfigLimits(RCNNavMeshConfig* pConfig);
    EXPORT_API void rcnFreeMesh3(RCNMesh3* pMesh);

    EXPORT_API bool rcnBuildRCNavMesh(RCNNavMeshConfig config
        , RCNMesh3* pSourceMesh
        , unsigned char* pAreas
        , RCNMessageBuffer* pMessages
        , rcPolyMesh* pPolyMesh
        , rcPolyMeshDetail* pDetailMesh);

    EXPORT_API dtStatus rcnBuildStaticDTNavMesh(rcPolyMesh* pPolyMesh
        , rcPolyMeshDetail* pDetailMesh
        , float walkableHeight
        , float walkableRadius
        , float walkableClimb
        , RCNOffMeshConnections* pOffMeshConnections
        , dtNavMesh** ppNavMesh);

    EXPORT_API dtStatus rcnBuildDTNavQuery(dtNavMesh* pNavMesh
        , const int maxNodes
        , dtNavMeshQuery** ppNavQuery);

    EXPORT_API bool rcnFlattenDetailMesh(rcPolyMeshDetail* detailMesh
        , RCNMesh3* resultMesh);
}

#endif
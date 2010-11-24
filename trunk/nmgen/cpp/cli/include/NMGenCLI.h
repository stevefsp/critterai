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
#pragma once
#include "NMGen.h"
#pragma managed

using namespace System;
using namespace System::Collections::Generic;

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

    /// <summary>
    /// Provides the 
    /// <a href="http://www.critterai.org/nmgen_settings" target="_parent">
    /// configuration</a> settings to use when building a navigation mesh.
    /// </summary>
    /// <remarks>
    /// <a href="http://www.critterai.org/nmgen_settings" target="_parent">
    /// Detailed information on configuration settings.</a>
    /// </remarks>
    public ref class BuildConfig sealed
    {
    public:

        /// <summary>
        /// The maximum smoothing allowed when limits are applied.
        /// </summary>
        static const int MaxSmoothing = MAX_SMOOTHING;

        /// <summary>
        /// The maximum slope allowed when limits are applied.
        /// </summary>
        static const float MaxAllowedSlope = MAX_ALLOWED_SLOPE;

        /// <summary>
        /// The standard lower limit applied to values that cannot be
        /// less then or equal to zero.
        /// </summary>
        static const float StandardEpsilon = EPSILON;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xzCellSize">
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// </param>
        /// <param name="yCellSize">
        /// The y-axis voxel size to use when sampling the source geometry.
        /// </param>
        /// <param name="minTraversableHeight">
        /// Minimum floor to ceiling height that will still allow the
        /// floor area to be considered traversable. (Usually the maximum
        /// agent height.)
        /// </param>
        /// <param name="maxTraversableStep">
        /// Maximum ledge height that is considered to still be
        /// traversable.  Allows the mesh to flow over curbs and up/down
        /// stairways. (Usually how far up/down the agent can step.)
        /// </param>
        /// <param name="maxTraversableSlope">
        /// The maximum slope that is considered traversable. (In degrees.)
        /// </param>
        /// <param name="clipLedges">
        /// Indicates whether ledges should be considered un-walkable
        /// </param>
        /// <param name="traversableAreaBorderSize">
        /// Represents the closest any part of a mesh can get to an
        /// obstruction in the source geometry. (Usually the agent radius.)
        /// </param>
        /// <param name="heightfieldBorderSize">
        /// The closest the mesh may come to the xz-plane AABB of the
        /// source geometry.
        /// </param>
        /// <param name="smoothingThreshold">
        /// The amount of smoothing to be performed when generating the
        /// distance field used for deriving regions.
        /// </param>
        /// <param name="minIslandRegionSize">
        /// The minimum size allowed for isolated island meshes. (XZ cells.)
        /// (Prevents the formation of meshes that are too small to be"
        /// of use.)
        /// </param>
        /// <param name="mergeRegionSize">
        /// Any regions smaller than this size will, if possible, be
        /// merged with larger regions. (XZ cells.)
        /// </param>
        /// <param name="maxEdgeLength">
        /// The maximum length of polygon edges along the border of the
        /// mesh. (Extra vertices will be inserted if needed.)
        /// </param>
        /// <param name="edgeMaxDeviation">
        /// The maximum distance the edges of the mesh may deviate from
        /// the source geometry. (Applies only to the xz-plane.)
        /// </param>
        /// <param name="maxVertsPerPoly">
        /// The maximum number of vertices per polygon for polygons
        /// generated during the contour to polygon conversion process.
        /// </param>
        /// <param name="contourSampleDistance">
        /// Sets the sampling distance to use when matching the
        /// mesh surface to the source geometry.
        /// </param>
        /// <param name="contourMaxDeviation">
        /// The maximum distance the mesh surface can deviate from the
        /// surface of the source geometry. 
        /// </param>
        BuildConfig(float xzCellSize
            , float yCellSize
            , float minTraversableHeight
            , float maxTraversableStep
            , float maxTraversableSlope
            , bool clipLedges
            , float traversableAreaBorderSize
            , float heightfieldBorderSize
            , int smoothingThreshold
            , int minIslandRegionSize
            , int mergeRegionSize
            , float maxEdgeLength
            , float edgeMaxDeviation
            , int maxVertsPerPoly
            , float contourSampleDistance
            , float contourMaxDeviation)
        {
            pConfig = new Configuration();
            pConfig->xzCellSize = xzCellSize;
            pConfig->yCellSize = yCellSize;
            pConfig->minTraversableHeight = minTraversableHeight;
            pConfig->maxTraversableStep = maxTraversableStep;
            pConfig->maxTraversableSlope = maxTraversableSlope;
            pConfig->traversableAreaBorderSize = traversableAreaBorderSize;
            pConfig->heightfieldBorderSize = heightfieldBorderSize;
            pConfig->maxEdgeLength = maxEdgeLength;
            pConfig->edgeMaxDeviation = edgeMaxDeviation;
            pConfig->contourSampleDistance = contourSampleDistance;
            pConfig->contourMaxDeviation = contourMaxDeviation;
            pConfig->smoothingThreshold = smoothingThreshold;
            pConfig->minIslandRegionSize = minIslandRegionSize;
            pConfig->mergeRegionSize = mergeRegionSize;
            pConfig->maxVertsPerPoly = maxVertsPerPoly;
            pConfig->clipLedges = clipLedges;
        };

        ~BuildConfig() { this->!BuildConfig(); };

        /// <summary>
        /// Disposes of the unmanaged memory allocated by the class.
        /// </summary>
        !BuildConfig()
        {
            delete pConfig;
            pConfig = 0;
        };

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// </summary>
        property float XZCellSize 
        { 
            float get() { return pConfig->xzCellSize; } 
        };

        /// <summary>
        /// The y-axis voxel size to use when sampling the source geometry.
        /// </summary>
        property float YCellSize 
        { 
            float get() { return pConfig->yCellSize; } 
        };

        /// <summary>
        /// Minimum floor to ceiling height that will still allow the
        /// floor area to be considered traversable. (Usually the maximum
        /// agent height.)
        /// </summary>
        property float MinTraversableHeight 
        { 
            float get() { return pConfig->minTraversableHeight; } 
        };

        /// <summary>
        /// Maximum ledge height that is considered to still be
        /// traversable.  Allows the mesh to flow over curbs and up/down
        /// stairways. (Usually how far up/down the agent can step.)
        /// </summary>
        property float MaxTraversableStep 
        { 
            float get() { return pConfig->maxTraversableStep; } 
        };

        /// <summary>
        /// The maximum slope that is considered traversable. (In degrees.)
        /// </summary>
        property float MaxTraversableSlope 
        { 
            float get() { return pConfig->maxTraversableSlope; } 
        };

        /// <summary>
        /// Represents the closest any part of a mesh can get to an
        /// obstruction in the source geometry. (Usually the agent radius.)
        /// </summary>
        property float TraversableAreaBorderSize 
        { 
            float get() { return pConfig->traversableAreaBorderSize; } 
        };

        /// <summary>
        /// The closest the mesh may come to the xz-plane AABB of the
        /// source geometry.
        /// </summary>
        property float HeightfieldBorderSize 
        { 
            float get() { return pConfig->heightfieldBorderSize; } 
        };

        /// <summary>
        /// The maximum length of polygon edges along the border of the
        /// mesh. (Extra vertices will be inserted if needed.)
        /// </summary>
        property float MaxEdgeLength 
        { 
            float get() { return pConfig->maxEdgeLength; } 
        };

        /// <summary>
        /// The maximum distance the edges of the mesh may deviate from
        /// the source geometry. (Applies only to the xz-plane.)
        /// </summary>
        property float EdgeMaxDeviation 
        { 
            float get() { return pConfig->edgeMaxDeviation; } 
        };

        /// <summary>
        /// Sets the sampling distance to use when matching the
        /// mesh surface to the source geometry.
        /// </summary>
        property float ContourSampleDistance 
        { 
            float get() { return pConfig->contourSampleDistance; } 
        };

        /// <summary>
        /// The maximum distance the mesh surface can deviate from the
        /// surface of the source geometry. 
        /// </summary>
        property float ContourMaxDeviation 
        { 
            float get() { return pConfig->contourMaxDeviation; } 
        };

        /// <summary>
        /// The amount of smoothing to be performed when generating the
        /// distance field used for deriving regions.
        /// </summary>
        property int SmoothingThreshold 
        { 
            int get() { return pConfig->smoothingThreshold; } 
        };

        /// <summary>
        /// The minimum size allowed for isolated island meshes. (XZ cells.)
        /// (Prevents the formation of meshes that are too small to be"
        /// of use.)
        /// </summary>
        property int MinIslandRegionSize 
        { 
            int get() { return pConfig->minIslandRegionSize; } 
        };

        /// <summary>
        /// Any regions smaller than this size will, if possible, be
        /// merged with larger regions. (XZ cells.)
        /// </summary>
        property int MergeRegionSize 
        { 
            int get() { return pConfig->mergeRegionSize; } 
        };

        /// <summary>
        /// The maximum number of vertices per polygon for polygons
        /// generated during the contour to polygon conversion process.
        /// </summary>
        property int MaxVertsPerPoly
        { 
            int get() { return pConfig->maxVertsPerPoly; } 
        };

        /// <summary>
        /// Indicates whether ledges should be considered un-walkable
        /// </summary>
        property bool ClipLedges 
        { 
            bool get() { return pConfig->clipLedges; } 
        };

        /// <summary>
        /// Updates the configuration so that it does not include any 
        /// completely invalid settings. 
        /// </summary>
        /// <remarks>
        /// <p>Does not guard against a poor quality configuration.</p>
        /// <p>This method is useful as an initial validation pass.  But that is 
        /// all. Since extreme edge cases are supported by by NMGen, 
        /// more validation is likely needed.  For example: A negative 
        /// xzCellSize is never valid.  So this method will fix that.  But 
        /// in a tiny number of cases, an xzCellSize of 0.01 is valid.  So 
        /// this operation won't fix that, even though in 99.99% of the cases
        /// 0.01 is not valid.</p>
        /// </remarks>
        void ApplyLimits() { pConfig->applyLimits(); };

    private:
        Configuration* pConfig;
    };

    /// <summary>
    /// Represents a simple triangle mesh.
    /// </summary>
    public ref class TriangleMesh sealed
    {
    public:

        /// <summary>
        /// The triangle indices in the form 
        /// (vertAIndex, vertBIndex, vertCIndex).
        /// </summary>
        array<int>^ triangles;

        /// <summary>
        /// The vertices in the form (x, y, z).
        /// </summary>
        array<float>^ vertices;

        TriangleMesh(int vertLength, int triLength);

        array<float>^ GetTriangleVerts(int index);

        // Odd doc generation issue.  Doc for properties is not being
        // detected in the definition.  So putting it here.

        /// <summary>
        /// The number of triangles in the mesh.
        /// </summary>
        property int TriangleCount { int get(); };

        /// <summary>
        /// The number of vertices in the mesh.
        /// </summary>
        property int VertCount { int get(); };
    };

    /// <summary>
    /// Provides methods used to generate navigation meshes.
    /// </summary>
    public ref class MeshBuilder
    {
    public:
        static TriangleMesh^ BuildSimpleMesh(
            array<float>^ sourceVerts
            , array<int>^ sourceTriangles
            , BuildConfig^ config
            , List<String^>^ messages
            , int messageDetail);
    private:
        MeshBuilder() { };
    };
}
}
}

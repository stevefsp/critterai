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

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents the build state for an <see cref="IncrementalBuilder"/>
    /// object.
    /// </summary>
    public enum BuildState
    {
        /// <summary>
        /// The build process is not active. (Not initialized, not ready.)
        /// </summary>
        Inactive,

        /// <summary>
        /// The build was aborted due to an error.
        /// </summary>
        Aborted,

        /// <summary>
        /// The build was completed.  Valid mesh data is available.
        /// </summary>
        Complete,

        /// <summary>
        /// At the step to clear unwalkable triangles.
        /// </summary>
        ClearUnwalkableTris,

        /// <summary>
        /// At the step to build the heightfield.
        /// </summary>
        HeightfieldBuild,

        /// <summary>
        /// At the step to build the compact heightfield.
        /// </summary>
        CompactFieldBuild,

        /// <summary>
        /// At the step to perform various optional span marking operations.
        /// </summary>
        MarkSpans,

        /// <summary>
        /// At the step to apply area markers to the heightfield.
        /// </summary>
        ApplyAreaMarkers,

        /// <summary>
        /// At the step to erode the heightfield's walkable area.
        /// </summary>
        ErodeWalkableArea,

        /// <summary>
        /// At the step to build the heightfield's distance field.
        /// </summary>
        DistanceFieldBuild,

        /// <summary>
        /// At the step to build the heightfield's regions.
        /// </summary>
        RegionBuild,

        /// <summary>
        /// At the step to build raw and detail contours.
        /// </summary>
        ContourBuild,

        /// <summary>
        /// At the step to build the polygon mesh.
        /// </summary>
        PolyMeshBuild,

        /// <summary>
        /// At the step to build the detail mesh.
        /// </summary>
        DetailMeshBuild
    }
}

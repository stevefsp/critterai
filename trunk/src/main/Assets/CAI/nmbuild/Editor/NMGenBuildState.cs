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
    /// <remarks>There are three finished states: <see cref="Complete"/>,
    /// <see cref="NoResult"/>, and <see cref="Aborted"/>.</remarks>
    public enum BuildState
    {
        /// <summary>
        /// The builder is initialized and ready to start the build.
        /// </summary>
        Initialized = 0,

        /// <summary>
        /// The build was aborted due to an error. (Finished state.)
        /// </summary>
        Aborted,

        /// <summary>
        /// The build was completed, producing a result. (Finished state.)
        /// </summary>
        /// <remarks>
        /// It is possible to complete, but have no resulting mesh.
        /// See <see cref="NoResult"/>.
        /// </remarks>
        Complete,

        /// <summary>
        /// The build completed without producing a result. (Finished state.)
        /// </summary>
        /// <remarks>
        /// <para>This state will result in the following cases:</para>
        /// <ul>
        /// <li>There is no source geometry for the build. 
        /// (E.g None within the build bounds.)</li>
        /// <li>The are no heightfield spans at the end of the voxeliation stage.</li>
        /// <li>There are no regions at the end of the region generation stage.</li>
        /// </ul>
        /// <para>While having no result is usually considered a failure
        /// when building a single tile mesh, it is not unexpected for tiled
        /// meshes. (Some tiles may not contain geometry.)</para>
        /// </remarks>
        NoResult,

        /// <summary>
        /// At the step to clear unwalkable triangles.
        /// </summary>
        ClearUnwalkableTris,

        /// <summary>
        /// At the step to build the heightfield.
        /// </summary>
        HeightfieldBuild,

        HeightfieldPostProcess,

        /// <summary>
        /// At the step to build the compact heightfield.
        /// </summary>
        CompactFieldBuild,

        /// <summary>
        /// At the step to perform various optional span marking operations.
        /// </summary>
        MarkHeightfieldSpans,

        /// <summary>
        /// At the step where mid-processors are applied to the compact
        /// heightfield.
        /// </summary>
        /// <remarks>
        /// <para>"Mid-processors" are processors that are not marked as post processors.
        /// The processors are run immediately after the compact heightfield is
        /// created, before the region generation process is begun.</para>
        /// </remarks>
        CompactFieldMidProcess,

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

        PolyMeshPostProcess,

        /// <summary>
        /// At the step to build the detail mesh.
        /// </summary>
        DetailMeshBuild,

        DetailMeshPostProcess
    }
}

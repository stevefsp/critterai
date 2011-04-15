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
using System;
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn.externs
{
    public static class NMGenUtilEx
    {
        /// <summary>
        /// Applies the standard min/max limits to the provided configuration.
        /// </summary>
        /// <param name="config">The configuration to check and update.</param>
        [DllImport("cai-nav-rcn", EntryPoint = "rcnApplyNavMeshConfigLimits")]
        public static extern void ApplyStandardLimits(ref NMGenParams config);

        /// <summary>
        /// Generates navigation mesh data that can be used to create
        /// a <see cref="Navmesh"/> object.
        /// </summary>
        /// <remarks>
        /// The polygon and detail meshes created by this method must be
        /// freed using the appropriate FreeEx method for each type of
        /// structure.  Otherwise a memory leak will occur.
        /// </remarks>
        /// <param name="config">The configuration parameters to use
        /// during the build.</param>
        /// <param name="sourceMesh">The source geometry to use for the build.
        /// </param>
        /// <param name="areas">The area ids for the triangles in the source
        /// geometry. (Optional)</param>
        /// <param name="polyMesh">The resulting polygon mesh. Null if the build
        /// fails.</param>
        /// <param name="detailMesh">The resulting polygon detail mesh.
        /// Null if the build fails.</param>
        /// <param name="messageBuffer">A message buffer to hold trace
        /// messages from the build. (Optional)</param>
        /// <param name="messageBufferSize">The size of the message bugger.
        /// </param>
        /// <returns>TRUE if the build was successful.</returns>
        [DllImport("cai-nav-rcn", EntryPoint = "rcnBuildRCNavMesh")]
        public static extern bool BuildMesh(NMGenParams config
            , ref TriMesh3Ex sourceMesh
            , [In] byte[] areas
            , ref PolyMeshEx polyMesh
            , ref PolyMeshDetailEx detailMesh
            , [In, Out] byte[] messageBuffer
            , int messageBufferSize);

        /// <summary>
        /// Flattens a polygon detail mesh into a triangle mesh structure.
        /// </summary>
        /// <remarks>The result mesh created by this method must be freed
        /// using the <see cref="TriMesh3Ex.FreeEx"/> method.  Otherwise
        /// a memory leak will occur.</remarks>
        /// <param name="detailMesh">The polygon detail mesh to flatten.
        /// </param>
        /// <param name="resultMesh">The resulting triangle mesh. Null if
        /// the method fails.</param>
        /// <returns>TRUE if the triangle mesh was successfully created.
        /// </returns>
        [DllImport("cai-nav-rcn", EntryPoint = "rcnFlattenDetailMesh")]
        public static extern bool FlattenDetailMesh(
            ref PolyMeshDetailEx detailMesh
            , ref TriMesh3Ex resultMesh);
    }
}

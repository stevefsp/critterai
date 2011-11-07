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
using UnityEngine;
using System.Collections.Generic;

namespace org.critterai.nmgen.u3d
{
    /// <summary>
    /// Provides various utility methods related to navigation mesh generation.
    /// </summary>
    public static class NMGenUtil
    {
        /// <summary>
        /// Returns human friendly text for the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Human friendly text.</returns>
        public static string GetBuildStateText(BuildState state)
        {
            switch (state)
            {
                case BuildState.Inactive:
                    return "Inactive";
                case BuildState.Aborted:
                    return "Aborted.";
                case BuildState.CompactFieldBuild:
                    return "Building compact heightfield.";
                case BuildState.Complete:
                    return "Complete";
                case BuildState.ContourBuild:
                    return "Building contours.";
                case BuildState.DetailMeshBuild:
                    return "Building detail mesh.";
                case BuildState.DistanceFieldBuild:
                    return "Building distance field.";
                case BuildState.ErodeWalkableArea:
                    return "Eroding walkable area.";
                case BuildState.HeightfieldBuild:
                    return "Building heightfield.";
                case BuildState.MarkSpans:
                    return "Marking heightfield spans.";
                case BuildState.ClearUnwalkableTris:
                    return "Clearing unwalkable triangles.";
                case BuildState.PolyMeshBuild:
                    return "Building polygon mesh.";
                case BuildState.RegionBuild:
                    return "Building regions.";
                case BuildState.ApplyAreaMarkers:
                    return "Applying area markers.";
            }
            return "Error";  
        }

        /// <summary>
        /// Returns a progress value associated with the specified state. 
        /// </summary>
        /// <remarks>
        /// The value will be between 0 and 1.0 and is suitable for providing
        /// build progress feedback.
        /// </remarks>
        /// <param name="state">The state.</param>
        /// <returns>A progress value for teh state.</returns>
        public static float GetBuildProgress(BuildState state)
        {
            switch (state)
            {
                case BuildState.Inactive:
                    return 0;
                case BuildState.ClearUnwalkableTris:
                    return 0;
                case BuildState.HeightfieldBuild:
                    return 0.05f;
                case BuildState.MarkSpans:
                    return 0.25f;
                case BuildState.CompactFieldBuild:
                    return 0.30f;
                case BuildState.ApplyAreaMarkers:
                    return 0.35f;
                case BuildState.ErodeWalkableArea:
                    return 0.40f;
                case BuildState.DistanceFieldBuild:
                    return 0.55f;
                case BuildState.RegionBuild:
                    return 0.60f;
                case BuildState.ContourBuild:
                    return 0.70f;
                case BuildState.PolyMeshBuild:
                    return 0.75f;
                case BuildState.DetailMeshBuild:
                    return 0.85f;
                case BuildState.Complete:
                    return 1;
                case BuildState.Aborted:
                    return 1;
            }
            return 1;  
        }
    }
}

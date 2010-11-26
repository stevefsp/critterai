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

namespace org.critterai.nav.nmgen
{
    internal static class SmokeTestMesh
    {
        /*
         * Design note:
         * 
         * The assert data has been manually compiled based on the
         * output of recast for the mesh and config.
         * I.e. Reviewed the output of recast and figured out what the
         * flattened results should look like.
         */

        private const float yCellSize = 0.1f;
        private const float xzCellSize = 0.2f;
        private const float minTraversableHeight = 2.1f;
        private const float maxTraversableStep = 0.5f;
        private const float maxTraversableSlope = 48.0f;
        private const bool clipLedges = false;
        private const float traversableAreaBorderSize = 0.3f;
        private const int smoothingThreshold = 2;
        private const int minIslandRegionSize = 10;
        private const int mergeRegionSize = 50;
        private const float maxEdgeLength = 5.0f;
        private const float edgeMaxDeviation = 0.4f;
        private const int maxVertsPerPoly = 6;
        private const float contourSampleDistance = 1.0f;
        private const float contourMaxDeviation = 0.55f;
        private const float heightfieldBoarderSize = 0;

        public static BuildConfig GetStandardBuildConfig()
        {
            return new BuildConfig(xzCellSize
                , yCellSize
                , minTraversableHeight
                , maxTraversableStep
                , maxTraversableSlope
                , clipLedges
                , traversableAreaBorderSize
                , heightfieldBoarderSize
                , smoothingThreshold
                , minIslandRegionSize
                , mergeRegionSize
                , maxEdgeLength
                , edgeMaxDeviation
                , maxVertsPerPoly
                , contourSampleDistance
                , contourMaxDeviation);
        }

        public static Configuration GetStandardConfig()
        {
            Configuration config = new Configuration();
            config.xzCellSize = xzCellSize;
            config.yCellSize = yCellSize;
            config.minTraversableHeight = minTraversableHeight;
            config.maxTraversableStep = maxTraversableStep;
            config.maxTraversableSlope = maxTraversableSlope;
            config.clipLedges = clipLedges;
            config.traversableAreaBorderSize = traversableAreaBorderSize;
            config.smoothingThreshold = smoothingThreshold;
            config.minIslandRegionSize = minIslandRegionSize;
            config.mergeRegionSize = mergeRegionSize;
            config.maxEdgeLength = maxEdgeLength;
            config.edgeMaxDeviation = edgeMaxDeviation;
            config.maxVertsPerPoly = maxVertsPerPoly;
            config.contourSampleDistance = contourSampleDistance;
            config.contourMaxDeviation = contourMaxDeviation;
            config.heightfieldBorderSize = heightfieldBoarderSize;
            return config;
        }

        public static float[] GetVerts()
        {
            float[] sourceVertices = 
            {
               -3, 0, -1
               , -2, 0, 3
               , 2, 0, 2
               , 1, 0, -2
            };
            return sourceVertices;
        }

        public static int[] GetTriangles()
        {
            int[] sourceTriangles =
            {
                0, 1, 2
                , 0, 2, 3
            };
            return sourceTriangles;
        }

        public static int ExpectedResultVertLength
        {
            get { return 36 * 3; }
        }

        public static int ExpectedResultTrianglesLength
        {
            get { return 34 * 3; }
        }

        public static bool PartialResultVertCheckOK(float[] resultVertices)
        {
            return (resultVertices.Length == ExpectedResultVertLength
                && SloppyEquals(resultVertices[0], -1.6f)
                && SloppyEquals(resultVertices[1], 0.2f)
                && SloppyEquals(resultVertices[2], 2.2f)
                && SloppyEquals(resultVertices[22 * 3 + 0], 0.2f)
                && SloppyEquals(resultVertices[22 * 3 + 1], 0.2f)
                && SloppyEquals(resultVertices[22 * 3 + 2], -1.2f)
                && SloppyEquals(resultVertices[35 * 3 + 0], -2.2f)
                && SloppyEquals(resultVertices[35 * 3 + 1], 0.2f)
                && SloppyEquals(resultVertices[35 * 3 + 2], -0.8f));
        }

        public static bool PartialResultTrianglesCheckOK(int[] resultTriangles)
        {
            return (resultTriangles.Length == ExpectedResultTrianglesLength
                && resultTriangles[0] == 2
                && resultTriangles[1] == 3
                && resultTriangles[2] == 1
                && resultTriangles[15 * 3 + 0] == 22
                && resultTriangles[15 * 3 + 1] == 17
                && resultTriangles[15 * 3 + 2] == 4
                && resultTriangles[33 * 3 + 0] == 19
                && resultTriangles[33 * 3 + 1] == 10
                && resultTriangles[33 * 3 + 2] == 31);
        }

        public static bool SloppyEquals(float a, float b)
        {
            return !(b < a - 0.0001f || b > a + 0.0001f);
        }

    }
}

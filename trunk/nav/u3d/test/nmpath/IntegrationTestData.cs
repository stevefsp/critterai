using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using org.critterai.util;
using org.critterai.mesh;
using System.Reflection;
using System.IO;

namespace org.critterai.nav.nmpath
{
    public sealed class IntegrationTestData
    {
        private const String TEST_FILE_NAME 
            = "org.critterai.nav.assets.BISHomeGAS.obj";

        // public readonly int sampleCount = 1000;
        public readonly float sampleDistance = 0.5f;

        public readonly int spacialDepth = 8;
        public readonly float planeTolerance = 0.5f;
        public readonly float offsetScale = 0.1f;
        public readonly DistanceHeuristicType heuristic 
            = DistanceHeuristicType.LongestAxis;
        public readonly int frameLength = 0;   // ticks (100ns)
        public readonly long maxFrameTimeslice = 20000; // ticks (100ns)
        public readonly int maxPathAge = 60000; // ms
        public readonly int repairSearchDepth = 2;
        public readonly int searchPoolMax = 40;
        public readonly long maintenanceFrequency = 500; // ms

        private static bool mResourcesExtracted = false;

        public readonly float[] sourceVerts;
        public readonly int[] sourceIndices;
        public readonly TriNavMesh navMesh;
        // public readonly float[] samplePoints;
        public readonly Vector3 meshMin;
        public readonly Vector3 meshMax;
        public readonly TriCell[] cells;
        public readonly ThreadedPlanner masterNavigator;
        public readonly TriNavMeshSamples samplePoints;

        public IntegrationTestData(bool mirrorMesh)
            : this(mirrorMesh, 60000)
        {
        }

        public IntegrationTestData(bool mirrorMesh, int maxPathAge)
        {
            if (!mResourcesExtracted)
                ExtractResources();

            this.maxPathAge = maxPathAge;

            BaseMesh mesh = Wavefront.build(TEST_FILE_NAME, true);
            sourceVerts = mesh.verts;
            sourceIndices = mesh.indices;
            meshMin = mesh.minBounds;
            meshMax = mesh.maxBounds;
            mesh = null;

            if (mirrorMesh)
            {
                for (int p = 0; p < sourceVerts.Length; p += 3)
                {
                    sourceVerts[p] *= -1;
                }
                for (int p = 0; p < sourceIndices.Length; p += 3)
                {
                    int t = sourceIndices[p + 1];
                    sourceIndices[p + 1] = sourceIndices[p + 2];
                    sourceIndices[p + 2] = t;
                }
                float tmp = meshMin.x * -1;
                meshMin.x = meshMax.x * -1;
                meshMax.x = tmp;
            }

            cells = TestUtil.GetAllCells(sourceVerts, sourceIndices);
            TestUtil.LinkAllCells(cells);

            navMesh = TriNavMesh.Build(sourceVerts
                , sourceIndices
                , spacialDepth
                , planeTolerance
                , offsetScale);

            samplePoints = new TriNavMeshSamples(navMesh, sampleDistance);

            //samplePoints = new float[sampleCount * 3];

            //System.Random r = new System.Random(89725);

            //Vector3 loc;
            //for (int p = 0; p < samplePoints.Length; p += 3)
            //{
            //    while (true)
            //    {
            //        float x = meshMin.x + (float)r.NextDouble() 
            //            * (meshMax.x - meshMin.x);
            //        float y = meshMin.y + (float)r.NextDouble() 
            //            * (meshMax.y - meshMin.y);
            //        float z = meshMin.z + (float)r.NextDouble() 
            //            * (meshMax.z - meshMin.z);

            //        if (navMesh.IsValidPosition(x, y, z, planeTolerance))
            //        {
            //            TriCell c = navMesh
            //                .GetClosestCell(x, y, z, true, out loc);  // Snap y.
            //            //if (p == 39)
            //            //    Console.WriteLine(c);
            //            samplePoints[p + 0] = loc.x;
            //            samplePoints[p + 1] = loc.y;
            //            samplePoints[p + 2] = loc.z;
            //            break;
            //        }
            //    }
            //}

            masterNavigator = PlannerUtil.GetThreadedPlanner(
                     this.sourceVerts
                     , this.sourceIndices
                     , this.spacialDepth
                     , this.planeTolerance
                     , this.offsetScale
                     , this.heuristic
                     , this.frameLength
                     , this.maxFrameTimeslice
                     , this.maxPathAge
                     , this.repairSearchDepth
                     , this.searchPoolMax
                     , this.maintenanceFrequency);
        }

        //public Vector3 GetSamplePoint(int index)
        //{
        //    return new Vector3(samplePoints[index * 3 + 0]
        //        , samplePoints[index * 3 + 1]
        //        , samplePoints[index * 3 + 2]);
        //}

        private static void ExtractResources()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            StreamReader reader = new StreamReader(
                asm.GetManifestResourceStream(TEST_FILE_NAME));

            StreamWriter writer = new StreamWriter(TEST_FILE_NAME);
            writer.Write(reader.ReadToEnd());
            writer.Close();
            mResourcesExtracted = true;
        }

    }
}

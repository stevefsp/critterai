using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using org.critterai.math;

namespace org.critterai.nav.nmpath
{
    /// <summary>
    /// Builds a list of sample points evenly distributed across the the surface
    /// of the navigation mesh.
    /// </summary>
    /// <remarks>
    /// <p>The [] operator has been implemented for this class.  
    /// E.g. samples[5].</p>
    /// 
    /// <p>Sampling along the height axis (y) is performed using the sample
    /// distance, with the y-value snapped to the surface of the mesh 
    /// based on the mesh's plane tolerance.  Checks are performed to ensure 
    /// no duplication of points occurs during this process.</p>
    /// </remarks>
    public sealed class TriNavMeshSamples
    {
        /*
         * Test Note:
         * 
         * There are currently no explicit tests for this class.  Most features
         * are inherently tested via several integration tests.
         * See: IntegrationTestData
         * 
         */

        /// <summary>
        /// The raw sample points in the form (x, y, z).  All sample points
        /// are guarenteed to be on the mesh's surface.
        /// </summary>
        /// <remarks>
        /// Vector3 points can be obtained using the [] operator. 
        /// E.g. sample[5].
        /// </remarks>
        public readonly float[] samples;

        /// <summary>
        /// The minimum bounds of the source mesh's axis-aligned bounding box.
        /// </summary>
        public readonly Vector3 meshMin;

        /// <summary>
        /// The maximum bounds of the source mesh's axis-aligned bounding box.
        /// </summary>
        public readonly Vector3 meshMax;

        /// <summary>
        /// The minimum bounds of the of the axix-aligned bounding box
        /// containing the sample points.
        /// </summary>
        public readonly Vector3 sampleMin = new Vector3(float.MaxValue
            , float.MaxValue
            , float.MaxValue);

        /// <summary>
        /// The maximum bounds of the of the axix-aligned bounding box
        /// containing the sample points.
        /// </summary>
        public readonly Vector3 sampleMax = new Vector3(float.MinValue
            , float.MinValue
            , float.MinValue);

        /// <summary>
        /// The sample distance used to build the list of points.
        /// </summary>
        public readonly float sampleDistance;

        /// <summary>
        /// The plane tolerance of the source mesh.
        /// </summary>
        /// <see cref="TriNavMesh.PlaneTolerance"/>
        public readonly float planeTolerance;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mesh">The source mesh to sample.</param>
        /// <param name="sampleDistance">The sample distance. (The sample
        /// increment used for all axes.</param>
        public TriNavMeshSamples(TriNavMesh mesh
            , float sampleDistance)
        {
            this.sampleDistance = Math.Max(float.Epsilon, sampleDistance);
            planeTolerance = mesh.PlaneTolerance;
            meshMin = mesh.MinimumBounds;
            meshMax = mesh.MaximumBounds;

            float x = meshMin.x;
            float y;
            float z;
            
            List<float> seenY = new List<float>();
            List<float> points = new List<float>();

            while (x <= meshMax.x)
            {
                z = meshMin.z;
                while (z <= meshMax.z)
                {
                    y = meshMin.y;
                    while (y <= meshMax.y)
                    {
                        Vector3 pointOnMesh;
                        if (mesh.GetClosestCell(x, y, z, true, out pointOnMesh)
                            != null)
                        {
                            bool isNew = true;
                            foreach (float f in seenY)
                            {
                                if (MathUtil.SloppyEquals(f
                                    , pointOnMesh.y
                                    , 0.001f))
                                {
                                    isNew = false;
                                    break;
                                }
                            }
                            if (isNew)
                            {
                                seenY.Add(pointOnMesh.y);
                                points.Add(pointOnMesh.x);
                                points.Add(pointOnMesh.y);
                                points.Add(pointOnMesh.z);
                                sampleMin.x =
                                    Math.Min(pointOnMesh.x, sampleMin.x);
                                sampleMin.y =
                                    Math.Min(pointOnMesh.y, sampleMin.y);
                                sampleMin.z =
                                    Math.Min(pointOnMesh.z, sampleMin.z);
                                sampleMax.x =
                                    Math.Max(pointOnMesh.x, sampleMax.x);
                                sampleMax.y =
                                    Math.Max(pointOnMesh.y, sampleMax.y);
                                sampleMax.z =
                                    Math.Max(pointOnMesh.z, sampleMax.z);
                            }
                        }
                        y += sampleDistance;
                    }
                    seenY.Clear();
                    z += this.sampleDistance;
                }
                x += this.sampleDistance;
            }

            samples = points.ToArray();
        }

        /// <summary>
        /// The number of sample points.
        /// </summary>
        public int Count { get { return samples.Length / 3; } }

        /// <summary>
        /// Returns a sample point on the surface of the source mesh.
        /// </summary>
        /// <remarks>
        /// <p>The [] operator can also be used.  E.g. samples[5].</p>
        /// </remarks>
        /// <param name="index">The index of the sample point.</param>
        /// <returns>A sample point on the surface of the source mesh.</returns>
        public Vector3 GetPoint(int index)
        {
            return this[index];
        }

        /// <summary>
        /// Returns a sample point on the surface of the source mesh.
        /// </summary>
        /// <param name="index">The index of the sample point.</param>
        /// <returns>A sample point on the surface of the source mesh.</returns>
        public Vector3 this[int index]
        {
            get
            {
                return new Vector3(samples[index * 3 + 0]
                    , samples[index * 3 + 1]
                    , samples[index * 3 + 2]);
            }
        }
    }
}

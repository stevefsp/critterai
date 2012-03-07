/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using org.critterai.geom;
using System.Collections.Generic;
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    public class InputGeometryBuilder
    {
        private ChunkyTriMeshBuilder mBuilder;
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;
        private readonly bool mIsThreadSafe;
        private InputGeometry mGeom;

        public bool IsThreadSafe { get { return mIsThreadSafe; } } 
        public InputGeometry Geometry { get { return mGeom; } }
        public bool IsFinished { get { return mGeom != null; } }

        private InputGeometryBuilder(ChunkyTriMeshBuilder builder
            , Vector3 bmin
            , Vector3 bmax
            , bool isThreadSafe)
        {
            mBuilder = builder;
            mBoundsMin = bmin;
            mBoundsMax = bmax;
            mIsThreadSafe = isThreadSafe;
        }

        public bool Build()
        {
            if (mBuilder == null)
                return false;

            if (mBuilder.Build())
                return true;

            mGeom = new InputGeometry(mBuilder.Mesh, mBoundsMin, mBoundsMax);
            mBuilder = null;

            return false;
        }

        public void BuildAll()
        {
            if (mBuilder == null)
                return;

            mBuilder.BuildAll();

            mGeom = new InputGeometry(mBuilder.Mesh, mBoundsMin, mBoundsMax);
            mBuilder = null;
        }

        public static InputGeometryBuilder Create(TriangleMesh mesh
            , byte[] areas
            , float walkableSlope)
        {
            if (!IsValid(mesh, areas))
                return null;

            TriangleMesh lmesh = new TriangleMesh(mesh.vertCount, mesh.triCount);

            System.Array.Copy(mesh.verts, 0, lmesh.verts, 0, lmesh.verts.Length);
            System.Array.Copy(mesh.tris, 0, lmesh.tris, 0, lmesh.tris.Length);

            byte[] lareas;
            if (areas == null)
                lareas = NMGen.CreateWalkableAreaBuffer(mesh.triCount);
            else
            {
                lareas = new byte[mesh.triCount];
                System.Array.Copy(areas, 0, lareas, 0, lareas.Length);
            }

            return CreateUnsafe(lmesh, lareas, walkableSlope, true);
        }

        // May modify the contents of the area's array.
        public static InputGeometryBuilder CreateUnsafe(TriangleMesh mesh
            , byte[] areas
            , float walkableSlope
            , bool isThreadSafe)
        {

            walkableSlope = System.Math.Min(NMGen.MaxAllowedSlope, walkableSlope);

            if (walkableSlope > 0)
            {
                BuildContext context = new BuildContext();
                if (!NMGen.ClearUnwalkableTriangles(context, mesh, walkableSlope, areas))
                    return null;
            }

            ChunkyTriMeshBuilder builder = ChunkyTriMeshBuilder.Create(mesh, areas, 32768);

            if (builder == null)
                return null;

            Vector3 bmin;
            Vector3 bmax;
            Vector3Util.GetBounds(mesh.verts, mesh.tris, mesh.triCount, out bmin, out bmax);

            return new InputGeometryBuilder(builder, bmin, bmax, isThreadSafe);
        }

        public static bool IsValid(TriangleMesh mesh, byte[] areas)
        {
            if (mesh == null || mesh.triCount == 0 || !TriangleMesh.Validate(mesh, true))
            {
                return false;
            }

            if (areas != null && (areas.Length != mesh.triCount
                    || !NMGen.ValidateAreaBuffer(areas, mesh.triCount)))
            {
                return false;
            }

            return true;
        }

    }
}

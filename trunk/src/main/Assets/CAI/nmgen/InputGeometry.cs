using System;
using org.critterai.geom;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents NMGen input geometry that has been validated and
    /// is ready to be used for a build.
    /// </summary>
	public sealed class InputGeometry
	{
        /*
         * Design note:
         * 
         * One of the primary purposes of this class is to put geometry data in a 
         * safe state for use by builders on multiple threads.  But, due to
         * peformance/memory concerns, the class is only thread-friendly.
         * It protects the data by scoping access as internal.
         * 
         * The issue with this design is the internal scope is ineffective 
         * for source distributions, such as in Unity. That is the reason 
         * for the naming conventions.  They provide a warning to users.
         * 
         * A secondary purpose of this class is to provide a common validation
         * point for geometry.
         * 
         */

        private readonly Vector3[] verts;
        private readonly int[] tris;
        private readonly byte[] areas;

        internal Vector3[] UnsafeVerts { get { return verts; } }
        internal int[] UnsafeTris { get { return tris; } }
        internal byte[] UnsafeAreas { get { return areas; } }

        private InputGeometry(Vector3[] verts, int[] tris, byte[] areas) 
        {
            this.verts = verts;
            this.tris = tris;
            this.areas = areas;
        }

        internal static InputGeometry UnsafeCreate(Vector3[] verts
            , int[] tris
            , byte[] areas)
        {
            return new InputGeometry(verts, tris, areas);
        }

        public static InputGeometry Create(TriangleMesh mesh, byte[] areas)
        {
            return Create(mesh.verts, mesh.vertCount
                , mesh.tris, areas, mesh.triCount);
        }

        public static InputGeometry Create(Vector3[] verts
            , int vertCount
            , int[] tris
            , byte[] areas
            , int triCount)
        {
            if (triCount == 0
                || !TriangleMesh.Validate(verts, vertCount, tris, triCount ,true))
            {
                return null;
            }

            if (areas != null
                && (areas.Length != tris.Length / 3
                    || !NMGen.ValidateAreaBuffer(areas, triCount)))
            {
                return null;
            }

            Vector3[] lverts = new Vector3[vertCount];
            int[] ltris = new int[triCount * 3];

            Array.Copy(verts, 0, lverts, 0, lverts.Length);
            Array.Copy(tris, 0, ltris, 0, ltris.Length);

            byte[] lareas;
            if (areas == null)
                lareas = NMGen.BuildWalkableAreaBuffer(triCount);
            else
            {
                lareas = new byte[triCount];
                Array.Copy(areas, 0, lareas, 0, lareas.Length);
            }
            
            return new InputGeometry(lverts, ltris, lareas);
        }
	}
}

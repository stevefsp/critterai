using System;

namespace org.critterai.geom
{
    public sealed class TriangleMesh
    {
        public float[] verts;
        public int[] tris;
        public int vertCount;
        public int triCount;

        public TriangleMesh() { }

        public TriangleMesh(float[] verts
            , int vertCount
            , int[] tris
            , int triCount)
        {
            this.verts = verts;
            this.vertCount = vertCount;
            this.tris = tris;
            this.triCount = triCount;
        }
    }
}

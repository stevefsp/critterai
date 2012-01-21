
namespace org.critterai.nmgen
{
    [System.Serializable]
	public class NMGenMesh
	{
        /*
         * Design note: 
         * The byte[] version of this data must be a class 
         * in order to support Unity serialization.  So this
         * is a class for consistency.
         */

        public int tileX;
        public int tileZ;
        public PolyMesh polyMesh;
        public PolyMeshDetail detailMesh;
        public bool noResult;

        public NMGenMesh() { }

        public NMGenMesh(int tx, int tz)
        {
            this.tileX = tx;
            this.tileZ = tz;
        }

        public NMGenMesh(int tx, int tz
            , PolyMesh polyMesh, PolyMeshDetail detailMesh)
        {
            this.tileX = tx;
            this.tileZ = tz;
            this.polyMesh = polyMesh;
            this.detailMesh = detailMesh;
            this.noResult = (polyMesh == null) ? true : false;
        }

        public NMGenMesh(int tx, int tz, bool noResult)
        {
            this.tileX = tx;
            this.tileZ = tz;
            this.noResult = noResult;
        }
	}
}

using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    [StructLayout(LayoutKind.Sequential)]
    public class LocalBoundaryData
    {
        /*
         * Design note:
         * 
         * Implemented as a class to permit use as a re-usable buffer.
         * 
         */

        public const int MaxSegments = 8;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] center = new float[3];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSegments * 6)]
        public float[] segments = new float[MaxSegments * 6];

        public int segmentCount = 0;

        public LocalBoundaryData() { }
    }
}

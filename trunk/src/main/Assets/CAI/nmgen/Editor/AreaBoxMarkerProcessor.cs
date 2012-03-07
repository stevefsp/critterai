using System;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
	public class AreaBoxProcessor
        : ICHFProcessor
	{
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;
        private byte mArea;

        public Vector3 BoundsMin { get { return mBoundsMin; } }
        public Vector3 BoundsMax { get { return mBoundsMax; } }
        public byte Area { get { return mArea; } }
        public bool IsThreadSafe { get { return true; } }
        public bool IsPostProcessor { get { return false; } }

        public AreaBoxProcessor(Vector3 boundsMin, Vector3 boundsMax, byte area)
        {
            mArea = Math.Max(NMGen.UnwalkableArea, Math.Min(NMGen.WalkableArea, area));
            mBoundsMin = boundsMin;
            mBoundsMax = boundsMax;
        }

        public CompactHeightfield Process(BuildContext context
            , CompactHeightfield field)
        {
            if (context == null || field == null || field.IsDisposed)
                return field;

            field.MarkBoxArea(context, mBoundsMin, mBoundsMax, mArea);

            return field;
        }

        //public bool Overlaps(Vector3 boundsMin, Vector3 boundsMax)
        //{
        //    bool overlap = true;
        //    overlap = (boundsMin.x > mBoundsMax.x || boundsMax.x < mBoundsMin.x) ? false : overlap;
        //    overlap = (boundsMin.z > mBoundsMax.z || boundsMax.z < mBoundsMin.z) ? false : overlap;
        //    return overlap;
        //}
    }
}

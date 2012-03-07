using System;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
	public class AreaCylinderProcessor
        : ICHFProcessor
	{
        private readonly Vector3 mCenterBase;
        private readonly float mRadius;
        private readonly float mHeight;
        private readonly byte mArea;

        public byte Area { get { return mArea; } }
        public float Radius { get { return mRadius; } }
        public float Height { get { return mHeight; } }
        public bool IsThreadSafe { get { return true; } }
        public bool IsPostProcessor { get { return false; } }

        public AreaCylinderProcessor(Vector3 centerBase
            , float radius
            , float height
            , byte area)
        {
            mArea = Math.Max((byte)0, Math.Min((byte)63, area));
            mCenterBase = centerBase;
            mRadius = Math.Max(0, radius);
            mHeight = Math.Max(0, height);
        }

        public Vector3 GetCenterBase { get { return mCenterBase; } }

        public CompactHeightfield Process(BuildContext context
            , CompactHeightfield field)
        {
            if (context == null || field == null || field.IsDisposed)
                return field;

            field.MarkCylinderArea(context
                , mCenterBase, mRadius, mHeight
                , mArea);
                
            return field;
        }

        //public bool Overlaps(Vector3 boundsMin, Vector3 boundsMax)
        //{
        //    bool overlap = true;

        //    overlap = (boundsMin.x > mCenterBase.x + mRadius 
        //        || boundsMax.x < mCenterBase.x - mRadius) ? false : overlap;

        //    overlap = (boundsMin.z > mCenterBase.z + mRadius 
        //        || boundsMax.z < mCenterBase.z - mRadius) ? false : overlap;

        //    return overlap;
        //}
    }
}

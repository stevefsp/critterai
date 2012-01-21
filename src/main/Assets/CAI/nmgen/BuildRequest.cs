using org.critterai.nmgen.intern;

namespace org.critterai.nmgen
{
    /// <summary>
    /// Provides access to Request State and Data.
    /// <para>When polling to see if a Request is complete, check <see cref="IsFinished">IsFinished</see>
    /// rather than <see cref="State">State</see>.  Using State incurs synchronization costs.</para>
    /// <para>Instances of this class are thread-safe only if its associated 
    /// master request object is used in a thread-safe manner.</para>
    /// </summary>
    public sealed class BuildRequest
    {
        /*
         * Design notes:  
         * 
         * Everything is locked on the root object.
         */

        private readonly MasterBuildRequest mRoot;

        private readonly int mTileX;
        private readonly int mTileZ;

        public int TileX { get { return mTileX; } }
        public int TileZ { get { return mTileZ; } } 

        public BuildResult Data
        {
            get 
            {
                // Never give out the reference until the request is complete.
                if (mRoot.data.isFinished)
                    lock (mRoot) { return mRoot.data; }
                else
                    return null;
            }
        }

        /// <summary>
        /// Indicates whether or not the request is finished being processed.
        /// The request is considered finished if its State is either
        /// <see cref="NavRequestState.Complete">Complete</see> or 
        /// <see cref="NavRequestState.Failed">Finished</see>.
        /// </summary>
        public bool IsFinished { get { return mRoot.data.isFinished; } }

        public BuildState State { get { return mRoot.data.state; } }

        internal BuildRequest(MasterBuildRequest root, int tx, int tz)
        {
            mRoot = root;
            mTileX = tx;
            mTileZ = tz;
        }
    }
}

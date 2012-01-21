
namespace org.critterai.nmgen.intern
{
    internal sealed class MasterBuildRequest
    {
        /*
         * Design notes:  
         * 
         * The owner is responsible for locking the object when appropriate.
         * The reader checks the lock on the master.
         */

        public readonly BuildResult data = new BuildResult();
        private readonly BuildRequest mReader;

        public MasterBuildRequest(int tx, int tz)
        {
            mReader = new BuildRequest(this, tx, tz);
        }

        /// <summary>
        /// Returns a shared request object suitable for use by clients.
        /// </summary>
        public BuildRequest Reader { get { return mReader; } }
    }
}

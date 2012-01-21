
namespace org.critterai.nmgen.intern
{
	internal class BuildTask
	{
        private IncrementalBuilder mBuilder;
        private readonly MasterBuildRequest mRequest;
        private bool mAbort = false;

        private BuildTask(IncrementalBuilder builder)
        {
            mBuilder = builder;
            mRequest = new MasterBuildRequest(builder.TileX, builder.TileZ);
        }

        public BuildRequest Request { get { return mRequest.Reader; } }

        public void RequestAbort()
        {
            RequestAbort("Build task manually aborted.");
        }

        public void RequestAbort(string reason)
        {
            if (mAbort)
                return;

            // Must get the lock first!
            lock (mRequest)
            {
                if (mAbort || mRequest.data.isFinished)
                    // The task finished before the lock could be obtained.
                    // Ignore the abort request.
                    return;

                mAbort = true;

                mRequest.data.messages = new string[1];
                mRequest.data.messages[0] = reason;
                mRequest.data.mesh = null;
                mRequest.data.state = BuildState.Aborted;
                mRequest.data.isFinished = true;
            }
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    BuildState state = mBuilder.Build();
                    if (mBuilder.IsFinished)
                        break;
                    // Note: Don't set state until the above check
                    // since don't want the result state showing complete
                    // before it is officially marked as finished.
                    mRequest.data.state = state;
                }

                // Must get the lock first!
                lock (mRequest)
                {
                    if (mAbort)
                        // An abort happened before the lock could be obtained.
                        // Just accept the abort.
                        return;

                    mRequest.data.messages = mBuilder.GetMessages();

                    if (mBuilder.State == BuildState.Complete)
                    {
                        if (mBuilder.NoResult)
                        {
                            mRequest.data.mesh = new NMGenMesh(
                                mRequest.Reader.TileX
                                , mRequest.Reader.TileZ
                                , true);
                        }
                        else
                        {
                            mRequest.data.mesh = new NMGenMesh(
                                mRequest.Reader.TileX
                                , mRequest.Reader.TileZ
                                , mBuilder.PolyMesh
                                , mBuilder.DetailMesh);
                        }
                    }

                    mRequest.data.state = mBuilder.State;
                    mRequest.data.isFinished = true;
                    mAbort = true;  // Prevents reuse of the task.
                }

                mBuilder = null;
            }
            catch (System.Exception ex)
            {
                // Ignore race issues.  Force the data into the request.
                string[] message = new string[1];
                message[0] = "Aborted on exception: " + ex.Message;
                mRequest.data.messages = message;
                mRequest.data.state = BuildState.Aborted;
                mRequest.data.isFinished = true;
            }
        }

        public static BuildTask Build(IncrementalBuilder builder)
        {
            if (builder == null || builder.State != BuildState.Initialized)
                return null;
            return new BuildTask(builder);
        }
	}
}

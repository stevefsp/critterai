
namespace org.critterai.nmgen
{
    public class BuildResult
    {
        public NMGenMesh mesh = null;
        public string[] messages = null;
        public BuildState state = BuildState.Initialized;
        public bool isFinished = false;

        internal BuildResult() { }
    }
}

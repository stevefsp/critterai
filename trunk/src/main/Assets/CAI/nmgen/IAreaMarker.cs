
namespace org.critterai.nmgen
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <para>All methods must be implemented as thread safe.
    /// </para></remarks>
	public interface IAreaMarker
	{
        bool Overlaps(float xmin, float zmin, float xmax, float zmax);
        bool MarkArea(BuildContext context, CompactHeightfield field);
	}
}

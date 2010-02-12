package org.critterai.nmgen;

/**
 * Holds performance and log related data.
 * <p>All time values are in nanoseconds (ns).
 */
public class LogResults 
{

	/**
	 * The data is undefined. (Has not been set.)
	 */
	public static final long UNDEFINED = -1;
	
	/**
	 * The time to perform voxelization. (ns)
	 */
	public long voxelizationTime;
	
	/**
	 * The time to perform region generation. (ns)
	 */
	public long regionGenTime;
	
	/**
	 * The time to perform contour generation. (ns)
	 */
	public long contourGenTime;
	
	/**
	 * The time to perform polygon generation. (ns)
	 */
	public long polyGenTime;
	
	/**
	 * The time to perform the final triangulation. (ns)
	 */
	public long finalMeshGenTime;
	
	/**
	 * Returns the total time to generate the navigation mesh. (ns)
	 * @return The total time to generate the navigation mesh. (ns)
	 */
	public long getTotalGenTime()
	{
		if (finalMeshGenTime == UNDEFINED)
			return UNDEFINED;
		return voxelizationTime 
			+ regionGenTime
			+ contourGenTime
			+ polyGenTime
			+ finalMeshGenTime;
	}
	
	/**
	 * Prepares the object to be reused.
	 */
	public void reset()
	{
		voxelizationTime = UNDEFINED;
		regionGenTime = UNDEFINED;
		contourGenTime = UNDEFINED;
		polyGenTime = UNDEFINED;
		finalMeshGenTime = UNDEFINED;
	}
	
}

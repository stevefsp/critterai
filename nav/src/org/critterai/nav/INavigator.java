package org.critterai.nav;

import org.critterai.math.Vector3;
import org.critterai.nav.MasterPath.Path;

/**
 * Provides a means of making navigation requests to a navigator.
 */
public interface INavigator {

    /*
     * Design note:
     * 
     * The reason for the existence of this interface is to permit
     * adapters to be created for the navigator object.  For example
     * a debug navigator that tracks performance. 
     */
    
    /**
     * Requests construction of a path from the start to the goal point.
     * <p>The request will fail if the start or goal point is outside the 
     * the xz-column of the mesh.</p>
     * <p>This is a potentially costly operation.  Consider using 
     * {@link #repairPath(float, float, float, int)} if the client already has a path
     * to the goal.</p>
     * @param startX The x-value of the start point of the path. (startX, startY, startZ)
     * @param startY The y-value of the start point of the path. (startX, startY, startZ)
     * @param startZ The z-value of the start point of the path. (startX, startY, startZ)
     * @param goalX The x-value of the goal point on the path. (goalX, goalY, goalZ)
     * @param goalY The y-value of the goal point on the path. (goalX, goalY, goalZ)
     * @param goalZ The z-value of the goal point on the path. (goalX, goalY, goalZ)
     * @return A resulting path request.
     */
    MasterNavRequest<Path>.NavRequest getPath(float startX, float startY,
            float startZ, float goalX, float goalY, float goalZ);

    /**
     * Requests that a in-progress path request be discarded.
     * This will result in the request being marked as {@link NavRequestState#FAILED}.
     * @param request The path request to cancel.
     */
    void discardPathRequest(MasterNavRequest<Path>.NavRequest request);

    /**
     * Request that an active path be maintained within the cache.  The path's age
     * will be reset.
     * <p>The request-type only applied if the navigator supports path caching.</p>
     * <p>To keep a path alive, this operation must be called more frequently
     * the maximum amount of time the navigator is configured to cache paths.
     * <p>This request-type is guaranteed to complete after one process cycle
     * of the navigator.  (Usually one frame.)</p>
     * @param path The path to keep alive.
     */
    void keepPathAlive(Path path);

    /**
     * Attempt to repair a path using the new start location.
     * A limited search will be performed to find a path from the new start location
     * back to the known path.  If the path cannot be repaired, a new full path
     * search will be required.
     * <p>If the repair is successful, a new path will be generated.</p>
     * <p>Under normal use cases, this request may take multiple frames
     * to complete.</p>
     * @param startX The x-value of the start point of the repaired path. 
     * (startX, startY, startZ)
     * @param startY The y-value of the start point of the repaired path. 
     * (startX, startY, startZ)
     * @param startZ The z-value of the start point of the repaired path. 
     * (startX, startY, startZ)
     * @param path The path to attempt to repair.
     * @return The path repair request.
     */
    MasterNavRequest<Path>.NavRequest repairPath(float startX, float startY,
            float startZ, Path path);

    /**
     * Gets the position on the navigation mesh that is closest to the
     * provided point.
     * <p>This request-type is guaranteed to complete after one process cycle
     * of the navigator. (Usually one frame.)</p>
     * <p>This request-type is guaranteed to complete successfully as long as the
     * {@link MasterNavigator} has not been disposed.</p>
     * @param x The x-value of the test point.  (x, y, z)
     * @param y The y-value of the test point.  (x, y, z)
     * @param z The z-value of the test point.  (x, y, z)
     * @return The pending request.
     */
    MasterNavRequest<Vector3>.NavRequest getNearestValidLocation(float x,
            float y, float z);

    /**
     * Indicates whether or not the supplied point is within the column of the
     * navigation mesh and within the y-axis distance of the surface of the mesh.
     * <p>This request-type is guaranteed to complete after one process cycle
     * of the navigator.  (Usually one frame.)</p>
     * <p>This request-type is guaranteed to complete successfully as long as the
     * navigator has not been disposed.</p>
     * @param x The x-value of the test point.  (x, y, z)
     * @param y The y-value of the test point.  (x, y, z)
     * @param z The z-value of the test point.  (x, y, z)
     * @param yTolerance The allowed y-axis distance the position may be from the surface
     * of the navigation mesh and still be considered valid.
     * @return The pending request.
     */
    MasterNavRequest<Boolean>.NavRequest isValidLocation(float x, float y,
            float z, float yTolerance);

    boolean isDisposed();

}
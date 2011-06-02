/*
 * Copyright (c) 2011 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using UnityEngine;
using org.critterai.nav;

/// <summary>
/// A <see cref="CrowdAgentParams"/> configuration suitable for sharing 
/// between multiple agents.
/// </summary>
[System.Serializable]
[AddComponentMenu("CAI/Agent Navigation Config")]
public sealed class AgentNavConfig 
    : MonoBehaviour 
{
    /*
     * Design notes:
     * 
     * Keep main fields in sync with org.critterai.nav.CrowdAgentParams
     * 
     */

    /// <summary>
    /// The <see cref="NavManager"/> agents are to use.
    /// </summary>
    public NavManager manager;

    /// <summary>
    /// Agent radius. [Limit: >= 0]
    /// </summary>
    public float radius = 0.4f;

    /// <summary>
    /// Agent height. [Limit: > 0]
    /// </summary>
    public float height = 1.8f;

    /// <summary>
    /// Maximum allowed acceleration. [Limit: >= 0]
    /// </summary>
    public float maxAcceleration = 8;

    /// <summary>
    /// Maximum allowed speed. [Limit: >= 0]
    /// </summary>
    public float maxSpeed = 3.5f;

    /// <summary>
    /// Defines how close a neighbor must be before it is considered
    /// in steering behaviors. [Limit: > 0]
    /// </summary>
    /// <remarks>
    /// The value is usually based on the agent radius and/or
    /// and maximum speed.  E.g. radius * 8</remarks>
    public float collisionQueryRange = 0.4f * 8;

    /// <summary>
    /// Path optimization range.
    /// </summary>
    /// <remarks>
    /// The value is usually based on the agent radius. E.g. radius * 30
    /// </remarks>
    public float pathOptimizationRange = 0.4f * 30;

    /// <summary>
    /// How aggresive the <see cref="CrowdManager"/> should be at avoiding
    /// collisions with this agent.
    /// </summary>
    /// <remarks>
    /// A higher value will result in agents trying to stay farther away 
    /// from each other, at the cost of more difficult steering in tight
    /// spaces.</remarks>
    public float separationWeight = 2.0f;

    /// <summary>
    /// Steering behavior flags.
    /// </summary>
    public CrowdUpdateFlags updateFlags = CrowdUpdateFlags.AnticipateTurns
        | CrowdUpdateFlags.CrowdSeparation | CrowdUpdateFlags.ObstacleAvoidance
        | CrowdUpdateFlags.OptimizeVis | CrowdUpdateFlags.OptimizeTopo;

    /// <summary>
    /// The <see cref="CrowdManager"/> avoidance parameters to use for the 
    /// agent.
    /// </summary>
    /// <remarks>
    /// <p>The <see cref="CrowdManager"/> permits agents to use different
    /// avoidance configurations.  (See 
    /// <see cref="CrowdManager.SetAvoidanceConfig"/>.)  This is the index 
    /// of the configuration to use.</p>
    /// </remarks>
    /// <seealso cref="CrowdAvoidanceParams"/>
    public byte avoidanceType = 3;

    /// <summary>
    /// The size of the path agent's path buffer.
    /// </summary>
    public int maxPathSize = 100;

    /// <summary>
    /// The crowd configuration.
    /// </summary>
    /// <returns>The crowd configuration.</returns>
    public CrowdAgentParams GetConfig()
    {
        CrowdAgentParams result = new CrowdAgentParams();
        result.avoidanceType = avoidanceType;
        result.collisionQueryRange = collisionQueryRange;
        result.height = height;
        result.maxAcceleration = maxAcceleration;
        result.maxSpeed = maxSpeed;
        result.pathOptimizationRange = pathOptimizationRange;
        result.radius = radius;
        result.separationWeight = separationWeight;
        result.updateFlags = updateFlags;
        return result;
    }
}

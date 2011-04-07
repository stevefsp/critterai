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
using System;
using System.Collections.Generic;
using System.Text;
using org.critterai.nav.rcn.externs;
using System.Runtime.InteropServices;

namespace org.critterai.nav.rcn
{
    public sealed class CrowdManager
        : IDisposable
    {
        /*
         * WARNING:  The current design is based on not allowing
         * re-initialization of the native class after construction.  
         * Re-evaluate the entire class is initailization is added.
         */

        public const int MaxObstacleAvoidanceParams =
            CrowdManagerEx.MaxObstacleAvoidanceParams;

        internal IntPtr root;
        
        private CrowdAgent[] mAgents = null;
        private int mAgentCount = 0;
        private NavmeshQueryFilter mFilter = null;
        private CrowdProximityGrid mGrid = null;
        private NavmeshQuery mQuery = null;

        internal CrowdManager(IntPtr dtCrowd)
        {
            root = dtCrowd;

            mAgents = new CrowdAgent[CrowdManagerEx.GetAgentMaxCount(root)];

            IntPtr ptr = CrowdManagerEx.GetQueryFilter(root);
            if (ptr != IntPtr.Zero)
                mFilter = 
                    new NavmeshQueryFilter(ptr, AllocType.ExternallyManaged);

            ptr = CrowdManagerEx.GetProximityGrid(root);
            mGrid = new CrowdProximityGrid(ptr);

            ptr = CrowdManagerEx.GetQuery(root);

            mQuery = 
                new NavmeshQuery(ptr, AllocType.ExternallyManaged);

        }

        ~CrowdManager()
        {
            Dispose();
        }

        public bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        public void Dispose()
        {
            if (root != IntPtr.Zero)
            {
                foreach (CrowdAgent agent in mAgents)
                {
                    if (agent != null)
                        agent.Dispose();
                }
                mAgents = null;
                mAgentCount = 0;

                mFilter.RequestDisposal();
                mGrid.Dispose();
                mQuery.RequestDisposal();

                CrowdManagerEx.FreeEx(root);
                root = IntPtr.Zero;
            }
        }

        public bool SetObstacleAvoidanceParams(int index
            , CrowdAvoidanceParams obstacleParams)
        {
            if (index < 0 || index >= MaxObstacleAvoidanceParams)
                return false;

            CrowdManagerEx.SetObstacleAvoidanceParams(root
                , index
                , obstacleParams);

            return true;
        }

        public CrowdAvoidanceParams GetObstacleAvoidanceParams(int index)
        {
            if (index >= 0 && index < MaxObstacleAvoidanceParams)
            {
                IntPtr ptr = CrowdManagerEx.GetObstacleAvoidanceParams(root, index);

                if (ptr != IntPtr.Zero)
                    return (CrowdAvoidanceParams)Marshal.PtrToStructure(
                        ptr, typeof(CrowdAvoidanceParams));                
            }
            return new CrowdAvoidanceParams();
        }

        public int AgentCount
        {
            get { return mAgentCount; }
        }

        public CrowdAgent GetAgent(int index)
        {
            return mAgents[index];
        }

        public int AgentMaxCount
        {
            get { return mAgents.Length; }
        }

        public CrowdProximityGrid ProximityGrid
        {
            get { return mGrid; }
        }

        public void Update(float deltaTime)
        {
            CrowdManagerEx.Update(root, deltaTime);
        }

        public float[] GetQueryExtents()
        {
            float[] result = new float[3];
            CrowdManagerEx.GetQueryExtents(root, result);
            return result;
        }

        public int GetVelocitySampleCount()
        {
            return CrowdManagerEx.GetVelocitySampleCount(root);
        }

        public int AddAgent(float[] position, CrowdAgentParams agentParams)
        {
            if (position == null || position.Length != 3)
                return -1;

            int index = CrowdManagerEx.AddAgent(root, position, agentParams);

            if (index == -1)
                return index;

            IntPtr agent = CrowdManagerEx.GetAgent(root, index);

            mAgents[index] = new CrowdAgent(agent);
            mAgentCount++;

            return index;
        }

        public CrowdAgent[] GetAllAgents()
        {
            if (mAgentCount == 0)
                return null;

            CrowdAgent[] result = new CrowdAgent[mAgentCount];
            int count = 0;

            foreach (CrowdAgent agent in mAgents)
            {
                if (agent != null)
                    result[count++] = agent;
            }

            return result;
        }

        public void UpdateAgentParameters(int index
            , CrowdAgentParams agentParams)
        {
            CrowdManagerEx.UpdateAgentParameters(root, index, agentParams);
        }

        public void RemoveAgent(int index)
        {
            if (index < 0 || index >= mAgents.Length
                    || mAgents[index] == null)
                return;

            mAgents[index].Dispose();
            mAgents[index] = null;
            mAgentCount--;

            CrowdManagerEx.RemoveAgent(root, index);
        }

        public NavmeshQueryFilter QueryFilter
        {
            get { return mFilter; }
        }

        public NavmeshQueryLite Query
        {
            get { return mQuery.LiteQuery; }
        }

        public static CrowdManager Build(int maxAgents
            , float maxAgentRadius
            , Navmesh navmesh)
        {
            if (navmesh.IsDisposed)
                return null;

            IntPtr root = CrowdManagerEx.Alloc(Math.Max(1, maxAgents)
                , Math.Max(0, maxAgentRadius)
                , navmesh.root);

            if (root == IntPtr.Zero)
                return null;

            return new CrowdManager(root);
        }

        public bool RequestMoveTarget(int agentIndex
            , uint polyId
            , float[] position)
        {
            return CrowdManagerEx.RequestMoveTarget(root
                , agentIndex
                , polyId
                , position);
        }

        public bool AdjustMoveTarget(int agentIndex
            , uint polyId
            , float[] position)
        {
            return CrowdManagerEx.AdjustMoveTarget(root
                , agentIndex
                , polyId
                , position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <p>This is the most efficient method of getting core agent data
        /// from the crowd manager.</p>
        /// <p>WARNING: All structures in the agent data array must be initialized
        /// before passing the array to this method.</p>
        /// </remarks>
        /// <param name="agentData">An array containing INITIALIZED agent data.
        /// </param>
        /// <param name="agentCount"></param>
        public int GetAgentCoreData(CrowdAgentCoreData[] agentData)
        {
            return CrowdManagerEx.GetAgentCoreData(root
                , agentData
                , agentData.Length);
        }

    }
}

using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using PathRequest = org.critterai.nav.nmpath.MasterNavRequest<
        org.critterai.nav.nmpath.MasterPath.Path>.NavRequest;
using Path = org.critterai.nav.nmpath.MasterPath.Path;

namespace org.critterai.nav.nmpath
{
    [TestClass]
    public sealed class ClientPathManagerTest
    {
        private const int SearchLength = 100;

        private IntegrationTestData plannerData;
        private NavigationData navData;
        private ClientPathManager pathManager;
        private MasterPlanner.Planner nav;
        private Vector3 initialTarget = new Vector3(-9, -9, -9);

        [TestInitialize]
        public void Setup()
        {
            plannerData = new IntegrationTestData(false);
            plannerData.masterNavigator.Start();
            nav = plannerData.masterNavigator.PathPlanner;
            
            navData = new NavigationData(
                new Vector3(plannerData.samplePoints.samples[0]
                    , plannerData.samplePoints.samples[1]
                    , plannerData.samplePoints.samples[2])
                , Quaternion.identity
                , 1);
            navData.goalPosition = new Vector3(
                plannerData.samplePoints.samples[3]
                    , plannerData.samplePoints.samples[4]
                    , plannerData.samplePoints.samples[5]);
            navData.targetPosition = initialTarget;

            pathManager = new ClientPathManager(navData);
            pathManager.pathPlanner = plannerData.masterNavigator.PathPlanner;
        }

        [TestCleanup]
        public void Cleanup()
        {
            plannerData.masterNavigator.Dispose();
        }

        [TestMethod]
        public void TestBasicSuccessNoMovement()
        {
            Vector3 pos = navData.position;
            Vector3 goal = navData.goalPosition;
            PathRequest req = nav.GetPath(pos.x, pos.y, pos.z
                , goal.x, goal.y, goal.z);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            // Pause to allow search to complete.
            Thread.Sleep(SearchLength);

            Vector3 expectedTarget;
            req.Data.GetTarget(pos.x, pos.y, pos.z, out expectedTarget);
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(pathManager.Update() == NavigationState.Active);
                Assert.IsTrue(navData.targetPosition == expectedTarget);
            }
        }

        [TestMethod]
        public void TestBasicSuccessFollowPath()
        {

            Vector3 pos = navData.position;
            Vector3 goal = navData.goalPosition;
            PathRequest req = nav.GetPath(pos.x, pos.y, pos.z
                , goal.x, goal.y, goal.z);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            // Pause to allow search to complete.
            Thread.Sleep(SearchLength);

            for (int i = 0; i < 100; i++)
            {
                pos = navData.position;
                Vector3 expectedTarget;
                req.Data.GetTarget(pos.x, pos.y, pos.z, out expectedTarget);
                Assert.IsTrue(pathManager.Update() == NavigationState.Active);
                Assert.IsTrue(expectedTarget == navData.targetPosition);
                navData.position = navData.targetPosition;
            }
        }

        [TestMethod]
        public void TestChangeGoal()
        {
            Vector3 pos = navData.position;
            Vector3 goal = navData.goalPosition;
            PathRequest req = nav.GetPath(pos.x, pos.y, pos.z
                , goal.x, goal.y, goal.z);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            // Pause to allow search to complete.
            Thread.Sleep(SearchLength);

            Vector3 expectedTarget;
            req.Data.GetTarget(pos.x, pos.y, pos.z, out expectedTarget);

            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            Assert.IsTrue(expectedTarget == navData.targetPosition);

            // Change goal.
            navData.goalPosition = new Vector3(
                plannerData.samplePoints.samples[6]
                    , plannerData.samplePoints.samples[7]
                    , plannerData.samplePoints.samples[8]);

            pos = navData.position;
            goal = navData.goalPosition;
            req = nav.GetPath(pos.x, pos.y, pos.z
                , goal.x, goal.y, goal.z);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            // Pause to allow search to complete.
            Thread.Sleep(SearchLength);

            req.Data.GetTarget(pos.x, pos.y, pos.z, out expectedTarget);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            Assert.IsTrue(expectedTarget == navData.targetPosition);
        }

        [TestMethod]
        public void TestFailOnPathPlannerDisposal()
        {
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            plannerData.masterNavigator.Dispose();
            // Pause to allow time for the threaded navigator to notify
            // the root navigator of the disposal.
            Thread.Sleep(SearchLength);
            Assert.IsTrue(pathManager.Update() == NavigationState.Failed);
            Assert.IsTrue(navData.targetPosition == navData.position);
        }

        [TestMethod]
        public void TestFailOnNullPathPlanner()
        {
            pathManager.pathPlanner = null;
            Assert.IsTrue(pathManager.Update() == NavigationState.Failed);
            Assert.IsTrue(navData.targetPosition == navData.position);
        }

        [TestMethod]
        public void TestFailOnPathPlannerRemoved()
        {
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            pathManager.pathPlanner = null;
            Assert.IsTrue(pathManager.Update() == NavigationState.Failed);
            Assert.IsTrue(navData.targetPosition == navData.position);
        }

        [TestMethod]
        public void TestFailOnPathPlannerDelayedRemove()
        {
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(pathManager.Update() == NavigationState.Active);
                Thread.Sleep(10);
            }

            pathManager.pathPlanner = null;
            Assert.IsTrue(pathManager.Update() == NavigationState.Failed);
            Assert.IsTrue(navData.targetPosition == navData.position);
        }

        [TestMethod]
        public void TestPathingFailureTemporary()
        {
            Vector3 badPos = plannerData.meshMin;
            badPos.x -= 1;

            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            Thread.Sleep(SearchLength);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);

            Vector3 goodPos = navData.targetPosition;

            navData.position = badPos;
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(pathManager.Update() == NavigationState.Active);
                Assert.IsTrue(navData.targetPosition == goodPos);
            }

            navData.position = goodPos;
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            }
        }

        [TestMethod]
        public void TestPathingFailurePermanent()
        {
            Vector3 badPos = plannerData.meshMin;
            badPos.x -= 1;

            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            Thread.Sleep(SearchLength);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);

            Vector3 goodPos = navData.targetPosition;

            navData.position = badPos;
            bool failed = true;
            for (int i = 0; i < 40; i++)
            {
                Thread.Sleep(10);  // Expecting a search to occur.
                if (pathManager.Update() == NavigationState.Failed)
                {
                    failed = true;
                    break;
                }
            }

            Assert.IsTrue(failed);
            Assert.IsTrue(navData.targetPosition == navData.position);
        }

        [TestMethod]
        public void TestReuse()
        {
            Vector3 pos = navData.position;
            Vector3 goal = navData.goalPosition;
            PathRequest req = nav.GetPath(pos.x, pos.y, pos.z
                , goal.x, goal.y, goal.z);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            // Pause to allow search to complete.
            Thread.Sleep(SearchLength);

            Vector3 expectedTarget;
            req.Data.GetTarget(pos.x, pos.y, pos.z, out expectedTarget);

            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            Assert.IsTrue(expectedTarget == navData.targetPosition);

            pathManager.Exit();
            Assert.IsTrue(navData.targetPosition == navData.position);

            // Swap goal and position.
            pos = navData.goalPosition;
            navData.goalPosition = navData.position;
            navData.position = pos;

            pos = navData.position;
            goal = navData.goalPosition;
            req = nav.GetPath(pos.x, pos.y, pos.z
                , goal.x, goal.y, goal.z);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            // Pause to allow search to complete.
            Thread.Sleep(SearchLength);

            req.Data.GetTarget(pos.x, pos.y, pos.z, out expectedTarget);
            Assert.IsTrue(pathManager.Update() == NavigationState.Active);
            Assert.IsTrue(expectedTarget == navData.targetPosition);
        }
    }
}

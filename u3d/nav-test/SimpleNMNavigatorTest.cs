using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using PathRequest = org.critterai.nav.nmpath.MasterNavRequest<
        org.critterai.nav.nmpath.MasterPath.Path>.NavRequest;
using Path = org.critterai.nav.nmpath.MasterPath.Path;
using org.critterai.nav.nmpath;

namespace org.critterai.nav
{
    [TestClass]
    public sealed class SimpleNMNavigatorTest
    {
        /*
         * Design notes:
         * 
         * The tests in this class tend to be integration level
         * since most functionality is implemented by other
         * classes.
         * 
         * The primary purpose of tests is to validate that the class is 
         * properly managing the classes it controls.
         * 
         */

        private const int SearchLength = 100;

        private IntegrationTestData plannerData;
        private NavigationData navData;
        private MasterPlanner.Planner nav;
        private ConcreteSimpleMover sm;
        private SimpleNMNavigator mgr;

        private Quaternion standardGoalRotation = new Quaternion(1, 1, 1, 1);
        private float initialDeltaTime = 0.5f;
        private float initialSpeed = 0.8f;

        [TestInitialize]
        public void Setup()
        {
            plannerData = new IntegrationTestData(false);
            plannerData.masterNavigator.Start();
            nav = plannerData.masterNavigator.PathPlanner;
            
            navData = new NavigationData(Vector3.zero
                , Quaternion.identity
                , 1);
            navData.maximumSpeed = initialSpeed;

            sm = new ConcreteSimpleMover(navData);
            sm.DeltaTime = initialDeltaTime;
            mgr = new SimpleNMNavigator(navData, sm);
            mgr.SetPathPlanner(nav);
        }

        [TestCleanup]
        public void Cleanup()
        {
            plannerData.masterNavigator.Dispose();
        }

        [TestMethod]
        public void TestHasBeenProcessedBehavior()
        {
            navData.hasBeenProcessed = false;
            mgr.Update();
            navData.hasBeenProcessed = true;
        }

        [TestMethod]
        public void TestIsAtGoalImmediate()
        {
            Assert.IsTrue(mgr.Update() == NavigationState.Complete);
            Assert.IsTrue(navData.navState == NavigationState.Complete);
            ValidateCleanNavData();
        }

        [TestMethod]
        public void TestMovementDisabledImmediate()
        {
            navData.movementEnabled = false;
            Assert.IsTrue(mgr.Update() == NavigationState.Inactive);
            Assert.IsTrue(navData.navState == NavigationState.Inactive);
            ValidateCleanNavData();
        }

        [TestMethod]
        public void TestForceMovementImmediate()
        {
            navData.goalPosition = plannerData.samplePoints[1];
            navData.forceMovement = true;
            Assert.IsTrue(mgr.Update() == NavigationState.Complete);
            Assert.IsTrue(navData.navState == NavigationState.Complete);
            ValidateCleanNavData();
            Assert.IsTrue(navData.position == navData.goalPosition);
        }

        [TestMethod]
        public void TestNormalCompletionOnce()
        {
            navData.position = plannerData.samplePoints.GetPoint(0);
            navData.goalPosition = plannerData.samplePoints.GetPoint(
                    plannerData.samplePoints.Count - 1);

            Assert.IsTrue(mgr.Update() == NavigationState.Active);
            Thread.Sleep(100);  // To allow path search to complete.

            // 1000 iterations is equivalent to 500 seconds of simulated time.
            for (int j = 0; j < 1000; j++)
            {
                Vector3 lastPos = navData.position;
                NavigationState state = mgr.Update();
                Assert.IsTrue(navData.navState == state);
                if (state == NavigationState.Complete)
                    break;
                Assert.IsTrue(state == NavigationState.Active);
                Assert.IsTrue(navData.position != lastPos);
            }
            Assert.IsTrue(navData.IsAtGoal);
        }

        [TestMethod]
        public void TestNormalCompletionMulti()
        {
            TriNavMeshSamples samples = plannerData.samplePoints;
            Assert.IsTrue(samples.Count >= 40); // Test supported.
            for (int i = 0; i < 20; i++)
            {
                navData.position = samples[i];
                navData.goalPosition =
                    samples[samples.Count - 1 - i];

                Assert.IsTrue(mgr.Update() == NavigationState.Active);
                Thread.Sleep(100);  // To allow path search to complete.

                // 1000 iterations is equivalent to 500 seconds of simulated time.
                for (int j = 0; j < 1000; j++)
                {
                    Vector3 lastPos = navData.position;
                    NavigationState state = mgr.Update();
                    Assert.IsTrue(navData.navState == state);
                    if (state == NavigationState.Complete)
                        break;
                    Assert.IsTrue(state == NavigationState.Active);
                    Assert.IsTrue(navData.position != lastPos);
                }
                Assert.IsTrue(navData.IsAtGoal);
            }
        }

        [TestMethod]
        public void TestFailAndReusePlanner()
        {
            TriNavMeshSamples samples = plannerData.samplePoints;
            navData.position = samples.GetPoint(0);
            navData.goalPosition =
                samples.GetPoint(samples.Count - 1);

            Assert.IsTrue(mgr.Update() == NavigationState.Active);
            Thread.Sleep(100);  // To allow path search to complete.

            mgr.SetPathPlanner(null);
            Assert.IsTrue(mgr.Update() == NavigationState.Failed);
            Assert.IsTrue(navData.navState == NavigationState.Failed);
            ValidateCleanNavData();

            mgr.SetPathPlanner(nav);
            Assert.IsTrue(mgr.Update() == NavigationState.Active);
        }

        [TestMethod]
        public void TestFailAndReuseMover()
        {
            TriNavMeshSamples samples = plannerData.samplePoints;
            navData.position = samples[0];
            navData.goalPosition =
                samples.GetPoint(samples.Count - 1);

            Assert.IsTrue(mgr.Update() == NavigationState.Active);
            Thread.Sleep(100);  // To allow path search to complete.

            sm.failOnPreUpdate = true;
            Assert.IsTrue(mgr.Update() == NavigationState.Failed);
            Assert.IsTrue(navData.navState == NavigationState.Failed);
            ValidateCleanNavData();

            sm.failOnPreUpdate = false;
            Assert.IsTrue(mgr.Update() == NavigationState.Active);
        }

        private void ValidateCleanNavData()
        {
            Assert.IsTrue(navData.forceMovement == false);
            Assert.IsTrue(navData.hasBeenProcessed == true);
            Assert.IsTrue(navData.targetPosition == navData.position);
            Assert.IsTrue(navData.targetVelocity == Vector3.zero);
            Assert.IsTrue(navData.velocity == Vector3.zero);
        }
    }
}

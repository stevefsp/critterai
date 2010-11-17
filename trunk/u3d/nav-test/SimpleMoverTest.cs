using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using org.critterai.math;

namespace org.critterai.nav
{
    [TestClass]
    public sealed class SimpleMoverTest
    {
        /*
         * Design Notes:
         * 
         * This group of tests validates the functionality of
         * an abstract class via a single concrete implementation.
         * 
         */

        private NavigationData navData;
        private Vector3 initialGoal = new Vector3(5, 2, -7);
        private Vector3 initialPosition = new Vector3(0.8f, -1.1f, -0.5f);
        private Vector3 initialTargetVelocity = new Vector3(0.4f, 0.1f, 1.0f);
        private Vector3 initialVelocity = new Vector3(1, 1, -1);
        private Quaternion initialRotation = new Quaternion(5, 5, 5, 5);
        private Quaternion initialGoalRotation = new Quaternion(1, 1, 1, 1);
        private float initialDeltaTime = 0.5f;
        private ConcreteSimpleMover sm;

        [TestInitialize]
        public void Setup()
        {
            navData = new NavigationData(initialPosition
                , initialRotation
                , 1);
            navData.goalPosition = initialGoal;
            navData.targetVelocity = initialTargetVelocity;
            navData.velocity = initialVelocity;
            navData.goalRotation = initialGoalRotation;

            sm = new ConcreteSimpleMover(navData);
            sm.DeltaTime = initialDeltaTime;
        }

        [TestMethod]
        public void TestBasicOperation()
        {
            for (int i = 1; i < 11; i++)
            {
                Vector3 startPos = navData.position;
                Assert.IsTrue(sm.Update() == NavigationState.Active);
                Assert.IsTrue(sm.applyMovementCallCount == i);
                Assert.IsTrue(sm.initializeCallCount == 1);
                Assert.IsTrue(sm.localExitCallCount == 0);
                Assert.IsTrue(sm.preUpdateCallCount == i);
                Assert.IsTrue(Vector3Util.SloppyEquals(navData.velocity
                    , navData.targetVelocity
                    , 0.0001f));
                Assert.IsTrue(Vector3Util.SloppyEquals(navData.position
                    , GetExpectedPosition(startPos
                        , navData.targetVelocity
                        , sm.DeltaTime)
                    , 0.0001f));
            }
            // A basic validation of the test conditions...
            Assert.IsTrue(navData.position != initialPosition);
        }

        [TestMethod]
        public void TestFailOnInitialize()
        {
            sm.failOnInitialize = true;
            Assert.IsTrue(sm.Update() == NavigationState.Failed);
            Assert.IsTrue(sm.applyMovementCallCount == 0);
            Assert.IsTrue(sm.initializeCallCount == 1);
            Assert.IsTrue(sm.localExitCallCount == 0);
            Assert.IsTrue(sm.preUpdateCallCount == 0);
            Assert.IsTrue(navData.velocity == initialVelocity);
            Assert.IsTrue(navData.position == initialPosition);
        }

        [TestMethod]
        public void TestFailOnPreUpdate()
        {
            sm.failOnPreUpdate = true;
            Assert.IsTrue(sm.Update() == NavigationState.Failed);
            Assert.IsTrue(sm.applyMovementCallCount == 0);
            Assert.IsTrue(sm.initializeCallCount == 1);
            Assert.IsTrue(sm.localExitCallCount == 0);
            Assert.IsTrue(sm.preUpdateCallCount == 1);
            Assert.IsTrue(navData.velocity == initialVelocity);
            Assert.IsTrue(navData.position == initialPosition);
        }

        [TestMethod]
        public void TestFailOnApplyMovement()
        {
            sm.failOnApplyMovement = true;
            Assert.IsTrue(sm.Update() == NavigationState.Failed);
            Assert.IsTrue(sm.applyMovementCallCount == 1);
            Assert.IsTrue(sm.initializeCallCount == 1);
            Assert.IsTrue(sm.localExitCallCount == 0);
            Assert.IsTrue(sm.preUpdateCallCount == 1);
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.velocity
                , navData.targetVelocity
                , 0.0001f));
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.position
                , GetExpectedPosition(initialPosition
                    , navData.targetVelocity
                    , sm.DeltaTime)
                , 0.0001f));
        }

        [TestMethod]
        public void TestImmediateCompletion()
        {
            navData.position = navData.goalPosition;
            Assert.IsTrue(sm.Update() == NavigationState.Complete);
            Assert.IsTrue(sm.applyMovementCallCount == 1);
            Assert.IsTrue(sm.initializeCallCount == 1);
            Assert.IsTrue(sm.localExitCallCount == 0);
            Assert.IsTrue(sm.preUpdateCallCount == 1);
            Assert.IsTrue(navData.velocity == Vector3.zero);
            Assert.IsTrue(navData.rotation == navData.goalRotation);
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.position
                , navData.goalPosition
                , 0.0001f));
        }

        [TestMethod]
        public void TestFailOnInvalidReuseAfterComplete()
        {
            navData.position = navData.goalPosition;
            Assert.IsTrue(sm.Update() == NavigationState.Complete);
            Assert.IsTrue(sm.Update() == NavigationState.Failed);
            Assert.IsTrue(sm.applyMovementCallCount == 1);
            Assert.IsTrue(sm.initializeCallCount == 1);
            Assert.IsTrue(sm.localExitCallCount == 0);
            Assert.IsTrue(sm.preUpdateCallCount == 1);
        }

        [TestMethod]
        public void TestFailOnInvalidReuseAfterFail()
        {
            sm.failOnInitialize = true;
            Assert.IsTrue(sm.Update() == NavigationState.Failed);
            Assert.IsTrue(sm.Update() == NavigationState.Failed);
            Assert.IsTrue(sm.applyMovementCallCount == 0);
            Assert.IsTrue(sm.initializeCallCount == 1);
            Assert.IsTrue(sm.localExitCallCount == 0);
            Assert.IsTrue(sm.preUpdateCallCount == 0);
        }

        [TestMethod]
        public void TestNormalResue()
        {
            navData.position = navData.goalPosition;
            Assert.IsTrue(sm.Update() == NavigationState.Complete);
            sm.Exit();
            Assert.IsTrue(sm.localExitCallCount == 1);
            navData.position = initialPosition;
            Assert.IsTrue(sm.Update() == NavigationState.Active);
            Assert.IsTrue(sm.applyMovementCallCount == 2);
            Assert.IsTrue(sm.initializeCallCount == 2);
            Assert.IsTrue(sm.localExitCallCount == 1);
            Assert.IsTrue(sm.preUpdateCallCount == 2);
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.velocity
                , navData.targetVelocity
                , 0.0001f));
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.position
                , GetExpectedPosition(initialPosition
                    , navData.targetVelocity
                    , sm.DeltaTime)
                , 0.0001f));
        }

        [TestMethod]
        public void TestForceMovement()
        {
            navData.forceMovement = true;
            Assert.IsTrue(sm.Update() == NavigationState.Complete);
            Assert.IsTrue(sm.applyMovementCallCount == 1);
            Assert.IsTrue(sm.initializeCallCount == 1);
            Assert.IsTrue(sm.localExitCallCount == 0);
            Assert.IsTrue(sm.preUpdateCallCount == 1);
            Assert.IsTrue(navData.velocity == Vector3.zero);
            Assert.IsTrue(navData.position == navData.goalPosition);
            Assert.IsTrue(navData.rotation == navData.goalRotation);
            Assert.IsTrue(!navData.forceMovement);
        }

        [TestMethod]
        public void TestNormalCompletion()
        {
            navData.targetVelocity = 
                (navData.goalPosition - navData.position) * 0.1f;
            int i;
            Vector3 startPos = Vector3.zero;
            for (i = 1; i < 50; i++)
            {
                startPos = navData.position;
                NavigationState state = sm.Update(); 
                Assert.IsTrue(sm.applyMovementCallCount == i);
                Assert.IsTrue(sm.initializeCallCount == 1);
                Assert.IsTrue(sm.localExitCallCount == 0);
                Assert.IsTrue(sm.preUpdateCallCount == i);
                if (state == NavigationState.Complete)
                    break;
                Assert.IsTrue(state == NavigationState.Active);
                Assert.IsTrue(Vector3Util.SloppyEquals(navData.velocity
                    , navData.targetVelocity
                    , 0.0001f));
                Assert.IsTrue(Vector3Util.SloppyEquals(navData.position
                    , GetExpectedPosition(startPos
                        , navData.targetVelocity
                        , sm.DeltaTime)
                    , 0.0001f));
            }
            Assert.IsTrue(sm.applyMovementCallCount == i);
            Assert.IsTrue(sm.initializeCallCount == 1);
            Assert.IsTrue(sm.localExitCallCount == 0);
            Assert.IsTrue(sm.preUpdateCallCount == i);
            Assert.IsTrue(navData.rotation == navData.goalRotation);
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.position
                , navData.goalPosition
                , 0.0001f));
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.velocity
                , GetExpectedVelocity(startPos, navData.position, sm.DeltaTime)
                , 0.0001f));
        }

        private Vector3 GetExpectedVelocity(Vector3 startPos
            , Vector3 endPos
            , float deltaTime)
        {
            return (endPos - startPos) / deltaTime;
        }

        private Vector3 GetExpectedPosition(Vector3 pos
            , Vector3 velocity
            , float deltaTime)
        {
            return pos + (velocity * deltaTime);
        }
    }
}

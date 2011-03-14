using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using org.critterai.math;

namespace org.critterai.nav
{
    [TestClass]
    public sealed class SimpleVelocityManagerTest
    {
        private NavigationData navData;
        private Vector3 initialTarget = new Vector3(-9, 4.2f, 7);
        private Vector3 initialGoal = new Vector3(5, 2, -7);
        private Vector3 initialPosition = new Vector3(0.8f, -1.1f, -0.5f);
        private float initialSpeed = 2.1f;
        private SimpleVelocityManager svm;

        [TestInitialize]
        public void Setup()
        {
            navData = new NavigationData(initialPosition
                , Quaternion.identity
                , 1);
            navData.goalPosition = initialGoal;
            navData.targetPosition = initialTarget;
            navData.maximumSpeed = initialSpeed;

            svm = new SimpleVelocityManager(navData);
        }

        [TestMethod]
        public void TestBasicVelocityCalculation()
        {
            Assert.IsTrue(svm.Update() == NavigationState.Active);
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.targetVelocity
                , GetExpectedVelocity(navData), 0.0001f));
        }

        [TestMethod]
        public void TestAtTarget()
        {
            svm.Update();
            Assert.IsTrue(navData.targetVelocity != Vector3.zero);
            navData.position = navData.targetPosition;
            navData.position.x += navData.xzTolerance * 0.998f;
            // Make sure it doesn't complete.
            Assert.IsTrue(svm.Update() == NavigationState.Active);
            Assert.IsTrue(navData.targetVelocity == Vector3.zero);
        }

        [TestMethod]
        public void TestStaticBehavior()
        {
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(svm.Update() == NavigationState.Active);
                Assert.IsTrue(Vector3Util.SloppyEquals(navData.targetVelocity
                    , GetExpectedVelocity(navData), 0.0001f));
            }
        }

        [TestMethod]
        public void TestChangeToPosition()
        {
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(svm.Update() == NavigationState.Active);
                Assert.IsTrue(Vector3Util.SloppyEquals(navData.targetVelocity
                    , GetExpectedVelocity(navData), 0.0001f));
                navData.position += new Vector3(0.1f, 1.5f, -0.8f);
            }
        }

        [TestMethod]
        public void TestChangeToTarget()
        {
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(svm.Update() == NavigationState.Active);
                Assert.IsTrue(Vector3Util.SloppyEquals(navData.targetVelocity
                    , GetExpectedVelocity(navData), 0.0001f));
                navData.targetPosition += new Vector3(0.1f, 1.5f, -0.8f);
            }
        }

        [TestMethod]
        public void TestExit()
        {
            svm.Update();
            Assert.IsTrue(navData.targetVelocity != Vector3.zero);
            svm.Exit();
            Assert.IsTrue(navData.targetVelocity == Vector3.zero);
        }

        [TestMethod]
        public void TestReuse()
        {
            svm.Update();
            svm.Exit();
            Assert.IsTrue(svm.Update() == NavigationState.Active);
            Assert.IsTrue(Vector3Util.SloppyEquals(navData.targetVelocity
                , GetExpectedVelocity(navData), 0.0001f));
        }

        public Vector3 GetExpectedVelocity(NavigationData data)
        {
            return (data.targetPosition - data.position).normalized
                * data.maximumSpeed;
        }
 
    }
}

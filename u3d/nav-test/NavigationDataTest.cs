using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;
using PathRequest = org.critterai.nav.nmpath.MasterNavRequest<
        org.critterai.nav.nmpath.MasterPath.Path>.NavRequest;
using Path = org.critterai.nav.nmpath.MasterPath.Path;

namespace org.critterai.nav
{
    [TestClass]
    public sealed class NavigationDataTest
    {
        private NavigationData navData;
        private Vector3 initialTarget = new Vector3(-9, 4.2f, 7);
        private Vector3 initialGoal = new Vector3(5, 2, -7);
        private Vector3 initialPosition = new Vector3(0.8f, -1.1f, -0.5f);

        [TestInitialize]
        public void Setup()
        {
            navData = new NavigationData(initialPosition
                , Quaternion.identity
                , 1);
            navData.goalPosition = initialGoal;
            navData.targetPosition = initialTarget;
        }

        [TestMethod]
        public void TestConstruction()
        {
            Quaternion q = new Quaternion(1, 2, 3, 4);
            float radius = 0.8f;
            navData = new NavigationData(initialPosition, q, radius);
            Assert.IsTrue(navData.forceMovement == false);
            Assert.IsTrue(navData.goalPosition == initialPosition);
            Assert.IsTrue(navData.goalRotation == q);
            Assert.IsTrue(navData.hasBeenProcessed == false);
            Assert.IsTrue(navData.IsAtGoal);
            Assert.IsTrue(navData.IsAtTarget);
            Assert.IsTrue(navData.maximumSpeed == 0);
            Assert.IsTrue(navData.movementEnabled == true);
            Assert.IsTrue(navData.navState == NavigationState.Inactive);
            Assert.IsTrue(navData.position == initialPosition);
            Assert.IsTrue(navData.radius == radius);
            Assert.IsTrue(navData.rotation == q);
            Assert.IsTrue(navData.targetPosition == initialPosition);
            Assert.IsTrue(navData.targetVelocity == Vector3.zero);
            Assert.IsTrue(navData.velocity == Vector3.zero);
            Assert.IsTrue(navData.xzTolerance 
                == NavigationData.DefaultPositionTolerance);
            Assert.IsTrue(navData.yTolerance
                == NavigationData.DefaultPositionTolerance);
        }

        [TestMethod]
        public void TestIsAtTargetX()
        {
            // Only testing basic functionality for the axis.
            navData.xzTolerance = NavigationData.DefaultPositionTolerance * 10;
            navData.yTolerance = NavigationData.DefaultPositionTolerance * 100f;
            navData.targetPosition = navData.position;
            navData.targetPosition.x += navData.xzTolerance * 0.998f;
            Assert.IsTrue(navData.IsAtTarget);
            navData.targetPosition.x += navData.xzTolerance;
            Assert.IsTrue(!navData.IsAtTarget);
        }

        [TestMethod]
        public void TestIsAtTargetY()
        {
            // Only testing basic functionality for the axis.
            navData.xzTolerance = NavigationData.DefaultPositionTolerance * 10;
            navData.yTolerance = NavigationData.DefaultPositionTolerance * 100f;
            navData.targetPosition = navData.position;
            navData.targetPosition.y += navData.yTolerance * 0.998f;
            Assert.IsTrue(navData.IsAtTarget);
            navData.targetPosition.y += navData.yTolerance;
            Assert.IsTrue(!navData.IsAtTarget);
        }

        [TestMethod]
        public void TestIsAtTargetZ()
        {
            // Only testing basic functionality for the axis.
            navData.xzTolerance = NavigationData.DefaultPositionTolerance * 10;
            navData.yTolerance = NavigationData.DefaultPositionTolerance * 100f;
            navData.targetPosition = navData.position;
            navData.targetPosition.z += navData.xzTolerance * 0.998f;
            Assert.IsTrue(navData.IsAtTarget);
            navData.targetPosition.z += navData.xzTolerance;
            Assert.IsTrue(!navData.IsAtTarget);
        }

        [TestMethod]
        public void TestIsAtGoalX()
        {
            // Only testing basic functionality for the axis.
            navData.xzTolerance = NavigationData.DefaultPositionTolerance * 10;
            navData.yTolerance = NavigationData.DefaultPositionTolerance * 100f;
            navData.goalPosition = navData.position;
            navData.goalPosition.x += navData.xzTolerance * 0.998f;
            Assert.IsTrue(navData.IsAtGoal);
            navData.goalPosition.x += navData.xzTolerance;
            Assert.IsTrue(!navData.IsAtGoal);
        }

        [TestMethod]
        public void TestIsAtGoalY()
        {
            // Only testing basic functionality for the axis.
            navData.xzTolerance = NavigationData.DefaultPositionTolerance * 10;
            navData.yTolerance = NavigationData.DefaultPositionTolerance * 100f;
            navData.goalPosition = navData.position;
            navData.goalPosition.y += navData.yTolerance * 0.998f;
            Assert.IsTrue(navData.IsAtGoal);
            navData.goalPosition.y += navData.yTolerance;
            Assert.IsTrue(!navData.IsAtGoal);
        }

        [TestMethod]
        public void TestIsAtGoalZ()
        {
            // Only testing basic functionality for the axis.
            navData.xzTolerance = NavigationData.DefaultPositionTolerance * 10;
            navData.yTolerance = NavigationData.DefaultPositionTolerance * 100f;
            navData.goalPosition = navData.position;
            navData.goalPosition.z += navData.xzTolerance * 0.998f;
            Assert.IsTrue(navData.IsAtGoal);
            navData.goalPosition.z += navData.xzTolerance;
            Assert.IsTrue(!navData.IsAtGoal);
        }
 
    }
}

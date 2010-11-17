using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace org.critterai.nav
{
    public class DynamicObstacleData
        : ObstacleData
    {
        public Vector3 velocity = Vector3.zero;

        public DynamicObstacleData(Vector3 position, Quaternion rotation, float radius)
            : base(position, rotation, radius)
        {
        }
    }
}

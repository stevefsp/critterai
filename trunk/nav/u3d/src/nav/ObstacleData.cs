using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace org.critterai.nav
{
    public class ObstacleData
    {
        public Vector3 position;
        public Quaternion rotation;
        public float radius;

        public ObstacleData(Vector3 position, Quaternion rotation, float radius)
        {
            this.position = position;
            this.rotation = rotation;
            this.radius = Math.Max(0, radius);
        }
    }
}

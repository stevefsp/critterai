#if NUNITY

using System.Runtime.InteropServices;

namespace org.critterai
{
    /// <summary>
    /// Represents a 3D floating point vector.
    /// </summary>
    /// <remarks>
    /// <para>Not present in the Unity build.
    /// </para>
    /// <para>This structure's API kept very simple in order to minimize
    /// conflicts with other Vector implementations.  The 
    /// <see cref="Vector3Util"/> class provides various standard Vector3 
    /// operations.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static bool operator ==(Vector3 v, Vector3 u)
        {
            return (v.x == u.x && v.y == u.y && v.z == u.z);
        }

        public static bool operator !=(Vector3 v, Vector3 u)
        {
            return !(v.x == u.x && v.y == u.y && v.z == u.z);
        }

        public static Vector3 operator -(Vector3 v, Vector3 u)
        {
            return new Vector3(v.x - u.x, v.y - u.y, v.z - u.z);
        }

        public static Vector3 operator +(Vector3 v, Vector3 u)
        {
            return new Vector3(v.x + u.x, v.y + u.y, v.z + u.z);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3)
            {
                Vector3 u = (Vector3)obj;
                return (x == u.x && y == u.y && z == u.z);
            }
            return false;
        }
    }
}

#endif
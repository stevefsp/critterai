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
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Vector2 v, Vector2 u)
        {
            return (v.x == u.x && v.y == u.y);
        }

        public static bool operator !=(Vector2 v, Vector2 u)
        {
            return !(v.x == u.x && v.y == u.y);
        }

        public static Vector2 operator -(Vector2 v, Vector2 u)
        {
            return new Vector2(v.x - u.x, v.y - u.y);
        }

        public static Vector2 operator +(Vector2 v, Vector2 u)
        {
            return new Vector2(v.x + u.x, v.y + u.y);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                Vector2 u = (Vector2)obj;
                return (x == u.x && y == u.y);
            }
            return false;
        }
    }
}

#endif
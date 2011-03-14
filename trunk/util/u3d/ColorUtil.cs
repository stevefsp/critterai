using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace org.critterai
{
    public static class ColorUtil
    {
        // Returns 1 if the bit at position b in a is 1
        // Otherwise returns 0.
        private static int bit(int a, int b)
        {
            return (a & (1 << b)) >> b;
        }

        public static Color IntToColor(int i, float a)
        {
            // r, g, and b are constrained to between 1 and 4 inclusive.
            float factor = 63 / 255;  // Approximately 0.25.
	        float r = bit(i, 1) + bit(i, 3) * 2 + 1;
	        float g = bit(i, 2) + bit(i, 4) * 2 + 1;
	        float b = bit(i, 0) + bit(i, 5) * 2 + 1;
            return new Color(r * factor, g * factor, b * factor, a);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace org.critterai.nav
{
    public static class NavUtil
    {
        public static bool IsComplete(NavigationState state)
        {
            return (state == NavigationState.Complete 
                || state == NavigationState.Failed);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace org.critterai.nav
{
    public abstract class BaseNavComponent
    {
        protected readonly NavigationData navData;
        protected NavigationState state = NavigationState.Inactive;

        public BaseNavComponent(NavigationData navData)
        {
            this.navData = navData;
        }
    }
}

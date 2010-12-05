using System;
using System.Collections.Generic;
using System.Text;

namespace org.critterai.nav
{
    public interface INavComponent
    {
        NavigationState Update();
        void Exit();
    }
}

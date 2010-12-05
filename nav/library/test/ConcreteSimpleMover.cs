using System;
using System.Collections.Generic;
using System.Text;

namespace org.critterai.nav
{
    public sealed class ConcreteSimpleMover
        : SimpleMover
    {

        /*
         * Design Notes: 
         * 
         * While used for otehr purposes as well, this class' primary purpose 
         * is to test its base class.
         * 
         */

        public bool failOnInitialize = false;
        public bool failOnPreUpdate = false;
        public bool failOnApplyMovement = false;

        public int initializeCallCount = 0;
        public int preUpdateCallCount = 0;
        public int localExitCallCount = 0;
        public int applyMovementCallCount = 0;

        public float DeltaTime
        {
            get { return deltaTime; }
            set { deltaTime = value; }
        }

        public ConcreteSimpleMover(NavigationData navData)
            : base(navData)
        {
        }

        protected override bool Initialize()
        {
            initializeCallCount++;
            return !failOnInitialize;
        }

        protected override void LocalExit()
        {
            localExitCallCount++;
        }

        protected override bool PreUpdate()
        {
            preUpdateCallCount++;
            return !failOnPreUpdate;
        }

        protected override bool ApplyMovement()
        {
            applyMovementCallCount++;
            return !failOnApplyMovement;
        }
    }
}

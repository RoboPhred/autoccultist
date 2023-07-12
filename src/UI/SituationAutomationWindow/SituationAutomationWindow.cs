namespace AutoccultistNS.UI
{
    using System;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameResources;
    using AutoccultistNS.GameState;
    using SecretHistories.Entities;

    public class SituationAutomationWindow : SituationWindow
    {
        private IWindowView currentView;

        private IResourceConstraint<ISituationState> lockOutAfterConstraint;
        private UISituationLockoutConstraint lockoutConstraint;

        public bool IsAutomating
        {
            get
            {
                return this.CurrentResourceConstraint is OperationReaction;
            }
        }

        public bool IsLockedOut
        {
            get
            {
                return this.lockOutAfterConstraint != null || this.lockoutConstraint != null;
            }
        }

        private IResourceConstraint<ISituationState> CurrentResourceConstraint
        {
            get
            {
                var situation = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == this.Situation.VerbId);
                return GameResources.GameResource.Of<ISituationState>().GetConstraint(situation);
            }
        }

        public static SituationAutomationWindow CreateWindow(Situation situation)
        {
            var window = SituationWindow.CreateWindow<SituationAutomationWindow>($"window_${situation.VerbId}_automation");
            window.Attach(situation);
            return window;
        }

        public void ToggleLockout()
        {
            if (this.lockOutAfterConstraint != null)
            {
                // We are pending a lock-out already, but the user is impatient.
                this.lockOutAfterConstraint.Dispose();
                return;
            }

            if (this.lockoutConstraint != null)
            {
                // Locked out, clear the lockout.
                this.lockoutConstraint.Dispose();
                this.lockoutConstraint = null;

                if (this.lockOutAfterConstraint != null)
                {
                    this.lockOutAfterConstraint.Disposed -= this.LockOutAfterConstraint;
                    this.lockOutAfterConstraint = null;
                }

                // Now that we can do something with the situation, wake up the brain.
                NucleusAccumbens.ReevaluateImpulses();

                return;
            }

            // Not locked out, establish one if we can.
            var currentConstraint = this.CurrentResourceConstraint;
            this.lockoutConstraint = new UISituationLockoutConstraint(this.Situation.VerbId);

            if (currentConstraint == null)
            {
                // Not doing anything, lock out immediately.
                GameResources.GameResource.Of<ISituationState>().AddConstraint(this.lockoutConstraint);
            }
            else
            {
                this.lockOutAfterConstraint = currentConstraint;

                // Doing something, lock out after the current constraint is done.
                currentConstraint.Disposed += this.LockOutAfterConstraint;
            }
        }

        protected override void OnUpdate()
        {
            if (this.IsClosed)
            {
                // HACK: Fuck scrollbars
                if (this.currentView != null)
                {
                    this.Content.Clear();
                    this.currentView = null;
                }

                return;
            }

            if (this.CurrentResourceConstraint == null && this.currentView is not NewOperationView)
            {
                this.Content.Clear();
                this.currentView = new NewOperationView(this, this.Content.GameObject.transform);
            }
            else if (this.CurrentResourceConstraint is UISituationLockoutConstraint && this.currentView is not LockoutView)
            {
                this.Content.Clear();
                this.currentView = new LockoutView(this, this.Content.GameObject.transform);
            }
            else if (this.CurrentResourceConstraint is OperationReaction reaction && (this.currentView is not OngoingOperationView view || view.Reaction != reaction))
            {
                this.Content.Clear();
                this.currentView = new OngoingOperationView(this, reaction, this.Content.GameObject.transform);
            }

            if (this.currentView != null)
            {
                this.currentView.UpdateContent();
            }
        }

        private void LockOutAfterConstraint(object sender, EventArgs e)
        {
            if (this.lockoutConstraint == null)
            {
                return;
            }

            this.lockOutAfterConstraint.Disposed -= this.LockOutAfterConstraint;
            this.lockOutAfterConstraint = null;
            GameResources.GameResource.Of<ISituationState>().AddConstraint(this.lockoutConstraint);
        }
    }
}

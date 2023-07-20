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
            var window = SituationWindow.CreateTabletopWindow<SituationAutomationWindow>($"Window_{situation.VerbId}_automation");
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

        protected override void OnAwake()
        {
            base.OnAwake();

            this.Icon.AddImage().Sprite(ResourcesManager.GetSpriteForUI("autoccultist_situation_automation_badge"));
        }

        protected override void OnAttach()
        {
            base.OnAttach();
            this.Title = $"{this.Situation.VerbId.Capitalize()} Automations";
        }

        protected override void OnUpdate()
        {
            if (this.IsClosed)
            {
                if (this.currentView != null)
                {
                    this.Clear();
                    this.currentView = null;
                }

                return;
            }

            if (this.CurrentResourceConstraint == null && this.currentView is not OperationListView)
            {
                this.Clear();
                this.currentView = new OperationListView(this, this.Content, this.Footer);
            }
            else if (this.CurrentResourceConstraint is UISituationLockoutConstraint && this.currentView is not LockoutView)
            {
                this.Clear();
                this.currentView = new LockoutView(this, this.Content);
            }
            else if (this.CurrentResourceConstraint is OperationReaction reaction && (this.currentView is not OngoingOperationView view || view.Reaction != reaction))
            {
                this.Clear();
                this.currentView = new OngoingOperationView(this, reaction, this.Content, this.Footer);
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

namespace AutoccultistNS.UI
{
    using System;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameResources;
    using AutoccultistNS.GameState;
    using Roost.Piebald;
    using SecretHistories.Entities;
    using UnityEngine;

    public class SituationAutomationWindow : AbstractTableViewWindow<SituationAutomationWindow.IWindowContext>, SituationAutomationWindow.IWindowContext
    {
        private IResourceConstraint<ISituationState> lockOutAfterConstraint;
        private UISituationLockoutConstraint lockoutConstraint;

        public interface IWindowContext : IWindowViewHost<IWindowContext>
        {
            Situation Situation { get; }

            bool IsLockedOut { get; }

            void ToggleLockout();
        }

        public Situation Situation { get; private set; }

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

        protected override int DefaultWidth => 500;

        protected override int DefaultHeight => 400;

        protected override string DefaultTitle => $"{this.Situation?.VerbId.Capitalize()} Automations";

        protected override Sprite DefaultIcon => ResourcesManager.GetSpriteForVerbLarge(this.Situation?.VerbId ?? "x");

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
            var window = WindowFactory.CreateWindow<SituationAutomationWindow>($"Window_{situation.VerbId}_automation");
            window.Attach(situation);
            return window;
        }

        public void Attach(Situation situation)
        {
            this.Situation = situation;
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

            NucleusAccumbens.ReevaluateImpulses();
        }

        protected override void OnUpdate()
        {
            if (!this.IsVisible)
            {
                return;
            }

            if (this.CurrentResourceConstraint == null && this.View is not OperationsListView)
            {
                this.View = new OperationsListView();
            }
            else if (this.CurrentResourceConstraint is UISituationLockoutConstraint && this.View is not LockoutView)
            {
                this.View = new LockoutView();
            }
            else if (this.CurrentResourceConstraint is OperationReaction reaction && (this.View is not OngoingOperationView view || view.Reaction != reaction))
            {
                this.View = new OngoingOperationView(reaction);
            }

            base.OnUpdate();
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

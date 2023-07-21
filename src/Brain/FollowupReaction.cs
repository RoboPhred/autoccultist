namespace AutoccultistNS.Brain
{
    using System;

    public class FollowupReaction : IReaction
    {
        private readonly IReaction first;
        private readonly IReaction followup;
        private FollowupState state = FollowupState.Unstarted;

        public FollowupReaction(IReaction first, IReaction followup)
        {
            this.first = first;
            this.followup = followup;
        }

        public event EventHandler<ReactionEndedEventArgs> Ended;

        private enum FollowupState
        {
            Unstarted,

            First,

            Followup,

            Complete,
        }

        public void Abort()
        {
            switch (this.state)
            {
                case FollowupState.First:
                    this.first.Abort();
                    break;
                case FollowupState.Followup:
                    this.followup.Abort();
                    break;
            }
        }

        public void Start()
        {
            if (this.state != FollowupState.Unstarted)
            {
                throw new InvalidOperationException("Cannot start a FollowupReaction more than once.");
            }

            this.state = FollowupState.First;
            this.first.Ended += this.OnFirstEnded;
            this.first.Start();
        }

        public override string ToString()
        {
            switch (this.state)
            {
                default:
                case FollowupState.Unstarted:
                case FollowupState.First:
                    return this.first.ToString();
                case FollowupState.Followup:
                case FollowupState.Complete:
                    return this.followup.ToString();
            }
        }

        private void OnFirstEnded(object sender, ReactionEndedEventArgs e)
        {
            this.first.Ended -= this.OnFirstEnded;

            if (e.Aborted)
            {
                this.state = FollowupState.Complete;
                this.Ended?.Invoke(this, e);
                return;
            }

            this.state = FollowupState.Followup;

            this.followup.Ended += this.OnFollowupEnded;
            this.followup.Start();
        }

        private void OnFollowupEnded(object sender, ReactionEndedEventArgs e)
        {
            this.state = FollowupState.Complete;

            this.followup.Ended -= this.OnFollowupEnded;

            this.Ended?.Invoke(this, e);
        }
    }
}

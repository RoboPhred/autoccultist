namespace AutoccultistNS.Brain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Actor;
    using AutoccultistNS.Actor.Actions;

    public class DumpSituationReaction : SituationReaction
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();

        private Task task;

        public DumpSituationReaction(string situationId)
            : base(situationId)
        {
        }

        public override void Start()
        {
            if (this.task != null)
            {
                throw new InvalidOperationException("DumpSituationReaction has already started.");
            }

            this.task = this.Dump();
        }

        public override string ToString()
        {
            return $"[{this.SituationId}] Empty Situation";
        }

        protected override async void OnAbort()
        {
            this.cancellationTokenSource.Cancel();

            try
            {
                await this.task;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Autoccultist.LogWarn(ex, $"Exception while waiting for DumpSituationReaction ${this.SituationId} to abort.");
            }

            this.TryComplete(true);
        }

        private async Task Dump()
        {
            try
            {
                await GameAPI.WhilePaused(
                    $"{this.GetType().Name}:{nameof(this.Dump)}",
                    async () =>
                    {
                        await GameAPI.AwaitNotInMansus(this.cancellationTokenSource.Token);
                        await Cerebellum.Coordinate(
                            (innerToken) => new ConcludeSituationAction(this.SituationId).ExecuteAndWait(innerToken),
                            this.cancellationTokenSource.Token);
                    },
                    this.cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                this.TryComplete();
            }
        }
    }
}

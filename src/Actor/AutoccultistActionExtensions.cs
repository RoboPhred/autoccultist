namespace AutoccultistNS.Actor
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class AutoccultistActionExtensions
    {
        public static async Task<bool> ExecuteAndWait(this IAutoccultistAction action, CancellationToken cancellationToken)
        {
            var result = await action.Execute(cancellationToken);
            if (result)
            {
                await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
            }

            return result;
        }
    }
}

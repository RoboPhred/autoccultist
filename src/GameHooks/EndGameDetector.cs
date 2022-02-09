namespace Autoccultist.GameHooks
{
    using Assets.CS.TabletopUI;
    using HarmonyLib;

    /// <summary>
    /// Hook for when the game proper ends.
    /// </summary>
    [HarmonyPatch(typeof(TabletopManager), "OnDestroy")]
    internal sealed class EndGameDetector
    {
        private static void Postfix()
        {
            GameEventSource.RaiseGameEnded();
        }
    }
}

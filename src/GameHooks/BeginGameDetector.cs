namespace Autoccultist.GameHooks
{
    using Assets.CS.TabletopUI;
    using HarmonyLib;

    /// <summary>
    /// Hook for when the game proper begins.
    /// </summary>
    [HarmonyPatch(typeof(TabletopManager), "BeginGame")]
    internal sealed class BeginGameDetector
    {
        private static void Postfix()
        {
            GameEventSource.RaiseGameStarted();
        }
    }
}

namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SecretHistories.Core;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Spheres;
    using SecretHistories.UI;

    /// <summary>
    /// Utility classes for logging the status of situations.
    /// </summary>
    internal static class SituationLogger
    {
        /// <summary>
        /// Dump information about the situations to the console.
        /// </summary>
        public static void LogSituations()
        {
            LogInfo("Seeking situation tokens...");
            foreach (var situation in Watchman.Get<HornedAxe>().GetRegisteredSituations())
            {
                LogInfo("We found a situation token - " + situation.VerbId);
                DumpSituationStatus(situation);
            }

            LogInfo("...Done seeking situation tokens");
        }

        private static void DumpSituationStatus(Situation situation)
        {
            LogInfo("- state: " + situation.State.Identifier);
            LogInfo("- recipe id: " + situation.RecipeId);
            LogInfo("- time remaining: " + situation.TimeRemaining);

            var storedAspects = AspectsToString(situation.GetAspects(true));
            LogInfo("- stored aspects: " + storedAspects);

            LogInfo("- slots:");
            DumpSphereSpec(situation.GetSpheresByCategory(SphereCategory.Threshold));

            LogInfo("- stored stacks");
            DumpSphereContent(situation.GetSingleSphereByCategory(SphereCategory.SituationStorage));

            LogInfo("- output stacks");
            DumpSphereContent(situation.GetSingleSphereByCategory(SphereCategory.Output));
        }

        private static void DumpSphereSpec(List<Sphere> spheres)
        {
            foreach (var sphere in spheres)
            {
                var spec = sphere.GoverningSphereSpec;
                LogInfo("- - " + spec.Id);
                LogInfo("- - - label: " + spec.Label);
                LogInfo("- - - description: " + spec.Description);
                LogInfo("- - - greedy: " + spec.Greedy);
                LogInfo("- - - consumes: " + spec.Consumes);

                var requiredAspects = AspectsToString(spec.Required);
                LogInfo("- - - required aspects: " + requiredAspects);

                var forbiddenAspects = AspectsToString(spec.Forbidden);
                LogInfo("- - - forbidden aspects: " + forbiddenAspects);

                foreach (var content in sphere.Tokens.Select(x => x.Payload).OfType<ElementStack>())
                {
                    LogInfo("- - - content: " + content.EntityId);
                    LogInfo("- - - quantity: " + content.Quantity);
                    LogInfo("- - - lifetime remaining: " + content.GetTimeshadow().LifetimeRemaining);
                    var stackAspects = AspectsToString(content.GetAspects());
                    LogInfo("- - - aspects: " + stackAspects);
                }
            }
        }

        private static void DumpSphereContent(Sphere sphere)
        {
            foreach (var stack in sphere.GetTokens().Select(x => x.Payload).OfType<ElementStack>())
            {
                LogInfo("- - " + stack.EntityId);
                LogInfo("- - - quantity: " + stack.Quantity);
                LogInfo("- - - lifetime remaining: " + stack.GetTimeshadow().LifetimeRemaining);
            }
        }

        private static string AspectsToString(AspectsDictionary aspects)
        {
            var builder = new StringBuilder();
            foreach (var aspect in aspects)
            {
                builder.AppendFormat("{0}({1}), ", aspect.Key, aspect.Value);
            }

            var str = builder.ToString();
            if (str.Length == 0)
            {
                return string.Empty;
            }

            return str.Substring(0, str.Length - 2);
        }

        private static void LogInfo(string message)
        {
            NoonUtility.Log(message);
        }
    }
}

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
        public static string DumpSituations()
        {
            var sb = new StringBuilder();

            foreach (var situation in Watchman.Get<HornedAxe>().GetRegisteredSituations())
            {
                sb.AppendLine("We found a situation token - " + situation.VerbId);
                DumpSituationStatus(situation, sb);
            }

            return sb.ToString();
        }

        private static void DumpSituationStatus(Situation situation, StringBuilder sb)
        {
            sb.AppendLine("- state: " + situation.State.Identifier);
            sb.AppendLine("- recipe id: " + situation.RecipeId);
            sb.AppendLine("- time remaining: " + situation.TimeRemaining);

            var storedAspects = AspectsToString(situation.GetAspects(true));
            sb.AppendLine("- stored aspects: " + storedAspects);

            sb.AppendLine("- slots:");
            DumpSphereSpec(situation.GetSpheresByCategory(SphereCategory.Threshold), sb);

            sb.AppendLine("- stored stacks");
            DumpSphereContent(situation.GetSingleSphereByCategory(SphereCategory.SituationStorage), sb);

            sb.AppendLine("- output stacks");
            DumpSphereContent(situation.GetSingleSphereByCategory(SphereCategory.Output), sb);
        }

        private static void DumpSphereSpec(List<Sphere> spheres, StringBuilder sb)
        {
            foreach (var sphere in spheres)
            {
                var spec = sphere.GoverningSphereSpec;
                sb.AppendLine("- - " + spec.Id);
                sb.AppendLine("- - - label: " + spec.Label);
                sb.AppendLine("- - - description: " + spec.Description);
                sb.AppendLine("- - - greedy: " + spec.Greedy);
                sb.AppendLine("- - - consumes: " + spec.Consumes);

                var requiredAspects = AspectsToString(spec.Required);
                sb.AppendLine("- - - required aspects: " + requiredAspects);

                var forbiddenAspects = AspectsToString(spec.Forbidden);
                sb.AppendLine("- - - forbidden aspects: " + forbiddenAspects);

                foreach (var content in sphere.Tokens.Select(x => x.Payload).OfType<ElementStack>())
                {
                    sb.AppendLine("- - - content: " + content.EntityId);
                    sb.AppendLine("- - - quantity: " + content.Quantity);
                    sb.AppendLine("- - - lifetime remaining: " + content.GetTimeshadow().LifetimeRemaining);
                    var stackAspects = AspectsToString(content.GetAspects());
                    sb.AppendLine("- - - aspects: " + stackAspects);
                }
            }
        }

        private static void DumpSphereContent(Sphere sphere, StringBuilder sb)
        {
            foreach (var stack in sphere.GetTokens().Select(x => x.Payload).OfType<ElementStack>())
            {
                sb.AppendLine("- - " + stack.EntityId);
                sb.AppendLine("- - - quantity: " + stack.Quantity);
                sb.AppendLine("- - - lifetime remaining: " + stack.GetTimeshadow().LifetimeRemaining);
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
    }
}

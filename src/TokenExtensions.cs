namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Spheres;
    using SecretHistories.States;
    using SecretHistories.UI;

    public static class TokenExtensions
    {
        public static bool TokensAreStable(this IEnumerable<Token> tokens)
        {
            return tokens.All(t => t.TokenIsStable());
        }

        public static bool TokenIsStable(this Token token)
        {
            if (token.Defunct)
            {
                // 'Dead is stable' - Moist Von Lipwig
                return true;
            }

            // Cards randomly end up in this state... hmmm
            // I think its base game, but it might be Shelves auto-sort
            if (token.CurrentState is UnknownState)
            {
                return token.Sphere is not EnRouteSphere;
            }

            return token.CurrentState.Docked();
        }
    }
}
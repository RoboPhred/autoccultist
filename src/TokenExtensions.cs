namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;
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

            // Is this a good option?  Tokens are sometimes in UnknownState, but not before the game does anything with them...
            // This function is mainly used to confirm that a token is located somewhere sane and not traveling.
            // We could check for the traveling states, or check to see if we are definitively in a sphere.
            return token.CurrentState.Docked();
        }
    }
}
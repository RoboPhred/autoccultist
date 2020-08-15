namespace Autoccultist
{
    using Assets.Core.Interfaces;

    /// <summary>
    /// Describes a class that can determine if a card matches its specifications.
    /// </summary>
    public interface ICardMatcher
    {
        /// <summary>
        /// Determine if a given card matches this matcher's specifications.
        /// </summary>
        /// <param name="card">The card to test against the matcher.</param>
        /// <returns>True if the card matches, or False otherwise.</returns>
        bool CardMatches(IElementStack card);
    }
}

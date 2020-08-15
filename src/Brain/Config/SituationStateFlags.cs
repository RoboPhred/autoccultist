namespace Autoccultist.Brain.Config
{
    /// <summary>
    /// Describes various states of situations that can be matched against.
    /// </summary>
    public enum SituationStateConfig
    {
        /// <summary>
        /// The situation is not present on the board
        /// <summary>
        Missing,

        /// <summary>
        /// The situation is present, but not doing anything.
        /// </summary>
        Unstarted,

        /// <summary>
        /// The situation is busy with a recipe
        /// </summary>
        Ongoing,
    }
}

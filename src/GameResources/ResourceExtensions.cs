namespace AutoccultistNS.GameResources
{
    using AutoccultistNS.GameState;

    public static class ResourceExtensions
    {
        // FIXME: I really want this to be part of game state / bot state, but putting it there means that the hash must also include availability status,
        // and IsAvailable() checks the hash when mangaging the candidates with a dictionary.
        // This is desirable so that we can run diagnostic checks on "what should we do, if nothing was reserved" by having wrapping game states
        // mask IsAvailable.
        // Possible fixes:
        // - Use a seperate function to get game state hashes vs raw GetHashCode
        // - Find a way to make IsAvailable not dependent on hashes.
        // FIXME: This isn't part of hash code (see above), but we cache results from it.  This isn't bad at the moment, but definitely
        // an unexpected quirk that may very well lead us into trouble.
        public static bool IsAvailable(this ISituationState situation)
        {
            return Resource.Of<ISituationState>().IsAvailable(situation);
        }
    }
}

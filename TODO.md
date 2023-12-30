### Pre-scan configs for validity

Check to make sure aspects and element ids specifid in configs actually exist in the compendium

### General game info condition

Condition matchers for:

- Current Legacy

### Parser is getting disgusting

Having to use property types to indicate things like "load this from the goals library" and "make this a flat array" is causing bloody chaos for caching.
Hack into the object parser and let us specify attributes on properties to use custom parsers for them.

### Ability to limit cards seen by nested impulses (particularly operations) by parent impulse card choosers.

This can be done by making a new IGameState implementer that wraps the parent state and restricts its cards to those intersected by the choosers.

This can be used for things like having a get-prisoner imperative, and extend / reuse it but with limitations on who it is allowed to choose as the prisoner.

This might also be useful for OperationImperatives to prefilter the list of cards available to the recipe solutions, although this will be iffy as those are ran
by a reaction and actively collect the current game state.

### Include resource availability in game state (see extension method ISituationState.IsAvailable)

I really want this to be part of game state / bot state, but putting it there means that the hash must also include availability status,
and IsAvailable() checks the hash when mangaging the candidates with a dictionary.
This is desirable so that we can run diagnostic checks on "what should we do, if nothing was reserved" by having wrapping game states
mask IsAvailable.

Possible fixes:

- Use a seperate function to get game state hashes vs raw GetHashCode
- Find a way to make IsAvailable not dependent on hashes.

We still make decisions on IsAvailable, which end up being cached behind the game state's hash code. This is despite the fact that availability
is not reflected in the hash code. While this doesn't give us any problems at the moment, it is a worrying edge case that can surface bugs later.

This is particularly painful as it stops us ever caching GetImpulses, and leads to incorrect caching around requirements/forbidders/completedWhen

### UI Jank

- Padding bottom doesn't work on anything. Buttons are squished.

### Load automations from mods

Should load automation stuff from an automation folder in all mods.

To do this, we will need to fix !import so that / paths are either relative to the current mod, or abstract the path fs so that
we treat / as the combined contents of all mods and choose the file from the mod that provides it thats farthest along in the load order.
Crucible filesystem abstractions for zips/directories might give some inspiration here.

### Cronicle stuff

- Snapshot after unshrouding cards from verb completion.
  - Might have to make OperationReaction call out to Chronicler to tell it what to cronicle / snapshot
- Snapshot and chronicle unknown recipes from OperationReaction
- Screenshots happen on a frame delay, so snapshots for starting recipe cards are happening after the cards were submitted. Definitely need to have OpReaction reach out to chronicler as this is now an async issue.

Its messy that OperationReaction now has broken down its actions into smaller unit and has timing issues in calling BrainEvents only when the screen is in a proper state to be screenshotted.

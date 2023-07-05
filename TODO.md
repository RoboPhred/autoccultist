### Hide hidden aspects from bot?

Do we want the bot to play it straight and not to have access to knowledge the player does not?
Caveat: Apostle run has 'trapped fascination'. The aspect for this is hidden, BUT: The player can know which one it is by noticing what verb spat it out.

### General game info condition

Condition matchers for:

- Current Legacy

### Parser is getting disgusting

Having to use property types to indicate things like "load this from the goals library" and "make this a flat array" is causing bloody chaos for caching.
Hack into the object parser and let us specify attributes on properties to use custom parsers for them.

### BUG: Hard lockup when a reaction tries to run every frame but ends on that frame

Happens when targeting ongoing situations but the situation has no ongoing slots.

- Sub-bug: Crash when the situation has no ongoing slots when we start it.

### Ability to limit cards seen by nested impulses (particularly operations) by parent impulse card choosers.

This can be done by making a new IGameState implementer that wraps the parent state and restricts its cards to those intersected by the choosers.

This can be used for things like having a get-prisoner imperative, and extend / reuse it but with limitations on who it is allowed to choose as the prisoner.

This might also be useful for OperationImperatives to prefilter the list of cards available to the recipe solutions, although this will be iffy as those are ran
by a reaction and actively collect the current game state.

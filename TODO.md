### Improve arcs

Rather than being a linear list of things to do, it should be a map of motivations.
Each motification can take dependencies on other motiviations
This will allow the bot to do more things at the same time and not block itself from continuing if
one of multiple primary goals completes early

### Do we even need imperatives?

Imperatives are an ancient ancestor design decision. operations are far smarter than imperatives
and their ability to introspect into the target situation makes them more flexible.
In theory, we could remove imperatives entirely (while keeping shims around for the configs)

Problems with this:
We will still need a way to reuse the bulk content of operations, so that we can make multiple versions
We will both want to replace or add to the base operation's conditions
This also means we need to turn the startingCondition prop of an operation into a IGameStateCondition
in addition to its current 'smart condition' enums

### Hide hidden aspects from bot?

Do we want the bot to play it straight and not to have access to knowledge the player does not?
Caveat: Apostle run has 'trapped fascination'. The aspect for this is hidden, BUT: The player can know which one it is by noticing what verb spat it out.

### General game info condition

Condition matchers for:

- Current Legacy

### Clean up NucleusAccumbens

- Mess of maps and hash sets in there... Way too much redundant codependent state
- Sometimes we get a reaction execution completion but can't find what imperative it belonged to...

### Mansus causes operation aborts

Mansus events have a high chance of causing operations to die, now that we can't pause the message pump when it is open.
In theory the game should be paused during all our ops and during the mansus event so no other operations might tick forward... How are ops still managing to run during it?

### Parser is getting disgusting

Having to use property types to indicate things like "load this from the goals library" and "make this a flat array" is causing bloody chaos for caching.
Hack into the object parser and let us specify attributes on properties to use custom parsers for them.

### Stop the bot doing things when it shouldnt

- Mansus is open, bot still tries to run things... We can't rely on pausing the actor anymore since everything is async.
- Victory is in progress, bot tries to dump the victorious verb and do other things while the victory is playing out.

We can probably add an interactability check to MechanicalHeart.AwaitBeat

- Do we want to never beat at all when the game isnt interactable?
- Do we want to trust the user interactability stuff the mansus uses for this?

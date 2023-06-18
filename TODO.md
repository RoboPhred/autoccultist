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

### Card consumption

-- Might not need this anymore now that we wait for an operation to get established before starting another.

Implement consumption into ICardState so simultanious orchestrations do not try to yoink eachother's cards.

If consumed and aborted: ElementStackToken.ReturnToTabletop
Only succeed if private ElementStackToken.IsOnTabletop `this.transform.parent.GetComponent<TabletopTokenContainer>() != null;`
Throw error if ElementStackToken.Defunct
Unique IDs? ElementStackToken.EntityWithMutationsId

### Hide hidden aspects from bot?

Do we want the bot to play it straight and not to have access to knowledge the player does not?
Caveat: Apostle run has 'trapped fascination'. The aspect for this is hidden, BUT: The player can know which one it is by noticing what verb spat it out.

### General game info condition

Condition matchers for:

- Current Legacy

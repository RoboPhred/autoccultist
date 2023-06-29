### Hide hidden aspects from bot?

Do we want the bot to play it straight and not to have access to knowledge the player does not?
Caveat: Apostle run has 'trapped fascination'. The aspect for this is hidden, BUT: The player can know which one it is by noticing what verb spat it out.

### General game info condition

Condition matchers for:

- Current Legacy

### Parser is getting disgusting

Having to use property types to indicate things like "load this from the goals library" and "make this a flat array" is causing bloody chaos for caching.
Hack into the object parser and let us specify attributes on properties to use custom parsers for them.

### BUG: Bot wont start impulses on rapid new-game loads

Start a power ascension, wait for a while, start a new power ascension.
Bot does not attempt to start the intro.
If the intro is manually started, bot recovers just fine.

Result of caching? Issue happens even if we start manually and wait for all the caches to bust.

## BUG: Hard lockup when a reaction tries to run every frame but ends on that frame

Happens when targeting ongoing situations but the situation has no ongoing slots.

- Sub-bug: Crash when the situation has no ongoing slots when we start it.

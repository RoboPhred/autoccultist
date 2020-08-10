# Autoccultist

An experimental automata for playing Cultist Simulator.

## Current Status

Capable of handling an aspirant start. Currently reads the bequest, finds the aquaintance, then levels up health until it gets to the Iron Physique.

## Issues

Aspirant brain died to health because it uses health instantly from the completion of one verb to the start of another, giving the magnet slot no time to yank the card.
Need to satisfy the magnet slots, which means waiting the 20 ticks for them to trigger. Might be cheating if I force the card into the magnet slot, but instantanious
card movement is already cheaty.
Probably something for the card manager / slotting scheduler to handle.

## Notes

Slot solutions - support by element id, and by aspects (required and rejected).
Also have optimistic slot solutions (do not lock cards, report readyness when cards are not available) and pessimistic slot solutions (lock cards, refuse to start situation solution until all cards are present)
Ongoing slots should take into account the delay before they can slot cards, to calculate if cards will be available by the time we are ready.

### Card Manager

Card manager should support taking reservations for cards, locking them from use by other solutions. This will let solutions reserve cards when they need a card for an ongoing slot.
Since card choosers can choose by aspects, we may need to pick out cards at lock-time to be the target card. If it was just by element id, we could simply track the number of locks
and make sure to leave that number of cards free.

If asked for a card type that is expirable, the card manager should return the card with the shortest expiration time.

### Imperatives should try to preemptively reserve cards

This one is fiddly. An imperative should, on seeing all cards it needs to run, reserve those cards even if its verb is still busy.
This needs to take into account card expiration (fail to reserve if its verb will be busy for longer than the card exists), ongoing
recipe duration (fail to reserve if the card will be gone by the time its called for), priority of the imperative (higher priority
imperatives should be able to break the reservation).

### Slotting manager

Need a way to schedule card movement and let it take place a few ticks after. Few advantages for this:

- Give magnet slots a chance to act.
  This is critical for things like sickness, where the bot might happily commit its health card straight out of
  another verb's output slot and prevent sickness from ever seeing health. This ends in death.
- Make the bot more interesting to look at.
  The bot should open and close verb windows, and slot cards one after the other. Makes it more pleasing to watch.

## Proposed architecture

The AI will have no state to track the game. Instead, it uses the existance or nonexistance of cards to determine what to do next.
This will allow it to never get caught off guard by random events or card draws, and let it react to any occurance.

### BrainConfig

A set of goals to accomplish. This drives the playthrough the AI will make.
The goals are not in any particular order. Instead, they will get chosen depending on their conditions and the available cards.

### Goal

A high level task for the AI to accomplish.
Example: Increase basic health skill to advanced health skill (healthskilla => healthskillb)

A goal has a starting condition, and a satisfied condition.
Goals are ready to activate if starting condition is met, and satisifed condition is not met.

Goals do not explicitly declare dependencies, but can depend on each other by their starting conditions.
For example, goal B depends on goal A if A produces a "healthskillb" card, and B declares "healthskillb" a starting requirement.

On startup, AI will go through its goals, find goals that are not satisified yet meet their starting condition, and run one at a time.
To avoid conflicting verb constraints, lets stick to one goal at a time, and design goals so that all their imperatives coexist.
Conflicting verbs may be sometimes ok, but other times we will be dealing with expiring cards. If dealing with expiring cards, the AI
might get stuck in a loop where it keeps switching which goal controls a contested verb, resulting in cards expiring before they can be used to complete
either goal.

A goal contains a collection of imperatives, all of which are active and working at the same time.

### Imperative

This is just the name I had in my notes, could use a better one. Imperative might mean a higher level concept than a goal...

An imperative is a set of conditions on which to activate a verb and perform a situation solution.
An imperative targets a single situation, so multiple imperatives can trigger at once.
An imperative will activate a solution when its conditions are met, the situation is free, and no higher priority imperatives want to use the same situation.

Imperatives have 3 priorites

- Critical - Things that need to be done in order to survive. These might get triggered if funds are low or the visions situation is ongoing.
- GoalOriented - Performing this imperative will bring us closer to our goal. Most imperatives should be this priority.
- Maintenance - This imperative is to do basic ongoing tasks like make money or take care of a dead card. It can be deferred if a goal oriented task is pending.

## Installation

This mod uses BepInEx 5.2.

- Install [version 5.2](https://github.com/BepInEx/BepInEx/releases/tag/v5.2) or later by extracting the zip file into your Cultist Simulator install location
- Run the game once, to let BepInEx create its folder structure.
- Place the cultist-hotbar.dll file from the download into `Cultist Sumulator/BepInEx/Plugins`

### Installing the mod to BepInEx

Place the autoccultist.dll file in `Cultist Sumulator/BepInEx/Plugins`

### Troubleshooting

If the mod isn't working, you can turn on the BepInEx logs to see what is going on.

Open your BepInEx config file at `Cultist Simulator/BepInEx/config/BepInEx.cfg` and enable the console by changing the `Enabled` key of `[Logging.Console]` to `true`.

```
[Logging.Console]
Enabled = true
```

Doing this will create a terminal window when you launch cultist simulator. If you do not see this new window open, then BepInEx is probably not installed correctly,
or the config file is misconfigured.

Once you get the window, check its output after you hit the play button on the Cultist Simulator launcher. You can either drag the terminal window to another
monitor, or tab out of Cultist Simulator to check it after launch.

If BepInEx is installed and configured properly, you should see messages similar to the following:

```
[Message:   BepInEx] BepInEx 5.0.0.0 RC1 - cultistsimulator
[Message:   BepInEx] Compiled in Unity v2018 mode
[Info   :   BepInEx] Running under Unity v2019.1.0.2698131
[Message:   BepInEx] Preloader started
[Info   :   BepInEx] 1 patcher plugin(s) loaded
[Info   :   BepInEx] Patching [UnityEngine.CoreModule] with [BepInEx.Chainloader]
[Message:   BepInEx] Preloader finished
```

Once you have confirmed BepInEx is installed properly, look for the mod loading message. Once you start the game from the launcher, the terminal window should contain:

```
[Info   :   BepInEx] Loading [Autoccultist 0.0.1]
```

and

```
[Info   :Autoccultist] Autoccultist initialized.
```

If you do not see these lines, then the mod isn't in the correct folder. Check the Installation instructions for details on where to put the mod.

If you have confirmed all of the above and still are having trouble, try looking at the terminal for lines starting with `[Error :Autoccultist]`. The mod will
try to log errors when it cannot do it's job properly. Create a github issue with any Autoccultist error messages you find, and I will try to help you further.

## Development

### Dependencies

Project dependencies should be placed in a folder called `externals` in the project's root directory.
This folder should include:

- BepInEx.dll - Copied from the BepInEx 5.0 installation under `BepInEx/core`
- Assembly-CSharp.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.CoreModule.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.UI.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`

### Compiling

This project uses the dotnet cli, provided by the .Net SDK. To compile, simply use `dotnet build` on the project's root directory.

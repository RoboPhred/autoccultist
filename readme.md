# Autoccultist

An experimental automaton for playing Cultist Simulator.

Previous descriptions called this an "AI".  That was true enough when it was written, but now with the advent of neural networks, it should be clarified that this is NOT an AI in the modern tensor-and-neuron sense, but rather a state machine "AI" that have traditionally driven NPCs in earl games.

Essentially, this project contains a massive tree of conditions matching game states to desired actions.  Every frame, the bot scans the entire game state, and tries to match it against its library of actions to take.  It then takes the highest priority action it is able to.

**WARNING**: This is experimental software that is still under development. It does its job, but it is **NOT** user-friendly. Only attempt to use this if you are willing to dig in to how this mod works and get your hands dirty writing configs. Others may want to wait until the mod is more refined and user-friendly before attempting to use it.

**WARNING**: This is prerelease documentation. It is incomplete, and as changes are made, may be incorrect.

The purpose of this mod is to provide a means of making Cultist Simulator play itself. Either though individual ongoing tasks (EG: Obtain, work, and submit a commission, and sell a Spintria if funds are low), or through a higher level list of tasks.

## Demo

You can watch the bot play and complete a power ascension [here](https://www.youtube.com/watch?v=UKe9wqMd32I)

## Usage

This mod adds a button to the top left of the game, which will open a panel showing all available automations.

There are two automation type:
- Goals
  Goals are automations for specific tasks in the game
- Arcs
  Arcs are full automated playthroughs, from new game all the way to completion.

Starting either of these will create a new verb block on the table representing the task.

Additionally, a button will appear on hover for all other verbs which will allow inspecting ongoing automations for that verb, disabling automations temporarily for manual use, or starting specific one-shot automations on that verb.

## Installation

This mod uses the Cultist Simulator built in dll mod loading.

To use it, you will have to create your own mod file and configure it to execute the compiled Autoccultist.dll as a standard dll mod.

At the moment, there is no pre-bundled file for this; this mod is DIY.

## Configuration

This automata is entirely configurable by yaml files.

**WARNING**: This documentation is woefully incomplete, and major overhauls have been made since this was written.  Some of it is still relevant, but you should study the config files included with the source rather than relying on this.

Despite this, some of the documentation below is still somewhat accurate, and might provide some additional hints on how the system works.

### BrainConfig (brain.yml)

A set of goals to accomplish. This drives the play through the AI will make.
The goals are not in any particular order. Instead, they will get chosen to depend on their conditions and the available cards.

### Goal (`goals[]`)

A high level task for the AI to accomplish.
Example: Increase basic health skill to advanced health skill (healthskilla =&gt; healthskillb)

A goal has a starting condition, and a satisfied condition.
Goals are ready to activate if starting condition is met, and satisfied condition is not met.

Goals do not explicitly declare dependencies, but can depend on each other by their starting conditions.
For example, goal B depends on goal A if A produces a "healthskillb" card, and B declares "healthskillb" a starting requirement.

On startup, AI will go through its goals, find goals that are not satisfied yet meet their starting condition, and run one at a time.
To avoid conflicting situation constraints, lets stick to one goal at a time, and design goals so that all their impulses coexist.
Conflicting situation may be sometimes ok, but other times we will be dealing with expiring cards. If dealing with expiring cards, the AI
might get stuck in a loop where it keeps switching which goal controls a contested situation, resulting in cards expiring before they can be used to complete
either goal.

A goal contains a collection of impulses, all of which are active and working at the same time.

### Impulse (`goals[].impulses`)

An impulse is a set of conditions which, when satisfied, cause the automation to perform an action on a situation / verb.
An impulse targets a single situation, so multiple impulses can trigger at once. However, only one impulse may interact with a situation at a time.
An impulse will activate when its conditions are met, the situation is free, and no higher priority impulses want to use the same situation.

Impulses have 4 priorities

- critical - Things that need to be done in order to survive. These might get triggered if funds are low or despair triggers.
- goal - Performing this impulse will bring us closer to our goal. Most impulses should be this priority.
- normal - Tasks that should be performed, but are secondary to accomplishing the goal.
- maintenance - This impulse is to do basic ongoing tasks like make money or take care of a dead card. It can be deferred if a goal oriented task is pending.

### Operation (`goals[].impulses[].operation`)

An operation is instructions for a full cycle of a verb or situation. It contains the starting recipe, and all ongoing recipes to drive the situation to completion.

### Troubleshooting

If the mod isn't working, you can turn on the BepInEx logs to see what is going on.

Open your BepInEx con fig file at `Cultist Simulator/BepInEx/con fig/BepInEx.cfg` and enable the console by changing the `Enabled` key of `[Logging. Console]` to `true`.

“` [Logging. Console] Enabled = true “`

Doing this will create a terminal window when you launch cultist simulator. If you do not see this new window open, then BepInEx is probably not installed correctly,
or the con fig file is misconfigured.

Once you get the window, check its output after you hit the play button on the Cultist Simulator launcher. You can either drag the terminal window to another
monitor, or tab out of Cultist Simulator to check it after launches.

If BepInEx is installed and configured properly, you should see messages similar to the following:

“` [Message: BepInEx] BepInEx 5.0.0.0 RC1 - cultist simulator [Message: BepInEx] Compiled in Unity v2018 mode [Info : BepInEx] Running under Unity v2019.1.0.2698131 [Message: BepInEx] Preloader started [Info : BepInEx] 1 patcher plugin(s) loaded [Info : BepInEx] Patching [UnityEngine.CoreModule] with [BepInEx.Chain loader] [Message: BepInEx] Preloader finished “`

Once you have confirmed BepInEx is installed properly, look for the mod loading message. Once you start the game from the launcher, the terminal window should contain:

“` [Info : BepInEx] Loading [Autoccultist 0.0.1] “`

and

“` [Info :Autoccultist] Autoccultist initialized. “`

If you do not see these lines, then the mod isn't in the correct folder. Check the Installation instructions for details on where to put the mod.

If you have confirmed all the above and still are having trouble, try looking at the terminal for lines starting with `[Error :Autoccultist]`. The mod will
try to log errors when it cannot do its job properly. Create a github issue with any Occultist error messages you find, and I will try to help you further.

## Development

### Dependencies

Project dependencies should be placed in a folder called `externals` in the project's root directory.
This folder should include:

- BepInEx.dll - Copied from the BepInEx installation under `BepInEx/core`
- 0Harmony.dll - Copied from the BepInEx installation under `BepInEx/core`
- Assembly-CSharp.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.CoreModule.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.UI.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.InputLegacyModule - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.IMGUIModule - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.UIModule - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`

### Compiling

This project uses the dotnet cli, provided by the .Net SDK. To compile, simply use `dotnet build` on the project's root directory.

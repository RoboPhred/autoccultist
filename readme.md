# Autoccultist

An experimental automaton for playing Cultist Simulator.

__WARNING__: This is experimental software that is still under development.  It does its job, but it is __NOT__ user-friendly.  Only attempt to use this if you are willing to dig in to how this mod works and get your hands dirty writing con figs.  Others may want to wait until the mod is more refined and user-friendly before attempting to use it.

__WARNING__: This is prerelease documentation.  It is incomplete, and as changes are made, may be incorrect.

The purpose of this mod is to provide a means of making Cultist Simulator play itself.  Either though individual ongoing tasks (EG: Obtain, work, and submit a commission, and sell a Spintria if funds are low), or through a higher level list of tasks.

It was originally conceived as a hobby project, one taken on for the challenge of it and to see if a complex game like Cultist Simulator can be 'solved' algorithmically.  However, as time goes on it is slowly acquiring the tools and UI necessary to allow it to be used as a utility for people who want to automate away repetitive tasks during normal game play.

## Usage
There are two ways of using Autoccultist: As a task automaton, and as a completely automated game player.

First, you must configure the behaviors you want.  See [configuration](#configuration).

With the mod installed, and once in-game, you can open the controls with F10.  This opens up the Diagnostic panel

### The diagnostic panel
The panel has these options:

If there were errors reading the config, the panel will say how many, and reveal a button to view those errors.

The `Mechanical Heart` checkbox acts as the master switch for this mod.  Nothing will happen without this checkbox checked.
A `Step heart` button is also available.  This makes the mod perform exactly one action then stop.  Useful for debugging.

The `Task Driver` checkbox enables reading the main `brain.yml` file and running all tasks.  This is intended for full game play automation, you can ignore it if you just want the mod to automate a few tasks and not play the game for you.

The `Toggle goals list` button opens up a new window containing the goals panel.  This is where you will be able to start the mod performing specific tasks.

### The goals panel
This is where you can turn on or off individual automatons (called "goals") for the mod, and is particularly useful for people who just want to automate one or two tasks without having the mod take over the game entirely.

The top of the window shows your active goals.  Click `Cancel` on these goals to cancel them.
Depending on how you set up the goal, the goal may run forever, or may run until it has completed its designated task.

The button of the panel shows available goals.  Click the `Activate` button on goals you want to start.

## Installation

This mod uses BepInEx 5.2.

- Install [version 5.2](https://github.com/BepInEx/BepInEx/releases/tag/v5.2) or later by extracting the zip file into your Cultist Simulator install location
- Run the game once, to let BepInEx create its folder structure.
- Extract the autoccultist folder from the download into `Cultist Simulator/BepInEx/Plugins`

## Configuration

This automata is entirely configurable by yaml files.

__WARNING__: This documentation is incomplete, and does not explain how to write the config files.  More detailed documentation is forthcoming.  Until then, inspect the config files included with the mod for examples.

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
To avoid conflicting situation constraints, lets stick to one goal at a time, and design goals so that all their imperatives coexist.
Conflicting situation may be sometimes ok, but other times we will be dealing with expiring cards. If dealing with expiring cards, the AI
might get stuck in a loop where it keeps switching which goal controls a contested situation, resulting in cards expiring before they can be used to complete
either goal.

A goal contains a collection of imperatives, all of which are active and working at the same time.

### Imperative (`goals[].imperatives`)

An imperative is a set of conditions on which to activate a situation and perform a situation solution.
An imperative targets a single situation, so multiple imperatives can trigger at once. However, only one imperative may interact with a situation at a time.
An imperative will activate a solution when its conditions are met, the situation is free, and no higher priority imperatives want to use the same situation.

Imperatives have 3 priorities

- Critical - Things that need to be done in order to survive. These might get triggered if funds are low or the visions situation is ongoing.
- GoalOriented - Performing this imperative will bring us closer to our goal. Most imperatives should be this priority.
- Maintenance - This imperative is to do basic ongoing tasks like make money or take care of a dead card. It can be deferred if a goal oriented task is pending.

### Operation (`goals[].imperatives[].operation`)

An operation is instructions for a full cycle of a verb or situation. It contains the starting recipe, and all ongoing recipes to drive the situation to completion.

### Troubleshooting

If the mod isn't working, you can turn on the BepInEx logs to see what is going on.

Open your BepInEx con fig file at `Cultist Simulator/BepInEx/con fig/BepInEx.cfg` and enable the console by changing the `Enabled` key of `[Logging. Console]` to `true`.

“`
[Logging. Console]
Enabled = true
“`

Doing this will create a terminal window when you launch cultist simulator. If you do not see this new window open, then BepInEx is probably not installed correctly,
or the con fig file is misconfigured.

Once you get the window, check its output after you hit the play button on the Cultist Simulator launcher. You can either drag the terminal window to another
monitor, or tab out of Cultist Simulator to check it after launches.

If BepInEx is installed and configured properly, you should see messages similar to the following:

“`
[Message: BepInEx] BepInEx 5.0.0.0 RC1 - cultist simulator
[Message: BepInEx] Compiled in Unity v2018 mode
[Info : BepInEx] Running under Unity v2019.1.0.2698131
[Message: BepInEx] Preloader started
[Info : BepInEx] 1 patcher plugin(s) loaded
[Info : BepInEx] Patching [UnityEngine.CoreModule] with [BepInEx.Chain loader]
[Message: BepInEx] Preloader finished
“`

Once you have confirmed BepInEx is installed properly, look for the mod loading message. Once you start the game from the launcher, the terminal window should contain:

“`
[Info : BepInEx] Loading [Autoccultist 0.0.1]
“`

and

“`
[Info :Autoccultist] Autoccultist initialized.
“`

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

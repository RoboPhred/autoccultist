- Operations and imperatives should automatically get a name based on the yaml file and line they were defined on
- BrainConfig validation should be implemented alongside the yaml parser to capture the file and line number
  - There is an example of this in the YamlDotNet github
- More robust yaml parsing: Record errors and cleanly display to user
  - Particulary care about what yaml file the error came from, can be multiple files in play due to !import
- extends property of imperatives should also be able to take a filename and auto-load that from the imperatives folder
- imperativeSets property of goals should also be able to take a filename and auto-load that from the imperative-sets folder
- Stop spamming errors on update if yaml is invalid.
- Deal with fascination. situation `visions`, counter card with aspect `fascination` using dream with `fleeting`, or let restlessness fester into dread
- Rethink requirements on goals. When doing goals linearly, it doesn't make much sense.
  - Can also get us stuck with no goals at all. Should we have a fallback goal?
  - This should be resolved with tasking, where goals can be looped / linear / parallel
- Make situation auto-dump optional (config in brain.yml?) - Consider cards in completed situations to be 'on table' / available in IGameState
- `firstMatch` ICardChooser that takes several card choosers and picks the first chooser to match. Use this so that skillhealth and skillpassion upgrade ops can be agnostic to what particular card is being upgraded.

### Mansus support

- MapTokenContainer, MapController
- TabletopManager.ReturnFromMansus receives the chosen card

MapController.SetupMap sets things up. private MapController.cards is an array of 3 elements. First is the face up card, rest are face down.
MapController.HandleOnSlotFilled is called with the "selected" card. In reality, user drops the selected card here.

### Card consumption

Implement consumption into ICardState so simultanious orchestrations do not try to yoink eachother's cards.

If consumed and aborted: ElementStackToken.ReturnToTabletop
Only succeed if private ElementStackToken.IsOnTabletop `this.transform.parent.GetComponent<TabletopTokenContainer>() != null;`
Throw error if ElementStackToken.Defunct
Unique IDs? ElementStackToken.EntityWithMutationsId

### Handle game state changes

- If the game isnt in 'game mode', dont update stuff.
- If the game transitions out of game mode, lobotomize the brain and other states.

TabletopManager.BeginGame
TabletopManager.OnDestroy

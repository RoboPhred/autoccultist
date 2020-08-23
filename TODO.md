- extends property of imperatives should also be able to take a filename and auto-load that from the imperatives folder
- imperativeSets property of goals should also be able to take a filename and auto-load that from the imperative-sets folder
- Deal with fascination. situation `visions`, counter card with aspect `fascination` using dream with `fleeting`, or let restlessness fester into dread
- Rethink requirements on goals. When doing goals linearly, it doesn't make much sense.
  - Can also get us stuck with no goals at all. Should we have a fallback goal?
  - This should be resolved with tasking, where goals can be looped / linear / parallel
- Make situation auto-dump optional (config in brain.yml?) - Consider cards in completed situations to be 'on table' / available in IGameState
- `firstMatch` ICardChooser that takes several card choosers and picks the first chooser to match. Use this so that skillhealth and skillpassion upgrade ops can be agnostic to what particular card is being upgraded.

### Card consumption

Implement consumption into ICardState so simultanious orchestrations do not try to yoink eachother's cards.

If consumed and aborted: ElementStackToken.ReturnToTabletop
Only succeed if private ElementStackToken.IsOnTabletop `this.transform.parent.GetComponent<TabletopTokenContainer>() != null;`
Throw error if ElementStackToken.Defunct
Unique IDs? ElementStackToken.EntityWithMutationsId

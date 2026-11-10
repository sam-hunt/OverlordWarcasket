# TODOs

## Features

- **Set tuning in play.** The pieces are tuned against Biotech's mechlord helmet and suit (the
  def headers have the numbers): check in game that the foundry lists the set only once both
  spacer warcaskets and ultra mechtech are done, that the chips are charged, that a full-set
  pilot reads 20 bandwidth, a control group and 80% psychic sensitivity and can still install a
  mechlink, and how the helmet's -3 shooting plays with a warcasket weapon.
  Still to compare: the Mechlord Warcasket mod's stats and costs.
- **Recharge rate.** The shipped 1% of capacity per day is one charge every ten days on a
  ten-charge core, a reserve rather than a well, taken literally from the brief; the setting
  slider spans up to a full core a day if play says otherwise.
- **CE subdomain compat scan** Check CE for any extra related subdomains that it takes over from vanilla
- **Patch for Alpha Mechs players** our warcasket should probably cost more, similar to Mech Master
- **Work profile in game.** Research (for hacking) and Smithing (for the mech work) open,
  Crafting, Tailoring and Dark study behind settings toggles; run the spec's verification
  (`Docs/Specs/Overlord-Work-Trait.md`, section 4), autohack and the Hack gizmo included.
- **Art.** `Docs/Art/WarcasketOverlordHelmet_east_alt.png` is the second helmet east facing the
  artist delivered (a few thousand pixels differ from the one shipped); pick one and delete the other.
- Run the `../MOD_DOWNLOAD_SIZE_TRIM.md` pass before the first release.
- Tweak trait text, currently "…is not capable of most work, but can still do smithing and hacking."
  maybe to something like "except mech-related/adjacent work" etc
- **Workshop images.** add gameplay images of the resurrection and the settings etc
- **Smoke test on both branches** (`Scripts/integration-smoke-test.py`) once the game can be
  booted with the full compat list, and a boot on 1.6.
- **Full review of player-facing copy**, the descriptions' lore paragraphs in particular.
- **First Workshop publish.** Commit `About/PublishedFileId.txt`, add the Workshop link and the
  Steam badges to the README, paste the description.

## Testing

- **In-game look at the cerebrex node art.** The patch is checked statically, but nobody has
  seen the art in game, the one live check a cosmetic patch gets: with the setting on, check the
  item on the ground, its icon in the inventory and the pack on the wearer's back on each facing,
  and that the settings summary reads the live art and the pending change correctly.
- **In-game run of the resurrect mech ability.** The code mirrors the apocriton's rules from
  the 1.7.4990 decompile and builds clean, but no cast has been watched yet: check the walk to
  the corpse, the warmup effecter on the corpse, that only the clicked mech comes back, the
  charge spend per weight class, the gauge's red bar and tooltip, the dev gizmos, and the
  "needs a mechlink" and "no charges" disabled reasons. Then the settings page end to end.
  Also the core's red sparks on the pilot: their size and spread against the warcasket's art,
  that they stop at zero charges and come back with the next one, and that they follow
  the draft with the drafted-only setting on and play undrafted with it off, and that a raider
  in the set (dev spawn) shows them while its core is charged. That they stop while the pilot
  is downed or asleep, come back when it gets up, and stop at its death, the corpse silent.
- **In-game run of the mechanitor command range.** The patches build clean and the rules are
  unit-tested, but no order has been given in game: with the helmet on, a drafted mech ordered
  across the map by right-click move, attack and the multi-pawn drag, the white ring gone; the
  radius scope at 29.9 and at 9.9 with the ring at that radius and the refusal back beyond it;
  "anywhere" with the mech on a second map; "applies to" moved to the shoulders; "no one" giving
  vanilla's "Out of command range" again. Then the settings page: the greyed second group, the
  slider selecting its radio, the summary line, and the choices surviving a reopen and a
  language switch (`Docs/Specs/Mechanitor-Command-Range.md`, section 5).

## Later

- **Sometimes spawn vanilla pod/crash mechanitors in the warcasket?**
- **Raider overlords.** The set is on the raid tables as a Controller with the baseline shield;
  AI pawns never cast apparel abilities. Toxin's raider auto-vent is the family precedent for
  giving raiders an ability, so a raider mechanitor resurrecting its escort is possible later.
  Probably also worth considering whether overlords raiders would spawn with a small mech contingent too

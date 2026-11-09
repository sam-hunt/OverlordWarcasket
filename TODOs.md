# TODOs

## Before v1.0.0

- **Create the remote repo**
- **Cerebrex node retexture** mod settings toggle gated patch
- **In-game run of the resurrect mech ability.** The code mirrors the apocriton's rules from
  the 1.7.4990 decompile and builds clean, but no cast has been watched yet: check the walk to
  the corpse, the warmup effecter on the corpse, that only the clicked mech comes back, the
  charge spend per weight class, the gauge's red bar and tooltip, the dev gizmos, and the
  "needs a mechlink" and "no charges" disabled reasons. Then the settings page end to end.
  Also the core's red sparks on the pilot: their size and spread against the warcasket's art,
  that they stop at zero charges and come back with the next one, and that they follow
  the draft with the drafted-only setting on and play undrafted with it off, and that a raider
  in the set (dev spawn) shows them while its core is charged.
- **CE subdomain compat scan** Check CE for any extra related subdomains that it takes over from vanilla
- **Set tuning pass** (`Docs/Specs/`, a Set-Tuning.md as the Medic's). The three defs carry the
  tier's standard plating, shield and costs with placeholder rationale; decide whether the core
  prices like the Controller's drone module (12 components) and what the shoulders and helmet do
  on their own. Candidates in the mechanitor's own vocabulary: a `MechBandwidth` offset (Biotech's
  bandwidth pack is the price reference), a `MechControlGroups` offset, a mech-repair or
  gestation speed offset for a smithing pilot. Keep the set-wide numbers inside the tier's band.
  Also compare stats/costs against the other mods: Alpha Mechs warcasket and Mechlord Warcasket
- **Patch for Alpha Mechs players** our warcasket should probably cost more, similar to Mech Master
- **Helmet/Shoulder passive** Drop command range limit for the mechanitor's mechs on the current map?
- **Recharge rate.** The shipped 1% of capacity per day is one charge every ten days on a
  ten-charge core, a reserve rather than a well, taken literally from the brief; the setting
  slider spans up to a full core a day if play says otherwise.
- **Work profile in game.** Research (for hacking) and Smithing (for the mech work) open,
  Crafting, Tailoring and Dark study behind settings toggles; run the spec's verification
  (`Docs/Specs/Overlord-Work-Trait.md`, section 4), autohack and the Hack gizmo included.
- **Art.** `Docs/Art/WarcasketOverlordHelmet_east_alt.png` is the second helmet east facing the
  artist delivered (a few thousand pixels differ from the one shipped); pick one and delete the other.
- **About art and size.** `About/ModIcon.png` position should be tweaked a little higher within the frame
  The version badge on `About/Preview.png` and `.steamworkshop/Preview.gif` moves with the
  `gif-patch` skill, never a re-export. Run the `../MOD_DOWNLOAD_SIZE_TRIM.md` pass before the
  first release.
- Ensure dead mechanitors in the suit don't play the sparks effect
- Tweak trait text, currently "…is not capable of most work, but can still do smithing and hacking."
  maybe to something like "except mech-related/adjacent work" etc
- **Workshop images.** add gameplay images of the resurrection and the settings etc
- **Smoke test on both branches** (`Scripts/integration-smoke-test.py`) once the game can be
  booted with the full compat list, and a boot on 1.6.
- **Full review of player-facing copy**, the descriptions' lore paragraphs in particular.
- **First Workshop publish.** Commit `About/PublishedFileId.txt`, add the Workshop link and the
  Steam badges to the README, paste the description.

## Later

- **Sometimes spawn vanilla pod/crash mechanitors in the warcasket?**
- **Raider overlords.** The set is on the raid tables as a Controller with the baseline shield;
  AI pawns never cast apparel abilities. Toxin's raider auto-vent is the family precedent for
  giving raiders an ability, so a raider mechanitor resurrecting its escort is possible later.

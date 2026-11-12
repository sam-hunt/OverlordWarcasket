# TODOs

## Features

- Run the `../MOD_DOWNLOAD_SIZE_TRIM.md` pass before the first release.
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
  charge spend per weight class, an ultraheavy (a centurion) included at 8 and refused below
  that, the gauge's red bar and tooltip, the dev gizmos, and the "needs a mechlink" and "no
  charges" disabled reasons. Then the settings page end to end, the summary's "8 for an
  ultraheavy mech" included.
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
  slider selecting its radio and naming the 24.9 rung "(vanilla)" and the 43.9 one "(Combat
  Extended)", the summary line, and the choices surviving a reopen and a
  language switch (`Docs/Specs/Mechanitor-Command-Range.md`, section 5).
- **Set tuning in play.** The pieces are tuned against Biotech's mechlord helmet and suit (the
  def headers have the numbers): check in game that the foundry lists the set only once both
  spacer warcaskets and ultra mechtech are done, that the chips are charged, that a full-set
  pilot reads 20 bandwidth, a control group and 50% psychic sensitivity and can still install a
  mechlink, and how the helmet's -3 shooting plays with a warcasket weapon.
- **In-game run of the core's recharge and reload.** Nobody has watched either in game: the core
  gaining a charge a day and the tooltip's wait; the handle at zero fetching nothing; the handle
  raised making an undrafted pilot fetch advanced components up to that level and no further; a
  right-click on components reloading drafted and undrafted, past the handle; the load button
  appearing only while the pilot carries components (picked up with vanilla's pick up order), its
  icon, red background and count, its disabled reasons when the core is full and when too few are
  carried, and the load taking the components out of the inventory; the reload cost and recharge
  sliders reaching a worn core; the armor's stat sheet showing the reload cost; with the
  automatic reload from inventory setting on, a pilot below the handle loading carried
  components up to it and no further, then fetching the rest. Also whether one
  charge a day is too generous in play (the core usually meets each raid full), with 0.5 as the
  fallback.

- **In-game run with Combat Extended.** The CE root is checked statically and the three C#
  answers to CE's patches build clean, but nothing has been seen in game: the helmet's stat
  sheet reading AimingAccuracy -15% and weapon handling -0.75; a pilot ordering a drafted mech
  across the map with no white ring, the ring at the chosen radius under the radius scope, and
  CE's 43.9 ring back for a mechanitor without the set; the settings window quoting 43.9 as the
  normal range and as the slider's default, and the slider running out to 119; no "Reload worn
  armor" button beside the gauge; a pilot below the handle with components in the inventory
  loading them up to the handle and dropping nothing, and fetching from the map instead with
  the automatic reload from inventory setting off; the load button after CE's own pick up
  order; the helmet's stat sheet listing the long range radio. Also a mech raised by the core: by the code it stands up with a freshly generated,
  loaded weapon and spare magazines, as an apocriton's does (vanilla regenerates the weapon and
  CE loads whatever that generates).
- **In-game run with Alpha Mechs.** The scan found nothing to patch (the armor and ability def
  headers have the findings) and nothing has been seen in game: the core raising one of its
  light, medium and heavy mechs for 1, 3 and 5 charges and an ultraheavy (a siegebreaker)
  for 8; a mech turned by the Mech Master helmet's IFF
  scrambler refused as not ours once dead; an entombed pilot analyzing a tier 4 chip and using
  a boss call building; the "(tier N)" chip labels on the foundry's cost list; this armor and
  helmet under Mech Master shoulders reading 44 bandwidth, with the command range moved to the
  helmet in the settings. Also a blitzkrieg, its suicide mech, raised after it detonates, if
  the corpse survives the blast: it costs a light mech's 1 charge, as it would an apocriton.

## Later

- **Reprice the set for Alpha Mechs?** Left at vanilla's top tier, which Alpha Mechs numbers
  tier 3 (the armor def's header has the comparison with its Mech Master warcasket). The one
  fitting change, if play shows the set crowding its tier 4 gear, is the shoulders' command
  relays taking a hyper-linkage chip, its beamcasting tier, in place of one SignalChip.
- **Sometimes spawn vanilla pod/crash mechanitors in the warcasket?**
- **Raider overlords.** The set is on the raid tables as a Controller with the baseline shield;
  AI pawns never cast apparel abilities. Toxin's raider auto-vent is the family precedent for
  giving raiders an ability, so a raider mechanitor resurrecting its escort is possible later.
  The cost table prices ultraheavies, so a raider's cast would have to refuse the class, as
  the apocriton does.
  Probably also worth considering whether overlords raiders would spawn with a small mech contingent too

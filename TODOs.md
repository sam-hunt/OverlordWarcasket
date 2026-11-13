# TODOs

## Features

- **Cerebrex node mask** pending art landing
- **Extraneous debug** "[Overlord Warcasket] Overlord warcasket trait disabled work types: Cooking, Construction, Growing, Mining, PlantCutting, Tailoring, Crafting."
- **Manual ability reload** should reload only up to the slider if it's less than it, maybe if it's reattempted at that point, then reload to full?
- **Automatic ability reload** doesn't seem to work from inventory? is "pick up to inventory" PUAH only? I don't see the vanilla float menu option to pick up, so can't reach the gizmo or reload behavior that way
- **Optimize images** Run the `../MOD_DOWNLOAD_SIZE_TRIM.md` pass before the first release.
- **Workshop images.** add gameplay images of the resurrection and the settings etc
- **Smoke test on both branches** (`Scripts/integration-smoke-test.py`) once the game can be
  booted with the full compat list, and a boot on 1.6.
- **Full review of player-facing copy**, the descriptions' lore paragraphs in particular.
- **First Workshop publish.** Commit `About/PublishedFileId.txt`, add the Workshop link and the
  Steam badges to the README, paste the description.

## Testing

- **In-game look at the cerebrex node on a pilot facing north.** The node over the shell and
  under the pauldrons, with Odyssey's art and with ours; unchanged on a mechlord suit, on the
  other three facings and on an outfit stand.
- **In-game run of the resurrect mech ability.** The cast's effects at their single mech
  size, on a light mech and on a centipede: the wave and the sparks on the corpse through the
  warmup, the burst and the flash as it stands up. A raised mech in the pilot's first control group
  and working at once, and a corpse refused with the bandwidth message when the pilot has
  no room for it (drop the helmet after the mech dies). Casts queued with shift: several
  corpses clicked in a row with the targeter staying open, raised in order with the cooldown
  waited out between them, and a corpse the queue leaves no charges for refused at the click.
  The resurrector's red sparks on the pilot:
  their size and spread against the warcasket's art. A raider in the set (dev spawn) shows
  them while its resurrector is charged. A drafted non-mechanitor pilot shows none, and does with the
  mechlink requirement off.
- **In-game run of the Anywhere command range from a caravan.** A pilot who left in a caravan
  drafting a mech at home, ordering it to move and attack with no out of range refusal, and
  changing its group's work mode; the draft button greyed again under the other two scopes,
  with the helmet off, and for a pilot in a transport pod in flight. Mechs drafted as the pilot
  walks off the map undraft, as vanilla has it, and draft again once the caravan has formed.
- **Set tuning in play.** How the helmet's -3 shooting plays with a warcasket weapon.
  Is the recharge too generous?
- **In-game run with Combat Extended.** The CE root is checked statically and the three C#
  answers to CE's patches build clean, but nothing has been seen in game: the helmet's stat
  sheet reading AimingAccuracy -15% and weapon handling -0.75; a pilot ordering a drafted mech
  across the map with no white ring, the ring at the chosen radius under the radius scope, and
  CE's 43.9 ring back for a mechanitor without the set; the settings window quoting 43.9 as the
  normal range and as the slider's default, and the slider running out to 118.9; no "Reload worn
  armor" button beside the gauge; a pilot below the handle with components in the inventory
  loading them up to the handle and dropping nothing, and fetching from the map instead with
  the automatic reload from inventory setting off; the load button after CE's own pick up
  order; the helmet's stat sheet listing the long range radio. Also a mech raised by the resurrector: by the code it stands up with a freshly generated,
  loaded weapon and spare magazines, as an apocriton's does (vanilla regenerates the weapon and
  CE loads whatever that generates).
- **In-game run with Alpha Mechs.** The scan found nothing to patch (the armor and ability def
  headers have the findings) and nothing has been seen in game: the resurrector raising one of its
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

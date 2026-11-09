# SPEC: Bench work for the entombed overlord

> Implemented. The code, its def headers and its tests are the tested current state and supersede this spec wherever the two differ.

Status: IMPLEMENTED on the shared `1.6/` tree (`Source/1.6/WorkProfile.cs`,
`1.6/Defs/TraitDefs/WarcasketTrait_Overlord.xml`, `Source/1.6/Settings/WorkSetting.cs`), not yet
run in game (`TODOs.md`). The mechanism is the Medic Warcasket repo's
(`../MedicWarcasket/Docs/Specs/Medic-Work-Trait.md`, sections 2 to 4, verified there against
RimWorld 1.7.4984 and VFEP v4.02); this spec records only what differs for the Overlord.

## 1. Goal

A pawn entombed in the Overlord set does the mechanitor's work: smithing, where Biotech files
mech gestation, subcore encoding and the mechanitor-only mech repair, and research, which
carries hacking. The player may also open crafting, tailoring and, with Anomaly, dark study.
Everything else VFEP's warcasket trait blocks stays blocked, and the blocking must follow what
the player's other warcasket mods do to VFEP's profile, as the Medic's does.

## 2. What differs from the Medic

The Medic opens Caring, a tag that covers only Doctor and Childcare, so dropping the tag and
re-blocking Childcare by name was the whole job. Smithing carries ManualSkilled, which also
covers Cooking, Construction, Growing, Mining, PlantCutting, Crafting and Tailoring, and Research
carries Intellectual, which also covers Anomaly's DarkStudy (the installed Core and DLC work
types, read from `Data/*/Defs/WorkTypeDefs`; no DLC adds another), so the mirror generalises:

- `WorkProfile.Openable` names the openable work types by defName with the fixed rule's
  default: Research and Smithing open, Crafting, Tailoring and DarkStudy blocked (DarkStudy
  skipped silently without Anomaly).
- `ResolveTags` (pure, tested) keeps the Medic's ManualDumb-to-Cleaning swap and removes every
  tag an openable work type carries that VFEP blocks (ManualSkilled and Intellectual).
- Every work type the removed tags covered that is not openable is re-blocked by name in
  `disabledWorkTypes`, beside VFEP's own list; a blocked toggleable is added by the settings
  write.
- A toggleable is open by default when its own default says so or when VFEP's trait, as
  patched, blocks neither its tags nor the type, so "Warcaskets can work!" opens the skilled
  ones.

## 3. Accepted consequences

- The Bio tab's "Incapable of" list reads tags alone, so it no longer names skilled labor for
  an Overlord pilot; the work tab, which reads both fields, shows every block. Skill greying
  reads tags too: the Crafting skill is no longer greyed.
- As for the Medic, a modded work type tagged ManualDumb opens for the Overlord, and the
  pilot does opportunistic hauling.
- Hacking is not a work type. Core's `Hack` work giver is Research work, and every hack path
  (the autohack work giver, the Hack gizmo, the float menu) asks
  `HackUtility.IsCapableOfHacking`, which refuses a pawn with the Intellectual tag blocked or
  the Intellectual skill totally disabled. With Research blocked by name the skill is totally
  disabled (DarkStudy is its only other work type), so hacking comes with Research or not at
  all; a hacking-only patch was weighed and dropped, as the disabled skill reads as level 0 and
  fails every terminal's skill prerequisite.
- Smithing cannot be split from the mech work without a new work type every pawn would see, so
  opening it opens the smithy, machining table, fabrication bench and bioferrite shaper too.

## 4. Verification (pending)

Dev mode, a pawn welded into the set: the work tab offers Smithing and Research and withholds
Crafting, Tailoring, Cooking and the rest; the pilot gestates and repairs mechs, and both
autohack and the Hack gizmo accept them. Flip Crafting in the settings and it appears at once,
unassigned; block Research and the Hack gizmo reports the pilot incapable. With "Warcaskets can
work!" active, the skilled toggleables are open by default. The startup log line names the
toggleables and the defaults.

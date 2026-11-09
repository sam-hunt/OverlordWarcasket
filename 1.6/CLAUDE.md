# 1.6/: defs, patches, languages and compat roots

Loaded when work touches the game content. Each def's header carries its values and their
rationale; this file holds the rules that cross defs. The one `1.6/` tree serves 1.6 and 1.7
(`LoadFolders.xml` lists it under both and says why). Defs load recursively and the deploy
manifest globs, so new files and subfolders need no build change.

## Defs

- **Warcasket parts are `VFEPirates.WarcasketDef`s** on VFEP's abstract bases
  (`VFEP_WarcasketArmorBase`, `VFEP_WarcasketShoulderPadBase`, `VFEP_WarcasketHelmetBase`), so
  the foundry, entombing and removal surgery pick them up without C#. Check VFEP's XML before
  reimplementing anything a base provides. The template is the Controller set; a field a def
  header does not discuss is the Controller's, so diff against it before changing one.
- **Every `ApparelExtension` we declare carries `priority` 1.** The VFEP bases carry their own,
  and VEF merges duplicates at resolve time, keeping the highest priority and dropping fields its
  `Merge` does not copy; any new `ApparelExtension` field, from any load root, goes on our entry.
  The armor def's header has the mechanics.
- **The ability is a VEF ability** (`VEF.Abilities.AbilityDef`, granted by the armor's
  `CompProperties_AbilitiesApparel`), never a vanilla `AbilityDef`: vanilla has no apparel grant,
  and the family's abilities are all VEF's. Biotech's defs it reuses (sounds, effecters, the icon,
  the `MechChargeCosts` entries) need no fallback; Biotech is a hard dependency.
- **Worn graphics go through the pawn render tree** (`apparel.renderNodeProperties`), never draw
  hooks. The Shipcracker repo's thruster glow is the precedent, including the zoomed-out pawn
  cache and draw-order traps; read its CLAUDE.md bullet before adding one here.
- **Tunables are def fields** the mod settings write at startup (`Source/1.6/CLAUDE.md`); the XML
  value is the default and the header stays the record.

## Languages

- `Languages/English/Keyed/OverlordWarcasket.xml` is the one home for every code-printed string,
  under the `OLWC_` prefix. Everything else is def text, which translators reach as DefInjected.
  Biotech's own Keyed strings (`MechResurrectionCharges`) may be printed directly: the DLC is a
  hard dependency, so the key always exists.
- `WarcasketDef` has no def database of its own: the game injects it under `ThingDef`. A
  `DefInjected/WarcasketDef/` folder would never load.
- Translation passes, the checker and the sidecar are described in `Scripts/CLAUDE.md`.

## Optional content and gating

- **Gate by load root, not by attribute.** `MayRequire` works on def roots and list items, but the
  DefInjected loader ignores attributes, so a gated def's injection in the main tree errors
  ("found no def named ...") whenever the gate is closed. Gated content ships from a compat root
  listed with `IfModActive` in `LoadFolders.xml` (`Mods/VanillaGravshipExpanded` is the model):
  `1.6/Mods/<Mod>/` for version-specific content, root-level `Mods/<Mod>/` for art. The def's own
  `MayRequire` is then redundant and dropped. A patch in a gated root, or behind a
  `PatchOperationFindMod` gate, fails silently for everyone but that mod's players, so
  `Tests/1.6/CompatPatchTests.cs` resolves every patch's xpaths against the main tree; keep every
  xpath to one node per `defName` it names, or zero for a `PatchOperationConditional` probe.
- **Compat roots sit beside the well-known folders, never inside them.** Anything under
  `1.6/Defs/**` or `1.6/Languages/**` loads unconditionally at any depth.
- **An injection is gated by the root that contains it, never by the def it targets.** A main-tree
  def's translation placed in a compat root silently vanishes when the gate closes.
- **A compat root's language files never reuse a main-tree language-relative path**
  (`DefInjected/<Type>/<File>.xml`, `Keyed/<File>.xml`): the game dedups per mod by that path and
  silently skips one whole file. Suffix the filename with the gate's name (`Apparel_Odyssey.xml`).
- **A compat art variant goes at the main tree's exact texture path** inside the gated root, not
  under a new name behind a patch. A later-listed load folder wins a shared path, so the gated
  root stays below `/` in `LoadFolders.xml`.
- **Gate on the package id** (`ludeon.rimworld.odyssey`), never `PatchOperationFindMod` (display
  names), unless the patch mirrors an upstream patch and copies its gate so both fire together.

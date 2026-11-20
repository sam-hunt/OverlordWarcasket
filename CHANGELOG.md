# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- Entries are short one-liners ready to paste as Steam Workshop change notes: what changed for the player, no rationale, no def names, no sub-bullets. -->

<!-- Convention: unreleased changes accumulate under the [Unreleased] heading at the top,
     directly below this intro, so a merged contribution is recorded where the release
     flow finds it. Each release renames that heading to its version and date and adds a
     link reference at the bottom of the file. The release workflow lifts the tagged
     version's section into the GitHub release body verbatim and FAILS a stable release
     if the section is missing, so write the section before tagging. Release candidates
     (X.Y.Z-rc.N) get no section: their changes land in the stable version's section at
     promotion. The /release skill walks through all of this. -->

## [Unreleased]

### Added

- A mod setting, off by default, that puts the overlord warcasket on the long-dead mechanitor in a crashed mechanitor ship and on 30% of those in ancient mechanitor complexes

### Fixed

- Pawns no longer fail to generate when another warcasket mod also keeps its parts off generated pawns

## [1.1.0] - 2026-11-19

### Added

- A mod setting for the range of resurrect mech, from 1.9 cells up to anywhere on the map
- Resurrect mech shows its range as a ring around the pilot while its button is hovered or a target is being chosen
- Mech stun pulse: when the pilot dies, the armor stuns every mech nearby, friend or foe, as an apocriton does
- Mod settings for the stun pulse's radius, and to release it when the pilot is downed instead

## [1.0.0] - 2026-11-17

### Added

- The Overlord, a spacer-tier mechanitor warcasket set for VFE Pirates
- Resurrect mech: the armor revives one of your recently killed mechs, as the apocriton does
- The armor's mech resurrector holds ten charges, recharges on its own and can be reloaded with advanced components, with a red gauge on the pilot
- A charged resurrector crackles with the apocriton's red sparks
- A mechanitor wearing the shoulders can order drafted mechs anywhere on the map, not just within 24.9 cells
- The helmet and armor add mech bandwidth, and the shoulders add a control group and hacking speed
- Mod settings choose who gets the extended command range and how far it reaches, down to a radius slider
- The pilot can still do smithing and research, so mech gestation, mech repair and hacking stay in reach
- More work types for the pilot can be enabled in the mod settings
- Mod settings for the mech resurrector, the ability and the pilot's work
- With Odyssey, a mod setting redraws the cerebrex node with the mod's own art, with a choice of how much of it a dye colors
- With Odyssey, a cerebrex node worn with a warcasket shows on its back instead of being hidden under the shell
- Combat Extended support
- Vanilla Gravship Expanded and Save Our Ship 2 support
- Machine-assisted translations for nine languages

[Unreleased]: https://github.com/sam-hunt/OverlordWarcasket/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/sam-hunt/OverlordWarcasket/releases/tag/v1.1.0
[1.0.0]: https://github.com/sam-hunt/OverlordWarcasket/releases/tag/v1.0.0

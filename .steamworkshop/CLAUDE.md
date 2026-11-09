# Workshop description: editorial rules

Loaded whenever work touches this folder. `README.md` beside this file holds the format, the
title rule and the publishing workflow; the toolkit's `l10n/workshop.md` holds the family-wide
mechanics (the 8000-byte limit, the checker, Steam's language names). This file is the
editorial contract for the text itself, English first and every translation after it.

## What the page is

`Description/English.txt` is the mod's main marketing artifact, pasted at release and nowhere
else. It sells the mod to a player skimming the Workshop, and it must hold up for the whole
life of that release.

- **Shipped state only.** The page goes up at a release and describes that release. No "in
  development", no "not tuned yet", no "coming soon", no feature that is not in the tagged
  build. Work-in-progress notes live in `TODOs.md` and the CHANGELOG, never here.
- **Headline features at a high level.** Name what the set does in the words a player uses
  (the pilot can doctor; one beam dresses every wound). Leave out how it is done (traits,
  mirrors, comps, def names), edge cases and residuals; those are for the def headers and the
  GitHub page a curious player can follow. A line earns its place by holding interest, not
  by being complete.
- **No def numbers.** Kits, offsets, ranges and prices change with tuning and go stale on a
  page nobody re-pastes. Say "a suite of medicine kits", not "five kits". The mod
  settings window shows the current numbers.
- **One compat line per mod, verdict first.** "Vanilla Gravship Expanded: follows its
  vacuum-resistance rules for warcaskets." A known conflict gets one plain sentence; the mechanism does not.

## Shape

Whitespace and structure are what keep a skimming player on the page. A wall of text loses
them before the first feature.

- `[h1]` for the page sections (About, Mod Compatibility, FAQ, Links), `[h2]` for the
  features inside About, `[hr][/hr]` between sections.
- Short paragraphs of one to three sentences; bullet lists for anything parallel (the pieces,
  the compat lines); `[b]Piece:[/b]` leads on list items so the eye can scan.
- The FAQ answers questions players actually ask (how do I get it); it is not a place to
  store caveats.

## Voice

The author writes and tweaks the English by hand; edits must read as that same hand, so a
player's attention stays on the mod and not on how its page was written.

- Plain, direct sentences with a verb. Say what the thing does.
- Avoid the patterns that mark machine-written copy: em dashes (already banned for all
  player-facing text), "not X, but Y" contrasts, three-item flourishes, rhetorical questions,
  "whether you're ... or ...", "seamlessly", "elevate", "unleash", "say goodbye to", a bolded
  phrase that opens every bullet with a colon, and a closing sign-off line.
- No enumerated edge cases or disclaimers in prose; a caveat that matters is one short line
  in the compat list.
- Match the register of VFE Pirates' own page: it is the set's neighbour on the Workshop.

## Length and translations

Steam saves at most 8000 UTF-8 bytes with CRLF newlines per language, and every translation
must fit, not only the English. Cyrillic doubles the byte count of prose and CJK scripts
triple it per character, so the English budget is set by the heaviest translation, not by
the English itself.

- Keep the English body (everything after the title and blank line) at or under about
  3500 CRLF bytes. Russian then lands near 7000 with its BBCode overhead, inside the
  toolkit's 7700-byte practical ceiling; the checker (`python3 Scripts/check-translations.py`)
  measures every language and is the final word.
- Growth is additive by translation count: a sentence added to the English is added nine
  times. Cut before adding.
- Translations follow the English structure but may be terser; they never add detail the
  English does not have.

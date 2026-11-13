# Glossary, Overlord Warcasket-specific terminology

Per-language files here (`German.md`, `Russian.md`, `ChineseSimplified.md`,
and so on, named after the RimWorld language folder) hold everything about
a language's translation that is specific to this mod: the VFEP warcasket
vocabulary decisions for that language (how "warcasket", "entomb", the set
names and "warcasket foundry" are rendered, and which community VFEP
translation, if any, they were grounded against), the overlord set's own
coined terms, and anything pending native review.

Every roster language has a seeded file, written before any translation
pass ran and before this set's own text was final. Each carries, in this
order: the community VFEP source it is grounded against (or the absence of
one); the rows inherited verbatim from the sibling Shipcracker Warcasket
repo's `.claude/skills/translate/glossary/<Language>.md` (warcasket, the
part nouns, the label pattern, foundry, entomb, set names, the VEF ability
strings, the settings words Shipcracker settled), so the sets read as one
family in-game; the armor's charge noun and that language's plural-lookup
constraint on a coined one; the list of terms the first pass must still
ground or coin; and what is pending native review. An inherited row is
never changed here first: reconcile it in the Shipcracker glossary, then
mirror it here and in Toxin Warcasket. A term that has since been corrected
by a native reviewer in the sibling repo is the one to carry.

Family-shared, mod-independent findings, LanguageWorker mechanics, style
and corpus rules, and vanilla-grounded common vocabulary (armor, helmet,
plasteel, quality tiers, tech levels, and so on), live upstream in the
`l10n/` submodule at `l10n/languages/<Language>.md` (canonical checkout:
`~/dev/rimworld-l10n`), since they apply to any mod in the family, not
just this one. The VFEP warcasket vocabulary is now seeded in four family
repos (this copy is the fourth); moving those rows upstream is a TODOs.md item.

When a future translation pass coins a new Overlord Warcasket-specific term,
record it here. If a pass instead surfaces a correction to shared
mechanics or vocabulary, send that fix upstream to the l10n repo rather
than duplicating it here.

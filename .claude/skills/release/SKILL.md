---
name: release
description: Prepare and publish a versioned release — version bumps, changelog, build, commit, tag, push
disable-model-invocation: true
argument-hint: "[major|minor|patch] [rc] | [promote|rc]"
---

# Release

Prepare and publish a new release for Overlord Warcasket — either a stable
release or a release candidate.

**Release candidates** (`X.Y.Z-rc.N`) are private test builds: a tagged GitHub
prerelease whose zip can be tried on another machine without building from
source. They never go to the Steam Workshop and get no `CHANGELOG.md` section.
Otherwise an RC is held to the same bar as a stable release (it should be what
would ship), so it runs every step below except the changelog and the
Workshop publish (step 9). SemVer orders `1.3.0 < 1.4.0-rc.1 < 1.4.0-rc.2 < 1.4.0`, so
candidates sit between stable versions without disturbing them.

`$ARGUMENTS` is optional and resolved at step 7, where the version is first
needed. From a stable version: a bump type (`major`, `minor`, `patch`),
optionally followed by `rc`. From an RC version: `promote` (to the stable
version) or `rc` (the next candidate). Ask for whatever is missing.

"The last stable tag" below means the newest tag with no prerelease suffix —
`git describe --tags --abbrev=0 --exclude '*-*'`. Every range in this skill
measures from it, never from an RC tag, so a stable release's changelog and
Workshop diff cover everything since the previous stable release.

## Current state

!`grep -o '<modVersion>[^<]*' About/About.xml | sed 's/<modVersion>/About.xml version: /'`
!`git describe --tags --abbrev=0 --exclude '*-*' 2>/dev/null || echo "no stable tags found"`
!`git -c versionsort.suffix=- tag -l 'v*-*' --sort=-v:refname | head -3`
!`git log "$(git describe --tags --abbrev=0 --exclude '*-*' 2>/dev/null || echo 'HEAD~10')..HEAD" --oneline --no-merges`

## Steps

Work through the steps below in order. Steps 1-6 are validation and may
generate their own commits, which is exactly why the release decision
(version, changelog, tag) happens once, at step 7, after everything that can
still change the history. Confirmations: the conditional translation commits
in steps 3-4 each get a diff review, and step 7 is the single release gate;
nothing else asks.

**Promoting an RC with nothing committed since its tag** (`git log
<rc-tag>..HEAD` is empty): the candidate already validated this exact tree, but
the world may have moved since (an upstream l10n release, a vanilla update
changing inherited text), so steps 2-3 always run. If step 3 commits nothing, skip
steps 4-6 and go straight to step 7 — say so. If it does commit (a pin bump,
a sidecar or translation change), the tree is no longer the one the candidate
validated: run the full sequence. Any other commit since the RC tag means the
full run.

### 1. Review changes

The commit log since the last stable tag is shown above — read it now to understand
what this release contains. If the repo has no tags yet this is the first
release: use the full history (`git log --oneline --no-merges`) and think in
terms of the mod's shipped feature set rather than a diff. No confirmation —
this is orientation, not a decision.

### 2. Tests and build gate

Run, in order:
```bash
dotnet test Tests/1.6/OverlordWarcasket.Tests.csproj
dotnet build OverlordWarcasket.sln -c Release
```

- Both projects set `TreatWarningsAsErrors`, so the build is also the lint
  gate: any compiler or analyzer warning fails it, and a passing build means
  there is nothing warnings-only left in the log to read out.
- This runs before the game boots in steps 3 and 6 because it takes seconds
  and fails fast; a broken tree must not reach the translation commits.
- On any failure, stop and help the user fix it, then rerun until both pass.
  No confirmation on success.

### 3. Refresh translation expectations and check freshness

Run, in order:
```bash
l10n/tools/bump-consumer.sh   # pin l10n to its latest release tag (commits; no-op when current)
python3 Scripts/refresh-translation-expectations.py
python3 Scripts/check-translations.py --strict
```

- The first command is one of the three moments the l10n pin moves (release,
  translation-pass start, new upstream major); the checker's `--strict`
  engine-pin check fails until the pin is on the latest tag. It commits the
  pointer directly; report its one-line result. If it reports a **MAJOR**
  bump, stop: upstream changed the shim/flow contract and this repo owes the
  accompanying edit before the release continues.
- The refresh script refuses to start while RimWorld is already open (it
  needs an exclusive boot for the mod-list swap). If it reports that, **stop
  and ask the user** to close the client, and rerun only after they confirm
  it is free.
- The refresh script regenerates `Scripts/expected-injections.json` by
  launching the local RimWorld client with `-l10nprobe` (graphical boot,
  ~1-2 min; the L10nProbe dev mod dumps every DefInjected key the live game
  expects, then quits). This is what surfaces vanilla-inherited and
  C#-default strings a def-XML scan cannot see. Report its diff summary. If
  it reports that the probe wrote no dump, check that
  `shunter.overlordwarcasket` is ticked in the probe's settings
  (`Config/Mod_L10nProbe_L10nProbeMod.xml`); the probe only dumps ticked
  mods.
- Player text register: diff the English player-facing text since the last
  stable tag (Keyed, def labels and descriptions, `About.xml`, the English
  Workshop page) and read every changed string against
  `l10n/languages/English.md` § 2. The checker's `[player text]` warnings
  cover the mechanical tells and fail this step under `--strict`; this read
  covers the judgment ones (a sentence frame repeated across strings,
  telegraphese, tricolons, coinages). Fix the English before any translation
  step, since a tell in the English ships in every language.
- If the diff shows **added or changed keys**, translate them in every
  language now (the `translate` skill's update pass), then rerun the checker.
  Keys from a compat root's defs (`1.6/Mods/<Mod Name>/`) belong in that
  root's `Languages/`, never the main tree (see CLAUDE.md's gating section).
- Report the per-language checker result (missing keys, stale entries,
  errors). CI's release gate runs the same script without `--strict` against
  the checked-in sidecar; the stricter local run surfaces warnings while
  there is still time to act on them.
- If the sidecar or any translations changed, commit them as their own
  l10n commit (show the diff and **ask the user to confirm**) before moving
  on — the release commit at step 8 stages only the version-bump files.
  Pick the prefix by what the change means to players: `fix(l10n)` for
  drift in content a previous release already shipped (strings players
  could see untranslated or stale), `feat(l10n)` for strings belonging to a
  feature that has not shipped yet (nothing was broken, the feature's work
  was just unfinished). Translations are player-facing, so never `chore`;
  only a sidecar-only regen with no translation change is a `chore(l10n)`.

### 4. Refresh Steam Workshop page translations

The Workshop title and description live in
`.steamworkshop/Description/<Language>.txt` — line 1 is the title, then a
blank line, then the BBCode description; one file per language folder in
`1.6/Languages/`, English being the source of truth (see
`.steamworkshop/README.md`).

- Diff the English source against the last stable release:
  ```bash
  git diff $(git describe --tags --abbrev=0 --exclude '*-*') -- .steamworkshop/Description/English.txt
  ```
  With no stable tag yet (the first release), treat `English.txt` as changed.
  If an earlier RC of this version already translated the change, the
  language files will already match — check before spawning anyone.
- Also check for languages in `1.6/Languages/` with no description file yet.
- If nothing changed and no file is missing, say so and move on.
- Otherwise spawn one translation subagent per affected language (cheaper
  model, in parallel) to update or create its file, grounded in the
  `translate` skill's glossary for that language and the mod's own committed
  `1.6/Languages/<Language>/` strings, preserving BBCode tags and the
  title-line format. Subagents never commit.
- Review the diffs, then commit them as their own `docs:` commit (show the
  diff and **ask the user to confirm**).

### 5. Clean build and deploy

Run:
```bash
dotnet clean OverlordWarcasket.sln
dotnet build OverlordWarcasket.sln -c Release
```

Report the build result. If the build fails, stop and help the user fix it.
On success, move straight to the smoke test — no confirmation.

### 6. Integration smoke test

Run (game closed — the script refuses while RimWorld is open, same as the
refresh in step 3; if it reports that, **stop and ask the user** to close the
client and rerun):

```bash
python3 Scripts/integration-smoke-test.py
```

- Boots the freshly deployed build once on a pinned list of Overlord
  Warcasket plus its dependency chain (Harmony, Vanilla Expanded Framework,
  VFE Pirates, Biotech, Odyssey) and its optional integrations (graphical
  boot, ~1-2 min, auto-quits), then classifies every Player.log error by
  origin and fails on anything attributed to Overlord Warcasket or an
  integration seam:
  - **Vanilla Gravship Expanded**, so the `1.6/Mods/VanillaGravshipExpanded`
    root opens and its vacuum-resistance patch runs.
  - **Combat Extended**, so the `1.6/Mods/CombatExtended` root opens and its
    stat patch runs.
  - Biotech is on the list as a hard dependency: the resurrect mech ability
    is built from its defs.
  - Odyssey is on the list for Vanilla Gravship Expanded; the
    `1.6/Mods/Odyssey` root's cerebrex art patch stays off on this
    default-settings boot and is covered statically by the tests instead.

  A failed PatchOperation or a broken compat-root def is exactly the kind of
  error only a boot with the mod active can surface, and this is the only
  automated coverage any seam gets. It exists because the
  BetterTradersGuild v1.1.0 CWTL regression showed such a problem can be
  invisible without an integration mod active (see this repo's CLAUDE.md
  patch-timing note).
- On PASS, report the summary line and move on — no confirmation. On FAIL,
  show the gated error blocks and **stop** — the release does not proceed
  until the errors are fixed or the user explicitly waives them. Third-party
  (`other`) errors are reported but not gating; mention them so the user can
  judge.
- The smoke test boots whatever Steam branch is installed (1.6 on `main`,
  1.7 on `unstable`). A Workshop release of a 1.6+1.7 mod is smoke-tested
  on both branches when the change touched C# or patches.

### 7. Version, changelog, and the single release confirmation

Everything that can change history has now run, so the release contents are
final. Do all of the following, then present it as **one** confirmation:

- Read the current version from `About/About.xml` (`<modVersion>`) and
  resolve the new version (from `$ARGUMENTS`, or ask now):
  - **Current is stable** (`1.3.0`): apply the bump type, then either stable
    (`1.4.0`) or the first candidate (`1.4.0-rc.1`).
  - **Current is an RC** (`1.4.0-rc.1`): either promote (`1.4.0`) or cut the
    next candidate (`1.4.0-rc.2`). A bump type doesn't apply here; if the
    user gives one anyway, ask what they mean (a different target version
    abandons the current candidate line).
  - Before an RC, confirm its tag doesn't already exist (`git tag -l`).
- **Stable releases only — the changelog.** An RC skips this bullet group
  entirely: no section, no link reference.
  - Start from whatever already sits under `## [Unreleased]` (entries
    recorded when a change or contribution landed, with their credits), then
    complete it from the full log since the last stable tag — including any
    commits steps 3-4 just created — grouped under Keep a Changelog headers
    (Added, Changed, Fixed), omitting chore/version-bump commits.
    When promoting, this spans every candidate: a fix for a bug that was
    introduced and fixed within the candidate line never reached Workshop
    users, so fold it into the entry it corrects or drop it.
  - Each changelog entry is a short one-liner fit for Steam Workshop change
    notes (see the note atop `CHANGELOG.md`).
  - Update `CHANGELOG.md`: rename the `## [Unreleased]` heading to
    `## [X.Y.Z] - YYYY-MM-DD` using today's date, put a fresh, empty
    `## [Unreleased]` heading back above it, add a
    `[X.Y.Z]: https://github.com/sam-hunt/OverlordWarcasket/releases/tag/vX.Y.Z`
    link reference at the bottom above any older ones, and point the
    `[Unreleased]` compare link at `compare/vX.Y.Z...HEAD`. The workflow's
    extraction anchors on the version heading, so the empty `[Unreleased]`
    section above it is harmless.
- Bump the version strings in both files:
  - `About/About.xml` `<modVersion>`: the full version, suffix included
    (`1.4.0-rc.1`). The game treats it as a display-only string, so testers
    with the Workshop copy also subscribed can tell the two apart.
  - `Source/1.6/Properties/AssemblyInfo.cs`: `AssemblyInformationalVersion`
    gets the same full version; `AssemblyVersion` and `AssemblyFileVersion`
    stay four-part numeric `X.Y.Z.0` with no suffix (they can't hold one), so
    they are identical across every candidate and the stable release.
- Show the user, together: current version → new version (and bump type, or
  RC / promotion), the changelog notes (stable only), the full diff of the
  changed files, and exactly what step 8 will do (rebuild, commit
  `chore: Bump version to <version>`, tag `v<version>`, push with tags).
- **Ask the user to confirm — this is the only release confirmation.** On
  edits, apply them and re-show only what changed.

### 8. Rebuild, commit, tag, push

No further questions unless something is unexpected:

- Rebuild (`dotnet build OverlordWarcasket.sln -c Release`) so the deployed
  DLL carries the bumped `AssemblyVersion`. Stop on failure.
- Stage only the release files: `About/About.xml`,
  `Source/1.6/Properties/AssemblyInfo.cs`, and (stable only) `CHANGELOG.md`.
  If other tracked files are modified, list them and ask whether to include
  them (the one conditional exception).
- Commit with message: `chore: Bump version to <version>`
- Tag with: `v<version>`
- Push: `git push && git push --tags`
- Show `git log --oneline -3` and
  `git -c versionsort.suffix=- tag -l 'v*' --sort=-v:refname | head -5` (the
  config puts each candidate *below* its stable; git's default ranks it above).
- **RC:** that's it. The tag-triggered workflow marks any suffixed tag as a
  GitHub prerelease (never "Latest") with a stub body plus the auto-generated
  commit list, and attaches `OverlordWarcasket-v<version>.zip`. Point the
  user at the release page for the zip, and remind them it's publicly
  downloadable but must not be uploaded to the Workshop.
- **Stable:** the **GitHub** release notes need no paste: the tag-triggered
  workflow lifts this version's `CHANGELOG.md` section into the release body
  itself (and hard-fails the release if the section is missing), so the
  changelog entry written at step 7 is the release body. Continue with step 9.

### 9. Publish to the Steam Workshop (stable only)

Nothing here is automated: Steam has no API for per-language text, page
images live outside the repo, and the upload itself runs from the game. Walk
the user through the hand-offs below in order and wait at each one.

- **Upload from the game.** In the mod list, select Overlord Warcasket, then
  Advanced... > Upload to Steam Workshop. This uploads the local deploy that
  step 8 just rebuilt; in one go it sets the content folder, the tags ("Mod"
  plus every supported version from `About.xml`) and the preview, which it
  always takes from `About/Preview.png`.
  - **First release:** the game writes the new item's id to
    `About/PublishedFileId.txt` of the *deployed* copy
    (`Mods/OverlordWarcasket/`), which the next build overwrites. Copy it into
    the repo's `About/` and commit it as
    `chore(about): Track the Workshop PublishedFileId`; the staging manifest
    ships it, so every later upload updates this item instead of creating
    another. Check the item's visibility on its page straight away and keep it
    hidden until the rest of this step is on the page.
  - **Which game client:** the 1.6 client, or the 1.7 client with its
    "Build asset pack" box unticked; the two send the same bytes (the mod
    folder verbatim, tags from `About.xml`), and that is what the CI zip and
    the smoke test saw. Ticked, the 1.7 uploader bakes an `Assets.zip` of
    BC7 textures into the root of every load folder its `<v1.7>` block names
    that has `Textures/` or `Sounds/`, writing it into the *deployed* copy
    (the next build removes it, so a later upload rebakes rather than reusing
    a stale pack) and ships a Workshop item that differs from the tested
    deploy. "Don't upload source textures" must stay unticked while 1.6 is
    in `supportedVersions`: 1.6 ignores the pack and would ship with no art.
- **Change notes:** show the changelog notes from step 7 for the user to paste
  into the Workshop change notes of this upload.
- **Title and description per language:** list every
  `.steamworkshop/Description/` file changed since the last stable tag
  (`git diff --name-only <last-stable-tag> -- .steamworkshop/Description/`;
  on the first release, every file), not only those step 4 just touched: an
  earlier RC may already have committed them, and the Workshop page has seen
  none of it. Remind the user to paste each listed title and description into
  the page's per-language edit UI (Steam's own language names differ:
  schinese, koreana, brazilian, latam, ...).
- **Page images:** the gallery lives in `.steamworkshop/Images/`. The user
  uploads new or changed ones through the page's images and videos UI; list
  any changed since the
  last stable tag (`git diff --name-only <last-stable-tag> --
  .steamworkshop/Images/`). Steam rejects gallery images of 2 MB or more and
  takes no video files, so a gameplay clip goes up as a GIF cut to fit:
  ffmpeg ships with Krita on the Windows side
  (`.../steamapps/common/Krita/krita/bin/ffmpeg.exe`), and lowering frame
  rate, scale and palette with Bayer dithering is what gets a few seconds of
  gas under the limit:
  ```bash
  ffmpeg -i Clip.mp4 -lavfi "fps=8,scale=iw*0.75:-2:flags=lanczos,split[a][b];[a]palettegen=max_colors=48:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=4:diff_mode=rectangle" -loop 0 Clip.gif
  ```
- **Animated preview** (only when `.steamworkshop/Preview.gif` exists; skip
  silently otherwise). The mod list loads `About/Preview.png` through Unity's
  `LoadImage`, which decodes only PNG and JPG, so a GIF shipped under that name
  draws nothing in-game. The Workshop preview is a separate Steam call from the
  content upload, so the page gets the GIF through a preview-only SteamCMD
  update *after* the in-game upload, which always resets the preview to the
  static PNG. This bullet therefore repeats on every stable release.
  - Steam rejects previews of 1 MB or more; check the file first:
    ```bash
    stat -c %s .steamworkshop/Preview.gif   # must be below 1048576
    ```
  - Write the update manifest (only the keys present are changed; omitting
    `contentfolder` leaves the uploaded content alone):
    ```bash
    printf '"workshopitem"\n{\n    "appid" "294100"\n    "publishedfileid" "%s"\n    "previewfile" "%s"\n}\n' \
      "$(cat About/PublishedFileId.txt)" "$PWD/.steamworkshop/Preview.gif" \
      > "${TMPDIR:-/tmp}/OverlordWarcasket-preview.vdf"
    ```
  - The user runs SteamCMD themselves in a separate terminal (the login
    prompts for a Steam Guard code, and some reports say it signs the desktop
    client out, so the game should be closed by now anyway):
    ```bash
    ~/steamcmd/steamcmd.sh +login <steam-account> +workshop_build_item "${TMPDIR:-/tmp}/OverlordWarcasket-preview.vdf" +quit
    ```
    If `~/steamcmd/steamcmd.sh` is missing, it is the Linux build from Valve
    (`sudo apt install lib32gcc-s1`, then
    `mkdir -p ~/steamcmd && curl -sqL https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz | tar -zxf - -C ~/steamcmd`);
    the first run self-updates and takes a minute.
  - Verify on the page: the thumbnail animates, and the tags still read "Mod"
    plus the supported versions. SteamCMD cannot set tags and has left them
    alone so far; if they are ever gone, upload from the game again (which
    restores them and the static preview), report it, and rerun the SteamCMD
    update to see whether it repeats.
- **First release:** set the item public once everything above is on the page.

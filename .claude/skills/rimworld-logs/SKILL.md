---
name: rimworld-logs
description: Find and read RimWorld's Player.log, or disassemble the RimWorld API to inspect vanilla types and method signatures. Use when debugging runtime behavior, chasing an exception, or checking what a vanilla method actually does.
---

# RimWorld Logs and API Inspection

## Reading the log

1. **Enable RimWorld Dev Mode:** Settings → Dev Mode → Logging
2. **Log folder** (the layout depends on the installed game version):
   - **Windows:** `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\`
   - **WSL:** `/mnt/c/Users/*/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/`
   - **Linux (Steam):** `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/`
3. **Log file:**
   - **1.7 (Waylith):** `Logs/Player.log` in that folder (`Logs/Player-prev.log` is the boot
     before). Every entry is `YYYY-MM-DD HH:MM:SS.mmm  LEVEL [Category] message` followed by
     `    at ...` stack frames, so levels can be grepped directly:
     ```bash
     grep -hE '^\S+ \S+ +(ERROR|WARN)' "$LOGDIR/Logs/Player.log" | grep -v '\[Vulkan\]'
     ```
   - **1.6 (Unity):** `Player.log` directly in that folder, Unity's unstructured format. A 1.7
     install leaves this file in place, stale; check the timestamp before reading it.

Our own messages are prefixed `[Overlord Warcasket]`, so grep for that to isolate them from
vanilla and other mods' output.

## Inspecting the RimWorld API

The local RimWorld installation is the source of truth for API shapes; prefer it over the
`Krafs.Rimworld.Ref` NuGet package the build compiles against. `monodis` gives signatures,
`ilspycmd -t "Namespace.ClassName"` method bodies.

- **1.7 (Waylith):** the assemblies sit flat in the install root. `Assembly-CSharp.dll` is the
  game; `UnityStubs.dll` beside it holds every `UnityEngine.*` type the game and mods bind to,
  so inspect it for Unity-facing signatures (`Rect`, `Texture2D`, `GUI`, ...):
  ```bash
  monodis "$RIMWORLD_PATH/Assembly-CSharp.dll"
  ilspycmd "$RIMWORLD_PATH/Assembly-CSharp.dll" -t Verse.ModContentPack
  monodis --typedef "$RIMWORLD_PATH/UnityStubs.dll" | grep 'UnityEngine.Rect '
  ```
- **1.6 (Unity):** `"$RIMWORLD_PATH/RimWorldWin64_Data/Managed/Assembly-CSharp.dll"` with the
  `UnityEngine.*Module.dll` assemblies beside it.

The install is whichever Steam branch is selected, so a call path verified against it is
verified on that version only; say which in the def or code header when it matters.

## Reading 1.7 shaders

The 1.6 Unity shaders are locked in asset bundles, but 1.7 ships its compiled Slang shaders as
SPIR-V in the install's `Assets.zip` (`shaders/<hash>.spv-cache`, zstd entries), and SPIR-V
keeps the uniform names. `spirv-dump.py` beside this file reads them: `find <uniform>` lists
every shader declaring a name and saves the matches, `dump <file>` prints the member names of
the material block and the body, which for a mote shader is a few dozen lines of loads,
multiplies and samples.

```bash
python3 -I .claude/skills/rimworld-logs/spirv-dump.py find _DetailScrollSpeed
python3 -I .claude/skills/rimworld-logs/spirv-dump.py dump "$TMPDIR/rimworld-shaders/<hash>.spv-cache"
```

Uniform names are matched exactly, so a `shaderParameters` entry whose case differs from the
shader's declaration binds nothing (Core's `_detailScrollSpeed` against the shader's
`_DetailScrollSpeed` is the known case). A fact read here holds on 1.7 only; the 1.6 shader
is checked in game.

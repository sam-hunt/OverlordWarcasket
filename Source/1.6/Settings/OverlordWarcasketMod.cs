using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// Owns the mod settings only. The constructor runs before defs load, so it neither patches
// (ModInit's comment) nor touches the defs the settings override: ModInit, after PatchAll,
// reads their defaults (LoadDefaultsFromDefs) and applies the settings (ApplyToDefs) once defs
// exist, and WriteSettings re-applies them whenever the settings window closes.
public class OverlordWarcasketMod : Mod
{
    public OverlordWarcasketMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<OverlordWarcasketSettings>();
    }

    public static OverlordWarcasketSettings Settings { get; private set; }

    public override string SettingsCategory() => "OLWC_SettingsCategory".Translate();

    public override void DoSettingsWindowContents(Rect inRect) => Settings.DoWindowContents(inRect);

    public override void WriteSettings()
    {
        base.WriteSettings();
        Settings.ApplyToDefs();
    }
}

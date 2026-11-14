using System.Reflection;
using System.Xml;
using Verse;

namespace OverlordWarcasket;

// Runs match while the named setting of OverlordWarcasketSettings holds the named choice, nomatch
// otherwise; either branch may be absent. The setting is one of the settings' public
// IPatchSetting fields: a BoolSetting, which needs no choice (on is the match), or a setting of
// several choices such as the cerebrex art, whose choice is one of its values by name. An unset
// setting reads as its default. The XML patches apply after every Mod constructor has read its
// saved settings (LoadedModManager.LoadAllActiveMods runs CreateModClasses before ApplyPatches,
// 1.7.5000 decompile), so the value is the saved one and a change takes effect at the next
// startup; the setting's rows say so. An absent branch succeeds without touching the XML, so the
// game logs no failed patch. A name that is no such field, or a choice the setting does not
// have, is an error at patch time, which Tests/1.6/CerebrexNodeArtTests.cs catches before a boot
// does.
public class PatchOperationSetting : PatchOperation
{
    public string setting;
    public string choice;
    public PatchOperation match;
    public PatchOperation nomatch;

    protected override bool ApplyWorker(XmlDocument xml)
    {
        FieldInfo field = typeof(OverlordWarcasketSettings).GetField(setting ?? "", BindingFlags.Instance | BindingFlags.Public);
        if (field?.GetValue(OverlordWarcasketMod.Settings) is not IPatchSetting found)
        {
            Log.Error($"[Overlord Warcasket] {this}: the settings have no setting named '{setting}' that a patch can read.");
            return false;
        }
        if (!found.TryMatch(choice, out bool matches))
        {
            Log.Error($"[Overlord Warcasket] {this}: the setting '{setting}' has no choice '{choice}'.");
            return false;
        }
        PatchOperation operation = matches ? match : nomatch;
        return operation == null || operation.Apply(xml);
    }

    public override string ToString() => choice.NullOrEmpty() ? $"{base.ToString()}({setting})" : $"{base.ToString()}({setting}={choice})";
}

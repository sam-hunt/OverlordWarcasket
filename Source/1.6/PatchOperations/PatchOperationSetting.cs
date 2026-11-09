using System.Reflection;
using System.Xml;
using Verse;

namespace OverlordWarcasket;

// Runs match while the named switch of OverlordWarcasketSettings is on, nomatch otherwise; either
// branch may be absent. The name is one of the settings' public BoolSetting fields, read as its
// Value, so an unset switch reads as its default. The XML patches apply after every Mod
// constructor has read its saved settings (LoadedModManager.LoadAllActiveMods runs
// CreateModClasses before ApplyPatches, 1.7.5000 decompile), so the value is the saved one and a
// change takes effect at the next startup; the switch's row says so (BoolSetting.RequiresRestart).
// An absent branch succeeds without touching the XML, so the game logs no failed patch. A name
// that is no such field is an error at patch time, which Tests/1.6/CerebrexNodeArtTests.cs
// catches before a boot does.
public class PatchOperationSetting : PatchOperation
{
    public string setting;
    public PatchOperation match;
    public PatchOperation nomatch;

    protected override bool ApplyWorker(XmlDocument xml)
    {
        FieldInfo field = typeof(OverlordWarcasketSettings).GetField(setting ?? "", BindingFlags.Instance | BindingFlags.Public);
        if (field?.FieldType != typeof(BoolSetting))
        {
            Log.Error($"[Overlord Warcasket] {this}: the settings have no switch named '{setting}'.");
            return false;
        }
        var found = (BoolSetting)field.GetValue(OverlordWarcasketMod.Settings);
        PatchOperation operation = found.Value ? match : nomatch;
        return operation == null || operation.Apply(xml);
    }

    public override string ToString() => $"{base.ToString()}({setting})";
}

using System.Collections.Generic;
using Verse;

namespace OverlordWarcasket;

// The mech resurrector's idle crackle (see CompResurrectorSparks). The effecter is named in the
// armor def; its values and their rationale are in its own def
// (Defs/Effects/ResurrectorSparks.xml).
public class CompProperties_ResurrectorSparks : CompProperties
{
    public EffecterDef effecterDef;

    // Plays only while the wearer is drafted and able to cast, for wearers who can be drafted
    // (see CompResurrectorSparks). A mod setting defaulting to the XML, written back here.
    public bool onlyWhileDrafted = true;

    public CompProperties_ResurrectorSparks()
    {
        compClass = typeof(CompResurrectorSparks);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (string error in base.ConfigErrors(parentDef))
            yield return error;
        if (effecterDef == null)
            yield return "effecterDef is null";
        if (!parentDef.comps.Any(c => c is CompProperties_MechResurrector))
            yield return "needs a CompProperties_MechResurrector on the same def";
    }
}

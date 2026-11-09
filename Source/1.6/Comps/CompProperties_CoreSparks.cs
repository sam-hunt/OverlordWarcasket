using System.Collections.Generic;
using Verse;

namespace OverlordWarcasket;

// The resurrection core's idle crackle (see CompCoreSparks). The effecter is named in the armor
// def; its values and their rationale are in its own def (Defs/Effects/CoreSparks.xml).
public class CompProperties_CoreSparks : CompProperties
{
    public EffecterDef effecterDef;

    // Plays only while the wearer is drafted, for wearers who can be (see CompCoreSparks). A mod
    // setting defaulting to the XML, written back here.
    public bool onlyWhileDrafted = true;

    public CompProperties_CoreSparks()
    {
        compClass = typeof(CompCoreSparks);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (string error in base.ConfigErrors(parentDef))
            yield return error;
        if (effecterDef == null)
            yield return "effecterDef is null";
        if (!parentDef.comps.Any(c => c is CompProperties_ResurrectionCore))
            yield return "needs a CompProperties_ResurrectionCore on the same def";
    }
}

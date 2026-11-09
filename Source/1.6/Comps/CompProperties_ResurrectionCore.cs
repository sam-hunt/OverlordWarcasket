using System.Collections.Generic;
using Verse;

namespace OverlordWarcasket;

// The armor's resurrection core (see CompResurrectionCore): the charge store the resurrect mech
// ability spends. Values live in the armor def, rationale in its header; both fields are mod
// settings defaulting to them, written back here.
public class CompProperties_ResurrectionCore : CompProperties
{
    // Charges held when full; the core starts full.
    public int maxCharges = 10;

    // Recharge rate as a fraction of maxCharges per day: 0.01 is one percent of the capacity a
    // day, a whole charge every ten days on a ten-charge core. Zero never recharges.
    public float regenPerDay = 0.01f;

    public CompProperties_ResurrectionCore()
    {
        compClass = typeof(CompResurrectionCore);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (string error in base.ConfigErrors(parentDef))
            yield return error;
        if (maxCharges <= 0)
            yield return $"maxCharges must be positive, is {maxCharges}";
        if (regenPerDay < 0f)
            yield return $"regenPerDay must not be negative, is {regenPerDay}";
    }
}

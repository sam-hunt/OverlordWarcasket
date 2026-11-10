using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OverlordWarcasket;

// The armor's resurrection core (see CompResurrectionCore): the charge store the resurrect mech
// ability spends. The reload fields (maxCharges, ammoDef, ammoCountPerCharge, baseReloadTicks,
// chargeNoun, ...) are vanilla CompApparelReloadable's; a charge is one resurrection charge.
// Values live in the armor def, rationale in its header; maxCharges, chargesPerDay and
// ammoCountPerCharge are mod settings defaulting to them, written back here.
public class CompProperties_ResurrectionCore : CompProperties_ApparelReloadable
{
    // Charges the core recharges on its own per day, at any capacity. Zero never recharges.
    public float chargesPerDay = 1f;

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
        if (chargesPerDay < 0f)
            yield return $"chargesPerDay must not be negative, is {chargesPerDay}";
    }
}

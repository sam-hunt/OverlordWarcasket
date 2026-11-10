using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OverlordWarcasket;

// The resurrect mech ability's tuning, Biotech's CompProperties_ResurrectMech as a def extension
// on a VEF ability (the comp class only attaches to vanilla abilities). Values live in the
// ability def (AbilityDefs/ResurrectMech.xml), rationale in its header; maxCorpseAgeTicks and
// requireMechlink are mod settings defaulting to them, written back here.
public class ResurrectMechExtension : DefModExtension
{
    // How long after death a mech can still be resurrected; 0 or less is no limit.
    public int maxCorpseAgeTicks = 60000;

    public bool LimitsCorpseAge => maxCorpseAgeTicks > 0;

    // Charges each mech weight class costs; a class absent here cannot be resurrected. Vanilla's
    // own entry type, so the XML reads as the apocriton's.
    public List<MechChargeCosts> costs = [];

    // Played on each resurrected mech, and once at the targets' centre after the cast.
    public EffecterDef appliedEffecterDef;
    public EffecterDef centerEffecterDef;

    // Whether the pilot must be a mechanitor (carry a mechlink) to use the core.
    public bool requireMechlink = true;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors())
            yield return error;
        if (costs.NullOrEmpty())
        {
            yield return "costs list is empty";
            yield break;
        }
        foreach (MechChargeCosts cost in costs)
        {
            if (cost.weightClass == null)
                yield return $"costs list contains a null weight class with cost {cost.cost}";
        }
    }
}

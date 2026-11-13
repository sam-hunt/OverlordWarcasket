using RimWorld;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace OverlordWarcasket;

[DefOf]
public static class OLWC_DefOf
{
    // The armor's active (AbilityDefs/ResurrectMech.xml): the gauge reads its
    // ResurrectMechExtension for the cost table and the corpse window it quotes.
    public static AbilityDef OLWC_ResurrectMech;

    // The carried reload of the resurrection core (JobDefs/LoadCore.xml), ordered by
    // Command_LoadCore.
    public static JobDef OLWC_LoadCore;

    // The resurrect mech ability's cast (JobDefs/CastResurrectMech.xml), ordered by
    // Ability_ResurrectMech.
    public static JobDef OLWC_CastResurrectMech;

    static OLWC_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(OLWC_DefOf));
}

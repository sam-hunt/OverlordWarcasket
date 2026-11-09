using RimWorld;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace OverlordWarcasket;

[DefOf]
public static class OLWC_DefOf
{
    // The armor's active (AbilityDefs/ResurrectMech.xml): the gauge reads its
    // ResurrectMechExtension for the cost table and the corpse window it quotes.
    public static AbilityDef OLWC_ResurrectMech;

    static OLWC_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(OLWC_DefOf));
}

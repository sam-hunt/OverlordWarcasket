using Verse;
using Ability = VEF.Abilities.Ability;
using Command_Ability = VEF.Abilities.Command_Ability;

namespace OverlordWarcasket;

// The resurrect mech gizmo: VEF's ability command plus the top-right "charges / max" count that
// vanilla draws on a reloadable's verb command (the Medic's Command_AdrenalineShot idiom), named
// as the def's gizmoClass. The count is read live on every draw, so the cached command in
// Ability_ResurrectMech.GetGizmo needs no refresh for it.
public class Command_ResurrectMech : Command_Ability
{
    public Command_ResurrectMech(Pawn pawn, Ability ability) : base(pawn, ability)
    {
    }

    public override string TopRightLabel => (ability as Ability_ResurrectMech)?.Core?.LabelCharges;
}

using System.Collections.Generic;
using Verse;

namespace OverlordWarcasket;

// What sets the armor's mech stun pulse off (see CompMechStunPulse): the pilot's death, as the
// apocriton's, or the pilot being downed in its place. One or the other, never both. The names
// are saved in the mod settings and end the Keyed keys of the settings' radio rows.
public enum StunPulseTrigger
{
    Death,
    Downed,
}

// The armor's mech stun pulse (see CompMechStunPulse). Values live in the armor def, rationale
// in its header; trigger and radius are mod settings defaulting to them, written back here.
public class CompProperties_MechStunPulse : CompProperties
{
    public StunPulseTrigger trigger = StunPulseTrigger.Death;

    // In cells, an explosion's radius: the pulse reaches the cells within it that the pilot's
    // cell has line of sight to.
    public float radius = 9.9f;

    // The stun, and with it the pulse's sound and effects. Only Biotech's MechBandShockwave
    // stuns mechanoids (CompMechStunPulse's header).
    public DamageDef damageDef;

    // Cells per tick over 1.5: how fast the wave travels out from the pilot.
    public float propagationSpeed = 0.5f;

    public CompProperties_MechStunPulse()
    {
        compClass = typeof(CompMechStunPulse);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (string error in base.ConfigErrors(parentDef))
            yield return error;
        if (damageDef == null)
            yield return "damageDef is null";
        if (radius <= 0f)
            yield return $"radius must be positive, is {radius}";
        if (propagationSpeed <= 0f)
            yield return $"propagationSpeed must be positive, is {propagationSpeed}";
    }
}

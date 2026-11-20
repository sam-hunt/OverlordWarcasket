using System.Collections.Generic;
using Verse;

namespace OverlordWarcasket;

// The long-dead mechanitors Biotech leaves for the player to find, dressed in the set. On the
// armor (ThingDefs_Misc/Warcasket_Overlord.xml, whose header has the numbers and why the set
// gives the player nothing this way), which with otherPieces is the set the corpse wears.
// AncientPilot has the rules and the two patches that call it; the settings write enabled.
public class AncientPilotExtension : DefModExtension
{
    // Off, no corpse is dressed and neither patch draws on Rand.
    public bool enabled;

    // The chance that the corpse of each source wears the set, 0 to 1.
    public float shipChance;
    public float podChance;

    // The set's pieces besides the armor carrying this extension. A corpse is dressed in all of
    // them or in none.
    public List<ThingDef> otherPieces = [];

    public float ChanceFor(AncientPilotSource source)
    {
        if (!enabled)
            return 0f;
        return source == AncientPilotSource.Ship ? shipChance : podChance;
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors())
            yield return error;
        if (shipChance < 0f || shipChance > 1f)
            yield return $"shipChance must be between 0 and 1, is {shipChance}";
        if (podChance < 0f || podChance > 1f)
            yield return $"podChance must be between 0 and 1, is {podChance}";
        foreach (ThingDef piece in otherPieces)
        {
            if (piece?.apparel == null)
                yield return $"otherPieces names {piece?.defName ?? "a missing def"}, which is not apparel";
        }
    }
}

// Where Biotech generates a dead mechanitor: the crashed ship of the MechanitorShip quest, and
// the cryptosleep pod of the ancient mechanitor complex.
public enum AncientPilotSource
{
    Ship,
    Pod,
}

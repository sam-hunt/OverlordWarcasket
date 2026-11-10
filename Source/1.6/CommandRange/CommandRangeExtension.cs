using System.Collections.Generic;
using Verse;

namespace OverlordWarcasket;

// How far a mechanitor wearing the piece can order their drafted mechs. Vanilla stops every
// move and attack order for a colony mech at 24.9 cells from its overseer; the piece carrying
// this extension lifts that to the scope below, through the patch on
// MechanitorUtility.InMechanitorCommandRange (CommandRange has the rules). The XML puts it on the
// shoulders (ThingDefs_Misc/WarcasketShoulders_Overlord.xml, whose header has the rationale); the
// settings move it to the helmet, widen it to every mechanitor or switch it off, and override
// scope and radius, all written back here by CommandRange.Apply.
public class CommandRangeExtension : DefModExtension
{
    // Where orders may go: anywhere on the mech's map while the overseer is on it (CurrentMap),
    // anywhere on any map while the overseer stands on some map (Anywhere), or within radius
    // cells of the overseer on the same map (Radius), vanilla's own rule at 24.9.
    public CommandRangeScope scope = CommandRangeScope.CurrentMap;

    // The Radius scope's reach in cells, compared as vanilla compares its own: the squared
    // distance strictly below the square, so a .9 radius excludes the cells at the next whole
    // distance, as 24.9 excludes those at exactly 25.
    public float radius = CommandRange.VanillaRadius;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors())
            yield return error;
        if (radius <= 0f)
            yield return $"radius must be positive, is {radius}";
    }
}

public enum CommandRangeScope
{
    CurrentMap,
    Anywhere,
    Radius,
}
